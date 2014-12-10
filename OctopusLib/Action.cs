using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace OctopusLib
{
    public class Action : ICloneable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private string _name;
        private ObservableCollectionEx<Command> _actioncommands;
        private Machine _machine;
        private bool _isenabled;
        private bool _isfixed;
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
                    OnPropertyChanging("Name");                    
                    _name = value;
                    OnPropertyChanged("Name");
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

        public ObservableCollectionEx<Command> ActionCommands
        {
            get
            {
                return _actioncommands;
            }
            set
            {
                if (value != _actioncommands)
                {
                    _actioncommands = value;
                    OnPropertyChanged("ActionCommands");
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

        public object Clone()
        {
            var action = new Action
            {
                Name = Name,
                IsEnabled = IsEnabled,
                IsFixed = IsFixed,
                Machine = Machine,
                Sequence = Sequence,
                ActionCommands = new ObservableCollectionEx<Command>(),
            };

            foreach(Command command in ActionCommands)
            {
                action.ActionCommands.Add(command.Clone() as Command);
            }

            return action;
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
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion
    }    
}
