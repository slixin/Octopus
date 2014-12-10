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
    /// Interaction logic for CopyCommandProperty.xaml
    /// </summary>
    public partial class CopyCommandProperty : UserControl
    {
        public ObservableCollectionEx<OctopusLib.Parameter> ParameterCollection { get; set; }
        public Window OwnerWindow { get; set; }

        public CopyCommandProperty()
        {
            InitializeComponent();
            InitializeDirectionComboBox();
        }

        private void InitializeDirectionComboBox()
        {
            cmbDirection.Items.Insert(0, "LocalToRemote");
            cmbDirection.Items.Insert(1, "RemoteToLocal");
            cmbDirection.SelectedIndex = 0;
        }

        private void tbFiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;

            if (string.IsNullOrEmpty(box.Text.Trim()))
            {
                tbSourceDir.IsEnabled = true;
                    
                if (cmbDirection.SelectedItem.ToString() == CopyCommand.Direction.LocalToRemote.ToString())
                {
                    btnSelectSourceDirectory.IsEnabled = true;
                }
            }
            else
            {
                tbSourceDir.IsEnabled = false;
                btnSelectSourceDirectory.IsEnabled = false;
            }      
            
        }

        private void tbSourceDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            if (string.IsNullOrEmpty(box.Text.Trim()))
            {
                tbFiles.IsEnabled = true;
                    
                if (cmbDirection.SelectedItem.ToString() == CopyCommand.Direction.LocalToRemote.ToString())
                {
                    btnSelectSourceFiles.IsEnabled = true;
                }
            }
            else
            {
                tbFiles.IsEnabled = false;
                btnSelectSourceFiles.IsEnabled = false;
            }            
        }

        private void btnSelectSourceFiles_Click(object sender, RoutedEventArgs e)
        {
            string[] files = null;
            string filestr = this.tbFiles.ValueText.Trim();

            if (Common.OpenFileDialog(out files, true, "*"))
            {
                if (files != null)
                {
                    if (files.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(filestr))
                        {
                            MessageBoxResult result = MessageBox.Show("Do you want to append current source files?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                filestr += string.Format(";{0}", string.Join(@";", files));
                            }
                            else
                            {
                                filestr = string.Join(@";", files);
                            }
                        }
                        else
                        {
                            filestr = string.Join(@";", files);
                        }
                        tbSourceDir.IsEnabled = false;
                        btnSelectSourceDirectory.IsEnabled = false;

                        ((CopyCommand)this.DataContext).CopySourceFiles = filestr;
                    }
                }
            }            
        }

        private void btnSelectSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            string folder = null;

            if (Common.OpenFolderDialog(out folder))
            {
                ((CopyCommand)this.DataContext).CopySourceDir = folder;
                tbFiles.IsEnabled = false;
                btnSelectSourceFiles.IsEnabled = false;
            }
        }

        private void btnSelectTargetDirectory_Click(object sender, RoutedEventArgs e)
        {
            string folder = null;

            if (Common.OpenFolderDialog(out folder))
            {
                ((CopyCommand)this.DataContext).CopyTargetDir = folder;
            }
        }

        private void cmbDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDirection.SelectedItem.ToString() == CopyCommand.Direction.LocalToRemote.ToString())
            {
                btnSelectSourceFiles.IsEnabled = true;
                btnSelectSourceDirectory.IsEnabled = true;
                btnSelectTargetDirectory.IsEnabled = false;
            }
            else if (cmbDirection.SelectedItem.ToString() == CopyCommand.Direction.RemoteToLocal.ToString())
            {
                btnSelectSourceFiles.IsEnabled = false;
                btnSelectSourceDirectory.IsEnabled = false;
                btnSelectTargetDirectory.IsEnabled = true;
            }
        }
    }

    public class CopyDirectionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((OctopusLib.CopyCommand.Direction)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string directionStr = (string)value;

            return Enum.Parse(typeof(OctopusLib.CopyCommand.Direction), value.ToString());
        }
    }
}
