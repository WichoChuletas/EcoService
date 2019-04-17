using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsServiceEco
{
	public partial class WinServEco : ServiceBase
	{
		zkemkeeperHandler zkh;
        ActiveDirHandler AcDir;
		public WinServEco()
		{
			InitializeComponent();

			eventLog1 = new EventLog();
			if (!System.Diagnostics.EventLog.SourceExists("MySource"))
			{
				System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
			}
			eventLog1.Source = "MySource";
			eventLog1.Log = "MyNewLog";

			eventLog1.WriteEntry("Initializing WinServEco!!");
		
			
		
		}

		private void startZkemKeeperHandler()
		{
			eventLog1.WriteEntry("Creating ZkemKeeper Handler Class");
			zkh = new zkemkeeperHandler();
            AcDir = new ActiveDirHandler();
            //zkh.OnFinger += OnFinger;
            //zkh.OnVerify += OnVerify;
            zkh.startService();
            AcDir.GetDirectoryEntry();
            AcDir.Authenticate();
            //AcDir.Disable();

        }

		private void stopZKHandler()
		{
			eventLog1.WriteEntry("Disconnecting From Device ("+ zkh.ip +")...");
			zkh.stopService();
		}


		protected override void OnStart(string[] args)
		{
            Thread createComAndMessagePumpThread = new Thread(() =>
            {
                startZkemKeeperHandler();

                Application.Run();

            });
            createComAndMessagePumpThread.SetApartmentState(ApartmentState.STA);

            createComAndMessagePumpThread.Start();


            eventLog1.WriteEntry("WinServEco Starts!!");
			
		}

		protected override void OnStop()
		{
			eventLog1.WriteEntry("WinServEco Stops!!");
            stopZKHandler();

        }
	}
}
