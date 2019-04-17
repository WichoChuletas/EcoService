using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using zkemkeeper;

namespace WindowsServiceEco
{
	class zkemkeeperHandler
	{
        public zkemkeeper.CZKEMClass axCZKEM1;

		public event EventHandler OnFinger;
		//public event EventHandler<VerifyEventArgs> OnVerify;
		private System.Diagnostics.EventLog eventLog1 = new System.Diagnostics.EventLog();
		
		private bool bIsConnected = false;
		private int iMachineNumber = 1;
		public string ip = "10.10.31.51"; //BioMetric Ip Device
		public string port = "4370"; //Biometric Port Device



		public zkemkeeperHandler()
		{
			((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
			this.eventLog1.Log = "MyNewLog";
			this.eventLog1.Source = "MySource";
			((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();

			eventLog1.WriteEntry("ZkemKeeperHandler Constructor");
		}

		public void startService()
		{
            axCZKEM1 = new zkemkeeper.CZKEMClass();
            eventLog1.WriteEntry("Connecting to Device ("+ip+":"+port+")");
			bIsConnected = axCZKEM1.Connect_Net(ip, Convert.ToInt32(port));
			if (bIsConnected == true)
			{
				eventLog1.WriteEntry("Connected to the Device !!");
				iMachineNumber = 1;
				if (axCZKEM1.RegEvent(iMachineNumber, 65535))
				{
					this.axCZKEM1.OnFinger += new zkemkeeper._IZKEMEvents_OnFingerEventHandler(axCZKEM1_OnFinger);
					this.axCZKEM1.OnVerify += new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                    this.axCZKEM1.OnAttTransactionEx += new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);
                   
                    //This Log Appears in Event Viewer
                    eventLog1.WriteEntry("Define events (OnFingers and OnVerify) !");
					//This Line Fires Event in Service1.cs for testing event handler
					//Finger(EventArgs.Empty);
				}
			}
			else
			{
				eventLog1.WriteEntry("Unable to Connect the Device");
			}


		}


		public void stopService()
		{
			if (bIsConnected)
			{
				axCZKEM1.Disconnect();
				bIsConnected = false;
			}
		}

        public void axCZKEM1_OnAttTransactionEx(string sEnrollNumber, int iIsInValid, int iAttState, int iVerifyMethod, int iYear, int iMonth, int iDay, int iHour, int iMinute, int iSecond, int iWorkCode)
        {
            
            eventLog1.WriteEntry("OnAttTrasactionEx has been Triggered \n Verified OK on date :" + DateTime.Now.ToString());
            axCZKEM1_GetUserInfo(sEnrollNumber);
        }

        public int sta_SetUserInfo(ListBox lblOutputInfo, TextBox txtUserID, TextBox txtName, ComboBox cbPrivilege, TextBox txtCardnumber, TextBox txtPassword)
        {
            if (GetConnectState() == false)
            {
                lblOutputInfo.Items.Add("*Please connect first!");
                return -1024;
            }

            if (txtUserID.Text.Trim() == "" || cbPrivilege.Text.Trim() == "")
            {
                lblOutputInfo.Items.Add("*Please input data first!");
                return -1023;
            }

            int iPrivilege = cbPrivilege.SelectedIndex;

            bool bFlag = false;
            if (iPrivilege == 5)
            {
                lblOutputInfo.Items.Add("*User Defined Role is Error! Please Register again!");
                return -1023;
            }

            /*
            if(iPrivilege == 4)
            {
                axCZKEM1.IsUserDefRoleEnable(iMachineNumber, 4, out bFlag);

                if (bFlag == false)
                {
                    lblOutputInfo.Items.Add("*User Defined Role is unable!");
                    return -1023;
                }
            }
             */
            //lblOutputInfo.Items.Add("[func IsUserDefRoleEnable]Temporarily unsupported");

            int iPIN2Width = 0;
            int iIsABCPinEnable = 0;
            int iT9FunOn = 0;
            string strTemp = "";
            axCZKEM1.GetSysOption(GetMachineNumber(), "~PIN2Width", out strTemp);
            iPIN2Width = Convert.ToInt32(strTemp);
            axCZKEM1.GetSysOption(GetMachineNumber(), "~IsABCPinEnable", out strTemp);
            iIsABCPinEnable = Convert.ToInt32(strTemp);
            axCZKEM1.GetSysOption(GetMachineNumber(), "~T9FunOn", out strTemp);
            iT9FunOn = Convert.ToInt32(strTemp);
            /*
            axCZKEM1.GetDeviceInfo(iMachineNumber, 76, ref iPIN2Width);
            axCZKEM1.GetDeviceInfo(iMachineNumber, 77, ref iIsABCPinEnable);
            axCZKEM1.GetDeviceInfo(iMachineNumber, 78, ref iT9FunOn);
            */

            if (txtUserID.Text.Length > iPIN2Width)
            {
                lblOutputInfo.Items.Add("*User ID error! The max length is " + iPIN2Width.ToString());
                return -1022;
            }

            if (iIsABCPinEnable == 0 || iT9FunOn == 0)
            {
                if (txtUserID.Text.Substring(0, 1) == "0")
                {
                    lblOutputInfo.Items.Add("*User ID error! The first letter can not be as 0");
                    return -1022;
                }

                foreach (char tempchar in txtUserID.Text.ToCharArray())
                {
                    if (!(char.IsDigit(tempchar)))
                    {
                        lblOutputInfo.Items.Add("*User ID error! User ID only support digital");
                        return -1022;
                    }
                }
            }

            int idwErrorCode = 0;
            string sdwEnrollNumber = txtUserID.Text.Trim();
            string sName = txtName.Text.Trim();
            string sCardnumber = txtCardnumber.Text.Trim();
            string sPassword = txtPassword.Text.Trim();

            bool bEnabled = true;
            /*if (iPrivilege == 4)
            {
                bEnabled = false;
                iPrivilege = 0;
            }
            else
            {
                bEnabled = true;
            }*/

            axCZKEM1.EnableDevice(iMachineNumber, false);
            axCZKEM1.SetStrCardNumber(sCardnumber);//Before you using function SetUserInfo,set the card number to make sure you can upload it to the device
            if (axCZKEM1.SSR_SetUserInfo(iMachineNumber, sdwEnrollNumber, sName, sPassword, iPrivilege, bEnabled))//upload the user's information(card number included)
            {
                lblOutputInfo.Items.Add("Set user information successfully");
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                lblOutputInfo.Items.Add("*Operation failed,ErrorCode=" + idwErrorCode.ToString());
            }
            axCZKEM1.RefreshData(iMachineNumber);//the data in the device should be refreshed
            axCZKEM1.EnableDevice(iMachineNumber, true);

            return 1;
        }


        public void axCZKEM1_GetUserInfo(string sEnrollNumber)
        {

            int idwErrorCode = 0;
            int iPrivilege = 0;
            string strName = "";
            string strCardno = "";
            string strPassword = "";
            bool bEnabled = false;

            if (axCZKEM1.SSR_GetUserInfo(iMachineNumber, sEnrollNumber, out strName, out strPassword, out iPrivilege, out bEnabled))//upload the user's information(card number included)
            {
                eventLog1.WriteEntry("Information User \n Name: " + strName + "\n Password: " + strPassword + "\n Card Number: " + strCardno + "\n Level Privilege: " + iPrivilege + "\n Get user information successfully");
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                eventLog1.WriteEntry("The User is not exist");
                eventLog1.WriteEntry("*Operation failed, ErrorCode: " + idwErrorCode.ToString());
            }


        }

        private void axCZKEM1_OnFinger()
		{
			eventLog1.WriteEntry("method Onfinger");
		}


		private void axCZKEM1_OnVerify(int iUserID)
		{
			eventLog1.WriteEntry("method Onverify");
		}


	}


}
