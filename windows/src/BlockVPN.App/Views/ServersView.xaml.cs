using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlockVPN.Core;

namespace BlockVPN.App.Views
{
    public partial class ServersView : UserControl
    {
        public ServersView()
        {
            InitializeComponent();
            AppSession.ProfilesChanged += RefreshList;
            Loaded += (s, e) => RefreshList();
        }

        private void RefreshList()
        {
            ProfilesList.ItemsSource = null;
            ProfilesList.ItemsSource = AppSession.Profiles;
            EmptyText.Visibility = AppSession.Profiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var active = AppSession.ActiveProfile();
            if (active != null)
                ProfilesList.SelectedItem = AppSession.Profiles.FirstOrDefault(p => p.Id == active.Id);
        }

        private void ProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesList.SelectedItem is VpnProfile p)
            {
                AppSession.Prefs.ActiveProfileId = p.Id;
                AppSession.SavePrefs();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AddProfileWindow { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true && dlg.Result != null)
            {
                AppSession.Profiles.Add(dlg.Result);
                AppSession.Prefs.ActiveProfileId = dlg.Result.Id;
                AppSession.SaveProfiles();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not VpnProfile p) return;
            if (MessageBox.Show("Удалить сервер «" + p.Name + "»?", "BlockVPN",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            AppSession.Profiles.Remove(p);
            AppSession.SaveProfiles();
        }
    }
}
