using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace BlockVPN.App
{
    /// <summary>
    /// Уникальная фоновая анимация для каждой темы, включается при подключении.
    /// Чистый декор: не влияет на VPN, отключается настройками «Анимации»/«Декор»
    /// и гаснет при отключении соединения.
    /// </summary>
    public class BackgroundFx
    {
        private static readonly Random Rnd = new Random();
        private readonly Grid _host;
        private int _activeTheme = -1;

        public BackgroundFx(Grid host)
        {
            _host = host;
        }

        public void Stop()
        {
            _activeTheme = -1;
            _host.Children.Clear();
        }

        public void Play(int themeIndex)
        {
            if (_activeTheme == themeIndex && _host.Children.Count > 0) return;
            Stop();
            _activeTheme = themeIndex;

            switch (themeIndex)
            {
                case 0: Rings(); break;        // Ночной район: кольца баса
                case 1: Twinkle(); break;      // Неоновый малл: искры
                case 2: RisingOrbs(); break;   // Атланта-трэп: всплывающие монеты
                case 3: Flicker(); break;      // Дриль-ночь: мерцающее зарево
                case 4: Sunset(); break;       // Вест-коаст: садящееся солнце
                case 5: Scanline(); break;     // Мемфис: сканирующая полоса
                case 6: Waves(); break;        // Майами-бас: волны
                case 7: GoldRain(); break;     // Бруклин-голд: золотой дождь
                case 8: Equalizer(); break;    // Ржавый фургон: эквалайзер
                case 9: Ufo(); break;          // Кибер-лаванда: НЛО
            }
        }

        private static Color A(Color c, byte a) => Color.FromArgb(a, c.R, c.G, c.B);

        private static DoubleAnimation Loop(double from, double to, TimeSpan dur, double beginSec = 0,
            bool autoReverse = false)
        {
            return new DoubleAnimation(from, to, dur)
            {
                AutoReverse = autoReverse,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(beginSec),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
        }

        /// <summary>Запуск бесконечного сториборда с одной анимацией на свойстве элемента.</summary>
        private void Run(DependencyObject target, string path, DoubleAnimation a)
        {
            Storyboard.SetTarget(a, target);
            Storyboard.SetTargetProperty(a, new PropertyPath(path));
            var sb = new Storyboard();
            sb.Children.Add(a);
            sb.Begin(_host, true);
        }

        private static string ScaleXPath(bool x) =>
            "(UIElement.RenderTransform).(ScaleTransform." + (x ? "ScaleX" : "ScaleY") + ")";

        private static string TranslatePath(bool x) =>
            "(UIElement.RenderTransform).(TranslateTransform." + (x ? "X" : "Y") + ")";

        private Ellipse GlowBlob(double size, Color color)
        {
            return new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(color),
                Effect = new BlurEffect { Radius = 40 }
            };
        }

        // 0: концентрические кольца баса из центра
        private void Rings()
        {
            var c = ThemeManager.CurrentAccent;
            for (int i = 0; i < 3; i++)
            {
                var ring = new Ellipse
                {
                    Width = 220,
                    Height = 220,
                    Stroke = new SolidColorBrush(A(c, 160)),
                    StrokeThickness = 5,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform(1, 1)
                };
                _host.Children.Add(ring);
                double begin = i * 0.8;
                var st = new Storyboard();
                var sx = Loop(0.6, 2.4, TimeSpan.FromSeconds(2.4), begin);
                var sy = Loop(0.6, 2.4, TimeSpan.FromSeconds(2.4), begin);
                var op = new DoubleAnimation(0.7, 0, TimeSpan.FromSeconds(2.4))
                {
                    BeginTime = TimeSpan.FromSeconds(begin),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                Storyboard.SetTarget(sx, ring); Storyboard.SetTargetProperty(sx, new PropertyPath(ScaleXPath(true)));
                Storyboard.SetTarget(sy, ring); Storyboard.SetTargetProperty(sy, new PropertyPath(ScaleXPath(false)));
                Storyboard.SetTarget(op, ring); Storyboard.SetTargetProperty(op, new PropertyPath("Opacity"));
                st.Children.Add(sx); st.Children.Add(sy); st.Children.Add(op);
                st.Begin(_host, true);
            }
        }

        // 1: мерцающие неоновые искры
        private void Twinkle()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            for (int i = 0; i < 16; i++)
            {
                var dot = new Ellipse
                {
                    Width = 5 + Rnd.Next(6),
                    Height = 5 + Rnd.Next(6),
                    Fill = new SolidColorBrush(A(i % 2 == 0 ? c : c2, 220)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(Rnd.Next(-500, 500), Rnd.Next(-280, 280), 0, 0)
                };
                _host.Children.Add(dot);
                Run(dot, "Opacity", new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300 + Rnd.Next(500)))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(Rnd.NextDouble() * 2)
                });
            }
        }

        // 2: золотые монеты всплывают снизу вверх
        private void RisingOrbs()
        {
            var c2 = ThemeManager.CurrentAccent2;
            double h = Math.Max(_host.ActualHeight, 700);
            for (int i = 0; i < 7; i++)
            {
                var orb = new Ellipse
                {
                    Width = 12 + Rnd.Next(18),
                    Height = 12 + Rnd.Next(18),
                    Fill = new SolidColorBrush(A(c2, 200)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(Rnd.Next(-480, 480), 0, 0, -40),
                    RenderTransform = new TranslateTransform()
                };
                _host.Children.Add(orb);
                Run(orb, TranslatePath(false),
                    Loop(0, -(h + 120), TimeSpan.FromSeconds(3.5 + Rnd.NextDouble() * 3), Rnd.NextDouble() * 4));
                Run(orb, "Opacity",
                    new DoubleAnimation(0.9, 0, TimeSpan.FromSeconds(3.5 + Rnd.NextDouble() * 3))
                    {
                        RepeatBehavior = RepeatBehavior.Forever,
                        BeginTime = TimeSpan.FromSeconds(Rnd.NextDouble() * 4)
                    });
            }
        }

        // 3: мерцающее красное зарево снизу
        private void Flicker()
        {
            var c = ThemeManager.CurrentAccent;
            var glow = GlowBlob(700, A(c, 90));
            glow.HorizontalAlignment = HorizontalAlignment.Center;
            glow.VerticalAlignment = VerticalAlignment.Bottom;
            glow.Margin = new Thickness(0, 0, 0, -200);
            _host.Children.Add(glow);

            var flick = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromSeconds(1.6)
            };
            flick.KeyFrames.Add(new LinearDoubleKeyFrame(0.5, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            flick.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3))));
            flick.KeyFrames.Add(new LinearDoubleKeyFrame(0.4, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6))));
            flick.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9))));
            flick.KeyFrames.Add(new LinearDoubleKeyFrame(0.5, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.3))));
            Run(glow, "Opacity", flick);
        }

        // 4: большое солнце медленно садится и снова поднимается
        private void Sunset()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            var sun = new Ellipse
            {
                Width = 260,
                Height = 260,
                Fill = new SolidColorBrush(A(c2, 120)),
                Effect = new BlurEffect { Radius = 60 },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 40, 0, 0),
                RenderTransform = new TranslateTransform()
            };
            _host.Children.Add(sun);
            Run(sun, TranslatePath(false), Loop(0, 160, TimeSpan.FromSeconds(7), 0, true));

            var haze = GlowBlob(500, A(c, 60));
            haze.HorizontalAlignment = HorizontalAlignment.Center;
            haze.VerticalAlignment = VerticalAlignment.Center;
            _host.Children.Add(haze);
            Run(haze, "Opacity", Loop(0.4, 0.9, TimeSpan.FromSeconds(3), 0, true));
        }

        // 5: неоновая сканирующая полоса проходит сверху вниз
        private void Scanline()
        {
            var c = ThemeManager.CurrentAccent;
            double h = Math.Max(_host.ActualHeight, 700);
            var line = new Rectangle
            {
                Width = 1600,
                Height = 4,
                Fill = new SolidColorBrush(A(c, 170)),
                Effect = new BlurEffect { Radius = 12 },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                RenderTransform = new TranslateTransform()
            };
            _host.Children.Add(line);
            Run(line, TranslatePath(false), Loop(0, h, TimeSpan.FromSeconds(3.2)));
            Run(line, "Opacity", Loop(0.2, 0.9, TimeSpan.FromSeconds(1.6), 0, true));
        }

        // 6: два неоновых сгустка плывут волнами навстречу
        private void Waves()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            for (int i = 0; i < 2; i++)
            {
                var wave = new Ellipse
                {
                    Width = 900,
                    Height = 260,
                    Fill = new SolidColorBrush(A(i == 0 ? c : c2, 70)),
                    Effect = new BlurEffect { Radius = 70 },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, i == 0 ? -120 : 120, 0, 0),
                    RenderTransform = new TranslateTransform()
                };
                _host.Children.Add(wave);
                int k = i;
                Run(wave, TranslatePath(true), Loop(k == 0 ? -220 : 220, k == 0 ? 220 : -220,
                    TimeSpan.FromSeconds(5), 0, true));
                Run(wave, "Opacity", Loop(0.5, 1.0, TimeSpan.FromSeconds(2.5), k * 1.2, true));
            }
        }

        // 7: золотые капли падают сверху
        private void GoldRain()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            double h = Math.Max(_host.ActualHeight, 700);
            for (int i = 0; i < 14; i++)
            {
                var drop = new Ellipse
                {
                    Width = 4 + Rnd.Next(5),
                    Height = 10 + Rnd.Next(14),
                    Fill = new SolidColorBrush(A(i % 3 == 0 ? c2 : c, 200)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(Rnd.Next(-520, 520), -30, 0, 0),
                    RenderTransform = new TranslateTransform()
                };
                _host.Children.Add(drop);
                double dur = 2.2 + Rnd.NextDouble() * 2.5;
                Run(drop, TranslatePath(false), Loop(0, h + 60, TimeSpan.FromSeconds(dur), Rnd.NextDouble() * 4));
            }
        }

        // 8: эквалайзер — прыгающие столбики внизу
        private void Equalizer()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, -14)
            };
            for (int i = 0; i < 9; i++)
            {
                var bar = new Rectangle
                {
                    Width = 26,
                    Height = 70,
                    RadiusX = 8,
                    RadiusY = 8,
                    Fill = new SolidColorBrush(A(i % 2 == 0 ? c : c2, 110)),
                    Margin = new Thickness(7, 0, 7, 0),
                    RenderTransformOrigin = new Point(0.5, 1),
                    RenderTransform = new ScaleTransform(1, 1)
                };
                panel.Children.Add(bar);
                double dur = 0.35 + Rnd.NextDouble() * 0.5;
                Run(bar, ScaleXPath(false),
                    Loop(0.3, 1.7, TimeSpan.FromSeconds(dur), Rnd.NextDouble(), true));
            }
            _host.Children.Add(panel);
        }

        // 9: НЛО — световой сгусток плавает по экрану
        private void Ufo()
        {
            var c = ThemeManager.CurrentAccent;
            var c2 = ThemeManager.CurrentAccent2;
            var orb = GlowBlob(220, A(c, 130));
            orb.HorizontalAlignment = HorizontalAlignment.Center;
            orb.VerticalAlignment = VerticalAlignment.Center;
            orb.RenderTransform = new TranslateTransform();
            _host.Children.Add(orb);
            Run(orb, TranslatePath(true), Loop(-320, 320, TimeSpan.FromSeconds(6), 0, true));
            Run(orb, TranslatePath(false), Loop(-60, 60, TimeSpan.FromSeconds(2.6), 0, true));

            var beam = GlowBlob(320, A(c2, 70));
            beam.HorizontalAlignment = HorizontalAlignment.Center;
            beam.VerticalAlignment = VerticalAlignment.Center;
            _host.Children.Add(beam);
            Run(beam, "Opacity", Loop(0.3, 0.8, TimeSpan.FromSeconds(1.8), 0, true));
        }
    }
}
