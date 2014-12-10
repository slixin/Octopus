using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.ComponentModel;
using AutomationHelper;
using System.Text.RegularExpressions;
using Renci.SshNet;

namespace OctopusLib
{
    [Serializable]
    public class LinuxMachine : Machine
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private members
        #endregion

        #region Properties
        public SSHHelper SSH { get; set; }
        #endregion

        public void Connect(string ip, string username, string password)
        {
            SSH = new SSHHelper(ip, username, password);

            if (SSH.SSHClient == null)
                throw new Exception(string.Format("Machine {0} cannot be connected", Name));
        }
        public void Disconnect()
        {
            if (SSH != null)
                SSH.SSHClient.Disconnect();
        }

        public override void Reboot(string ip, string username, string password)
        {
            if (SSH == null)
                return;
            string output = string.Empty;
            string error = string.Empty;
            if (SSH.RunCommand("reboot", out output, out error) == 0)
            {
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
            else
            {
                throw new Exception(error);
            }
            

            return;
        }

        public override bool IsActive(string ip, string username, string password)
        {
            //Check machine is rebooted and back
            DateTime rebootTime = DateTime.Now;
            bool isActive = false;

            while (!isActive && DateTime.Now.Subtract(rebootTime).TotalSeconds < RebootTimeoutSeconds)
            {
                try
                {
                    if (PingHost(ip))
                    {
                        isActive = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return isActive;
        }

        private bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();

            try
            {
                PingReply reply = pinger.Send(nameOrAddress);

                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
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
