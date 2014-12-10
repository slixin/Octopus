using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OctopusLib;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Collections;
using System.IO;
using System.Xml;
using AutomationHelper;
using System.Runtime.InteropServices;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace OctopusGUI.ViewModelClass
{
    public class InstanceViewModel : ViewModelBase
    {
        #region private members
        private ObservableCollectionEx<OctopusLib.RunInstance> _runinstancecollection;
        private BackgroundWorkerEx runWorker;
        private OctopusLib.RunInstance _selectedrow;
        private List<Thread> runInstanceThreads;
        private bool _isReplayFailedTaskOnly;

        private RelayCommand<object> startTaskCommand;
        private RelayCommand<object> stopTaskCommand;
        private RelayCommand<object> showLogCommand;
        private RelayCommand<object> getConsoleCommand;
        private RelayCommand<object> replayFailedTaskCommand;
        #endregion

        #region Properties
        public Window ParentWindow { get; set; }

        public OctopusInstance OctopusInst { get; set; }

        public bool IsExecuting { get; set; }

        public string ConfigFile { get; set; }

        public ObservableCollectionEx<OctopusLib.RunInstance> RunInstanceCollection
        {
            get
            {
                return _runinstancecollection;
            }
            set
            {
                if (value != _runinstancecollection)
                {
                    _runinstancecollection = value;
                    OnPropertyChanged("RunInstanceCollection");
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected row.
        /// </summary>
        /// <value>The selected row.</value>
        public OctopusLib.RunInstance SelectedRow
        {
            get
            {
                return _selectedrow;
            }
            set
            {
                _selectedrow = value;
                OnPropertyChanged("SelectedRow");
            }
        }
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public InstanceViewModel()
        {
            _isReplayFailedTaskOnly = false;
        }

        #region Commands
        public System.Windows.Input.ICommand GetConsoleCommand
        {
            get
            {
                if (getConsoleCommand == null)
                {
                    getConsoleCommand = new RelayCommand<object>(x => this.GetConsole(), x => this.CanGetConsole());
                }
                return getConsoleCommand;
            }
        }
        public System.Windows.Input.ICommand ShowLogCommand
        {
            get
            {
                if (showLogCommand == null)
                {
                    showLogCommand = new RelayCommand<object>(x => this.ShowLog(), x => this.CanShowLog());
                }
                return showLogCommand;
            }
        }

        public System.Windows.Input.ICommand StartTaskCommand
        {
            get
            {
                if (startTaskCommand == null)
                {
                    startTaskCommand = new RelayCommand<object>(x => this.StartTask(), x => this.CanStartTask());
                }
                return startTaskCommand;
            }
        }

        public System.Windows.Input.ICommand StopTaskCommand
        {
            get
            {
                if (stopTaskCommand == null)
                {
                    stopTaskCommand = new RelayCommand<object>(x => this.StopTask(), x => this.CanStopTask());
                }
                return stopTaskCommand;
            }
        }

        public System.Windows.Input.ICommand ReplayFailedTaskCommand
        {
            get
            {
                if (replayFailedTaskCommand == null)
                {
                    replayFailedTaskCommand = new RelayCommand<object>(x => this.ReplayFailedTask(), x => this.CanReplayFailedTask());
                }
                return replayFailedTaskCommand;
            }
        }
        #endregion

        #region Can methods
        private bool CanStartTask()
        {
            if (RunInstanceCollection == null)
            {
                if (((MainWindow)ParentWindow).m_taskVM == null)
                    return false;
                else
                {
                    if (((MainWindow)ParentWindow).m_taskVM.TaskCollection == null)
                        return false;
                    else
                    {
                        if (((MainWindow)ParentWindow).m_taskVM.TaskCollection.Count == 0)
                            return false;
                        else
                        {
                            if (IsExecuting)
                                return false;
                            else
                                return true;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private bool CanStopTask()
        {
            if (RunInstanceCollection == null)
                return false;
            else
            {
                if (RunInstanceCollection.Count == 0)
                    return false;
                else
                {
                    if (IsExecuting)
                        return true;
                    else
                        return false;
                }
            }
        }

        private bool CanReplayFailedTask()
        {
            if (RunInstanceCollection != null)
            {
                if (RunInstanceCollection.Where(o => o.Status == Status.Fail).Count() > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private bool CanShowLog()
        {
            if (SelectedRow != null)
            {
                if (System.IO.File.Exists(SelectedRow.LogFile))
                    return true;
            }

            return false;
        }

        private bool CanGetConsole()
        {
            return true;
        }
        #endregion

        #region
        public void ShowLog()
        {
            ProcessHelper.Launch(SelectedRow.LogFile);
        }

        public void GetConsole()
        {
            string OctopusZip = Path.Combine(Environment.CurrentDirectory, "Octopus.zip");

            if (File.Exists(OctopusZip))
                File.Delete(OctopusZip);

            #region Zip Octopus console
            ZipFile z = ZipFile.Create(OctopusZip);
            z.BeginUpdate();

            //add the file to the zip file        
            z.Add(Path.Combine(Environment.CurrentDirectory, "AutomationHelper.dll"), "AutomationHelper.dll");
            z.Add(Path.Combine(Environment.CurrentDirectory, "DiffieHellman.dll"), "DiffieHellman.dll");
            z.Add(Path.Combine(Environment.CurrentDirectory, "Interop.WMEncoderLib.dll"), "Interop.WMEncoderLib.dll");
            z.Add(Path.Combine(Environment.CurrentDirectory, "OctopusLib.dll"), "OctopusLib.dll");
            z.Add(Path.Combine(Environment.CurrentDirectory, "Octopus.exe"), "Octopus.exe");
            z.Add(Path.Combine(Environment.CurrentDirectory, "Org.Mentalis.Security.dll"), "Org.Mentalis.Security.dll");
            z.Add(Path.Combine(Environment.CurrentDirectory, "PaExec.exe"), "PaExec.exe");
            z.Add(Path.Combine(Environment.CurrentDirectory, "PsService.exe"), "PsService.exe");
            z.Add(Path.Combine(Environment.CurrentDirectory, "psshutdown.exe"), "psshutdown.exe");
            z.Add(Path.Combine(Environment.CurrentDirectory, "Tamir.SharpSSH.dll"), "Tamir.SharpSSH.dll");
            //commit the update once we are done
            z.CommitUpdate();
            //close the file
            z.Close();
            #endregion


            #region Download
            string file = null;
            if (Common.SaveFileDialog(out file, "zip", "Octopus"))
            {
                File.Copy(OctopusZip, file, true);
                ProcessHelper.Launch(Path.GetDirectoryName(file));
            }
            #endregion
        }
        
        public void StartTask()
        {
            if (((MainWindow)ParentWindow).m_paramsVM.IsModified || ((MainWindow)ParentWindow).m_machineVM.IsModified || ((MainWindow)ParentWindow).m_actionVM.IsModified || ((MainWindow)ParentWindow).m_taskVM.IsModified)
            {
                MessageBoxResult resultSave = MessageBox.Show("Something changed, have to be saved before task, Yes or No?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resultSave == MessageBoxResult.Yes)
                {
                    ((MainWindow)ParentWindow).SaveFile();
                }
                else
                {
                    return;
                }
            }

            if (RunInstanceCollection != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you going to task again? Previous runs will be cleaned.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RunInstanceCollection.Clear();
                    RunInstanceCollection = null;
                    Run();
                }
            }
            else
            {
                Run();
            }
            
        }

        public void StopTask()
        {
            if (runWorker.WorkerSupportsCancellation == true)
            {
                if (runWorker.IsBusy == true)
                {
                    MessageBoxResult result = MessageBox.Show("Are you going to stop the runs?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        #region Abort task instance threads
                        foreach (Thread thread in runInstanceThreads.Where(o=>o.IsAlive))
                        {
                            thread.Abort();
                        }
                        #endregion

                        #region Set all running task instance and action instance status to be fail
                        foreach (OctopusLib.RunInstance ri in RunInstanceCollection.Where(o => o.Status == Status.InProgress))
                        {
                            foreach (OctopusLib.ActionInstance ai in ri.ActionInstanceCollection.Where(p => p.Status == Status.InProgress))
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                                {
                                    ai.Status = Status.Fail;
                                });
                            }
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                ri.Status = Status.Fail;
                            });
                        }
                        #endregion

                        runWorker.Abort();
                        runWorker.Dispose();
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        public void ReplayFailedTask()
        {
            MessageBoxResult result = MessageBox.Show("Are you going to run the failed task again? Any definition changes will not be applied on replay failed run.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _isReplayFailedTaskOnly = true;
                Run();
            }
            else
            {
                return;
            }
        }

        public void StopExecution()
        {
            IsExecuting = false;
            ((MainWindow)ParentWindow).tabParams.IsEnabled = true;
            ((MainWindow)ParentWindow).tabMachines.IsEnabled = true;
            ((MainWindow)ParentWindow).tabActions.IsEnabled = true;
            ((MainWindow)ParentWindow).tabTasks.IsEnabled = true;

            ((MainWindow)ParentWindow).m_actionVM.IsExecuting = false;
            ((MainWindow)ParentWindow).m_machineVM.IsExecuting = false;
            ((MainWindow)ParentWindow).m_taskVM.IsExecuting = false;
            ((MainWindow)ParentWindow).m_paramsVM.IsExecuting = false;
        }
        #endregion

        #region private methods
        private void Run()
        {
            ((MainWindow)ParentWindow).tabExecution.IsSelected = true;
            ((MainWindow)ParentWindow).tabParams.IsEnabled = false;
            ((MainWindow)ParentWindow).tabMachines.IsEnabled = false;
            ((MainWindow)ParentWindow).tabActions.IsEnabled = false;
            ((MainWindow)ParentWindow).tabTasks.IsEnabled = false;
            ((MainWindow)ParentWindow).m_actionVM.IsExecuting = true;
            ((MainWindow)ParentWindow).m_machineVM.IsExecuting = true;
            ((MainWindow)ParentWindow).m_taskVM.IsExecuting = true;
            ((MainWindow)ParentWindow).m_paramsVM.IsExecuting = true;
            IsExecuting = true;
            runWorker = new BackgroundWorkerEx();
            runWorker.WorkerSupportsCancellation = true;
            runWorker.DoWork += this.Run_DoWork;
            runWorker.RunWorkerCompleted += this.Run_Completed;
            runWorker.RunWorkerAsync();
        }

        private void Run_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isContinue = true;
            if (!_isReplayFailedTaskOnly)
            {
                OctopusInst.CreateRunInstance();
                RunInstanceCollection = OctopusInst.RunInstanceCollection;
                if (RunInstanceCollection == null)
                {
                    return;
                }
                else if (RunInstanceCollection.Count > 0)
                {
                    runInstanceThreads = new List<Thread>();
                    foreach (var runInstancesGroup in RunInstanceCollection.OrderBy(k => k.Task.Sequence).GroupBy(p => p.Task.Sequence))
                    {
                        if (!isContinue)
                        {
                            return;
                        }

                        foreach (var runInstance in runInstancesGroup)
                        {
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                runInstance.Status = Status.InProgress;
                            });
                            runInstance.UIDispatcher = Application.Current.Dispatcher;
                            Thread runInstanceThread = new Thread(runInstance.StartRun);
                            runInstanceThread.Priority = ThreadPriority.AboveNormal;
                            runInstanceThread.Start();
                            runInstanceThreads.Add(runInstanceThread);
                        }
                        foreach (Thread thread in runInstanceThreads)
                        {
                            // Wait until thread is finished.
                            thread.Join();
                        }

                        var failcount = 0;
                        foreach(var runInstance in runInstancesGroup)
                        {
                            failcount += runInstance.ActionInstanceCollection.Where(o=>o.Status == Status.Fail).Count();
                        }

                        if (failcount > 0)
                            isContinue = false;
                    }
                }
            }
            else
            {
                runInstanceThreads = new List<Thread>();
                foreach (var runInstancesGroup in RunInstanceCollection.Where(o=>o.Status == Status.Fail).OrderBy(k => k.Task.Sequence).GroupBy(p => p.Task.Sequence))
                {
                    if (!isContinue)
                    {
                        return;
                    }

                    foreach (var runInstance in runInstancesGroup)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                        {
                            runInstance.Status = Status.InProgress;
                        });

                        foreach (var actionInstance in runInstance.ActionInstanceCollection)
                        {
                            foreach (var command in actionInstance.Action.ActionCommands)
                            {
                                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                                {
                                    command.Status = Status.NotRun;
                                });
                            }
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate
                            {
                                actionInstance.Status = Status.NotRun;
                            });
                        }

                        runInstance.UIDispatcher = Application.Current.Dispatcher;
                        Thread runInstanceThread = new Thread(runInstance.StartRun);
                        runInstanceThread.Priority = ThreadPriority.AboveNormal;
                        runInstanceThread.Start();
                        runInstanceThreads.Add(runInstanceThread);
                    }
                    foreach (Thread thread in runInstanceThreads)
                    {
                        // Wait until thread is finished.
                        thread.Join();
                    }

                    var failcount = 0;
                    foreach (var runInstance in runInstancesGroup)
                    {
                        failcount += runInstance.ActionInstanceCollection.Where(o => o.Status == Status.Fail).Count();
                    }

                    if (failcount > 0)
                        isContinue = false;
                }
            }
            
        }

        private void Run_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_isReplayFailedTaskOnly)
                _isReplayFailedTaskOnly = false;
            StopExecution();
        }
        
        #endregion
    }
}
