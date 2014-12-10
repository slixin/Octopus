using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using OctopusLib;
using System.Data;
using Microsoft.Practices.Prism.Events;
using System.Text.RegularExpressions;

namespace OctopusGUI.ViewModelClass
{
    public class ParameterViewModel : ViewModelBase
    {
        #region Public properties
        public bool IsModified { get; set; }
        public bool IsExecuting { get; set; }

        /// <summary>
        /// Parameter collection
        /// </summary>
        public ObservableCollectionEx<OctopusLib.Parameter> ParameterCollection
        {
            get
            {
                return _parameterCollection;
            }
            set
            {
                if (value != _parameterCollection)
                {
                    _parameterCollection = value;
                    OnPropertyChanged("ParameterCollection");
                }
            }
        }

        /// <summary>
        /// Update message
        /// </summary>
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
        public OctopusLib.Parameter SelectedRow
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
        #endregion

        #region Private members
        private string _message;
        private OctopusLib.Parameter _selectedRow;
        private ObservableCollectionEx<OctopusLib.Parameter> _parameterCollection;
        private string _oldParameterName;

        private RelayCommand<object> insertCommand;
        private RelayCommand<object> deleteCommand;
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public ParameterViewModel(){
        }

        #region Commands
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

        private bool CanInsert()
        {
            if (ParameterCollection == null)
                return false;
            else
                return true;
        }

        public void Insert()
        {
            OctopusLib.Parameter newParam = new OctopusLib.Parameter
            {
                Name = string.Format("New Parameter {0}", ParameterCollection.Count() + 1),
                Value = "New Parameter Value",
                IsEncrypted = false,
            };
            this.SelectedRow = newParam;
            this.ParameterCollection.Add(newParam);

            IsModified = true;
            Message = string.Format(@"New Parameter be added.");
        }

        private bool CanDelete(object obj)
        {
            if (ParameterCollection == null)
                return false;

            if (obj != null)
            {
                try
                {
                    var collection = ((IList)obj).Cast<OctopusLib.Parameter>();
                    List<OctopusLib.Parameter> selectedParameters = new List<OctopusLib.Parameter>(collection);

                    if (selectedParameters.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch { }
            }

            return false;
        }

        private void Delete(object obj)
        {
            if (obj != null)
            {
                var collection = ((IList)obj).Cast<OctopusLib.Parameter>();

                List<OctopusLib.Parameter> selectedParameters = new List<OctopusLib.Parameter>(collection);

                if (selectedParameters.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected items?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (OctopusLib.Parameter parameter in selectedParameters)
                        {
                            this.ParameterCollection.Remove(parameter);
                        }
                        IsModified = true;
                        Message = "Parameters be deleted.";
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }  

        private void UpdateParameter(string oldParamName, string newParamName)
        {
            foreach(Parameter param in ParameterCollection)
            {
                MatchCollection paramVars = Regex.Matches(param.Value, @"{%(.+?)%}", RegexOptions.IgnoreCase);
                if (paramVars.Count > 0)
                {
                    foreach (Match paramVar in paramVars)
                    {
                        string paramVarName = paramVar.Groups[1].Value;
                        switch (paramVarName)
                        {
                            case "MACHINE_NAME":
                            case "MACHINE_IP":
                            case "MACHINE_USERNAME":
                            case "MACHINE_PASSWORD":
                            case "MACHINE_DOMAIN":
                            case "MACHINE_ISLINUX":
                            case "NOW":
                                continue;
                            default:
                                if (paramVarName.Equals(oldParamName))
                                {
                                    param.Value = Regex.Replace(param.Value, string.Format(@"{{%{0}%}}", paramVarName), string.Format(@"{{%{0}%}}", newParamName));
                                }
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Property Changed
        public void PropertyChangingHandler(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                var param = sender as Parameter;
                _oldParameterName = param.Name;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsExecuting)
                IsModified = true;

            if (e.PropertyName.Equals("Name"))
            {
                var param = sender as Parameter;
                UpdateParameter(_oldParameterName, param.Name);

                ParameterNameChangedGlobalEvent.Instance.Publish(new ParameterNameChange() { ParameterCollection = this.ParameterCollection, OldValue = _oldParameterName, NewValue = param.Name });
            }
        }
        #endregion
    }

    public class ParameterNameChange
    {
        public ObservableCollectionEx<Parameter> ParameterCollection { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class ParameterNameChangedGlobalEvent : CompositePresentationEvent<ParameterNameChange>
    {
        private static readonly EventAggregator _eventAggregator;
        private static readonly ParameterNameChangedGlobalEvent _event;

        static ParameterNameChangedGlobalEvent()
        {
            _eventAggregator = new EventAggregator();
            _event = _eventAggregator.GetEvent<ParameterNameChangedGlobalEvent>();
        }

        public static ParameterNameChangedGlobalEvent Instance
        {
            get { return _event; }
        }
    }
}
