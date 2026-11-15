using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BlockVPN.App.Views;
using BlockVPN.Core;

namespace BlockVPN.App
{
    public partial class MainWindow : Window
    {
        private readonly MainView _mainView;
        private readonly ServersView _serversView;
        private readonly TagView _tagView;
        private readonly SettingsView _settingsView;
        private readonly HelpView _helpView;
        private readonly BackgroundFx _fx;

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

            ApplyBackground(animate: false);
            _fx = new BackgroundFx(FxHost);
            AppSession.PrefsChanged += OnPrefsChanged;
            AppSession.Manager.StateChanged += OnFxStateChanged;

            ContentHost.Content = _mainView;
            Closed += (s, e) =>
            {
                AppSession.PrefsChanged -= OnPrefsChanged;
                AppSession.Manager.StateChanged -= OnFxStateChanged;
                AppSession.Manager.ShutdownCleanup();
            };

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

        private void OnPrefsChanged()
        {
            ApplyBackground(animate: true);
            UpdateFx();
        }

        private void OnFxStateChanged(VpnState state, string message)
        {
            Dispatcher.BeginInvoke(UpdateFx);
        }

        /// <summary>Уникальная анимация фона темы: играет при подключении, гаснет при отключении.</summary>
        private void UpdateFx()
        {
            bool decor = AppSession.Prefs == null || AppSession.Prefs.Decor;
            bool anim = AppSession.Prefs == null || AppSession.Prefs.Animations;
            var state = AppSession.Manager.State;
            if (decor && anim && (state == VpnState.Secured || state == VpnState.Connecting || state == VpnState.Reconnecting))
                _fx.Play(AppSession.Prefs == null ? 0 : AppSession.Prefs.ThemeIndex);
            else
                _fx.Stop();
        }

        /// <summary>Фон темы: градиент + неоновые пятна. При переключении темы цвета плавно перетекают.</summary>
        private void ApplyBackground(bool animate)
        {
            ApplyStop(BgStop1, ThemeManager.CurrentBg1, animate);
            ApplyStop(BgStop2, ThemeManager.CurrentBg2, animate);
            ApplyStop(GlowStop1, WithAlpha(ThemeManager.CurrentAccent, 0x16), animate);
            ApplyStop(GlowStop2, WithAlpha(ThemeManager.CurrentAccent2, 0x12), animate);
        }

        private static void ApplyStop(GradientStop stop, Color target, bool animate)
        {
            if (animate && (AppSession.Prefs == null || AppSession.Prefs.Animations))
            {
                stop.BeginAnimation(GradientStop.ColorProperty,
                    new ColorAnimation(target, TimeSpan.FromMilliseconds(500))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    });
            }
            else
            {
                stop.BeginAnimation(GradientStop.ColorProperty, null);
                stop.Color = target;
            }
        }

        private static Color WithAlpha(Color c, byte a)
        {
            return Color.FromArgb(a, c.R, c.G, c.B);
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            if (!rb.IsChecked.Value) return;
            if (rb == NavMain) ShowView(_mainView);
            else if (rb == NavServers) ShowView(_serversView);
            else if (rb == NavTag) ShowView(_tagView);
            else if (rb == NavSettings) ShowView(_settingsView);
            else if (rb == NavHelp) ShowView(_helpView);
        }

        /// <summary>Плавное появление вкладки (лёгкий fade + сдвиг), если анимации включены.</summary>
        private void ShowView(UserControl view)
        {
            if (ContentHost.Content == view) return;
            ContentHost.Content = view;

            bool anim = AppSession.Prefs == null || AppSession.Prefs.Animations;
            if (anim)
            {
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(240))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                view.BeginAnimation(UIElement.OpacityProperty, fade);

                var slide = new ThicknessAnimation(new Thickness(0, 14, 0, 0), new Thickness(0), TimeSpan.FromMilliseconds(240))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                view.BeginAnimation(FrameworkElement.MarginProperty, slide);
            }
            else
            {
                view.BeginAnimation(UIElement.OpacityProperty, null);
                view.BeginAnimation(FrameworkElement.MarginProperty, null);
                view.Opacity = 1;
                view.Margin = new Thickness(0);
            }
        }
    }
}
