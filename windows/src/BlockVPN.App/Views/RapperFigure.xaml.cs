using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class RapperFigure : UserControl
    {
        private bool _hovering;
        private bool _idle;

        public RapperFigure()
        {
            InitializeComponent();
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            Loaded += (s, e) => RefreshAnimationState();
        }

        /// <summary>Перезапуск фоновых анимаций с учётом настроек (вызывается при смене темы/настроек).</summary>
        public void RefreshAnimationState()
        {
            if (AnimationsEnabled) StartIdle();
            else StopAll();
        }

        /// <summary>Неоновая подсветка, когда VPN в состоянии «Защищено».</summary>
        public void SetSecuredGlow(bool secured)
        {
            if (secured)
            {
                Effect = new DropShadowEffect
                {
                    Color = ThemeManager.CurrentAccent,
                    BlurRadius = 30,
                    ShadowDepth = 0,
                    Opacity = 0.55
                };
            }
            else
            {
                Effect = null;
            }
        }

        private bool AnimationsEnabled => AppSession.Prefs == null || AppSession.Prefs.Animations;

        /// <summary>Спокойное «дыхание» в покое: еле заметное покачивание и подъём.</summary>
        private void StartIdle()
        {
            if (_idle) return;
            _idle = true;

            var bob = new DoubleAnimation(0, -3, TimeSpan.FromSeconds(2.6))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            FigBob.BeginAnimation(TranslateTransform.YProperty, bob);

            var sway = new DoubleAnimation(-1, 1, TimeSpan.FromSeconds(3.4))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            FigRotate.BeginAnimation(RotateTransform.AngleProperty, sway);
        }

        private void StopIdle()
        {
            _idle = false;
            FigBob.BeginAnimation(TranslateTransform.YProperty, null);
            FigBob.Y = 0;
            FigRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            FigRotate.Angle = 0;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!AnimationsEnabled) return;
            _hovering = true;
            StopIdle();

            var sway = new DoubleAnimation(-2.5, 2.5, TimeSpan.FromSeconds(0.7))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            FigRotate.BeginAnimation(RotateTransform.AngleProperty, sway);

            var bob = new DoubleAnimation(0, -6, TimeSpan.FromSeconds(0.45))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            FigBob.BeginAnimation(TranslateTransform.YProperty, bob);

            var nod = new DoubleAnimation(0, 5, TimeSpan.FromSeconds(0.7))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            HeadRotate.BeginAnimation(RotateTransform.AngleProperty, nod);

            var grow = new DoubleAnimation(1, 1.04, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            FigScale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            FigScale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!_hovering) return;
            _hovering = false;
            StopAll();
            RefreshAnimationState();
        }

        public void StopAll()
        {
            _idle = false;
            FigRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            FigRotate.Angle = 0;
            FigBob.BeginAnimation(TranslateTransform.YProperty, null);
            FigBob.Y = 0;
            HeadRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            HeadRotate.Angle = 0;
            FigScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            FigScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            FigScale.ScaleX = 1;
            FigScale.ScaleY = 1;
        }
    }
}
