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

        // ---------- ЕЩЁ: меню ----------

        private const string RepoUrl = "https://github.com/sodelnikapor-dot/blockvpn";
        private const string TelegramUrl = ""; // когда создашь канал — впиши сюда ссылку

        private void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }

        private void Contact_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl(RepoUrl + "/issues");
        }

        private void Telegram_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TelegramUrl))
            {
                TelegramNote.Text = "Канал пока не создан. Когда появится — ссылка будет здесь.";
                TelegramNote.Visibility = Visibility.Visible;
                return;
            }
            OpenUrl(TelegramUrl);
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(RepoUrl);
                ShareNote.Text = "Ссылка скопирована: " + RepoUrl;
            }
            catch
            {
                ShareNote.Text = RepoUrl;
            }
            ShareNote.Visibility = Visibility.Visible;
        }

        private void Terms_Click(object sender, RoutedEventArgs e)
        {
            TermsPanel.Visibility = TermsPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Privacy_Click(object sender, RoutedEventArgs e)
        {
            PrivacyPanel.Visibility = PrivacyPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void NetTool_Click(object sender, RoutedEventArgs e)
        {
            NetPanel.Visibility = NetPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void NetCheck_Click(object sender, RoutedEventArgs e)
        {
            NetResult.Text = "Проверяю…";
            string dns = await System.Threading.Tasks.Task.Run(() => Diagnostics.ResolveHost("one.one.one.one"));
            NetResult.Text = dns + "\n" + Diagnostics.AdapterStatusText();
        }
    }
}
