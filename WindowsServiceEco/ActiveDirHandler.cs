using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace WindowsServiceEco
{

    public class ActiveDirHandler
    {
        private System.Diagnostics.EventLog eventLog1 = new System.Diagnostics.EventLog();
        public string Domain;
        public ActiveDirHandler()
        {
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.eventLog1.Log = "MyNewLog";
            this.eventLog1.Source = "MySource";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();

            Domain = "10.11.1.206";
            eventLog1.WriteEntry("ActiveDirHandler Constructor");
        }
        public DirectoryEntry GetDirectoryEntry()
        {
            DirectoryEntry entry = new DirectoryEntry();
            entry.Path = "LDAP://10.11.1.206/CN=Administrator;DC=eclab.local";
            entry.AuthenticationType = AuthenticationTypes.Secure;
            return entry;
        }

        public bool Authenticate()
        {
            string userName = "luis.vera@eclab.local";
            string password = "C1sc0123..";
            bool authentic = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain, userName, password);
                object nativeObject = entry.NativeObject;
                authentic = true;
                eventLog1.WriteEntry("Usuario Verificado");
            }
            catch (DirectoryServicesCOMException e)
            {
                eventLog1.WriteEntry("Error: " + e);
            }
            return authentic;
        }

        public void Disable()
        {
            string userName = "luis.vera@eclab.local";
            string password = "C1sc0123..";
            try
            {
                DirectoryEntry user = new DirectoryEntry("LDAP://" + Domain, userName, password);
                int val = (int)user.Properties["userAccountControl"].Value;
                user.Properties["userAccountControl"].Value = val | 0x2;
                //ADS_UF_ACCOUNTDISABLE;

                user.CommitChanges();
                

                eventLog1.WriteEntry("Usuario deahabilitado");
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                eventLog1.WriteEntry("Error: " + E);

            }
        }

    }
}
