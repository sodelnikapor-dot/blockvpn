using System.Windows;
using System.Windows.Controls;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void DnsCheck_Click(object sender, RoutedEventArgs e)
        {
            DiagOutput.Text = Diagnostics.ResolveHost("one.one.one.one") + "\n" + Diagnostics.ResolveHost("example.com");
        }

        private void AdapterCheck_Click(object sender, RoutedEventArgs e)
        {
            DiagOutput.Text = Diagnostics.AdapterStatusText();
        }

        private void ServiceCheck_Click(object sender, RoutedEventArgs e)
        {
            DiagOutput.Text = Diagnostics.ServiceStatusText();
        }

        private void LogCheck_Click(object sender, RoutedEventArgs e)
        {
            DiagOutput.Text = Diagnostics.RecentLogLines(30);
        }
    }
}
