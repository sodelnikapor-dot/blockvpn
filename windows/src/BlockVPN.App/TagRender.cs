using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace BlockVPN.App
{
    /// <summary>
    /// Отрисовка граффити-тега «Мой тег». Чистый декор: не влияет на VPN,
    /// отключается в настройках (декор) и не анимируется, если анимации выключены.
    /// </summary>
    public static class TagRender
    {
        public static readonly string[] Colors =
        {
            "#39FF14", "#FF2E88", "#00E5FF", "#FFD600", "#FF8A00", "#B388FF"
        };

        public static readonly string[] StyleNames = { "НАКЛОН", "ПРЯМО", "КОНТУР" };

        public static Color ParseColor(int index)
        {
            if (index < 0 || index >= Colors.Length) index = 0;
            return (Color)ColorConverter.ConvertFromString(Colors[index]);
        }

        public static void Configure(Border sticker, string text, int colorIndex, int styleIndex, bool glow, bool animate)
        {
            var color = ParseColor(colorIndex);
            var brush = new SolidColorBrush(color);
            sticker.Child = null;
            sticker.BorderBrush = brush;
            sticker.Background = Brushes.Transparent;
            sticker.BorderThickness = new Thickness(2);
            sticker.CornerRadius = new CornerRadius(6);
            sticker.Padding = new Thickness(14, 8, 14, 8);

            var tb = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(text) ? "TAG" : text.ToUpperInvariant(),
                FontFamily = new FontFamily("Segoe UI Black"),
                FontSize = 18,
                Foreground = brush
            };

            if (styleIndex == 0)
            {
                sticker.Background = new SolidColorBrush(Color.FromArgb(0x22, color.R, color.G, color.B));
                sticker.RenderTransform = new RotateTransform(-5);
            }
            else if (styleIndex == 1)
            {
                sticker.RenderTransform = new RotateTransform(0);
            }
            else
            {
                sticker.RenderTransform = new RotateTransform(2);
            }

            if (glow)
            {
                sticker.Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = animate ? 26 : 14,
                    ShadowDepth = 0,
                    Opacity = 0.85
                };
            }
            else
            {
                sticker.Effect = null;
            }

            sticker.Child = tb;
        }
    }
}
