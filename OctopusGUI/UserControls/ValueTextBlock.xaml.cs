using OctopusLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ValueTextBlock.xaml
    /// </summary>
    public partial class ValueTextBlock : UserControl
    {
        public ValueTextBlock()
        {
            InitializeComponent();
        }

        public ObservableCollectionEx<Parameter> Parameters
        {
            get { return (ObservableCollectionEx<Parameter>)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters", typeof(ObservableCollectionEx<Parameter>), typeof(ValueTextBlock), new PropertyMetadata(null));


        public string TextValue
        {
            get { return (string)GetValue(TextValueProperty); }
            set { SetValue(TextValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register("TextValue", typeof(string), typeof(ValueTextBlock), new PropertyMetadata(string.Empty));
    }

    public class ValueBackGroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isvalid = true;
            if (values[0] == null || values[1] == null)
                return null;

            string paramvalue = values[0].ToString();
            if (string.IsNullOrEmpty(paramvalue))
                return null;

            if (values[1] != DependencyProperty.UnsetValue)
            {
                ObservableCollectionEx<Parameter> siblings = (ObservableCollectionEx<Parameter>)values[1];
                string value = Command.Normalize(paramvalue, siblings);
                if (Regex.IsMatch(value, @"{%(.+?)%}"))
                    isvalid = false;
                else
                    isvalid = true;
            }

            if (!isvalid)
                return Brushes.Yellow;
            else
                return SystemColors.WindowBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not implemented");
        }
    }

    public class ParameterValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
                return null;

            string paramvalue = values[0].ToString();
            if (string.IsNullOrEmpty(paramvalue))
                return null;
            string value = paramvalue;

            if (values[1] != DependencyProperty.UnsetValue)
            {
                ObservableCollectionEx<Parameter> siblings = (ObservableCollectionEx<Parameter>)values[1];
                if (Regex.IsMatch(paramvalue, @"{%(.+?)%}"))
                {
                    value = Command.Normalize(paramvalue, siblings);
                }
            }            

            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not implemented");
        }
    }
}
