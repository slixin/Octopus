using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace OctopusLib
{
    public class RunInstance : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        #region Private members
        private Machine _machine;
        private Status _status;
        private Task _run;
        private ObservableCollectionEx<ActionInstance> _actioninstancecollection;
        private List<OctopusLib.Parameter> _tempParameters;
        #endregion

        #region Properties
        public Task Task
        {
            get
            {
                return _run;
            }
            set
            {
                if (value != _run)
                {
                    _run = value;
                    OnPropertyChanged("Task");
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

        public  ObservableCollectionEx<ActionInstance> ActionInstanceCollection
        {
            get
            {
                return _actioninstancecollection;
            }
            set
            {
                if (value != _actioninstancecollection)
                {
                    _actioninstancecollection = value;
                    OnPropertyChanged("ActionInstanceCollection");
                }
            }
        }

        public string LogFile { get; set; }

        public System.Windows.Threading.Dispatcher UIDispatcher { get; set; }
        #endregion

        #region Public Methods
        public void StartRun()
        {
            try
            {
                LogHelper.Log(LogFile, string.Format("****************** Machine: {0} ******************", Machine.Name));
                LogHelper.Log(LogFile, string.Format("===== RUN: {0} =====", Task.Name));
                _tempParameters = new List<Parameter>();
                Status currentRunStatus = Status;

                // Loop actions for current task
                foreach (OctopusLib.ActionInstance actionInstance in ActionInstanceCollection)
                {
                    LogHelper.Log(LogFile, string.Format("========== ACTION: {0} ==========", actionInstance.Action.Name));
                    actionInstance.Machine.Architecture = Machine.Architecture;

                    #region when task status is fail and current action is not a fixed action, do not exeucte the action
                    if (currentRunStatus == Status.Fail && (!actionInstance.Action.IsFixed))
                    {
                        LogHelper.Warn(LogFile, string.Format(@"[Warning] Action '{0}' is not executed, because of latest action failed.", actionInstance.Action.Name));
                        continue;
                    }
                    else
                    {
                        UpdateActionInstanceStatus(actionInstance, OctopusLib.Status.InProgress);
                    }
                    #endregion
                    Status currentActionStatus = actionInstance.Status;
                    
                    int totalCommands = actionInstance.Action.ActionCommands.Where(o => o.IsEnabled).Count();
                    #region Exeucte commands in action
                    foreach (OctopusLib.Command cmd in actionInstance.Action.ActionCommands.Where(o => o.IsEnabled).OrderBy(o => o.Sequence))
                    {
                        OctopusLib.ICommand currentCommand = null;
                        bool cmdResult = false;

                        try
                        {
                            #region Get Command by type
                            switch (cmd.CommandType)
                            {
                                case OctopusLib.RunCommandType.Copy:
                                    currentCommand = cmd as OctopusLib.CopyCommand;
                                    LogHelper.Log(LogFile, string.Format("=============== Command Copy ==============="));
                                    LogHelper.Log(LogFile, string.Format("Direction={0}, Source Directory={1}, Target Directory={2}, Files={3}, IsForce={4}, IsTry={5}",
                                        ((OctopusLib.CopyCommand)currentCommand).CopyDirection.ToString(),
                                        ((OctopusLib.CopyCommand)currentCommand).CopySourceDir,
                                        ((OctopusLib.CopyCommand)currentCommand).CopyTargetDir,
                                        ((OctopusLib.CopyCommand)currentCommand).CopySourceFiles,
                                        ((OctopusLib.CopyCommand)currentCommand).IsForce,
                                        ((OctopusLib.CopyCommand)currentCommand).Try));
                                    break;
                                case OctopusLib.RunCommandType.Local:
                                    currentCommand = cmd as OctopusLib.LocalCommand;
                                    // Ignore the step, because it can not be task on this OS.
                                    if (((OctopusLib.LocalCommand)currentCommand).Architecture != 0 && actionInstance.Machine.Architecture != ((OctopusLib.LocalCommand)currentCommand).Architecture)
                                    {
                                        currentCommand.Status = Status.NotRun;
                                        continue;
                                    }
                                    LogHelper.Log(LogFile, string.Format("=============== Command Local ==============="));
                                    LogHelper.Log(LogFile, string.Format("Expected Result={0}, Task on Architecture={1}, IsTry={2}",
                                        ((OctopusLib.LocalCommand)currentCommand).ExpectedResult,
                                        ((OctopusLib.LocalCommand)currentCommand).Architecture,
                                        ((OctopusLib.LocalCommand)currentCommand).Try));
                                    break;
                                case OctopusLib.RunCommandType.Remote:
                                    currentCommand = cmd as OctopusLib.RemoteCommand;
                                    // Ignore the step, because it can not be task on this OS.
                                    if (((OctopusLib.RemoteCommand)currentCommand).Architecture != 0 && actionInstance.Machine.Architecture != ((OctopusLib.RemoteCommand)currentCommand).Architecture)
                                    {
                                        currentCommand.Status = Status.NotRun;
                                        continue;
                                    }
                                    LogHelper.Log(LogFile, string.Format("=============== Command Remote ==============="));
                                    LogHelper.Log(LogFile,
                                        string.Format("Expected Result={0}, Task on Architecture={1}, IsReboot={2}, UIInteractive={3}, IsTry={4}, NotLoadProfile={5}, RunAsSystemAccount={6}, RemoteRunAsUsername={7}, RemoteRunAsPassword={8}",
                                            ((OctopusLib.RemoteCommand)currentCommand).ExpectedResult,
                                            ((OctopusLib.RemoteCommand)currentCommand).Architecture,
                                            ((OctopusLib.RemoteCommand)currentCommand).IsRebootRequired,
                                            ((OctopusLib.RemoteCommand)currentCommand).IsUIInteractive,
                                            ((OctopusLib.RemoteCommand)currentCommand).Try,
                                            ((OctopusLib.RemoteCommand)currentCommand).IsNotLoadProfile,
                                            ((OctopusLib.RemoteCommand)currentCommand).IsRunAsSystemAccount,
                                            ((OctopusLib.RemoteCommand)currentCommand).RemoteRunAsUsername,
                                            ((OctopusLib.RemoteCommand)currentCommand).RemoteRunAsPassword));
                                    break;
                                case OctopusLib.RunCommandType.LinuxSSH:
                                    currentCommand = cmd as OctopusLib.LinuxSSHCommand;
                                    LogHelper.Log(LogFile, string.Format("=============== Command Linux ==============="));
                                    LogHelper.Log(LogFile, string.Format("Expected Result={0}, Expected Prompt={1}",
                                        ((OctopusLib.LinuxSSHCommand)currentCommand).ExpectedResult,
                                        ((OctopusLib.LinuxSSHCommand)currentCommand).ExpectedPrompt,
                                        ((OctopusLib.LinuxSSHCommand)currentCommand).Try));
                                    break;
                            }
                            #endregion

                            LogHelper.Log(LogFile, string.Format("Command: {0}", string.IsNullOrEmpty(currentCommand.CommandText) ? 
                                string.Empty : 
                                Command.Normalize(currentCommand.CommandText, currentCommand.ParameterCollection, Machine)));
                            UpdateParameterCollection(currentCommand);                        
                            UpdateCommandStatus(currentCommand, OctopusLib.Status.InProgress);
                            cmdResult = currentCommand.Execute(actionInstance.Machine);
                            if (!string.IsNullOrEmpty(cmd.OutputParameter))
                            {
                                _tempParameters.Add(new OctopusLib.Parameter() { Name = cmd.OutputParameter, Value = cmd.OutputValue, IsEncrypted = false, });
                            }
                            LogHelper.Log(LogFile, string.Format("ExitCode:\r\n {0}", currentCommand.ExitCode));
                            //if (currentCommand.OutputValue.Length < 5000)
                                LogHelper.Log(LogFile, string.Format("Output:\r\n {0}", currentCommand.OutputValue));
                            //else
                                //LogHelper.Log(LogFile, string.Format("Output:\r\n {0} ...", currentCommand.OutputValue.Substring(0, 5000)));
                            LogHelper.Log(LogFile, string.Format("Message:\r\n {0}", currentCommand.Message));
                            
                            if (cmdResult)
                            {
                                LogHelper.Log(LogFile, string.Format("=============== Command Succeed ==============="));
                                UpdateCommandStatus(currentCommand, OctopusLib.Status.Pass);
                            }                                
                            else
                            {                                
                                if (currentCommand.Try)
                                {
                                    UpdateCommandStatus(currentCommand, OctopusLib.Status.Warn);
                                    LogHelper.Log(LogFile, string.Format("=============== Command Failed, But ignored ==============="));
                                }
                                else
                                {
                                    UpdateCommandStatus(currentCommand, OctopusLib.Status.Fail);
                                    LogHelper.Log(LogFile, string.Format("=============== Command Failed ==============="));
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {                            
                            LogHelper.Log(LogFile, ex.Message);
                            if ((currentCommand.Try))
                            {
                                UpdateCommandStatus(currentCommand, OctopusLib.Status.Warn);
                                LogHelper.Log(LogFile, string.Format("=============== Command Failed, But ignored ==============="));
                            }
                            else
                            {
                                UpdateCommandStatus(currentCommand, OctopusLib.Status.Fail);
                                LogHelper.Log(LogFile, string.Format("=============== Command Failed ==============="));
                                break;
                            }
                        }
                        finally
                        {
                            #region If command failed, and it is not a try command, then action is failure.
                            if (currentCommand.Status != OctopusLib.Status.NotRun && !cmdResult)
                            {
                                if (currentCommand.Try)
                                    currentActionStatus = OctopusLib.Status.Warn;
                                    //UpdateActionInstanceStatus(actionInstance, OctopusLib.Status.Warn);
                                else
                                    currentActionStatus = OctopusLib.Status.Fail;
                                    //UpdateActionInstanceStatus(actionInstance, OctopusLib.Status.Fail);
                            }
                            #endregion
                        }
                    }
                    #endregion

                    if (currentActionStatus == OctopusLib.Status.InProgress)
                        currentActionStatus = OctopusLib.Status.Pass;
                    UpdateActionInstanceStatus(actionInstance, currentActionStatus);

                    if (actionInstance.Status == Status.Fail)
                    {
                        LogHelper.Log(LogFile, string.Format("========== ACTION FAILED =========="));
                        #region Set Task instance status -> Fail, because Action failed.
                        // Set current task to be failure, because one of the action failed.
                        currentRunStatus = OctopusLib.Status.Fail;                        
                        #endregion
                    }
                    else
                    {
                        #region action instance status does not be set as failure after executed all commands, so set it to be pass
                        if(actionInstance.Status == OctopusLib.Status.Warn)
                            currentRunStatus = OctopusLib.Status.Warn;
                            //UpdateStatus(OctopusLib.Status.Warn);
                        else
                            currentRunStatus = OctopusLib.Status.Pass;
                            //UpdateActionInstanceStatus(actionInstance, OctopusLib.Status.Pass);
                        #endregion

                        LogHelper.Log(LogFile, string.Format("========== ACTION SUCCEED =========="));
                    }
                }

                if (currentRunStatus == OctopusLib.Status.InProgress)
                    currentRunStatus = OctopusLib.Status.Pass;
                UpdateStatus(currentRunStatus);
            }
            catch
            {
                UpdateStatus(OctopusLib.Status.Fail);
            }

            if (Status == Status.Fail)
                LogHelper.Log(LogFile, string.Format("===== RUN FAILED ====="));
            else
                LogHelper.Log(LogFile, string.Format("===== RUN SUCCEED ====="));
        }
        #endregion

        #region Private methods
        private void UpdateParameterCollection(ICommand cmd)
        {
            if(_tempParameters.Count > 0)
            {
                if (UIDispatcher != null)
                {
                    UIDispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                    {
                        foreach(OctopusLib.Parameter param in _tempParameters)
                        {
                            if (cmd.ParameterCollection.Where(o => o.Name.Equals(param.Name)).Count() > 0)
                            {
                                (cmd.ParameterCollection.Where(o => o.Name.Equals(param.Name)).Single() as OctopusLib.Parameter).Value = param.Value;
                            }
                            else
                            {
                                cmd.ParameterCollection.Add(new Parameter() { Name = param.Name, Value = param.Value, IsEncrypted = param.IsEncrypted, });
                            }
                        }
                        

                    });
                }
                else
                {
                    foreach (OctopusLib.Parameter param in _tempParameters)
                    {
                        if (cmd.ParameterCollection.Where(o => o.Name.Equals(param.Name)).Count() > 0)
                        {
                            (cmd.ParameterCollection.Where(o => o.Name.Equals(param.Name)).Single() as OctopusLib.Parameter).Value = param.Value;
                        }
                        else
                        {
                            cmd.ParameterCollection.Add(new Parameter() { Name = param.Name, Value = param.Value, IsEncrypted = param.IsEncrypted, });
                        }
                    }
                }
            }
        }

        private void UpdateActionInstanceStatus(ActionInstance ai, Status status)
        {
            if (UIDispatcher != null)
            {
                UIDispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    ai.Status = status;
                });
            }
            else
            {
                ai.Status = status;
            }
            
        }

        private void UpdateStatus(Status status)
        {
            if (UIDispatcher != null)
            {
                UIDispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    Status = status;
                });
            }
            else
            {
                Status = status;
            }

        }

        private void UpdateCommandStatus(ICommand cmd, Status status)
        {
            if (UIDispatcher != null)
            {
                UIDispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                {
                    cmd.Status = status;
                });
            }
            else
            {
                cmd.Status = status;
            }

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
