using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Reflection;
namespace OctopusGUI
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        protected About()
        {
            InitializeComponent();
            Assembly app = Assembly.GetExecutingAssembly();

            AssemblyTitleAttribute asstitle = (AssemblyTitleAttribute)app.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            AssemblyProductAttribute assproduct = (AssemblyProductAttribute)app.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            AssemblyCopyrightAttribute asscopyright = (AssemblyCopyrightAttribute)app.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            AssemblyCompanyAttribute asscompany = (AssemblyCompanyAttribute)app.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0];
            AssemblyDescriptionAttribute assdescription = (AssemblyDescriptionAttribute)app.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0];
            AssemblyFileVersionAttribute assversion = (AssemblyFileVersionAttribute)app.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0];
            
            version.Content = assversion.Version;
            copyright.Content = asscopyright.Copyright;
            company.Content = asscompany.Company;
            description.Text = assdescription.Description;
        }

        public About(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string uri = e.Uri.AbsoluteUri;
            Process.Start(new ProcessStartInfo(uri));

            e.Handled = true;
        }
    }
}
