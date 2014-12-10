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
using System.Windows.Shapes;
using OctopusLib;
using System.Collections;

namespace OctopusGUI
{
    /// <summary>
    /// Interaction logic for TaskActionSelect.xaml
    /// </summary>
    public partial class TaskActionSelect : Window
    {
        public OctopusLib.TaskAction SelectedTaskAction { get; set; }

        private ObservableCollectionEx<OctopusLib.Action> availableActions;

        public TaskActionSelect()
        {
            InitializeComponent();
        }

        public void BindingAvailableActionList()
        {
            availableActions = (Owner as MainWindow).m_actionVM.ActionCollection;
            availableActions.Sort(o => o.Name);
            lbAction.ItemsSource = availableActions;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            foreach (OctopusLib.Action action in lbAction.SelectedItems)
            {
                SelectedTaskAction = new TaskAction() { Name = action.Name, IsEnabled = true, IsFixed=false };
                break;
            }

            this.Close();
        }
    }
}
