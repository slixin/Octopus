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
    /// Interaction logic for RemoteCommandProperty.xaml
    /// </summary>
    public partial class RemoteCommandProperty : UserControl
    {
        public ObservableCollectionEx<OctopusLib.Parameter> ParameterCollection { get; set; }
        public Window OwnerWindow { get; set; }

        public RemoteCommandProperty()
        {
            InitializeComponent();
            InitializeArchitectureComboBox();
        }

        private void InitializeArchitectureComboBox()
        {
            Dictionary<int, string> archList = new Dictionary<int, string>();
            archList.Add(0, "Not Specified");
            archList.Add(32, "x86");
            archList.Add(64, "x64");

            cmbArchitecture.ItemsSource = archList;
        }
    }
}
