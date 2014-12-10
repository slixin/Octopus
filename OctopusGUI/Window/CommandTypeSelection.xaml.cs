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

namespace OctopusGUI
{
    /// <summary>
    /// Interaction logic for CommandTypeSelection.xaml
    /// </summary>
    public partial class CommandTypeSelection : Window
    {
        public string CommandType { get; set; }

        public CommandTypeSelection()
        {
            InitializeComponent();
            btnOK.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            CommandType = ((ComboBoxItem)cmbCommandType.SelectedValue).Content.ToString();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
