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
using OctopusLib;

namespace OctopusGUI
{
    /// <summary>
    /// Interaction logic for LinuxSSHCommandProperty.xaml
    /// </summary>
    public partial class LinuxSSHCommandProperty : UserControl
    {
        public ObservableCollectionEx<OctopusLib.Parameter> ParameterCollection { get; set; }
        public Window OwnerWindow { get; set; }

        public LinuxSSHCommandProperty()
        {
            InitializeComponent();
            InitializeSSHTypeComboBox();
        }

        private void InitializeSSHTypeComboBox()
        {
            cmbSSHType.ItemsSource = Enum.GetValues(typeof(SSHType));
            cmbSSHType.SelectedIndex = 0;
        }

        private void cmbSSHType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSSHType.SelectedItem.ToString() == OctopusLib.SSHType.Exec.ToString())
            {
                tbExpectedPrompt.IsEnabled = false;
                tbTimeoutSeconds.IsEnabled = false;
            }
            else if (cmbSSHType.SelectedItem.ToString() == OctopusLib.SSHType.Stream.ToString())
            {
                tbExpectedPrompt.IsEnabled = true;
                tbTimeoutSeconds.IsEnabled = true;
            }
        }
    }
}
