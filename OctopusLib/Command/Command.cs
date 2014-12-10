using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace OctopusLib
{
    [Serializable]
    public abstract class Command : ICommand, ICloneable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private bool _try;
        private bool _isenabled;
        private int _sequence;
        private string _commandtext;
        private RunCommandType _commandtype;
        private Status _status;
        private int _retryTimes;
        private int _retryIntervalSeconds;
        private string _outputParameter;
        #endregion

        #region Properties
        public bool Try
        {
            get
            {
                return _try;
            }
            set
            {
                if (value != _try)
                {
                    _try = value;
                    OnPropertyChanged("Try");
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

        public string CommandText
        {
            get
            {
                return _commandtext;
            }
            set
            {
                if (value != _commandtext)
                {
                    _commandtext = value;
                    OnPropertyChanged("CommandText");
                }
            }
        }

        public RunCommandType CommandType
        {
            get
            {
                return _commandtype;
            }
            set
            {
                if (value != _commandtype)
                {
                    _commandtype = value;
                    OnPropertyChanged("CommandType");
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

        public int RetryTimes
        {
            get
            {
                return _retryTimes;
            }
            set
            {
                if (value != _retryTimes)
                {
                    _retryTimes = value;
                    OnPropertyChanged("RetryTimes");
                }
            }
        }

        public int RetryIntervalSeconds
        {
            get
            {
                return _retryIntervalSeconds;
            }
            set
            {
                if (value != _retryIntervalSeconds)
                {
                    _retryIntervalSeconds = value;
                    OnPropertyChanged("RetryIntervalSeconds");
                }
            }
        }

        public string OutputParameter
        {
            get
            {
                return _outputParameter;
            }
            set
            {
                if (value != _outputParameter)
                {
                    _outputParameter = value;
                    OnPropertyChanged("OutputParameter");
                }
            }
        }

        public string Message { get; set; }
        public string OutputValue { get; set; }
        public int ExitCode { get; set; }

        public ObservableCollectionEx<Parameter> ParameterCollection { get; set; }
        #endregion

        #region Public methods
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public virtual bool Execute(Machine machine) { return true; }

        public static string Normalize(string text, ObservableCollectionEx<Parameter> parameterList)
        {
            return Normalize(text, parameterList, null);
        }

        public static string Normalize(string text, ObservableCollectionEx<Parameter> parameterList, Machine machine)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int normalizedCount = 0;

            string normalizedCommand = text;

            MatchCollection cmdVars = Regex.Matches(text, @"{%(.+?)%}", RegexOptions.IgnoreCase);
            if (cmdVars.Count > 0)
            {
                foreach (Match cmdVar in cmdVars)
                {
                    string cmdVarName = cmdVar.Groups[1].Value;

                    switch (cmdVarName)
                    {
                        case "MACHINE_NAME":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_NAME" : machine.Name);
                            normalizedCount++;
                            break;
                        case "MACHINE_IP":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_IP" : machine.IP);
                            normalizedCount++;
                            break;
                        case "MACHINE_USERNAME":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_USERNAME" : machine.Username);
                            normalizedCount++;
                            break;
                        case "MACHINE_PASSWORD":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_PASSWORD" : machine.Password);
                            normalizedCount++;
                            break;
                        case "MACHINE_DOMAIN":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_DOMAIN" : machine.Domain);
                            normalizedCount++;
                            break;
                        case "MACHINE_ISLINUX":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, machine == null ? "MACHINE_ISLINUX" : machine.IsLinux.ToString());
                            normalizedCount++;
                            break;
                        case "NOW":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss"));
                            normalizedCount++;
                            break;
                        case "ME":
                            normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                            normalizedCount++;                            
                            break;
                        default:
                            if (parameterList.Where(o => o.Name.Equals(cmdVarName)).Count() > 0)
                            {
                                string cmdVarValue = (parameterList.Where(o => o.Name.Equals(cmdVarName)).Single() as Parameter).Value;
                                normalizedCommand = Regex.Replace(normalizedCommand, cmdVar.Value, cmdVarValue);
                                normalizedCount++;
                            }
                            break;
                    }
                }

                if (normalizedCount > 0 && Regex.IsMatch(normalizedCommand, @"{%(.+?)%}", RegexOptions.IgnoreCase))
                    normalizedCommand = Normalize(normalizedCommand, parameterList, machine);
            }
            else
            {
                normalizedCommand = text;
            }
            return normalizedCommand;
        }
        #endregion

        #region INotifyPropertyChanged Members
        protected void OnPropertyChanged(string propertyName)
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

        #region Private methods
        #endregion

    }
}
