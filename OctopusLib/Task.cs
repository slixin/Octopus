using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace OctopusLib
{
    public class TaskAction : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private string _name;
        private bool _isfixed;
        private Machine _machine;
        private bool _isenabled;
        private int _sequence;
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

        public bool IsFixed
        {
            get
            {
                return _isfixed;
            }
            set
            {
                if (value != _isfixed)
                {
                    _isfixed = value;
                    OnPropertyChanged("IsFixed");
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

        public bool IsEnabled
        {
            get
            {
                return _isenabled;
            }
            set
            {
                if (value != _isenabled)
                {
                    _isenabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public int Sequence
        {
            get
            {
                return _sequence;
            }
            set
            {
                if (value != _sequence)
                {
                    _sequence = value;
                    OnPropertyChanged("Sequence");
                }
            }
        }
        
        #endregion

        public object Clone()
        {
            return this.MemberwiseClone();
        }

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    public class Task : ICloneable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private string _name;
        private ObservableCollectionEx<TaskAction> _taskActionCollection;
        private ObservableCollectionEx<Machine> _machines;
        private bool _isenabled;
        private int _sequence;
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

        public ObservableCollectionEx<TaskAction> TaskActionCollection
        {
            get
            {
                return _taskActionCollection;
            }
            set
            {
                if (value != _taskActionCollection)
                {
                    _taskActionCollection = value;
                    OnPropertyChanged("TaskActionCollection");
                }
            }
        }

        public ObservableCollectionEx<Machine> Machines
        {
            get
            {
                return _machines;
            }
            set
            {
                if (value != _machines)
                {
                    _machines = value;
                    OnPropertyChanged("Machines");
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isenabled;
            }
            set
            {
                if (value != _isenabled)
                {
                    _isenabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public int Sequence
        {
            get
            {
                return _sequence;
            }
            set
            {
                if (value != _sequence)
                {
                    _sequence = value;
                    OnPropertyChanged("Sequence");
                }
            }
        }
        
        #endregion

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #region INotifyPropertyChanged Members
        private void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
