using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace OctopusLib
{
    public class ActionInstance : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private Action _action;
        private Machine _machine;
        private Status _status;
        #endregion

        #region Properties

        public Action Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value != _status)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        
        public Machine Machine
        {
            get
            {
                return _machine;
            }
            set
            {
                if (value != _machine)
                {
                    _machine = value;
                    OnPropertyChanged("Machine");
                }
            }
        }

        #endregion

        #region Public Methods

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
