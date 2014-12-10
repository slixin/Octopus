using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using OctopusLib;
using System.Text.RegularExpressions;
using Microsoft.Practices.Prism.Events;

namespace OctopusGUI.ViewModelClass
{
    public class ActionViewModel : ViewModelBase
    {
        #region Properties
        public bool IsModified { get; set; }
        public bool IsExecuting { get; set; }
        public ObservableCollectionEx<OctopusLib.Action> ActionCollection
        {
            get
            {
                return _actionCollection;
            }
            set
            {
                if (value != _actionCollection)
                {
                    _actionCollection = value;
                    OnPropertyChanged("ActionCollection");
                }
            }
        }              

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }                

        public OctopusLib.Action SelectedRow
        {
            get
            {
                return _selectedRow;
            }
            set
            {
                if (value != _selectedRow)
                {
                    _selectedRow = value;
                    OnPropertyChanged("SelectedRow");
                }
            }
        }

        public OctopusLib.Command SelectedCommandRow
        {
            get
            {
                return _selectedCommandRow;
            }
            set
            {
                if (value != _selectedCommandRow)
                {
                    _selectedCommandRow = value;
                    OnPropertyChanged("SelectedCommandRow");
                }
            }
        }
        #endregion

        #region Private members
        private string _message;
        private string _oldActionName;

        private ObservableCollectionEx<OctopusLib.Action> _actionCollection;
        private OctopusLib.Action _selectedRow;
        private OctopusLib.Command _selectedCommandRow;

        private RelayCommand<object> insertActionCommand;
        private RelayCommand<object> deleteActionCommand;
        private RelayCommand<object> insertCommandCommand;
        private RelayCommand<object> deleteCommandCommand;
        private RelayCommand<object> moveupCommand;
        private RelayCommand<object> movedownCommand;
        #endregion
        

        public ActionViewModel()
        {
            ParameterNameChangedGlobalEvent.Instance.Subscribe(ParameterNameChanged);
        }

        public void PropertyChangingHandler(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                var action = sender as OctopusLib.Action;
                _oldActionName = action.Name;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsExecuting)
                IsModified = true;

            if (e.PropertyName.Equals("Name"))
            {
                var action = sender as OctopusLib.Action;
                ActionNameChangedGlobalEvent.Instance.Publish(new ActionNameChange() { ActionCollection = this.ActionCollection, OldValue = _oldActionName, NewValue = action.Name });
            }
        }

        private void ParameterNameChanged(ParameterNameChange paramNameChange)
        {
            UpdateParameter(paramNameChange);
        }

        private void UpdateParameter(ParameterNameChange pnc)
        {
            foreach (OctopusLib.Action action in ActionCollection)
            {
                foreach(Command command in action.ActionCommands)
                {
                    switch(command.CommandType)
                    {
                        case RunCommandType.Local:
                            LocalCommand lc = command as LocalCommand;
                            MatchCollection lccmdVars = Regex.Matches(lc.CommandText, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                            if (lccmdVars.Count > 0)
                            {
                                foreach (Match var in lccmdVars)
                                {
                                    string varName = var.Groups[1].Value;
                                    if (varName.Equals(pnc.OldValue))
                                    {
                                        lc.CommandText = Regex.Replace(lc.CommandText, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                    }
                                }
                            }
                            break;
                        case RunCommandType.Remote:
                            RemoteCommand rc = command as RemoteCommand;
                            MatchCollection rccmdVars = Regex.Matches(rc.CommandText, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                            if (rccmdVars.Count > 0)
                            {
                                foreach (Match var in rccmdVars)
                                {
                                    string varName = var.Groups[1].Value;
                                    if (varName.Equals(pnc.OldValue))
                                    {
                                        rc.CommandText = Regex.Replace(rc.CommandText, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(rc.RemoteRunAsUsername))
                            {
                                MatchCollection rcrunasusernameVars = Regex.Matches(rc.RemoteRunAsUsername, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (rcrunasusernameVars.Count > 0)
                                {
                                    foreach (Match var in rcrunasusernameVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            rc.RemoteRunAsUsername = Regex.Replace(rc.RemoteRunAsUsername, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(rc.RemoteRunAsPassword))
                            {
                                MatchCollection rcrunaspwdVars = Regex.Matches(rc.RemoteRunAsPassword, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (rcrunaspwdVars.Count > 0)
                                {
                                    foreach (Match var in rcrunaspwdVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            rc.RemoteRunAsPassword = Regex.Replace(rc.RemoteRunAsPassword, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(rc.WorkingDirectory))
                            {
                                MatchCollection rcwdVars = Regex.Matches(rc.WorkingDirectory, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (rcwdVars.Count > 0)
                                {
                                    foreach (Match var in rcwdVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            rc.WorkingDirectory = Regex.Replace(rc.WorkingDirectory, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            break;
                        case RunCommandType.LinuxSSH:
                            LinuxSSHCommand lsshc = command as LinuxSSHCommand;
                            MatchCollection lsshccmdVars = Regex.Matches(lsshc.CommandText, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                            if (lsshccmdVars.Count > 0)
                            {
                                foreach (Match var in lsshccmdVars)
                                {
                                    string varName = var.Groups[1].Value;
                                    if (varName.Equals(pnc.OldValue))
                                    {
                                        lsshc.CommandText = Regex.Replace(lsshc.CommandText, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                    }
                                }
                            }
                            break;
                        case RunCommandType.Copy:
                            CopyCommand cc = command as CopyCommand;
                            if (!string.IsNullOrEmpty(cc.CopySourceDir))
                            {
                                MatchCollection ccsdVars = Regex.Matches(cc.CopySourceDir, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (ccsdVars.Count > 0)
                                {
                                    foreach (Match var in ccsdVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            cc.CopySourceDir = Regex.Replace(cc.CopySourceDir, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cc.CopySourceFiles))
                            {
                                MatchCollection ccsfVars = Regex.Matches(cc.CopySourceFiles, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (ccsfVars.Count > 0)
                                {
                                    foreach (Match var in ccsfVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            cc.CopySourceFiles = Regex.Replace(cc.CopySourceFiles, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(cc.CopyTargetDir))
                            {
                                MatchCollection cctdVars = Regex.Matches(cc.CopyTargetDir, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                                if (cctdVars.Count > 0)
                                {
                                    foreach (Match var in cctdVars)
                                    {
                                        string varName = var.Groups[1].Value;
                                        if (varName.Equals(pnc.OldValue))
                                        {
                                            cc.CopyTargetDir = Regex.Replace(cc.CopyTargetDir, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        public System.Windows.Input.ICommand InsertActionCommand
        {
            get
            {
                if (insertActionCommand == null)
                {
                    insertActionCommand = new RelayCommand<object>(x => this.InsertAction(), x=>this.CanInsertAction());
                }
                return insertActionCommand;
            }
        }

        public System.Windows.Input.ICommand DeleteActionCommand
        {
            get
            {
                if (deleteActionCommand == null)
                {
                    deleteActionCommand = new RelayCommand<object>(x => this.DeleteAction(x), x => this.CanDeleteAction(x));
                }
                return deleteActionCommand;
            }
        }

        public System.Windows.Input.ICommand InsertCommandCommand
        {
            get
            {
                if (insertCommandCommand == null)
                {
                    insertCommandCommand = new RelayCommand<object>(x => this.InsertCommand(this.SelectedRow), x => this.CanInsertCommand(this.SelectedRow));
                }
                return insertCommandCommand;
            }
        }

        public System.Windows.Input.ICommand DeleteCommandCommand
        {
            get
            {
                if (deleteCommandCommand == null)
                {
                    deleteCommandCommand = new RelayCommand<object>(x => this.DeleteCommand(this.SelectedRow, x), x => this.CanDeleteCommand(x));
                }
                return deleteCommandCommand;
            }
        }
        
        public System.Windows.Input.ICommand MoveUpCommand
        {
            get
            {
                if (moveupCommand == null)
                {
                    moveupCommand = new RelayCommand<object>(x => this.MoveUp(this.SelectedRow, x), x => this.CanMoveUp(x));
                }
                return moveupCommand;
            }
        }

        public System.Windows.Input.ICommand MoveDownCommand
        {
            get
            {
                if (movedownCommand == null)
                {
                    movedownCommand = new RelayCommand<object>(x => this.MoveDown(this.SelectedRow, x), x => this.CanMoveDown(x));
                }
                return movedownCommand;
            }
        }

        public void InsertCommand(OctopusLib.Action selectedAction)
        {
            if (selectedAction != null)
            {
                CommandTypeSelection ctsWindow = new CommandTypeSelection()
                {
                    Title = "Command Type Selection Dialog",
                    ShowInTaskbar = false,               // don't show the dialog on the taskbar
                    Topmost = true,                      // ensure we're Always On Top
                    ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                };

                ctsWindow.ShowDialog();

                if (!string.IsNullOrEmpty(ctsWindow.CommandType))
                {
                    int sequence = selectedAction.ActionCommands.Count + 1;
                    switch (ctsWindow.CommandType)
                    {
                        case "LinuxSSH":
                            OctopusLib.LinuxSSHCommand linuxCommand = new OctopusLib.LinuxSSHCommand() { Sequence = sequence, 
                                CommandType = OctopusLib.RunCommandType.LinuxSSH, 
                                IsEnabled=true,
                                TimeOutSeconds = 30, 
                                RetryTimes=1, 
                                RetryIntervalSeconds=5 };
                            selectedAction.ActionCommands.Add(linuxCommand);
                            this.SelectedCommandRow = linuxCommand;
                            break;
                        case "Copy":
                            OctopusLib.CopyCommand copyCommand = new OctopusLib.CopyCommand() { Sequence = sequence, 
                                CommandType = OctopusLib.RunCommandType.Copy, 
                                CopyDirection = OctopusLib.CopyCommand.Direction.LocalToRemote, 
                                IsEnabled = true, 
                                IsForce = true, 
                                RetryTimes = 1, 
                                RetryIntervalSeconds = 5 };
                            selectedAction.ActionCommands.Add(copyCommand);
                            this.SelectedCommandRow = copyCommand;
                            break;
                        case "Remote":
                            OctopusLib.RemoteCommand remoteCommand = new OctopusLib.RemoteCommand() { Sequence = sequence, 
                                CommandType = OctopusLib.RunCommandType.Remote, 
                                IsEnabled = true, 
                                TimeOutSeconds = 120, 
                                ExpectedResult = "0", 
                                IsRunAsSystemAccount = true,
                                RetryTimes = 1, 
                                RetryIntervalSeconds = 5,  };
                            selectedAction.ActionCommands.Add(remoteCommand);
                            this.SelectedCommandRow = remoteCommand;
                            break;
                        case "Local":
                            OctopusLib.LocalCommand localCommand = new OctopusLib.LocalCommand() { Sequence = sequence, 
                                CommandType = OctopusLib.RunCommandType.Local, 
                                IsEnabled = true, 
                                TimeOutSeconds = 120, 
                                ExpectedResult = "0", 
                                RetryTimes = 1, 
                                RetryIntervalSeconds = 5 };
                            selectedAction.ActionCommands.Add(localCommand);
                            this.SelectedCommandRow = localCommand;
                            break;
                    }
                    
                    IsModified = true;
                    Message = "New action command be added.";
                }
            }
        }

        public bool CanInsertCommand(object obj)
        {
            if (obj == null)
                return false;
            else
                return true;
        }

        public void DeleteCommand(OctopusLib.Action selectedAction, object obj)
        {
            if (selectedAction != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Command>();

                List<OctopusLib.Command> selectedCommands = new List<OctopusLib.Command>(collection);

                if (selectedCommands.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected items?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (OctopusLib.Command command in selectedCommands)
                        {
                            this.SelectedRow.ActionCommands.Remove(command);
                        }
                        ReSequence(this.SelectedRow);
                        IsModified = true;
                        Message = "Action commands be deleted.";
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        public bool CanDeleteCommand(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                var collection = ((IList)obj).Cast<OctopusLib.Command>();
                if (collection.Count() > 0)
                    return true;
                else
                    return false;
            }
        }

        public void MoveUp(OctopusLib.Action selectedAction, object obj)
        {
            if (selectedAction != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Command>();
                OctopusLib.Command selectedCommand = new List<OctopusLib.Command>(collection)[0];

                int targetIndex = selectedCommand.Sequence - 1;

                OctopusLib.Command targetCommand = selectedAction.ActionCommands.Where(o => o.Sequence == targetIndex).Single() as OctopusLib.Command;

                targetCommand.Sequence = targetIndex + 1;
                selectedCommand.Sequence = targetIndex;
                selectedAction.ActionCommands.Sort(o => o.Sequence);
                IsModified = true;
            }
        }

        public bool CanMoveUp(object obj)
        {
            if (this.SelectedRow != null)
            {
                if (obj == null)
                    return false;
                else
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Command>();
                    if (collection.Count() == 1)
                    {
                        List<OctopusLib.Command> selectedCommands = new List<OctopusLib.Command>(collection);
                        if (selectedCommands[0].Sequence == 1)
                            return false;
                        else
                            return true;
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public void MoveDown(OctopusLib.Action selectedAction, object obj)
        {
            if (selectedAction != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Command>();
                OctopusLib.Command selectedCommand = new List<OctopusLib.Command>(collection)[0];

                int targetIndex = selectedCommand.Sequence + 1;

                OctopusLib.Command targetCommand = selectedAction.ActionCommands.Where(o => o.Sequence == targetIndex).Single() as OctopusLib.Command;

                targetCommand.Sequence = targetIndex - 1;
                selectedCommand.Sequence = targetIndex;

                selectedAction.ActionCommands.Sort(o => o.Sequence);
                IsModified = true;
            }
        }

        public bool CanMoveDown(object obj)
        {
            if (this.SelectedRow != null)
            {
                if (obj == null)
                    return false;
                else
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Command>();
                    if (collection.Count() == 1)
                    {
                        List<OctopusLib.Command> selectedCommands = new List<OctopusLib.Command>(collection);
                        if (selectedCommands[0].Sequence == this.SelectedRow.ActionCommands.Count)
                            return false;
                        else
                            return true;
                    }
                    else
                        return false;
                }
            }

            return false;
        }

        private bool CanInsertAction()
        {
            if (ActionCollection == null)
                return false;
            else
                return true;
        }

        public void InsertAction()
        {
            OctopusLib.Action newAction = new OctopusLib.Action
                {
                    Name = string.Format("New Action {0}", ActionCollection.Count() + 1),
                    ActionCommands = new ObservableCollectionEx<OctopusLib.Command>(),
                };
            this.SelectedRow = newAction;
            newAction.ActionCommands.ItemPropertyChanged += PropertyChangedHandler;
            this.ActionCollection.Add(newAction);

            IsModified = true;
            Message = "New action be added.";
        }

        #region Private methods
        private void DeleteAction(object obj)
        {
            if (obj != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Action>();
                List<OctopusLib.Action> selectedActions = new List<OctopusLib.Action>(collection);
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected item?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (OctopusLib.Action action in selectedActions)
                    {
                        this.ActionCollection.Remove(action);
                    }
                    IsModified = true;
                    Message = "Actions be deleted.";
                }
                else
                {
                    return;
                }
            }            
        }        

        private bool CanDeleteAction(object obj)
        {
            if (obj != null)
                return true;

            return false;
        }

        private void ReSequence(OctopusLib.Action selectedAction)
        {
            if (selectedAction != null)
            {
                int index = 0;
                foreach (OctopusLib.Command command in selectedAction.ActionCommands.OrderBy(o=>o.Sequence))
                {
                    index++;
                    if (command.Sequence == index)
                        continue;
                    else
                        command.Sequence = index;
                }
            }
        }

        
        #endregion
    }

    public class ActionNameChange
    {
        public ObservableCollectionEx<OctopusLib.Action> ActionCollection { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class ActionNameChangedGlobalEvent : CompositePresentationEvent<ActionNameChange>
    {
        private static readonly EventAggregator _eventAggregator;
        private static readonly ActionNameChangedGlobalEvent _event;

        static ActionNameChangedGlobalEvent()
        {
            _eventAggregator = new EventAggregator();
            _event = _eventAggregator.GetEvent<ActionNameChangedGlobalEvent>();
        }

        public static ActionNameChangedGlobalEvent Instance
        {
            get { return _event; }
        }
    }
}
