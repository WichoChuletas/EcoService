using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using zkemkeeper;

namespace WindowsServiceEco
{
	class zkemkeeperHandler
	{
		public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
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

        //This method doesn't run :(
        private void axCZKEM1_OnFinger()
        {
            Finger(EventArgs.Empty);
        }

        //This method doesn't run too :(
        private void axCZKEM1_OnVerify(int iUserID)
        {
            VerifyEventArgs args = new VerifyEventArgs();
            args.UserID = iUserID;
            Verify(args);
        }



    }


}
