using OctopusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OctopusGUI.UserControls
{
    /// <summary>
    /// Interaction logic for ValueTextBox.xaml
    /// </summary>
    public partial class ValueTextBox : UserControl
    {
        public ValueTextBox()
        {
            InitializeComponent();
        }

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(ValueTextBox), new PropertyMetadata(string.Empty));

        public ObservableCollectionEx<Parameter> Parameters
        {
            get { return (ObservableCollectionEx<Parameter>)GetValue(ParametersProperty); }
            set {  SetValue(ParametersProperty, value);  }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters", typeof(ObservableCollectionEx<Parameter>), typeof(ValueTextBox), new PropertyMetadata(null));

        private void cmbParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbParameters.SelectedIndex >= 0)
            {
                int currentPosition = txtValue.CaretIndex;
                string selectedText = txtValue.SelectedText;
                string parameter = cmbParameters.SelectedValue.ToString();

                int start = txtValue.Text.LastIndexOf("{%", currentPosition);
                int end = txtValue.Text.IndexOf("%}", currentPosition);

                if (start >= 0 && end > 0)
                {
                    string before = txtValue.Text.Substring(0, start);
                    string after = txtValue.Text.Substring(end + 2);
                    txtValue.Text = string.Format("{0}{{%{1}%}}{2}", before, parameter, after);
                    txtValue.Focus();
                    txtValue.CaretIndex = start + parameter.Length+4;
                }
                else
                {
                    if (selectedText.Length > 0)
                    {
                        int selectionStart = txtValue.SelectionStart;
                        string before = txtValue.Text.Substring(0, txtValue.SelectionStart);
                        string after = txtValue.Text.Substring(txtValue.SelectionStart + txtValue.SelectionLength, txtValue.Text.Length - txtValue.SelectionStart - txtValue.SelectionLength);
                        txtValue.Text = string.Format("{0}{{%{1}%}}{2}", before, parameter, after);
                        txtValue.Focus();
                        txtValue.CaretIndex = selectionStart + parameter.Length + 4;
                    }
                    else
                    {
                        string before = txtValue.Text.Substring(0, currentPosition);
                        string after = txtValue.Text.Substring(currentPosition, txtValue.Text.Length - currentPosition);
                        txtValue.Text = string.Format("{0}{{%{1}%}}{2}", before, parameter, after);
                        txtValue.Focus();
                        txtValue.CaretIndex = currentPosition + parameter.Length + 4;
                    }
                }
                cmbParameters.SelectedIndex = -1;
                cmbParameters.Visibility = System.Windows.Visibility.Collapsed;            
            }
            
        }

        private void txtValue_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            cmbParameters.ItemsSource = Parameters.Select(o => o.Name).ToList();
            if (cmbParameters.Visibility == System.Windows.Visibility.Collapsed)
                cmbParameters.Visibility = System.Windows.Visibility.Visible;
            else
                cmbParameters.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
