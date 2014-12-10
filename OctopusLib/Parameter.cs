using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Net;
using System.ComponentModel;

namespace OctopusLib
{
    [Serializable]
    public class Parameter : ICloneable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private string _name;
        private string _value;
        private bool _isencrypted;
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
                    OnPropertyChanging("Name");
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public bool IsEncrypted
        {
            get
            {
                return _isencrypted;
            }
            set
            {
                if (value != _isencrypted)
                {
                    _isencrypted = value;
                    OnPropertyChanged("IsPassword");
                }
            }
        }

        public string MachineID { get; set; }
        #endregion

        #region Public methods
        public object Clone()
        {
            var param = new Parameter
            {
                Name = Name,
                Value = Value,
                IsEncrypted = IsEncrypted,
            };

            return param;
        }
        #endregion

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
