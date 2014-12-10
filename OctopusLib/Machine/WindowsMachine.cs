using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.ComponentModel;
using AutomationHelper;
using System.Text.RegularExpressions;

namespace OctopusLib
{
    [Serializable]
    public class WindowsMachine : Machine
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private members
        #endregion

        #region Properties
        #endregion
        
        public string GetEnvironmentVariable(string ip, string username, string password, string envVarName)
        {
            string value = null;
            try
            {
                WMIHelper helper = new WMIHelper(ip, username, password);
                value = helper.GetEnvironmentVariable(envVarName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Can not get the Environment Variable {0} of remote machine {1}. {2}", envVarName, Name, ex.Message));
            }
            return value;
        }

        public override void Reboot(string ip, string username, string password)
        {
            string output = null, error = null;
            string exe = string.Format(@"{0}\psshutdown.exe", Environment.CurrentDirectory);
            string args = string.Format("-accepteula \\\\{0} -u {1} -p {2} -r -f -t 0", ip, username, password);
            ProcessHelper.Execute(exe, string.Empty, args, 60, out output, out error);
            Console.WriteLine("{0} is in rebooting", Name);
            // Wait until cannot ping it, that means the machine is in rebooting status.
            Ping ping = new Ping();
            while (true)
            {
                var reply = ping.Send(IP, 10000);
                if (reply.Status != IPStatus.Success)
                    break;
                System.Threading.Thread.Sleep(1000);
            }
        }

        public override bool IsActive(string ip, string username, string password)
        {
            //Check machine is rebooted and back
            DateTime rebootTime = DateTime.Now;
            bool isActive = false;

            while (!isActive && DateTime.Now.Subtract(rebootTime).TotalSeconds < RebootTimeoutSeconds)
            {
                string output = null, error = null;

                string exe = string.Format(@"{0}\PsService.exe", Environment.CurrentDirectory);
                string args = string.Format("-accepteula \\\\{0} -u {1} -p {2}  query spooler", ip, username, password);
                ProcessHelper.Execute(exe, string.Empty, args, 60, out output, out error);
                if (!string.IsNullOrEmpty(output))
                {
                    if (output.IndexOf("SERVICE_NAME: Spooler") >= 0)
                    {
                        isActive = true;
                    }
                }

                Console.Write(".");
                System.Threading.Thread.Sleep(1000);
            }

            return isActive;
        }

        #region INotifyPropertyChanged Members
        private void OnPropertyChanged(string propertyName)
        {
            if (!propertyName.Equals("IsSelected"))
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }   
        }
        #endregion


    }
}
