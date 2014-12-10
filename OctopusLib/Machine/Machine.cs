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
    public class Machine : ICloneable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private string _name;
        private string _ip;
        private string _username;
        private string _password;
        private string _domain;
        private int _architecture;
        private bool _islinux;
        #endregion

        #region Properties

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                if (value != _ip)
                {
                    _ip = value;
                    OnPropertyChanged("IP");
                }
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (value != _username)
                {
                    _username = value;
                    OnPropertyChanged("Username");
                }
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value != _password)
                {
                    _password = value;
                    OnPropertyChanged("Password");
                }
            }
        }

        public string Domain
        {
            get
            {
                return _domain;
            }
            set
            {
                if (value != _domain)
                {
                    _domain = value;
                    OnPropertyChanged("Domain");
                }
            }
        }

        public int Architecture
        {
            get
            {
                return _architecture;
            }
            set
            {
                if (value != _architecture)
                {
                    _architecture = value;
                    OnPropertyChanged("Architecture");
                }
            }
        }

        public bool IsLinux
        {
            get
            {
                return _islinux;
            }
            set
            {
                if (value != _islinux)
                {
                    _islinux = value;
                    OnPropertyChanged("IsLinux");
                }
            }
        }

        public int RebootTimeoutSeconds = 900;
        #endregion
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public Machine()
        {
        }

        public virtual void Reboot(string ip, string username, string password) {}

        public virtual bool IsActive(string ip, string username, string password) { return true; }

        #region INotifyPropertyChanged Members
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }  
        }

        private void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }
}
