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
using Microsoft.Practices.Prism.Events;
using System.Text.RegularExpressions;

namespace OctopusGUI.ViewModelClass
{
    public class MachineViewModel : ViewModelBase
    {
        #region Properties
        public bool IsModified { get; set; }
        public bool IsExecuting { get; set; }
        public ObservableCollectionEx<OctopusLib.Machine> MachineCollection
        {
            get
            {
                return _machineCollection;
            }
            set
            {
                if (value != _machineCollection)
                {
                    _machineCollection = value;
                    OnPropertyChanged("MachineCollection");
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
        public OctopusLib.Machine SelectedRow
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

        public Dictionary<int, string> ArchitectureList { get; set; }
        #endregion

        #region Private Methods
        private string _message;
        private OctopusLib.Machine _selectedRow;
        private ObservableCollectionEx<OctopusLib.Machine> _machineCollection;

        private RelayCommand<object> insertCommand;
        private RelayCommand<object> deleteCommand;
        #endregion

        public MachineViewModel()
        {
            ParameterNameChangedGlobalEvent.Instance.Subscribe(ParameterNameChanged);
        }

        private void ParameterNameChanged(ParameterNameChange paramNameChange)
        {
            UpdateParameter(paramNameChange);
        }

        private void UpdateParameter(ParameterNameChange pnc)
        {
            foreach (Machine m in MachineCollection)
            {
                MatchCollection domainVars = Regex.Matches(m.Domain, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                if (domainVars.Count > 0)
                {
                    foreach (Match var in domainVars)
                    {
                        string varName = var.Groups[1].Value;
                        if (varName.Equals(pnc.OldValue))
                        {
                            m.Domain = Regex.Replace(m.Domain, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                        }
                    }
                }

                MatchCollection usernameVars = Regex.Matches(m.Username, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                if (usernameVars.Count > 0)
                {
                    foreach (Match var in usernameVars)
                    {
                        string varName = var.Groups[1].Value;
                        if (varName.Equals(pnc.OldValue))
                        {
                            m.Username = Regex.Replace(m.Username, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                        }
                    }
                }

                MatchCollection passwordVars = Regex.Matches(m.Password, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                if (passwordVars.Count > 0)
                {
                    foreach (Match var in passwordVars)
                    {
                        string varName = var.Groups[1].Value;
                        if (varName.Equals(pnc.OldValue))
                        {
                            m.Password = Regex.Replace(m.Password, string.Format(@"{{%{0}%}}", pnc.OldValue), string.Format(@"{{%{0}%}}", pnc.NewValue));
                        }
                    }
                }
            }
        }


        public System.Windows.Input.ICommand InsertCommand
        {
            get
            {
                if (insertCommand == null)
                {
                    insertCommand = new RelayCommand<object>(x => this.Insert(), x=>this.CanInsert());
                }
                return insertCommand;
            }
        }

        public System.Windows.Input.ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand<object>(x=> this.Delete(x), x=> this.CanDelete(x));
                }
                return deleteCommand;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsExecuting)
                IsModified = true;
        }

        private bool CanInsert()
        {
            if (MachineCollection == null)
                return false;
            else
                return true;
        }

        public void Insert()
        {
            OctopusLib.Machine newMachine = new OctopusLib.Machine
                {
                    Name = string.Format("Machine {0}", MachineCollection.Count() + 1),
                    IP = "192.168.1.1",
                    Domain = string.Empty,
                    Password = "password",
                    Username = "username",
                };
            this.SelectedRow = newMachine;

            this.MachineCollection.Add(newMachine);
            IsModified = true;
            Message = "New machine be added.";
        }

        #region Private methods 
        private void Delete(object obj)
        {
            if (obj != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Machine>();

                List<OctopusLib.Machine> selectedMachines = new List<OctopusLib.Machine>(collection);

                if (selectedMachines.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected items?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (OctopusLib.Machine machine in selectedMachines)
                        {
                            this.MachineCollection.Remove(machine);
                        }
                        IsModified = true;
                        Message = "Machines be deleted.";
                    }
                    else
                    {
                        return;
                    }
                }
            }            
        }

        private bool CanDelete(object obj)
        {
            if (obj != null)
            {
                try
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Machine>();
                    List<OctopusLib.Machine> selectedMachines = new List<OctopusLib.Machine>(collection);

                    if (selectedMachines.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch { }                
            }

            return false;
        }
        #endregion
    }
}
