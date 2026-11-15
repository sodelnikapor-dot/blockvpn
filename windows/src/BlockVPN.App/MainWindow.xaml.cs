using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using BlockVPN.App.Views;

namespace BlockVPN.App
{
    public partial class MainWindow : Window
    {
        private readonly MainView _mainView;
        private readonly ServersView _serversView;
        private readonly TagView _tagView;
        private readonly SettingsView _settingsView;
        private readonly HelpView _helpView;

        public MainWindow()
        {
            AppSession.Init();
            ThemeManager.Apply(AppSession.Prefs.ThemeIndex);

            InitializeComponent();

            _mainView = new MainView();
            _serversView = new ServersView();
            _tagView = new TagView();
            _settingsView = new SettingsView();
            _helpView = new HelpView();

            VersionText.Text = "BlockVPN " + (Assembly.GetExecutingAssembly()
                .GetName().Version?.ToString(3) ?? "0.1.0") + " // MVP";

            ContentHost.Content = _mainView;
            Closed += (s, e) => AppSession.Manager.ShutdownCleanup();

            if (AppSession.Prefs.AutoConnect && !AppSession.Manager.IsActive)
            {
                var profile = AppSession.ActiveProfile();
                if (profile != null)
                {
                    AppSession.Prefs.TotalConnects++;
                    AppSession.SavePrefs();
                    AppSession.Manager.Connect(profile);
                }
            }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            if (!rb.IsChecked.Value) return;
            if (rb == NavMain) ContentHost.Content = _mainView;
            else if (rb == NavServers) ContentHost.Content = _serversView;
            else if (rb == NavTag) ContentHost.Content = _tagView;
            else if (rb == NavSettings) ContentHost.Content = _settingsView;
            else if (rb == NavHelp) ContentHost.Content = _helpView;
        }
    }
}
