using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class SettingsView : UserControl
    {
        private bool _suppress;

        public SettingsView()
        {
            InitializeComponent();
            foreach (var p in ThemeManager.Palettes)
                ThemeCombo.Items.Add(p.Name);
            Loaded += (s, e) =>
            {
                _suppress = true;
                ThemeCombo.SelectedIndex = AppSession.Prefs.ThemeIndex;
                AnimationsCheck.IsChecked = AppSession.Prefs.Animations;
                DecorCheck.IsChecked = AppSession.Prefs.Decor;
                AutoStartCheck.IsChecked = AutoStart.IsSet();
                AutoConnectCheck.IsChecked = AppSession.Prefs.AutoConnect;
                _suppress = false;
            };
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppress || ThemeCombo.SelectedIndex < 0) return;
            AppSession.Prefs.ThemeIndex = ThemeCombo.SelectedIndex;
            ThemeManager.Apply(ThemeCombo.SelectedIndex);
            AppSession.SavePrefs();
        }

        private void Anim_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppress) return;
            AppSession.Prefs.Animations = AnimationsCheck.IsChecked == true;
            AppSession.SavePrefs();
        }

        private void Decor_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppress) return;
            AppSession.Prefs.Decor = DecorCheck.IsChecked == true;
            AppSession.SavePrefs();
        }

        private void AutoStart_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppress) return;
            bool enabled = AutoStartCheck.IsChecked == true;
            AutoStart.Set(enabled);
            AppSession.Prefs.AutoStart = enabled;
            AppSession.SavePrefs();
        }

        private void AutoConnect_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppress) return;
            AppSession.Prefs.AutoConnect = AutoConnectCheck.IsChecked == true;
            AppSession.SavePrefs();
        }

        private void Firewall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("wf.msc") { UseShellExecute = true });
            }
            catch { }
        }
    }
}
