using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AutomationHelper;
using System.Text.RegularExpressions;

namespace OctopusLib
{
    public class OctopusInstance
    {
        private string _configfile;
        private string _logfilePath;
        private string _machineId;

        public ObservableCollectionEx<OctopusLib.Task> TaskCollection;
        public ObservableCollectionEx<OctopusLib.Action> ActionCollection;
        public ObservableCollectionEx<OctopusLib.Machine> MachineCollection;
        public ObservableCollectionEx<OctopusLib.Parameter> ParameterCollection;
        public ObservableCollectionEx<OctopusLib.RunInstance> RunInstanceCollection;

        public OctopusInstance()
        {
            _machineId = Common.WMI.GetProcessorSerial();
            _logfilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Log");
            if (!System.IO.Directory.Exists(_logfilePath))
                System.IO.Directory.CreateDirectory(_logfilePath);

            ParameterCollection = new ObservableCollectionEx<Parameter>();
            MachineCollection = new ObservableCollectionEx<Machine>();
            ActionCollection = new ObservableCollectionEx<Action>();
            TaskCollection = new ObservableCollectionEx<Task>();
            RunInstanceCollection = new ObservableCollectionEx<RunInstance>();
        }

        public OctopusInstance(string configFile)
        {
            _machineId = Common.WMI.GetProcessorSerial();
            _configfile = configFile;
            _logfilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Log");
            if (!System.IO.Directory.Exists(_logfilePath))
                System.IO.Directory.CreateDirectory(_logfilePath);

            ParameterCollection = new ObservableCollectionEx<Parameter>();
            MachineCollection = new ObservableCollectionEx<Machine>();
            ActionCollection = new ObservableCollectionEx<Action>();
            TaskCollection = new ObservableCollectionEx<Task>();
            RunInstanceCollection = new ObservableCollectionEx<RunInstance>();
        }

        public void Load()
        {
            try
            {
                if (!System.IO.File.Exists(_configfile))
                    throw new Exception(string.Format("The config file {0} is not exists.", _configfile));

                XmlDocument doc = new XmlDocument();
                doc.Load(_configfile);

                // Get Parameter list
                foreach (XmlNode paramNode in doc.SelectNodes("Topology/Parameters/Parameter"))
                {
                    bool isencrypted = paramNode.Attributes["IsEncrypted"] == null ? false : Convert.ToBoolean(paramNode.Attributes["IsEncrypted"].Value);
                    string value = string.Empty;
                    if (isencrypted)
                    {
                        try { value = Encryption.Decrypt(paramNode.Attributes["Value"].Value.Trim(), _machineId); }
                        catch { }
                    }
                    else
                    {
                        value = paramNode.Attributes["Value"].Value.Trim();
                    }
                    ParameterCollection.Add(new Parameter()
                    {
                        Name = paramNode.Attributes["Name"].Value.Trim(),
                        Value = value,
                        IsEncrypted = isencrypted,
                        MachineID = _machineId,
                    });
                }

                // Get machine list
                foreach (XmlNode machineNode in doc.SelectNodes("Topology/Machines/Machine"))
                {
                    if (Convert.ToBoolean(machineNode.Attributes["IsLinux"].Value))
                    {
                        MachineCollection.Add(new OctopusLib.LinuxMachine()
                        {
                            Name = machineNode.Attributes["Name"].Value,
                            IP = machineNode.Attributes["IP"].Value,
                            Username = machineNode.Attributes["Username"].Value,
                            Password = machineNode.Attributes["Password"].Value,
                            Domain = machineNode.Attributes["Domain"].Value,
                            Architecture = Convert.ToInt32(machineNode.Attributes["Architecture"].Value),
                            IsLinux = true,
                        });
                    }
                    else
                    {
                        MachineCollection.Add(new OctopusLib.WindowsMachine()
                        {
                            Name = machineNode.Attributes["Name"].Value,
                            IP = machineNode.Attributes["IP"].Value,
                            Username = machineNode.Attributes["Username"].Value,
                            Password = machineNode.Attributes["Password"].Value,
                            Domain = machineNode.Attributes["Domain"].Value,
                            Architecture = Convert.ToInt32(machineNode.Attributes["Architecture"].Value),
                            IsLinux = false,
                        });
                    }
                }

                // get Deployment action list
                foreach (XmlNode actionNode in doc.SelectNodes("Topology/Actions/Action"))
                {
                    OctopusLib.Action action = new OctopusLib.Action();
                    action.Name = actionNode.Attributes["Name"].Value;
                    action.ActionCommands = new ObservableCollectionEx<Command>();
                    foreach (XmlNode cmdNode in actionNode.SelectNodes("Command"))
                    {
                        string cmdType = cmdNode.Attributes["Type"] == null ? "Execute" : cmdNode.Attributes["Type"].Value;
                        int seq = action.ActionCommands.Count + 1;
                        switch (cmdType.ToLower())
                        {
                            case "copy":
                                action.ActionCommands.Add(new OctopusLib.CopyCommand()
                                {
                                    CommandText = string.Empty,
                                    CommandType = OctopusLib.RunCommandType.Copy,
                                    CopyDirection =  cmdNode.Attributes["Direction"].Value.Equals("LocalToRemote", StringComparison.InvariantCultureIgnoreCase) ? OctopusLib.CopyCommand.Direction.LocalToRemote : OctopusLib.CopyCommand.Direction.RemoteToLocal,
                                    CopySourceFiles = cmdNode.Attributes["SourceFiles"] == null ? string.Empty : cmdNode.Attributes["SourceFiles"].Value,
                                    CopySourceDir = cmdNode.Attributes["SourceDir"] == null ? string.Empty : cmdNode.Attributes["SourceDir"].Value,
                                    CopyTargetDir = cmdNode.Attributes["TargetDir"].Value,
                                    IsEnabled = Convert.ToBoolean(cmdNode.Attributes["IsEnabled"].Value),
                                    IsForce = Convert.ToBoolean(cmdNode.Attributes["Force"].Value),
                                    Try = Convert.ToBoolean(cmdNode.Attributes["Try"].Value),
                                    Status = OctopusLib.Status.NotRun,
                                    Sequence = seq,
                                    RetryTimes = Convert.ToInt32(cmdNode.Attributes["RetryTimes"].Value),
                                    RetryIntervalSeconds = Convert.ToInt32(cmdNode.Attributes["RetryIntervalSeconds"].Value),
                                });
                                break;
                            case "linuxssh":
                                action.ActionCommands.Add(new OctopusLib.LinuxSSHCommand()
                                {
                                    CommandText = System.Net.WebUtility.HtmlDecode(cmdNode.InnerText),
                                    CommandType = OctopusLib.RunCommandType.LinuxSSH,
                                    IsRebootRequired = cmdNode.Attributes["Reboot"] == null ? false : Convert.ToBoolean(cmdNode.Attributes["Reboot"].Value),
                                    ExpectedPrompt = cmdNode.Attributes["ExpectedPrompt"] == null ? string.Empty : cmdNode.Attributes["ExpectedPrompt"].Value,
                                    ExpectedResult = cmdNode.Attributes["ExpectedResult"] == null ? string.Empty : cmdNode.Attributes["ExpectedResult"].Value,
                                    IsEnabled = Convert.ToBoolean(cmdNode.Attributes["IsEnabled"].Value),
                                    Try = Convert.ToBoolean(cmdNode.Attributes["Try"].Value),
                                    Status = OctopusLib.Status.NotRun,
                                    Sequence = seq,
                                    RetryTimes = Convert.ToInt32(cmdNode.Attributes["RetryTimes"].Value),
                                    RetryIntervalSeconds = Convert.ToInt32(cmdNode.Attributes["RetryIntervalSeconds"].Value),
                                    SshType = (SSHType)Enum.Parse(typeof(SSHType), cmdNode.Attributes["SSHType"].Value),
                                    TimeOutSeconds = cmdNode.Attributes["TimeOutSeconds"] == null ? 0 : Convert.ToInt32(cmdNode.Attributes["TimeOutSeconds"].Value),
                                });
                                break;
                            case "local":
                                action.ActionCommands.Add(new OctopusLib.LocalCommand()
                                {
                                    CommandText = System.Net.WebUtility.HtmlDecode(cmdNode.InnerText),
                                    CommandType = OctopusLib.RunCommandType.Local,
                                    Architecture = Convert.ToInt32(cmdNode.Attributes["Architecture"].Value),
                                    IsEnabled = Convert.ToBoolean(cmdNode.Attributes["IsEnabled"].Value),
                                    ExpectedResult = cmdNode.Attributes["ExpectedResult"] == null ? string.Empty : cmdNode.Attributes["ExpectedResult"].Value,
                                    Status = OctopusLib.Status.NotRun,
                                    Sequence = seq,
                                    Try = Convert.ToBoolean(cmdNode.Attributes["Try"].Value),
                                    TimeOutSeconds = cmdNode.Attributes["TimeOutSeconds"] == null ? 0 : Convert.ToInt32(cmdNode.Attributes["TimeOutSeconds"].Value),
                                    RetryTimes = Convert.ToInt32(cmdNode.Attributes["RetryTimes"].Value),
                                    RetryIntervalSeconds = Convert.ToInt32(cmdNode.Attributes["RetryIntervalSeconds"].Value),
                                    OutputParameter = cmdNode.Attributes["OutputParameter"] == null ? string.Empty : cmdNode.Attributes["OutputParameter"].Value,
                                    IsSystemCommand = cmdNode.Attributes["IsSystemCommand"] == null ? false : Convert.ToBoolean(cmdNode.Attributes["IsSystemCommand"].Value),
                                });
                                break;
                            case "remote":
                            default:
                                action.ActionCommands.Add(new OctopusLib.RemoteCommand()
                                {
                                    CommandText = System.Net.WebUtility.HtmlDecode(cmdNode.InnerText),
                                    CommandType = OctopusLib.RunCommandType.Remote,
                                    Architecture = Convert.ToInt32(cmdNode.Attributes["Architecture"].Value),
                                    IsRebootRequired = Convert.ToBoolean(cmdNode.Attributes["Reboot"].Value),
                                    IsUIInteractive = Convert.ToBoolean(cmdNode.Attributes["Interactive"].Value),
                                    IsEnabled = Convert.ToBoolean(cmdNode.Attributes["IsEnabled"].Value),
                                    ExpectedResult = cmdNode.Attributes["ExpectedResult"] == null ? string.Empty : cmdNode.Attributes["ExpectedResult"].Value,
                                    IsNotLoadProfile = Convert.ToBoolean(cmdNode.Attributes["NotLoadProfile"].Value),
                                    IsRunAsSystemAccount = Convert.ToBoolean(cmdNode.Attributes["RunAsSystemAccount"].Value),
                                    IsRunAsLimittedUser = Convert.ToBoolean(cmdNode.Attributes["RunAsLimittedUser"].Value),
                                    IsNotWaitForTerminate = cmdNode.Attributes["Terminate"] == null ? false : Convert.ToBoolean(cmdNode.Attributes["Terminate"].Value),
                                    WorkingDirectory = cmdNode.Attributes["WorkingDirectory"] == null ? string.Empty : cmdNode.Attributes["WorkingDirectory"].Value,
                                    Status = OctopusLib.Status.NotRun,
                                    Sequence = seq,
                                    Try = Convert.ToBoolean(cmdNode.Attributes["Try"].Value),
                                    TimeOutSeconds = cmdNode.Attributes["TimeOutSeconds"] == null ? 0 : Convert.ToInt32(cmdNode.Attributes["TimeOutSeconds"].Value),
                                    RemoteRunAsUsername = string.IsNullOrEmpty(cmdNode.Attributes["RemoteRunAsUsername"].Value) ? string.Empty : cmdNode.Attributes["RemoteRunAsUsername"].Value,
                                    RemoteRunAsPassword = string.IsNullOrEmpty(cmdNode.Attributes["RemoteRunAsPassword"].Value) ? string.Empty : cmdNode.Attributes["RemoteRunAsPassword"].Value,
                                    RetryTimes = Convert.ToInt32(cmdNode.Attributes["RetryTimes"].Value),
                                    RetryIntervalSeconds = Convert.ToInt32(cmdNode.Attributes["RetryIntervalSeconds"].Value),
                                    OutputParameter = cmdNode.Attributes["OutputParameter"] == null ? string.Empty : cmdNode.Attributes["OutputParameter"].Value,
                                });
                                break;
                        }
                    }

                    ActionCollection.Add(action);
                }

                //get Deployment task list
                foreach (XmlNode runNode in doc.SelectNodes("Topology/Tasks/Task"))
                {
                    OctopusLib.Task task = new OctopusLib.Task();
                    task.Name = runNode.Attributes["Name"].Value;
                    task.IsEnabled = Convert.ToBoolean(runNode.Attributes["IsEnabled"].Value);
                    task.Sequence = runNode.Attributes["Sequence"] == null ? 0 : Convert.ToInt32(runNode.Attributes["Sequence"].Value);

                    task.Machines = new ObservableCollectionEx<OctopusLib.Machine>();
                    foreach (string machineName in runNode.Attributes["Machine"].Value.Split(new char[] { ',', ';', '|' }))
                    {
                        if (MachineCollection.Where(o => o.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                            task.Machines.Add(MachineCollection.Where(o => o.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase)).Single() as OctopusLib.Machine);
                    }

                    task.TaskActionCollection = new ObservableCollectionEx<OctopusLib.TaskAction>();
                    foreach (XmlNode actionNode in runNode.SelectNodes("TaskAction"))
                    {
                        int seq = task.TaskActionCollection.Count + 1;
                        if (ActionCollection.Where(o => o.Name.Equals(actionNode.InnerText.Trim(), StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                        {
                            OctopusLib.TaskAction taskAction = new TaskAction()
                            {
                                IsEnabled = Convert.ToBoolean(actionNode.Attributes["IsEnabled"].Value),
                                IsFixed = Convert.ToBoolean(actionNode.Attributes["Fixed"].Value),
                                Machine = actionNode.Attributes["Machine"] != null ? MachineCollection.Where(o => o.Name.Equals(actionNode.Attributes["Machine"].Value, StringComparison.InvariantCultureIgnoreCase)).Single() as OctopusLib.Machine: null,
                                Sequence = seq,
                                Name = actionNode.InnerText.Trim(),
                            };
                            task.TaskActionCollection.Add(taskAction);
                        }
                    }
                    TaskCollection.Add(task);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Save(string filename)
        {
            try
            {
                XmlDocument doc = XMLHelper.CreateXmlDocument("Topology");
                XmlNode rootNode = doc.LastChild;

                #region Parameter Section
                XmlNode paramsNode = XMLHelper.CreateChildNode(rootNode, "Parameters");
                foreach (OctopusLib.Parameter param in ParameterCollection)
                {
                    XmlNode paramNode = XMLHelper.CreateChildNode(paramsNode, "Parameter");
                    XMLHelper.CreateAttribute(paramNode, "Name", param.Name);
                    XMLHelper.CreateAttribute(paramNode, "Value", param.IsEncrypted ? Encryption.Encrypt(param.Value, _machineId) : param.Value);
                    if (param.IsEncrypted)
                        XMLHelper.CreateAttribute(paramNode, "IsEncrypted", param.IsEncrypted.ToString());
                }
                #endregion

                #region Machine Section
                XmlNode machinesNode = XMLHelper.CreateChildNode(rootNode, "Machines");
                foreach (OctopusLib.Machine machine in MachineCollection)
                {
                    XmlNode machineNode = XMLHelper.CreateChildNode(machinesNode, "Machine");
                    XMLHelper.CreateAttribute(machineNode, "Name", machine.Name);
                    XMLHelper.CreateAttribute(machineNode, "IP", machine.IP);
                    XMLHelper.CreateAttribute(machineNode, "Username", machine.Username);
                    XMLHelper.CreateAttribute(machineNode, "Password", Regex.IsMatch(machine.Password, @"{%(.+?)%}") ? machine.Password : Encryption.Encrypt(machine.Password, _machineId));
                    XMLHelper.CreateAttribute(machineNode, "Domain", machine.Domain);
                    XMLHelper.CreateAttribute(machineNode, "Architecture", machine.Architecture.ToString());
                    XMLHelper.CreateAttribute(machineNode, "IsLinux", machine.IsLinux.ToString());
                }
                #endregion

                #region Action Section
                XmlNode actionsNode = XMLHelper.CreateChildNode(rootNode, "Actions");
                foreach (OctopusLib.Action action in ActionCollection)
                {
                    XmlNode actionNode = XMLHelper.CreateChildNode(actionsNode, "Action");
                    XMLHelper.CreateAttribute(actionNode, "Name", action.Name);
                    foreach (OctopusLib.Command command in action.ActionCommands.OrderBy(o => o.Sequence))
                    {
                        XmlNode cmdNode = XMLHelper.CreateChildNode(actionNode, "Command");
                        XMLHelper.CreateAttribute(cmdNode, "Type", command.CommandType.ToString());
                        XMLHelper.CreateAttribute(cmdNode, "Try", command.Try.ToString());
                        XMLHelper.CreateAttribute(cmdNode, "IsEnabled", command.IsEnabled.ToString());
                        XMLHelper.CreateAttribute(cmdNode, "RetryTimes", command.RetryTimes.ToString());
                        XMLHelper.CreateAttribute(cmdNode, "RetryIntervalSeconds", command.RetryIntervalSeconds.ToString());
                        switch (command.CommandType)
                        {
                            case RunCommandType.Copy:
                                XMLHelper.CreateAttribute(cmdNode, "Direction", ((OctopusLib.CopyCommand)command).CopyDirection.ToString());
                                if (!string.IsNullOrEmpty(((OctopusLib.CopyCommand)command).CopySourceFiles))
                                    XMLHelper.CreateAttribute(cmdNode, "SourceFiles", ((OctopusLib.CopyCommand)command).CopySourceFiles.ToString());
                                if (!string.IsNullOrEmpty(((OctopusLib.CopyCommand)command).CopySourceDir))
                                    XMLHelper.CreateAttribute(cmdNode, "SourceDir", ((OctopusLib.CopyCommand)command).CopySourceDir.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "TargetDir", ((OctopusLib.CopyCommand)command).CopyTargetDir.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Force", ((OctopusLib.CopyCommand)command).IsForce.ToString());
                                break;
                            case RunCommandType.Remote:
                                XMLHelper.CreateAttribute(cmdNode, "ExpectedResult", ((OctopusLib.RemoteCommand)command).ExpectedResult.ToString());
                                if (!string.IsNullOrEmpty(((OctopusLib.RemoteCommand)command).WorkingDirectory))
                                    XMLHelper.CreateAttribute(cmdNode, "WorkingDirectory", ((OctopusLib.RemoteCommand)command).WorkingDirectory.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Reboot", ((OctopusLib.RemoteCommand)command).IsRebootRequired.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Interactive", ((OctopusLib.RemoteCommand)command).IsUIInteractive.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "NotLoadProfile", ((OctopusLib.RemoteCommand)command).IsNotLoadProfile.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "RunAsSystemAccount", ((OctopusLib.RemoteCommand)command).IsRunAsSystemAccount.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "RunAsLimittedUser", ((OctopusLib.RemoteCommand)command).IsRunAsLimittedUser.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Terminate", ((OctopusLib.RemoteCommand)command).IsNotWaitForTerminate.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Architecture", ((OctopusLib.RemoteCommand)command).Architecture.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "TimeOutSeconds", ((OctopusLib.RemoteCommand)command).TimeOutSeconds.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "RemoteRunAsUsername", string.IsNullOrEmpty(((OctopusLib.RemoteCommand)command).RemoteRunAsUsername) ? string.Empty : ((OctopusLib.RemoteCommand)command).RemoteRunAsUsername.ToString());
                                XMLHelper.CreateAttribute(cmdNode, 
                                    "RemoteRunAsPassword", 
                                    string.IsNullOrEmpty(((OctopusLib.RemoteCommand)command).RemoteRunAsPassword) ? string.Empty : Regex.IsMatch(((OctopusLib.RemoteCommand)command).RemoteRunAsPassword.ToString(), @"{%(.+?)%}") ?
                                    ((OctopusLib.RemoteCommand)command).RemoteRunAsPassword.ToString() : Encryption.Encrypt(((OctopusLib.RemoteCommand)command).RemoteRunAsPassword.ToString(), _machineId));
                                XMLHelper.CreateAttribute(cmdNode, "OutputParameter", string.IsNullOrEmpty(((OctopusLib.RemoteCommand)command).OutputParameter) ? string.Empty : ((OctopusLib.RemoteCommand)command).OutputParameter.ToString());
                                cmdNode.InnerXml = System.Net.WebUtility.HtmlEncode(command.CommandText);
                                break;
                            case RunCommandType.Local:
                                XMLHelper.CreateAttribute(cmdNode, "IsSystemCommand", ((OctopusLib.LocalCommand)command).IsSystemCommand.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "ExpectedResult", ((OctopusLib.LocalCommand)command).ExpectedResult.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Architecture", ((OctopusLib.LocalCommand)command).Architecture.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "TimeOutSeconds", ((OctopusLib.LocalCommand)command).TimeOutSeconds.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "OutputParameter", string.IsNullOrEmpty(((OctopusLib.LocalCommand)command).OutputParameter) ? string.Empty : ((OctopusLib.LocalCommand)command).OutputParameter.ToString());
                                cmdNode.InnerXml = System.Net.WebUtility.HtmlEncode(command.CommandText);
                                break;
                            case RunCommandType.LinuxSSH:
                                XMLHelper.CreateAttribute(cmdNode, "ExpectedResult", ((OctopusLib.LinuxSSHCommand)command).ExpectedResult == null ? string.Empty : ((OctopusLib.LinuxSSHCommand)command).ExpectedResult.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "ExpectedPrompt", ((OctopusLib.LinuxSSHCommand)command).ExpectedPrompt == null ? string.Empty : ((OctopusLib.LinuxSSHCommand)command).ExpectedPrompt.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "SSHType", ((OctopusLib.LinuxSSHCommand)command).SshType.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "TimeOutSeconds", ((OctopusLib.LinuxSSHCommand)command).TimeOutSeconds.ToString());
                                XMLHelper.CreateAttribute(cmdNode, "Reboot", ((OctopusLib.LinuxSSHCommand)command).IsRebootRequired.ToString());
                                if (!string.IsNullOrEmpty(command.CommandText))
                                    cmdNode.InnerXml = System.Net.WebUtility.HtmlEncode(command.CommandText);
                                break;
                        }
                    }
                }
                #endregion

                #region Task Section
                XmlNode runsNode = XMLHelper.CreateChildNode(rootNode, "Tasks");
                foreach (OctopusLib.Task task in TaskCollection.OrderBy(o => o.Sequence))
                {
                    XmlNode runNode = XMLHelper.CreateChildNode(runsNode, "Task");
                    XMLHelper.CreateAttribute(runNode, "Name", task.Name);
                    XMLHelper.CreateAttribute(runNode, "Sequence", task.Sequence.ToString());
                    XMLHelper.CreateAttribute(runNode, "IsEnabled", task.IsEnabled.ToString());

                    StringBuilder msb = new StringBuilder();
                    foreach (OctopusLib.Machine machine in task.Machines)
                    {
                        if (msb.Length == 0)
                            msb.Append(string.Format("{0}", machine.Name));
                        else
                            msb.Append(string.Format(",{0}", machine.Name));
                    }
                    XMLHelper.CreateAttribute(runNode, "Machine", msb.ToString());
                    foreach (OctopusLib.TaskAction taskAction in task.TaskActionCollection.OrderBy(o => o.Sequence))
                    {
                        XmlNode taskActionNode = XMLHelper.CreateChildNode(runNode, "TaskAction");
                        taskActionNode.InnerText = taskAction.Name;
                        if (taskAction.Machine != null)
                            XMLHelper.CreateAttribute(taskActionNode, "Machine", taskAction.Machine.Name.ToString());
                        XMLHelper.CreateAttribute(taskActionNode, "IsEnabled", taskAction.IsEnabled.ToString());
                        XMLHelper.CreateAttribute(taskActionNode, "Fixed", taskAction.IsFixed.ToString());
                    }
                }
                #endregion

                doc.Save(filename);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void CreateRunInstance()
        {
            RunInstanceCollection = new ObservableCollectionEx<RunInstance>();
            foreach (OctopusLib.Task task in TaskCollection.Where(o => o.IsEnabled).OrderBy(o => o.Sequence))
            {
                foreach (OctopusLib.Machine machine in task.Machines)
                {
                    try
                    {
                        OctopusLib.RunInstance instance = new OctopusLib.RunInstance();
                        instance.Machine = machine.Clone() as OctopusLib.Machine;
                        instance.Task = task.Clone() as OctopusLib.Task;
                        instance.LogFile = string.Format(@"{0}\{1}-{2}-{3}.log", _logfilePath, instance.Task.Name, instance.Machine.Name, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss"));
                        instance.Status = Status.NotRun;

                        instance.ActionInstanceCollection = new ObservableCollectionEx<ActionInstance>();
                        foreach (OctopusLib.TaskAction taskAction in task.TaskActionCollection.Where(o => o.IsEnabled).OrderBy(o => o.Sequence))
                        {
                            OctopusLib.Action currentAction = ActionCollection.Where(o => o.Name.Equals(taskAction.Name)).Single().Clone() as OctopusLib.Action;
                            OctopusLib.ActionInstance ai = new OctopusLib.ActionInstance();
                            ai.Action = currentAction;
                            ai.Status = Status.NotRun;
                            if (taskAction.Machine == null)
                                ai.Machine = machine.Clone() as OctopusLib.Machine;
                            else
                                ai.Machine = taskAction.Machine.Clone() as OctopusLib.Machine;
                            foreach(Command cmd in ai.Action.ActionCommands)
                            {
                                cmd.Status = Status.NotRun;
                                cmd.ParameterCollection = new ObservableCollectionEx<Parameter>();
                                foreach(Parameter param in ParameterCollection)
                                {
                                    cmd.ParameterCollection.Add(param.Clone() as Parameter);
                                }
                            }    

                            instance.ActionInstanceCollection.Add(ai);                            
                        }

                        RunInstanceCollection.Add(instance);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }
    }
}
