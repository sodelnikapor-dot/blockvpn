using System.Collections.Generic;
using System.Windows;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class AddProfileWindow : Window
    {
        public VpnProfile Result { get; private set; }

        public AddProfileWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var errors = WgConfigParser.Validate(ConfigBox.Text);
            if (errors.Count > 0)
            {
                ErrorText.Text = "Конфиг не принят: " + string.Join(" ", errors);
                ErrorText.Visibility = Visibility.Visible;
                return;
            }
            Result = new VpnProfile
            {
                Name = string.IsNullOrWhiteSpace(NameBox.Text) ? "Сервер " + System.DateTime.Now.ToString("HH:mm") : NameBox.Text.Trim(),
                Region = RegionBox.Text.Trim(),
                ConfigText = ConfigBox.Text
            };
            DialogResult = true;
        }
    }
}
