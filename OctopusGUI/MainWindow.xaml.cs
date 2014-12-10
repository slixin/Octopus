using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Collections;
using OctopusLib;
using System.Xml;
using System.Runtime.InteropServices;
using AutomationHelper;
using System.ComponentModel;
using System.Windows.Markup;
using OctopusGUI.ViewModelClass;
using OctopusGUI.UserControls;
using System.Diagnostics;

namespace OctopusGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ParameterViewModel m_paramsVM;
        public MachineViewModel m_machineVM;
        public ActionViewModel m_actionVM;
        public TaskViewModel m_taskVM;
        public InstanceViewModel m_executionVM;

        public string MachineID;

        private OctopusInstance m_octopusInstance;
        private const string windowTitle = "HP Octopus";
        private string currentFile = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVM();
            MachineID = OctopusLib.Common.WMI.GetProcessorSerial();
        }

        public void SaveFile()
        {
            string filename = null;

            if (string.IsNullOrEmpty(currentFile))
            {
                if (Common.SaveFileDialog(out filename))
                {
                    if (SaveDefinitionFile(filename))
                    {
                        this.Title = string.Format("{0} - {1}", windowTitle, filename);
                        currentFile = filename;
                    }
                }
            }
            else
            {
                SaveDefinitionFile(currentFile);                
            }
        }

        #region private methods  
        private void InitializeVM()
        {
            InitializeParametersTab();
            InitializeMachinesTab();
            InitializeActionsTab();
            InitializeTaskTab();
            InitializeExecutionTab();
        }

        private void InitializeParametersTab()
        {
            m_paramsVM = new ParameterViewModel();
            tabParams.DataContext = m_paramsVM;
        }

        private void InitializeMachinesTab()
        {
            m_machineVM = new MachineViewModel();
            m_machineVM.ArchitectureList = new Dictionary<int, string>();
            m_machineVM.ArchitectureList.Add(32, "x86");
            m_machineVM.ArchitectureList.Add(64, "x64");
            tabMachines.DataContext = m_machineVM;
        } 

        private void InitializeActionsTab()
        {
            m_actionVM = new ActionViewModel();
            tabActions.DataContext = m_actionVM;
        }

        private void InitializeTaskTab()
        {
            m_taskVM = new TaskViewModel();
            tabTasks.DataContext = m_taskVM;
            dgCmbSequence.ItemsSource = Enumerable.Range(0, 20).ToList();
        }

        private void InitializeExecutionTab()
        {
            m_executionVM = new InstanceViewModel();
            tabExecution.DataContext = m_executionVM;
            m_executionVM.ParentWindow = this;
        }

        private void LoadDefinitionFile(string filename)
        {
            try
            {
                m_octopusInstance = new OctopusInstance(filename);
                m_octopusInstance.Load();

                m_paramsVM.ParameterCollection = m_octopusInstance.ParameterCollection;
                m_paramsVM.ParameterCollection.ItemPropertyChanged += m_paramsVM.PropertyChangedHandler;
                m_paramsVM.ParameterCollection.ItemPropertyChanging += m_paramsVM.PropertyChangingHandler;
                m_machineVM.MachineCollection = m_octopusInstance.MachineCollection;
                m_machineVM.MachineCollection.ItemPropertyChanged += m_machineVM.PropertyChangedHandler;
                m_actionVM.ActionCollection = m_octopusInstance.ActionCollection;
                m_actionVM.ActionCollection.ItemPropertyChanged += m_actionVM.PropertyChangedHandler;
                m_actionVM.ActionCollection.ItemPropertyChanging += m_actionVM.PropertyChangingHandler;
                m_taskVM.TaskCollection = m_octopusInstance.TaskCollection;
                m_taskVM.TaskCollection.ItemPropertyChanged += m_taskVM.PropertyChangedHandler;
                foreach(OctopusLib.Task task in m_taskVM.TaskCollection)
                {
                    task.TaskActionCollection.ItemPropertyChanged += m_taskVM.PropertyChangedHandler;
                }
                m_executionVM.OctopusInst = m_octopusInstance;
                m_executionVM.ConfigFile = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.currentFile = string.Empty;
            }
        }

        private bool SaveDefinitionFile(string filename)
        {
            bool result = false;
            try
            {
                m_octopusInstance.Save(filename);
                m_executionVM.ConfigFile = filename;
                result = true;
                m_paramsVM.IsModified = false;
                m_actionVM.IsModified = false;
                m_machineVM.IsModified = false;
                m_taskVM.IsModified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;
        }

        private void InitializeVMCollection()
        {
            m_paramsVM.ParameterCollection = new ObservableCollectionEx<OctopusLib.Parameter>();
            m_machineVM.MachineCollection = new ObservableCollectionEx<OctopusLib.Machine>();
            m_actionVM.ActionCollection = new ObservableCollectionEx<OctopusLib.Action>();
            m_taskVM.TaskCollection = new ObservableCollectionEx<OctopusLib.Task>();
        }

        private void CreateNewDefinition()
        {
            m_paramsVM.Insert();
            m_machineVM.Insert();
            m_actionVM.InsertAction();
            m_taskVM.InsertTask();
        }

        private void ClearDefinition()
        {
            m_paramsVM.ParameterCollection.Clear();
            m_machineVM.MachineCollection.Clear();
            m_actionVM.ActionCollection.Clear();
            m_taskVM.TaskCollection.Clear();

            m_paramsVM.ParameterCollection.ItemPropertyChanging -= m_paramsVM.PropertyChangingHandler;
            m_actionVM.ActionCollection.ItemPropertyChanging -= m_actionVM.PropertyChangingHandler;

            m_paramsVM.ParameterCollection.ItemPropertyChanged -= m_paramsVM.PropertyChangedHandler;
            m_machineVM.MachineCollection.ItemPropertyChanged -= m_machineVM.PropertyChangedHandler;
            m_actionVM.ActionCollection.ItemPropertyChanged -= m_actionVM.PropertyChangedHandler;
            m_taskVM.TaskCollection.ItemPropertyChanged -= m_taskVM.PropertyChangedHandler;
            foreach(OctopusLib.Task task in m_taskVM.TaskCollection)
            {
                task.TaskActionCollection.ItemPropertyChanged -= m_taskVM.PropertyChangedHandler;
            }

            m_paramsVM.ParameterCollection = null;
            m_machineVM.MachineCollection = null;
            m_actionVM.ActionCollection = null;
            m_taskVM.TaskCollection = null;

            m_paramsVM.IsModified = false;
            m_machineVM.IsModified = false;
            m_actionVM.IsModified = false;
            m_taskVM.IsModified = false;

            m_paramsVM.Message = string.Empty;
            m_machineVM.Message = string.Empty;
            m_actionVM.Message = string.Empty;
            m_taskVM.Message = string.Empty;
        }

        private bool ExitApplication()
        {
            if (m_executionVM.IsExecuting)
                return true;

            MessageBoxResult resultExit = MessageBox.Show("Do you want to exit Octopus?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultExit == MessageBoxResult.Yes)
            {
                if (m_actionVM.IsModified || m_machineVM.IsModified || m_paramsVM.IsModified || m_taskVM.IsModified)
                {
                    MessageBoxResult result = MessageBox.Show("Do you want to save the modification before exit Octopus?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!SaveDefinitionFile(currentFile))
                            return false;
                    }
                }
                App.Current.Shutdown();
                return false;
            }
            else
                return true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = ExitApplication();
        }

        private void actionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 1)
                {
                    OctopusLib.Action selectedAction = e.AddedItems[0] as OctopusLib.Action;
                    if (selectedAction != null)
                    {
                        foreach (OctopusLib.Action action in m_actionVM.ActionCollection)
                        {
                            action.ActionCommands.ItemPropertyChanged += m_actionVM.PropertyChangedHandler;
                        }
                        m_actionVM.SelectedRow = selectedAction;
                        actionsCommandsDataGrid.ItemsSource = selectedAction.ActionCommands;
                    }
                }
                else
                {
                    actionsCommandsDataGrid.ItemsSource = null;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private void actionsCommandsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (actionsCommandsDataGrid.SelectedItem != null)
            {
                if (actionsCommandsDataGrid.SelectedItems.Count == 1)
                {
                    OctopusLib.Command selectedCommand = actionsCommandsDataGrid.SelectedItem as OctopusLib.Command;

                    if (selectedCommand != null)
                    {
                        gridCMDProperties.Children.Clear();
                        selectedCommand.ParameterCollection = m_paramsVM.ParameterCollection;
                        switch (selectedCommand.CommandType)
                        {
                            case OctopusLib.RunCommandType.LinuxSSH:
                                LinuxSSHCommandProperty linuxCommandCtrl = new LinuxSSHCommandProperty() { DataContext = selectedCommand as OctopusLib.LinuxSSHCommand };
                                linuxCommandCtrl.ParameterCollection = m_paramsVM.ParameterCollection;
                                linuxCommandCtrl.OwnerWindow = this;
                                gridCMDProperties.Children.Add(linuxCommandCtrl);
                                gridCMDProperties.UpdateLayout();
                                break;
                            case OctopusLib.RunCommandType.Copy:
                                CopyCommandProperty copyCommandCtrl = new CopyCommandProperty() { DataContext = selectedCommand as OctopusLib.CopyCommand };
                                copyCommandCtrl.ParameterCollection = m_paramsVM.ParameterCollection;
                                copyCommandCtrl.OwnerWindow = this;
                                gridCMDProperties.Children.Add(copyCommandCtrl);
                                gridCMDProperties.UpdateLayout();
                                break;
                            case OctopusLib.RunCommandType.Remote:
                                RemoteCommandProperty remoteCommandCtrl = new RemoteCommandProperty() { DataContext = selectedCommand as OctopusLib.RemoteCommand };
                                remoteCommandCtrl.ParameterCollection = m_paramsVM.ParameterCollection;
                                remoteCommandCtrl.OwnerWindow = this;
                                gridCMDProperties.Children.Add(remoteCommandCtrl);
                                gridCMDProperties.UpdateLayout();
                                break;
                            case OctopusLib.RunCommandType.Local:
                                LocalCommandProperty localCommandCtrl = new LocalCommandProperty() { DataContext = selectedCommand as OctopusLib.LocalCommand };
                                localCommandCtrl.ParameterCollection = m_paramsVM.ParameterCollection;
                                localCommandCtrl.OwnerWindow = this;
                                gridCMDProperties.Children.Add(localCommandCtrl);
                                gridCMDProperties.UpdateLayout();
                                break;
                        }
                    }
                }
                else
                {
                    gridCMDProperties.Children.Clear();
                }
            }
            else
            {
                gridCMDProperties.Children.Clear();
            }
        }

        private void taskDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskDataGrid.SelectedItems.Count == 1)
            {
                OctopusLib.Task selectedTask = taskDataGrid.SelectedItem as OctopusLib.Task;
                if (selectedTask != null)
                {
                    m_taskVM.SelectedRow = selectedTask;
                    taskActionsDataGrid.ItemsSource = selectedTask.TaskActionCollection;
                }
            }
            else
            {
                taskActionsDataGrid.ItemsSource = null;
            }
        }

        #endregion

        #region Execute Methods
        private void NewExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_actionVM != null && m_machineVM != null && m_paramsVM != null && m_taskVM != null)
            {
                if (m_actionVM.IsModified || m_machineVM.IsModified || m_paramsVM.IsModified || m_taskVM.IsModified)
                {
                    MessageBoxResult result = MessageBox.Show("Do you want to save the modification before create a new Octopus Definition file?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (!SaveDefinitionFile(currentFile))
                            return;
                    }
                }
            }
            m_executionVM.StopExecution();
            if (m_executionVM.RunInstanceCollection != null)
                m_executionVM.RunInstanceCollection.Clear();
            tabParams.IsSelected = true;

            InitializeVMCollection();
            m_octopusInstance = new OctopusInstance();
            m_paramsVM.ParameterCollection = m_octopusInstance.ParameterCollection;
            m_paramsVM.ParameterCollection.ItemPropertyChanged += m_paramsVM.PropertyChangedHandler;
            m_machineVM.MachineCollection = m_octopusInstance.MachineCollection;
            m_machineVM.MachineCollection.ItemPropertyChanged += m_machineVM.PropertyChangedHandler;
            m_actionVM.ActionCollection = m_octopusInstance.ActionCollection;
            m_actionVM.ActionCollection.ItemPropertyChanged += m_actionVM.PropertyChangedHandler;
            m_actionVM.ActionCollection.ItemPropertyChanging += m_actionVM.PropertyChangingHandler;
            m_taskVM.TaskCollection = m_octopusInstance.TaskCollection;
            m_taskVM.TaskCollection.ItemPropertyChanged += m_taskVM.PropertyChangedHandler;
            foreach (OctopusLib.Task task in m_taskVM.TaskCollection)
            {
                task.TaskActionCollection.ItemPropertyChanged += m_taskVM.PropertyChangedHandler;
            }
            m_executionVM.OctopusInst = m_octopusInstance;
            CreateNewDefinition();
            currentFile = null;
            this.Title = string.Format("{0} - Untitled*", windowTitle);            
        }

        private void OpenExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string[] filename = null;
            if (Common.OpenFileDialog(out filename))
            {
                InitializeVMCollection();
                LoadDefinitionFile(filename[0]);                
                this.Title = string.Format("{0} - {1}", windowTitle, filename[0]);
                currentFile = filename[0];
            }

            m_executionVM.StopExecution();
            if (m_executionVM.RunInstanceCollection != null)
                m_executionVM.RunInstanceCollection.Clear();
            tabParams.IsSelected = true;
        }

        private void SaveExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveAsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            string filename = null;

            if (Common.SaveFileDialog(out filename))
            {
                if (SaveDefinitionFile(filename))
                {
                    this.Title = string.Format("{0} - {1}", windowTitle, filename);
                    currentFile = filename;
                }
            }
        }

        private void CloseExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_actionVM.IsModified || m_machineVM.IsModified || m_paramsVM.IsModified || m_taskVM.IsModified)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save the modification before close the Octopus definition?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (!SaveDefinitionFile(currentFile))
                        return;
                }
            }
            ClearDefinition();
            m_executionVM.StopExecution();
            if (m_executionVM.RunInstanceCollection != null)
                m_executionVM.RunInstanceCollection.Clear();
            tabParams.IsSelected = true;

            this.Title = string.Format("{0}", windowTitle);
        }

        private void StartRunExecute(object sender, ExecutedRoutedEventArgs e)
        {
            m_executionVM.StartTask();
        }

        private void StopRunExecute(object sender, ExecutedRoutedEventArgs e)
        {
            m_executionVM.StopTask();
        }

        private void ReplayFailedRunExecute(object sender, ExecutedRoutedEventArgs e)
        {
            m_executionVM.ReplayFailedTask();
        }

        private void ShowLogExecute(object sender, ExecutedRoutedEventArgs e)
        {
            m_executionVM.ShowLog();
        }

        private void GetConsoleExecute(object sender, ExecutedRoutedEventArgs e)
        {
            m_executionVM.GetConsole();
        }

        private void HelpExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(@"http://hpoctopus.codeplex.com/documentation"));
            return;
        }

        private void ExitExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void AboutExecute(object sender, ExecutedRoutedEventArgs e)
        {
            About about = new About(this);
            about.ShowDialog();
        }
        #endregion

        #region CanExecute methods
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_actionVM != null && m_machineVM != null && m_paramsVM != null && m_taskVM != null)
            {
                if (m_actionVM.IsModified || m_machineVM.IsModified || m_paramsVM.IsModified || m_taskVM.IsModified)
                {
                    if (m_executionVM.IsExecuting)
                        e.CanExecute = false;
                    else
                        e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_taskVM != null)
                if (m_executionVM.IsExecuting)
                    e.CanExecute = false;
                else
                    e.CanExecute = true;
            else
                e.CanExecute = true;
        }

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_taskVM != null)
                if (m_executionVM.IsExecuting)
                    e.CanExecute = false;
                else
                    e.CanExecute = true;
            else
                e.CanExecute = true;
        }        

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_actionVM == null || m_machineVM == null || m_paramsVM == null || m_taskVM == null)
                e.CanExecute = false;
            else
            {
                if (m_actionVM.ActionCollection != null && m_machineVM.MachineCollection != null && m_paramsVM.ParameterCollection != null && m_taskVM.TaskCollection != null)
                {
                    if (m_actionVM.ActionCollection.Count > 0 ||
                        m_machineVM.MachineCollection.Count > 0 ||
                        m_paramsVM.ParameterCollection.Count > 0 ||
                        m_taskVM.TaskCollection.Count > 0)
                    {
                        if (m_executionVM.IsExecuting)
                            e.CanExecute = false;
                        else
                            e.CanExecute = true;
                    }
                    else
                    {
                        e.CanExecute = false;
                    }
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        }
        
        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_actionVM != null && m_machineVM != null && m_paramsVM != null && m_taskVM != null)
            {
                if (m_actionVM.ActionCollection == null || m_machineVM.MachineCollection == null || m_paramsVM.ParameterCollection == null || m_taskVM.TaskCollection == null)
                    e.CanExecute = false;
                else
                {
                    if (m_executionVM.IsExecuting)
                        e.CanExecute = false;
                    else
                        e.CanExecute = true;
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void StartRunCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_executionVM != null)
                if (m_executionVM.IsExecuting)
                    e.CanExecute = false;
                else
                    if (m_taskVM.TaskCollection != null)
                        e.CanExecute = true;
                    else
                        e.CanExecute = false;
            else
                e.CanExecute = false;
        }

        private void ReplayFailedRunCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_executionVM != null)
                if (m_executionVM.IsExecuting)
                    e.CanExecute = false;
                else
                    if (m_executionVM.RunInstanceCollection != null)
                    {
                        if (m_executionVM.RunInstanceCollection.Where(o => o.Status == Status.Fail).Count() > 0)
                            e.CanExecute = true;
                        else
                            e.CanExecute = false;
                    }
                    else
                    {
                        e.CanExecute = false;
                    }
                    
            else
                e.CanExecute = false;
        }

        private void StopRunCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_executionVM != null)
                if (m_executionVM.IsExecuting)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            else
                e.CanExecute = false;
        }

        private void ShowLogCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (m_executionVM != null)
                if (m_executionVM.SelectedRow != null)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            else
                e.CanExecute = false;
        }

        private void GetConsoleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AboutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion
    }

    public class StatusImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;

            Image img = new Image();

            switch ((OctopusLib.Status)value)
            {
                case OctopusLib.Status.Fail:                    
                    img.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/fail.png", UriKind.RelativeOrAbsolute));
                    img.Width = 16;
                    img.Height = 16;
                    result = img;
                    break;
                case OctopusLib.Status.Warn:
                    img.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/warn.png", UriKind.RelativeOrAbsolute));
                    img.Width = 16;
                    img.Height = 16;
                    result = img;
                    break;
                case OctopusLib.Status.InProgress:
                    Control ctrl = new Control();
                    ctrl.Template = Application.Current.MainWindow.FindResource("loadingAnimation") as ControlTemplate;
                    ctrl.Visibility = Visibility.Visible;
                    ctrl.Width = 16;
                    ctrl.Height = 16;
                    result = ctrl;
                    break;
                case OctopusLib.Status.Pass:
                    img.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/success.png", UriKind.RelativeOrAbsolute));
                    img.Width = 16;
                    img.Height = 16;
                    result = img;
                    break;
                case OctopusLib.Status.NotRun:
                default:
                    img.Source = new BitmapImage(new Uri(@"pack://application:,,,/images/notrun.png", UriKind.RelativeOrAbsolute));
                    img.Width = 16;
                    img.Height = 16;
                    result = img;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MachinesStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollectionEx<OctopusLib.Machine> machines = value as ObservableCollectionEx<OctopusLib.Machine>;
            StringBuilder sbMachine = new StringBuilder();
            if (machines != null)
            {
                foreach (OctopusLib.Machine machine in machines)
                {
                    if (sbMachine.Length == 0)
                        sbMachine.AppendFormat("{0}", machine.Name);
                    else
                        sbMachine.AppendFormat(",{0}", machine.Name);
                }
            }

            return sbMachine.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MachinesNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "Inherited";
            }
            else
            {
                OctopusLib.Machine machine = value as OctopusLib.Machine;
                return machine.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EncryptedValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string paramvalue = values[0].ToString();
            bool isencrypted = (bool)values[1];
            string mid = values[2] as string;

            string value = paramvalue;

            if (isencrypted)
            {
                try { value = Encryption.Encrypt(paramvalue, mid); }
                catch { }
            }

            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not implemented");
        }
    }   

}
