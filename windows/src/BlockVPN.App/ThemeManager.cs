using System.Windows;
using System.Windows.Media;

namespace BlockVPN.App
{
    /// <summary>Уличные палитры. Меняются в рантайме через динамические ресурсы.
    /// У каждой темы своя «фишка» — эмблема сцены на главном экране.
    /// Темы названы по городам и жанрам сцен (не по именам реальных артистов —
    /// их образы и бренды копировать нельзя).</summary>
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
            public string FlairEmoji;
            public string FlairName;
        }

        public static readonly Palette[] Palettes = new Palette[]
        {
            new Palette
            {
                Name = "Ночной район",
                Background = Color.FromRgb(0x0B,0x0B,0x10),
                Panel = Color.FromRgb(0x15,0x15,0x1C),
                Card = Color.FromRgb(0x1B,0x1B,0x24),
                Accent = Color.FromRgb(0x39,0xFF,0x14),
                Accent2 = Color.FromRgb(0xFF,0xD6,0x00),
                Text = Color.FromRgb(0xF2,0xF2,0xF2),
                Dim = Color.FromRgb(0x9A,0x9A,0xA5),
                FlairEmoji = "🎤",
                FlairName = "МИКРОФОН"
            },
            new Palette
            {
                Name = "Неоновый малл",
                Background = Color.FromRgb(0x10,0x0C,0x18),
                Panel = Color.FromRgb(0x18,0x12,0x24),
                Card = Color.FromRgb(0x20,0x18,0x30),
                Accent = Color.FromRgb(0xFF,0x2E,0x88),
                Accent2 = Color.FromRgb(0x00,0xE5,0xFF),
                Text = Color.FromRgb(0xF5,0xF0,0xF7),
                Dim = Color.FromRgb(0x9E,0x96,0xAB),
                FlairEmoji = "💎",
                FlairName = "БРИЛЛИАНТ"
            },
            new Palette
            {
                Name = "Атланта-трэп",
                Background = Color.FromRgb(0x0D,0x0A,0x16),
                Panel = Color.FromRgb(0x16,0x11,0x22),
                Card = Color.FromRgb(0x1E,0x17,0x2E),
                Accent = Color.FromRgb(0xA8,0x55,0xF7),
                Accent2 = Color.FromRgb(0xFF,0xD6,0x00),
                Text = Color.FromRgb(0xF4,0xF0,0xFA),
                Dim = Color.FromRgb(0xA0,0x97,0xB0),
                FlairEmoji = "💰",
                FlairName = "ПАЧКА"
            },
            new Palette
            {
                Name = "Дриль-ночь",
                Background = Color.FromRgb(0x0C,0x0A,0x0B),
                Panel = Color.FromRgb(0x16,0x12,0x13),
                Card = Color.FromRgb(0x1E,0x18,0x1A),
                Accent = Color.FromRgb(0xFF,0x1E,0x3C),
                Accent2 = Color.FromRgb(0xE8,0xE8,0xE8),
                Text = Color.FromRgb(0xF5,0xF2,0xF2),
                Dim = Color.FromRgb(0xA5,0x9C,0x9E),
                FlairEmoji = "🔥",
                FlairName = "ОГОНЬ"
            },
            new Palette
            {
                Name = "Вест-коаст закат",
                Background = Color.FromRgb(0x0A,0x0E,0x16),
                Panel = Color.FromRgb(0x12,0x17,0x22),
                Card = Color.FromRgb(0x18,0x1F,0x2C),
                Accent = Color.FromRgb(0x2E,0x9B,0xFF),
                Accent2 = Color.FromRgb(0xFF,0x8A,0x00),
                Text = Color.FromRgb(0xF0,0xF4,0xFA),
                Dim = Color.FromRgb(0x97,0xA2,0xB2),
                FlairEmoji = "🌴",
                FlairName = "ПАЛЬМА"
            },
            new Palette
            {
                Name = "Мемфис-кассета",
                Background = Color.FromRgb(0x0B,0x0E,0x0A),
                Panel = Color.FromRgb(0x13,0x17,0x11),
                Card = Color.FromRgb(0x1A,0x1F,0x17),
                Accent = Color.FromRgb(0x9D,0xFF,0x00),
                Accent2 = Color.FromRgb(0xB3,0x6B,0xFF),
                Text = Color.FromRgb(0xF2,0xF6,0xEE),
                Dim = Color.FromRgb(0x9C,0xA5,0x96),
                FlairEmoji = "📼",
                FlairName = "КАССЕТА"
            },
            new Palette
            {
                Name = "Майами-бас",
                Background = Color.FromRgb(0x0E,0x0A,0x14),
                Panel = Color.FromRgb(0x17,0x11,0x21),
                Card = Color.FromRgb(0x1F,0x17,0x2D),
                Accent = Color.FromRgb(0xFF,0x5C,0xA8),
                Accent2 = Color.FromRgb(0x00,0xFF,0xC6),
                Text = Color.FromRgb(0xF6,0xF0,0xF8),
                Dim = Color.FromRgb(0xA3,0x98,0xAE),
                FlairEmoji = "🐊",
                FlairName = "КРОКОДИЛ"
            },
            new Palette
            {
                Name = "Бруклин-голд",
                Background = Color.FromRgb(0x0D,0x0B,0x08),
                Panel = Color.FromRgb(0x17,0x14,0x0E),
                Card = Color.FromRgb(0x1F,0x1B,0x13),
                Accent = Color.FromRgb(0xFF,0xC4,0x00),
                Accent2 = Color.FromRgb(0xFF,0x3B,0x3B),
                Text = Color.FromRgb(0xF7,0xF4,0xEC),
                Dim = Color.FromRgb(0xA7,0xA0,0x92),
                FlairEmoji = "👑",
                FlairName = "КОРОНА"
            },
            new Palette
            {
                Name = "Ржавый фургон",
                Background = Color.FromRgb(0x14,0x10,0x0C),
                Panel = Color.FromRgb(0x1F,0x18,0x12),
                Card = Color.FromRgb(0x27,0x1F,0x17),
                Accent = Color.FromRgb(0xFF,0x8A,0x00),
                Accent2 = Color.FromRgb(0x00,0xD9,0xFF),
                Text = Color.FromRgb(0xF7,0xF2,0xEA),
                Dim = Color.FromRgb(0xA8,0x9C,0x8C),
                FlairEmoji = "📻",
                FlairName = "БУМБОКС"
            },
            new Palette
            {
                Name = "Кибер-лаванда",
                Background = Color.FromRgb(0x0C,0x0A,0x14),
                Panel = Color.FromRgb(0x14,0x11,0x20),
                Card = Color.FromRgb(0x1B,0x17,0x2B),
                Accent = Color.FromRgb(0xB3,0x6B,0xFF),
                Accent2 = Color.FromRgb(0x00,0xFF,0xC6),
                Text = Color.FromRgb(0xF3,0xF0,0xFA),
                Dim = Color.FromRgb(0xA2,0x9B,0xB5),
                FlairEmoji = "🛸",
                FlairName = "ТАРЕЛКА"
            }
        };

        public static Color CurrentAccent { get; private set; } = Palettes[0].Accent;
        public static Color CurrentAccent2 { get; private set; } = Palettes[0].Accent2;
        public static Color CurrentBg1 { get; private set; } = Palettes[0].Background;
        public static Color CurrentBg2 { get; private set; } = Darken(Palettes[0].Background, 0.45);
        public static Color CurrentDanger { get; private set; } = Color.FromRgb(0xFF,0x3B,0x3B);
        public static Color CurrentNeutral { get; private set; } = Color.FromRgb(0x8A,0x8A,0x95);
        public static string CurrentFlairEmoji { get; private set; } = Palettes[0].FlairEmoji;
        public static string CurrentFlairName { get; private set; } = Palettes[0].FlairName;

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
            SetColor(res, "AccentColor", p.Accent);
            SetColor(res, "Accent2Color", p.Accent2);
            CurrentAccent = p.Accent;
            CurrentAccent2 = p.Accent2;
            CurrentFlairEmoji = p.FlairEmoji;
            CurrentFlairName = p.FlairName;
            CurrentBg1 = Blend(p.Background, p.Accent, 0.07);
            CurrentBg2 = Darken(p.Background, 0.45);
        }

        /// <summary>Смешивание двух цветов (t — доля второго).</summary>
        public static Color Blend(Color a, Color b, double t)
        {
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }

        private static Color Darken(Color c, double k)
        {
            return Color.FromRgb((byte)(c.R * k), (byte)(c.G * k), (byte)(c.B * k));
        }

        private static void Set(ResourceDictionary res, string key, Color c)
        {
            res[key] = new SolidColorBrush(c);
        }

        private static void SetColor(ResourceDictionary res, string key, Color c)
        {
            res[key] = c;
        }
    }
}
