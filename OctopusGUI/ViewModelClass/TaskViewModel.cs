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

namespace OctopusGUI.ViewModelClass
{
    public class TaskViewModel : ViewModelBase
    {
        #region Properties
        public bool IsModified { get; set; }
        public bool IsExecuting { get; set; }
        public ObservableCollectionEx<OctopusLib.Task> TaskCollection
        {
            get
            {
                return _taskCollection;
            }
            set
            {
                if (value != _taskCollection)
                {
                    _taskCollection = value;
                    OnPropertyChanged("TaskCollection");
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

        /// <summary>
        /// Gets or sets the selected row.
        /// </summary>
        /// <value>The selected row.</value>
        public OctopusLib.Task SelectedRow
        {
            get
            {
                return _selectedRow;
            }
            set
            {
                _selectedRow = value;
                OnPropertyChanged("SelectedRow");
            }
        }

        /// <summary>
        /// Gets or sets the selected row.
        /// </summary>
        /// <value>The selected row.</value>
        public OctopusLib.TaskAction SelectedTaskActionRow
        {
            get
            {
                return _selectedActionRow;
            }
            set
            {
                _selectedActionRow = value;
                OnPropertyChanged("SelectedTaskActionRow");
            }
        }
        #endregion

        #region Private members
        private string _message;
        private OctopusLib.Task _selectedRow;
        private OctopusLib.TaskAction _selectedActionRow;
        private ObservableCollectionEx<OctopusLib.Task> _taskCollection;

        private RelayCommand<object> insertTaskCommand;
        private RelayCommand<object> deleteTaskCommand;
        private RelayCommand<object> insertTaskActionCommand;
        private RelayCommand<object> deleteTaskActionCommand;
        private RelayCommand<object> moveupTaskActionCommand;
        private RelayCommand<object> movedownTaskActionCommand;
        private RelayCommand<object> machineOnRunChangeCommand;
        private RelayCommand<object> machineOnRunActionChangeCommand;
        #endregion

        public TaskViewModel()
        {
            ActionNameChangedGlobalEvent.Instance.Subscribe(ActionNameChanged);
        }

        private void ActionNameChanged(ActionNameChange actionNameChange)
        {
            UpdateTaskAction(actionNameChange);
        }

        private void UpdateTaskAction(ActionNameChange anc)
        {
            foreach (Task task in TaskCollection)
            {
                foreach(TaskAction ta in task.TaskActionCollection)
                {
                    if (ta.Name.Equals(anc.OldValue))
                    {
                        ta.Name = anc.NewValue;
                    }
                }
            }
        }

        public System.Windows.Input.ICommand MachineOnTaskChangeCommand
        {
            get
            {
                if (machineOnRunChangeCommand == null)
                {
                    machineOnRunChangeCommand = new RelayCommand<object>(x => this.MachineOnTaskChange(x), x => this.CanMachineOnTaskChange(x));
                }
                return machineOnRunChangeCommand;
            }
        }

        public System.Windows.Input.ICommand MachineOnTaskActionChangeCommand
        {
            get
            {
                if (machineOnRunActionChangeCommand == null)
                {
                    machineOnRunActionChangeCommand = new RelayCommand<object>(x => this.MachineOnTaskActionChange(x), x => this.CanMachineOnTaskActionChange(x));
                }
                return machineOnRunActionChangeCommand;
            }
        }

        public System.Windows.Input.ICommand InsertTaskCommand
        {
            get
            {
                if (insertTaskCommand == null)
                {
                    insertTaskCommand = new RelayCommand<object>(x => this.InsertTask(), x=> this.CanInsertTask());
                }
                return insertTaskCommand;
            }
        }

        public System.Windows.Input.ICommand DeleteTaskCommand
        {
            get
            {
                if (deleteTaskCommand == null)
                {
                    deleteTaskCommand = new RelayCommand<object>(x => this.DeleteRun(x), x => this.CanDeleteRun(x));
                }
                return deleteTaskCommand;
            }
        }

        public System.Windows.Input.ICommand InsertTaskActionCommand
        {
            get
            {
                if (insertTaskActionCommand == null)
                {
                    insertTaskActionCommand = new RelayCommand<object>(x => this.InsertTaskAction(this.SelectedRow), x => this.CanInsertTaskAction(this.SelectedRow));
                }
                return insertTaskActionCommand;
            }
        }

        public System.Windows.Input.ICommand DeleteTaskActionCommand
        {
            get
            {
                if (deleteTaskActionCommand == null)
                {
                    deleteTaskActionCommand = new RelayCommand<object>(x => this.DeleteTaskAction(this.SelectedRow, x), x => this.CanDeleteTaskAction(x));
                }
                return deleteTaskActionCommand;
            }
        }

        public System.Windows.Input.ICommand MoveUpTaskActionCommand
        {
            get
            {
                if (moveupTaskActionCommand == null)
                {
                    moveupTaskActionCommand = new RelayCommand<object>(x => this.MoveUpTaskAction(this.SelectedRow, x), x => this.CanMoveUpTaskAction(x));
                }
                return moveupTaskActionCommand;
            }
        }

        public System.Windows.Input.ICommand MoveDownTaskActionCommand
        {
            get
            {
                if (movedownTaskActionCommand == null)
                {
                    movedownTaskActionCommand = new RelayCommand<object>(x => this.MoveDownTaskAction(this.SelectedRow, x), x => this.CanMoveDownTaskAction(x));
                }
                return movedownTaskActionCommand;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsExecuting)
                IsModified = true;
        }

        public void InsertTaskAction(OctopusLib.Task selectedTask)
        {
            if (selectedTask != null)
            {
                int sequence = selectedTask.TaskActionCollection.Count + 1;

                TaskActionSelect rasWindow = new TaskActionSelect()
                {
                    Title = "Task Action selection Dialog",
                    ShowInTaskbar = false,               // don't show the dialog on the taskbar
                    Topmost = true,                      // ensure we're Always On Top
                    ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                };
                rasWindow.BindingAvailableActionList();
                rasWindow.ShowDialog();

                if (rasWindow.SelectedTaskAction != null)
                {
                    OctopusLib.TaskAction selectedTaskAction = rasWindow.SelectedTaskAction;
                    selectedTaskAction.Sequence = sequence;

                    this.SelectedTaskActionRow = selectedTaskAction;

                    selectedTask.TaskActionCollection.Add(selectedTaskAction);
                    selectedTask.TaskActionCollection.ItemPropertyChanged += PropertyChangedHandler;
                    IsModified = true;
                    Message = "New task action be added.";
                }
            }
        }

        public bool CanInsertTaskAction(object obj)
        {
            if (obj == null)
                return false;
            else
                return true;
        }

        public void DeleteTaskAction(OctopusLib.Task selectedTask, object obj)
        {
            if (selectedTask != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();

                List<OctopusLib.TaskAction> selectedTaskActions = new List<OctopusLib.TaskAction>(collection);

                if (selectedTaskActions.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected items?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (OctopusLib.TaskAction taskAction in selectedTaskActions)
                        {
                            this.SelectedRow.TaskActionCollection.Remove(taskAction);
                        }
                        ReSequence(this.SelectedRow);
                        IsModified = true;
                        Message = "Task actions be deleted.";
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        public bool CanDeleteTaskAction(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();
                if (collection.Count() > 0)
                    return true;
                else
                    return false;
            }
        }

        public void MoveUpTaskAction(OctopusLib.Task selectedTask, object obj)
        {
            if (selectedTask != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();
                OctopusLib.TaskAction selectedTaskAction = new List<OctopusLib.TaskAction>(collection)[0];

                int targetIndex = selectedTaskAction.Sequence - 1;

                OctopusLib.TaskAction targetTaskAction = selectedTask.TaskActionCollection.Where(o => o.Sequence == targetIndex).Single() as OctopusLib.TaskAction;

                targetTaskAction.Sequence = targetIndex + 1;
                selectedTaskAction.Sequence = targetIndex;
                selectedTask.TaskActionCollection.Sort(o => o.Sequence);
                IsModified = true;
            }
        }

        public bool CanMoveUpTaskAction(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();
                if (collection.Count() == 1)
                {
                    List<OctopusLib.TaskAction> selectedTaskActions = new List<OctopusLib.TaskAction>(collection);
                    if (selectedTaskActions[0].Sequence == 1)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
        }

        public void MoveDownTaskAction(OctopusLib.Task selectedTask, object obj)
        {
            if (selectedTask != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();
                OctopusLib.TaskAction selectedTaskAction = new List<OctopusLib.TaskAction>(collection)[0];

                int targetIndex = selectedTaskAction.Sequence + 1;

                OctopusLib.TaskAction targetTaskAction = selectedTask.TaskActionCollection.Where(o => o.Sequence == targetIndex).Single() as OctopusLib.TaskAction;

                targetTaskAction.Sequence = targetIndex - 1;
                selectedTaskAction.Sequence = targetIndex;

                selectedTask.TaskActionCollection.Sort(o => o.Sequence);
                IsModified = true;
            }
        }

        public bool CanMoveDownTaskAction(object obj)
        {
            if (obj == null)
                return false;
            else
            {
                var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();
                if (collection.Count() == 1)
                {
                    List<OctopusLib.TaskAction> selectedTaskActions = new List<OctopusLib.TaskAction>(collection);
                    if (selectedTaskActions[0].Sequence == this.SelectedRow.TaskActionCollection.Count)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
        }

        private bool CanInsertTask()
        {
            if (TaskCollection == null)
                return false;
            else
                return true;
        }

        public void InsertTask()
        {
            OctopusLib.Task newTask = new OctopusLib.Task
            {
                Name = string.Format("New Task {0}", TaskCollection.Count() + 1),
                IsEnabled = true,
                TaskActionCollection = new ObservableCollectionEx<OctopusLib.TaskAction>(),
                Machines = new ObservableCollectionEx<Machine>(),
                Sequence = 0,
            };
            this.SelectedRow = newTask;
            this.TaskCollection.Add(newTask);

            IsModified = true;
            Message = "New task be added.";
        }

        #region Private methods
        private bool CanMachineOnTaskChange(object obj)
        {
            if (obj != null)
            {
                try
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Task>();

                    if (collection.Count() == 1)
                        return true;
                    else
                        return false;
                }
                catch { }
            }

            return false;
        }

        private void MachineOnTaskChange(object obj)
        {
            var collectionTask = ((IList)obj).Cast<OctopusLib.Task>();
            List<OctopusLib.Task> selectedTasks = new List<OctopusLib.Task>(collectionTask);
            if (selectedTasks != null)
            {
                ObservableCollectionEx<OctopusLib.Machine> selectedMachines = new ObservableCollectionEx<Machine>();
                selectedMachines = this.SelectedRow == null ? null : this.SelectedRow.Machines;

                MachineSelect rmsWindow = new MachineSelect()
                {
                    Title = "Machines selection dialog",
                    ShowInTaskbar = false,               // don't show the dialog on the taskbar
                    Topmost = true,                      // ensure we're Always On Top
                    ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                    SelectedMachines = selectedMachines,
                    IsAllowSelectAll = true,
                };
                rmsWindow.BindingAvailableMachineList();
                rmsWindow.ShowDialog();

                if (rmsWindow.SelectedMachines != null)
                {
                    this.SelectedRow.Machines = rmsWindow.SelectedMachines;
                }
            }
        }

        private bool CanMachineOnTaskActionChange(object obj)
        {
            if (obj != null)
            {
                try
                {
                    var collection = ((IList)obj).Cast<OctopusLib.TaskAction>();

                    if (collection.Count() == 1)
                        return true;
                    else
                        return false;
                }
                catch { }
            }

            return false;
        }

        private void MachineOnTaskActionChange(object obj)
        {
            var collectionTaskAction = ((IList)obj).Cast<OctopusLib.TaskAction>();
            List<OctopusLib.TaskAction> selectedTaskActions = new List<OctopusLib.TaskAction>(collectionTaskAction);
            if (selectedTaskActions != null)
            {
                ObservableCollectionEx<OctopusLib.Machine> selectedMachines = new ObservableCollectionEx<Machine>();
                if (this.SelectedTaskActionRow.Machine != null)
                    selectedMachines.Add(this.SelectedTaskActionRow.Machine);

                MachineSelect rmsWindow = new MachineSelect()
                {
                    Title = "Task Machines selection Dialog",
                    ShowInTaskbar = false,               // don't show the dialog on the taskbar
                    Topmost = true,                      // ensure we're Always On Top
                    ResizeMode = ResizeMode.NoResize,    // remove excess caption bar buttons
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                    SelectedMachines = selectedMachines,
                    IsAllowSelectAll = false,
                };
                rmsWindow.BindingAvailableMachineList();
                rmsWindow.ShowDialog();

                if (rmsWindow.SelectedMachines != null)
                {
                    this.SelectedTaskActionRow.Machine = rmsWindow.SelectedMachines.Count == 0 ? null : rmsWindow.SelectedMachines[0];
                }
            }
        }

        private void DeleteRun(object obj)
        {
            if (obj != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Task>();

                List<OctopusLib.Task> selectedRuns = new List<OctopusLib.Task>(collection);

                if (selectedRuns.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected items?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (OctopusLib.Task task in selectedRuns)
                        {
                            this.TaskCollection.Remove(task);
                        }
                        IsModified = true;
                        Message = "Runs be deleted.";
                    }
                    else
                    {
                        return;
                    }
                }
            }            
        }        

        private bool CanDeleteRun(object obj)
        {
            if (obj != null)
            {
                try
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Task>();

                    if (collection.Count() > 0)
                        return true;
                    else
                        return false;
                }
                catch { }                
            }

            return false;
        }

        private void ReSequence(OctopusLib.Task selectedTask)
        {
            if (selectedTask != null)
            {
                int index = 0;
                foreach (OctopusLib.TaskAction taskAction in selectedTask.TaskActionCollection.OrderBy(o => o.Sequence))
                {
                    index++;
                    if (taskAction.Sequence == index)
                        continue;
                    else
                        taskAction.Sequence = index;
                }
            }
        }       
        #endregion
    }
}
