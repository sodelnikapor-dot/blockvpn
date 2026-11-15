using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlockVPN.App.Views
{
    public partial class TagView : UserControl
    {
        private bool _suppress;

        public TagView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                _suppress = true;
                var prefs = AppSession.Prefs;
                TagBox.Text = prefs.TagText;
                BuildControls();
                RenderPreview(false);
                _suppress = false;
            };
        }

        private void BuildControls()
        {
            var prefs = AppSession.Prefs;

            if (ColorRow.Children.Count == 0)
            {
                for (int i = 0; i < TagRender.Colors.Length; i++)
                {
                    var idx = i;
                    var btn = new Button
                    {
                        Width = 34, Height = 34, Margin = new Thickness(0,0,8,0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Background = new SolidColorBrush(TagRender.ParseColor(i)),
                        BorderThickness = new Thickness(0)
                    };
                    btn.Template = MakeCircleTemplate();
                    btn.Click += (o, a) => { if (!_suppress) { prefs.TagColorIndex = idx; SaveAndRender(); } };
                    ColorRow.Children.Add(btn);
                }
            }

            if (StyleRow.Children.Count == 0)
            {
                for (int i = 0; i < TagRender.StyleNames.Length; i++)
                {
                    var idx = i;
                    var btn = new Button
                    {
                        Content = TagRender.StyleNames[i],
                        Style = (Style)FindResource("GhostButton"),
                        Margin = new Thickness(0,0,10,0)
                    };
                    btn.Click += (o, a) => { if (!_suppress) { prefs.TagStyleIndex = idx; SaveAndRender(); } };
                    StyleRow.Children.Add(btn);
                }
            }
        }

        private static ControlTemplate MakeCircleTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border), "bd");
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(17));
            factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            template.VisualTree = factory;
            return template;
        }

        private void TagBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppress) return;
            AppSession.Prefs.TagText = TagBox.Text;
            SaveAndRender();
        }

        private void SaveAndRender()
        {
            AppSession.SavePrefs();
            RenderPreview(AppSession.Manager.State == VpnState.Secured);
        }

        private void RenderPreview(bool glow)
        {
            var prefs = AppSession.Prefs;
            TagRender.Configure(Preview, prefs.TagText, prefs.TagColorIndex, prefs.TagStyleIndex, glow, prefs.Animations);
        }
    }
}
