using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationHelper;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;

namespace OctopusLib
{
    [Serializable]
    public class LocalCommand : Command
    {
        #region Private members
        private int _architecture;
        private string _expectedresult;
        private int _timeoutSeconds;
        private bool _issystemcommand;
        #endregion

        #region Properties
        public bool IsSystemCommand
        {
            get
            {
                return _issystemcommand;
            }
            set
            {
                if (value != _issystemcommand)
                {
                    _issystemcommand = value;
                    OnPropertyChanged("IsSystemCommand");
                }
            }
        }
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
        #endregion

        #region Public methods
        public override bool Execute(Machine machine)
        {
            bool result = false;

            int exitcode = -1;

            string cmd = OctopusLib.Command.Normalize(CommandText, ParameterCollection, machine);
            string exe = @"cmd.exe";
            string args = null;
            string output = null, error = null;

            int retryCount = 0;
            while (!result && retryCount <= RetryTimes)
            {
                try
                {
                    if (IsSystemCommand)
                    {
                        args = cmd;
                    }
                    else
                    {
                        if (Regex.IsMatch(cmd, "^[A-Za-z][:]"))
                            args = string.Format(@"/c {0}", cmd);
                        else
                            args = string.Format(@"/c {0}", Path.Combine(Environment.CurrentDirectory, cmd));
                    }
                    
                    exitcode = ProcessHelper.Execute(exe, string.Empty, args, TimeOutSeconds, out output, out error);
                    ExitCode = exitcode;
                    OutputValue = output;

                    if (exitcode == Convert.ToInt32(ExpectedResult))
                    {
                        result = true;
                    }
                    else
                    {
                        Message = string.Format("ExitCode is {0}, not equals expected result {1}.", exitcode, ExpectedResult);
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    Message = ex.Message;
                }
                retryCount++;
                System.Threading.Thread.Sleep(RetryIntervalSeconds * 3000);
            }

            return result;
        }
        #endregion
    }
}
