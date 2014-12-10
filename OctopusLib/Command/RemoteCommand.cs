using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationHelper;
using System.ComponentModel;

namespace OctopusLib
{
    [Serializable]
    public class RemoteCommand : Command
    {
        #region Private members
        private int _architecture;
        private int _timeoutSeconds;
        private string _expectedresult;        
        private bool _isRebootRequired;
        private bool _isUIInteractive;
        private bool _isNottLoadProfile;
        private bool _isRunAsSystemAccount;
        private bool _isRunAsLimittedUser;
        private string _workingdirectory;
        private string _remoteRunAsUsername;
        private string _remoteRunAsPassword;
        private bool _isNotWaitForTerminate;
        #endregion

        #region Properties
        public int TimeOutSeconds
        {
            get
            {
                return _timeoutSeconds;
            }
            set
            {
                if (value != _timeoutSeconds)
                {
                    _timeoutSeconds = value;
                    OnPropertyChanged("TimeOutSeconds");
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

        public bool IsRebootRequired
        {
            get
            {
                return _isRebootRequired;
            }
            set
            {
                if (value != _isRebootRequired)
                {
                    _isRebootRequired = value;
                    OnPropertyChanged("IsRebootRequired");
                }
            }
        }

        public bool IsUIInteractive
        {
            get
            {
                return _isUIInteractive;
            }
            set
            {
                if (value != _isUIInteractive)
                {
                    _isUIInteractive = value;
                    OnPropertyChanged("IsUIInteractive");
                }
            }
        }

        public bool IsNotLoadProfile
        {
            get
            {
                return _isNottLoadProfile;
            }
            set
            {
                if (value != _isNottLoadProfile)
                {
                    _isNottLoadProfile = value;
                    OnPropertyChanged("IsNotLoadProfile");
                }
            }
        }

        public bool IsRunAsSystemAccount
        {
            get
            {
                return _isRunAsSystemAccount;
            }
            set
            {
                if (value != _isRunAsSystemAccount)
                {
                    _isRunAsSystemAccount = value;
                    OnPropertyChanged("IsRunAsSystemAccount");
                }
            }
        }

        public bool IsRunAsLimittedUser
        {
            get
            {
                return _isRunAsLimittedUser;
            }
            set
            {
                if (value != _isRunAsLimittedUser)
                {
                    _isRunAsLimittedUser = value;
                    OnPropertyChanged("IsRunAsLimittedUser");
                }
            }
        }

        public string ExpectedResult
        {
            get
            {
                return _expectedresult;
            }
            set
            {
                if (value != _expectedresult)
                {
                    _expectedresult = value;
                    OnPropertyChanged("ExpectedResult");
                }
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return _workingdirectory;
            }
            set
            {
                if (value != _workingdirectory)
                {
                    _workingdirectory = value;
                    OnPropertyChanged("WorkingDirectory");
                }
            }
        }

        public string RemoteRunAsUsername
        {
            get
            {
                return _remoteRunAsUsername;
            }
            set
            {
                if (value != _remoteRunAsUsername)
                {
                    _remoteRunAsUsername = value;
                    OnPropertyChanged("RemoteRunAsUsername");
                }
            }
        }

        public string RemoteRunAsPassword
        {
            get
            {
                return _remoteRunAsPassword;
            }
            set
            {
                if (value != _remoteRunAsPassword)
                {
                    _remoteRunAsPassword = value;
                    OnPropertyChanged("RemoteRunAsPassword");
                }
            }
        }

        public bool IsNotWaitForTerminate
        {
            get
            {
                return _isNotWaitForTerminate;
            }
            set
            {
                if (value != _isNotWaitForTerminate)
                {
                    _isNotWaitForTerminate = value;
                    OnPropertyChanged("IsNotWaitForTerminate");
                }
            }
        }

        #endregion

        #region Public methods
        public override bool Execute(Machine machine)
        {
            bool result = false;
            int retryCount = 0;
            while(!result && retryCount <= RetryTimes)
            {
                try
                {
                    result = RemoteExec(machine);
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                }
                retryCount++;
                System.Threading.Thread.Sleep(RetryIntervalSeconds * 1000);
            }

            return result;
        }
        #endregion

        #region private methods
        private bool RemoteExec(Machine machine)
        {
            bool result = false;
            int exitcode = -1;

            string cmd = OctopusLib.Command.Normalize(CommandText, ParameterCollection, machine);
            string exe = string.Format(@"{0}\PaExec.exe", Environment.CurrentDirectory);
            if (!System.IO.File.Exists(exe))
                throw new Exception(string.Format("{0} is not exists.", exe));
            string args = null;
            string output = null, error = null;

            string remoterunas_username = null;
            string remoterunas_password = null;

            remoterunas_username = string.IsNullOrEmpty(RemoteRunAsUsername) ? 
                                        (string.IsNullOrEmpty(machine.Domain) ? OctopusLib.Command.Normalize(machine.Username, ParameterCollection) : 
                                                                                string.Format(@"{0}\{1}", OctopusLib.Command.Normalize(machine.Domain, ParameterCollection), OctopusLib.Command.Normalize(machine.Username, ParameterCollection))) : 
                                        OctopusLib.Command.Normalize(RemoteRunAsUsername, ParameterCollection);
            remoterunas_password = string.IsNullOrEmpty(RemoteRunAsPassword) ? OctopusLib.Command.Normalize(machine.Password, ParameterCollection) : OctopusLib.Command.Normalize(RemoteRunAsPassword, ParameterCollection);

            args = string.Format("-accepteula \\\\{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                machine.IP,
                string.Format("-u {0}", remoterunas_username),
                string.Format("-p {0}", remoterunas_password),
                IsUIInteractive ? "-i 1" : string.Empty,
                IsNotLoadProfile ? "-e" : string.Empty,
                IsRunAsSystemAccount ?"-s" : string.Empty,
                IsRunAsLimittedUser ? "-l" : string.Empty,
                IsNotWaitForTerminate ? "-d" : string.Empty,
                string.IsNullOrEmpty(WorkingDirectory) ? string.Empty : string.Format("-w \"{0}\"", OctopusLib.Command.Normalize(WorkingDirectory, ParameterCollection)),
                cmd);

            exitcode = ProcessHelper.Execute(exe, string.Empty, args, TimeOutSeconds, out output, out error);
            OutputValue = output.Replace("\r\n", string.Empty);
            ExitCode = exitcode;
            Message = string.Format("ExitCode: {0}\r\n{1}", exitcode, OutputValue);
            if (exitcode == Convert.ToInt32(ExpectedResult))
                result = true;
            else
                result = false;  

            if (IsRebootRequired)
            {
                machine.Reboot(machine.IP,
                    string.IsNullOrEmpty(RemoteRunAsUsername) ? OctopusLib.Command.Normalize(machine.Username, ParameterCollection) : OctopusLib.Command.Normalize(RemoteRunAsUsername, ParameterCollection),
                    string.IsNullOrEmpty(RemoteRunAsPassword) ? OctopusLib.Command.Normalize(machine.Password, ParameterCollection) : OctopusLib.Command.Normalize(RemoteRunAsPassword, ParameterCollection));
                if (!machine.IsActive(machine.IP,
                    string.IsNullOrEmpty(RemoteRunAsUsername) ? OctopusLib.Command.Normalize(machine.Username, ParameterCollection) : OctopusLib.Command.Normalize(RemoteRunAsUsername, ParameterCollection),
                    string.IsNullOrEmpty(RemoteRunAsPassword) ? OctopusLib.Command.Normalize(machine.Password, ParameterCollection) : OctopusLib.Command.Normalize(RemoteRunAsPassword, ParameterCollection)))
                {
                    result = false;
                    Message = "Reboot machine fail, it is still not back yet.";
                }
            }

            return result;
        }
        #endregion
    }
}
