using System.Windows;
using TetrisClient.Managers;

namespace TetrisClient
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();

            SoundButton.Content = SettingManager.MusicOn ? "🕪" : "🕨";
        }
        private void ButtonSinglePlayer(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        private void ButtonMultiPlayer(object sender, RoutedEventArgs e)
        {
            MultiplayerWindow multiplayerWindow = new MultiplayerWindow();
            multiplayerWindow.Show();
            Close();
        }
        private void ButtonSound(object sender, RoutedEventArgs e)
        {
            if ((bool) SoundButton.IsChecked)
            {
                SettingManager.MusicOn = true;
                SoundButton.Content = "🕪";
            }
            else
            {
                SettingManager.MusicOn = false;
                SoundButton.Content = "🕨";
            }
        }
    }
}
