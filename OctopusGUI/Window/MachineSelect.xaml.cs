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
    /// Interaction logic for MachineSelect.xaml
    /// </summary>
    public partial class MachineSelect : Window
    {
        public ObservableCollectionEx<OctopusLib.Machine> SelectedMachines { get; set; }
        public bool IsAllowSelectAll { get; set; }

        private ObservableCollectionEx<OctopusLib.Machine> availableMachines;

        public MachineSelect()
        {
            InitializeComponent();
        }

        public void BindingAvailableMachineList()
        {
            if (IsAllowSelectAll)
            {
                lbMachines.SelectionMode = SelectionMode.Multiple;
                ckbSelectAll.IsEnabled = true;
            }
            else
            {
                lbMachines.SelectionMode = SelectionMode.Single;
                ckbSelectAll.IsEnabled = false;
            }
                

            availableMachines = (Owner as MainWindow).m_machineVM.MachineCollection;
            lbMachines.ItemsSource = availableMachines;
            if (SelectedMachines != null)
            {
                if (SelectedMachines.Count > 0)
                {
                    foreach (OctopusLib.Machine am in lbMachines.Items)
                    {
                        if (SelectedMachines.Where(o => o.Name.Equals(am.Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                        {
                            if (IsAllowSelectAll)
                                lbMachines.SelectedItems.Add(am);
                            else
                                lbMachines.SelectedItem = am;
                        }
                    }      
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedMachines = new ObservableCollectionEx<Machine>();

            foreach (OctopusLib.Machine machine in lbMachines.SelectedItems)
            {
                SelectedMachines.Add(machine);
            }

            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            lbMachines.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            lbMachines.UnselectAll();
        }
    }
}
