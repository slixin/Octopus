using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutomationHelper;

namespace OctopusLib
{
    [Serializable]
    public class LinuxSSHCommand : Command
    {
        #region Private members
        private string _expectedresult;
        private string _expectedprompt;
        private SSHType _sshtype;
        private int _timeoutSeconds;
        private bool _isRebootRequired;
        #endregion

        #region Properties
        public SSHType SshType 
        {
            get
            {
                return _sshtype;
            }
            set
            {
                if (value != _sshtype)
                {
                    _sshtype = value;
                    OnPropertyChanged("SshType");
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

        public string ExpectedPrompt
        {
            get
            {
                return _expectedprompt;
            }
            set
            {
                if (value != _expectedprompt)
                {
                    _expectedprompt = value;
                    OnPropertyChanged("ExpectedPrompt");
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
        #endregion
        
        public override bool Execute(Machine machine)
        {
            bool result = false;
            

            LinuxMachine lMachine = machine as LinuxMachine;

            int retryCount = 0;
            while (!result && retryCount <= RetryTimes)
            {
                try
                {
                    string output = string.Empty;
                    string error = string.Empty;
                    lMachine.Connect(machine.IP,
                            OctopusLib.Command.Normalize(machine.Username, ParameterCollection),
                            OctopusLib.Command.Normalize(machine.Password, ParameterCollection));

                    if (SshType == SSHType.Exec)
                    { 
                        int res = lMachine.SSH.RunCommand(OctopusLib.Command.Normalize(CommandText, ParameterCollection, machine), out output, out error);
                        this.OutputValue = output;
                        if (res != 0)
                        {
                            result = false;
                            if (!string.IsNullOrEmpty(error))
                                this.Message = error;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else if (SshType == SSHType.Stream)
                    {
                        int res = lMachine.SSH.SendCommand(
                            OctopusLib.Command.Normalize(CommandText, ParameterCollection, machine),
                            OctopusLib.Command.Normalize(ExpectedPrompt, ParameterCollection, machine),
                            TimeOutSeconds,
                            out output);
                        result = true;
                        this.OutputValue = output;
                    }


                    if (result)
                    {
                        if (string.IsNullOrEmpty(ExpectedResult))
                        {
                            result = true;
                        }
                        else
                        {
                            if (Regex.IsMatch(OutputValue, OctopusLib.Command.Normalize(ExpectedResult, ParameterCollection, machine), RegexOptions.Multiline))
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                                Message = string.Format("The command result is not as expected. Expected Result: {0}, Actual Result: {1}", OctopusLib.Command.Normalize(ExpectedResult, ParameterCollection, machine), OutputValue);
                            }

                        }
                    }        
            
                    if (result)
                    {
                        if (IsRebootRequired)
                        {
                            lMachine.Reboot(
                                machine.IP, 
                                OctopusLib.Command.Normalize(machine.Username, ParameterCollection),
                                OctopusLib.Command.Normalize(machine.Password, ParameterCollection));
                            Console.WriteLine("Wait until active");
                            if (!machine.IsActive(
                                    machine.IP,
                                    OctopusLib.Command.Normalize(machine.Username, ParameterCollection),
                                    OctopusLib.Command.Normalize(machine.Password, ParameterCollection)))
                            {
                                result = false;
                                Message = "Reboot machine fail, it is still not back yet.";
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Message = ex.Message;
                } 
                finally
                {
                    lMachine.Disconnect();
                }
                retryCount++;
                System.Threading.Thread.Sleep(RetryIntervalSeconds * 1000);
                Console.WriteLine("Result: " + result);
            }

            return result;
        }
    }
}
