using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;


namespace WindowsServiceEco
{
	public partial class WinServEco : ServiceBase
	{
		zkemkeeperHandler zkh;
		public WinServEco()
		{
			InitializeComponent();

			eventLog1 = new EventLog();
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
			{
				System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
			}
			eventLog1.WriteEntry("Initializing WinServEco!!");
		
			startZkemKeeperHandler();
		
		}

		private void startZkemKeeperHandler()
		{
			eventLog1.WriteEntry("Creating ZkemKeeper Handler Class");
			zkh = new zkemkeeperHandler();
			//zkh.OnFinger += OnFinger;
			//zkh.OnVerify += OnVerify;
			zkh.startService();
		}

		private void stopZKHandler()
		{
			eventLog1.WriteEntry("Disconnecting From Device ("+ zkh.ip +")...");
			zkh.stopService();
		}


		protected override void OnStart(string[] args)
		{
			eventLog1.WriteEntry("WinServEco Starts!!");
		}

		protected override void OnStop()
		{
			eventLog1.WriteEntry("WinServEco Stops!!");
		}
	}
}
