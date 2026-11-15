using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class MainView : UserControl
    {
        private static readonly string[] Stickers = { "🧢", "🎤", "🔊", "🛡️", "👑" };
        private static readonly int[] StickerUnlockAt = { 0, 1, 3, 5, 10 };

        private readonly DispatcherTimer _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private TimeSpan? _lastSession;
        private ulong _lastRx, _lastTx;
        private DateTime? _lastHandshake;

        public MainView()
        {
            InitializeComponent();
            _clock.Tick += (s, e) => RefreshTime();
            _clock.Start();

            var mgr = AppSession.Manager;
            mgr.StateChanged += OnStateChanged;
            mgr.StatsUpdated += OnStatsUpdated;

            AppSession.ProfilesChanged += RefreshProfileInfo;
            Loaded += (s, e) => { RefreshProfileInfo(); ApplyState(mgr.State, mgr.LastError); };
        }

        private void OnStateChanged(VpnState state, string message)
        {
            Dispatcher.BeginInvoke(() => { ApplyState(state, message); RefreshProfileInfo(); });
        }

        private void OnStatsUpdated(ulong rx, ulong tx, DateTime? handshake, TimeSpan? session)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _lastRx = rx; _lastTx = tx; _lastHandshake = handshake; _lastSession = session;
                RxValue.Text = Format.FormatBytes(rx);
                TxValue.Text = Format.FormatBytes(tx);
                HandshakeValue.Text = Format.FormatHandshakeAgo(handshake);
                RefreshTime();
            });
        }

        private void RefreshTime()
        {
            TimeValue.Text = Format.FormatDuration(_lastSession);
        }

        private void RefreshProfileInfo()
        {
            var p = AppSession.ActiveProfile();
            DevBanner.Visibility = p == null ? Visibility.Visible : Visibility.Collapsed;
            ProfileText.Text = p == null ? "Сервер не выбран" : (p.Name + " · " + (string.IsNullOrEmpty(p.Region) ? "регион не указан" : p.Region));
            RenderTag();
            RenderStickers();
            ApplyButtonState(AppSession.Manager.State);
        }

        private void ApplyState(VpnState state, string message)
        {
            var accent = ThemeManager.CurrentAccent;
            var danger = (Color)ColorConverter.ConvertFromString("#FF3B3B");
            var neutral = (Color)ColorConverter.ConvertFromString("#8A8A95");

            Color stroke; string title; double glowOpacity;
            switch (state)
            {
                case VpnState.Secured:
                    stroke = accent; title = "ЗАЩИЩЕНО"; glowOpacity = 0.85; break;
                case VpnState.Connecting:
                    stroke = (Color)ColorConverter.ConvertFromString("#FFD600"); title = "ПОДКЛЮЧЕНИЕ…"; glowOpacity = 0.5; break;
                case VpnState.Reconnecting:
                    stroke = (Color)ColorConverter.ConvertFromString("#FFD600"); title = "ПЕРЕПОДКЛЮЧЕНИЕ…"; glowOpacity = 0.5; break;
                case VpnState.Error:
                    stroke = danger; title = "ОШИБКА"; glowOpacity = 0.6; break;
                default:
                    stroke = neutral; title = "ОТКЛЮЧЕНО"; glowOpacity = 0; break;
            }

            StatusEllipse.Stroke = new SolidColorBrush(stroke);
            Glow.Color = stroke;
            Glow.Opacity = glowOpacity;
            StateText.Text = title;
            SubStateText.Text = message ?? "";

            ErrorText.Visibility = state == VpnState.Error ? Visibility.Visible : Visibility.Collapsed;
            ErrorText.Text = state == VpnState.Error ? (message ?? "Неизвестная ошибка") : "";

            RenderTag();
            ApplyButtonState(state);
        }

        private void ApplyButtonState(VpnState state)
        {
            var profile = AppSession.ActiveProfile();
            switch (state)
            {
                case VpnState.Secured:
                    MainButton.Content = "ОТКЛЮЧИТЬ"; MainButton.IsEnabled = true; RetryButton.Visibility = Visibility.Collapsed; break;
                case VpnState.Connecting:
                case VpnState.Reconnecting:
                    MainButton.Content = "ОТМЕНИТЬ"; MainButton.IsEnabled = true; RetryButton.Visibility = Visibility.Collapsed; break;
                case VpnState.Error:
                    MainButton.Content = "ОТКЛЮЧЕНО"; MainButton.IsEnabled = false; RetryButton.Visibility = Visibility.Visible; break;
                default:
                    MainButton.Content = "ПОДКЛЮЧИТЬ"; MainButton.IsEnabled = profile != null; RetryButton.Visibility = Visibility.Collapsed; break;
            }
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            var mgr = AppSession.Manager;
            if (mgr.IsActive) mgr.Disconnect();
            else
            {
                var profile = AppSession.ActiveProfile();
                if (profile == null) return;
                AppSession.Prefs.TotalConnects++;
                AppSession.SavePrefs();
                mgr.Connect(profile);
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = AppSession.ActiveProfile();
            if (profile == null) return;
            AppSession.Manager.Connect(profile);
        }

        private void RenderTag()
        {
            var prefs = AppSession.Prefs;
            bool decor = prefs.Decor;
            TagSticker.Visibility = decor ? Visibility.Visible : Visibility.Collapsed;
            if (!decor) return;
            bool glow = AppSession.Manager.State == VpnState.Secured;
            TagRender.Configure(TagSticker, prefs.TagText, prefs.TagColorIndex, prefs.TagStyleIndex, glow, prefs.Animations);
        }

        private void RenderStickers()
        {
            if (!AppSession.Prefs.Decor) { StickerBar.Children.Clear(); return; }
            var total = AppSession.Prefs.TotalConnects;
            if (StickerBar.Children.Count == Stickers.Length) return; // уже отрисованы
            StickerBar.Children.Clear();
            for (int i = 0; i < Stickers.Length; i++)
            {
                bool unlocked = total >= StickerUnlockAt[i];
                var tb = new TextBlock
                {
                    Text = unlocked ? Stickers[i] : "🔒",
                    FontSize = 24,
                    Opacity = unlocked ? 1.0 : 0.35,
                    Margin = new Thickness(6, 0, 6, 0),
                    ToolTip = unlocked ? "Стикер открыт" : ("Откроется после " + StickerUnlockAt[i] + " подключений")
                };
                StickerBar.Children.Add(tb);
            }
        }
    }
}
