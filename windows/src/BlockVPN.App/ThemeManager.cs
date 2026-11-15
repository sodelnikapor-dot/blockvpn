using System.Windows;
using System.Windows.Media;

namespace BlockVPN.App
{
    /// <summary>Три уличные палитры. Меняются в рантайме через динамические ресурсы.</summary>
    public static class ThemeManager
    {
        public struct Palette
        {
            public string Name;
            public Color Background;
            public Color Panel;
            public Color Card;
            public Color Accent;
            public Color Accent2;
            public Color Text;
            public Color Dim;
        }

        public static readonly Palette[] Palettes = new Palette[]
        {
            new Palette
            {
                Name = "Ночной район",
                Background = Color.FromRgb(0x0F,0x0F,0x12),
                Panel = Color.FromRgb(0x17,0x17,0x1C),
                Card = Color.FromRgb(0x1C,0x1C,0x23),
                Accent = Color.FromRgb(0x39,0xFF,0x14),
                Accent2 = Color.FromRgb(0xFF,0xD6,0x00),
                Text = Color.FromRgb(0xF2,0xF2,0xF2),
                Dim = Color.FromRgb(0x9A,0x9A,0xA5)
            },
            new Palette
            {
                Name = "Неоновый малл",
                Background = Color.FromRgb(0x12,0x12,0x1A),
                Panel = Color.FromRgb(0x1A,0x1A,0x26),
                Card = Color.FromRgb(0x20,0x20,0x2E),
                Accent = Color.FromRgb(0xFF,0x2E,0x88),
                Accent2 = Color.FromRgb(0x00,0xE5,0xFF),
                Text = Color.FromRgb(0xF5,0xF0,0xF7),
                Dim = Color.FromRgb(0x9E,0x96,0xAB)
            },
            new Palette
            {
                Name = "Ржавый фургон",
                Background = Color.FromRgb(0x17,0x13,0x0F),
                Panel = Color.FromRgb(0x20,0x1A,0x14),
                Card = Color.FromRgb(0x27,0x20,0x18),
                Accent = Color.FromRgb(0xFF,0x8A,0x00),
                Accent2 = Color.FromRgb(0xFF,0xE0,0x82),
                Text = Color.FromRgb(0xF7,0xF2,0xEA),
                Dim = Color.FromRgb(0xA8,0x9C,0x8C)
            }
        };

        public static Color CurrentAccent { get; private set; } = Palettes[0].Accent;
        public static Color CurrentDanger { get; private set; } = Color.FromRgb(0xFF,0x3B,0x3B);
        public static Color CurrentNeutral { get; private set; } = Color.FromRgb(0x8A,0x8A,0x95);

        public static void Apply(int index)
        {
            if (index < 0 || index >= Palettes.Length) index = 0;
            var p = Palettes[index];
            var res = Application.Current.Resources;
            Set(res, "BgBrush", p.Background);
            Set(res, "PanelBrush", p.Panel);
            Set(res, "CardBrush", p.Card);
            Set(res, "AccentBrush", p.Accent);
            Set(res, "Accent2Brush", p.Accent2);
            Set(res, "TextBrush", p.Text);
            Set(res, "DimBrush", p.Dim);
            CurrentAccent = p.Accent;
        }

        private static void Set(ResourceDictionary res, string key, Color c)
        {
            res[key] = new SolidColorBrush(c);
        }
    }
}
