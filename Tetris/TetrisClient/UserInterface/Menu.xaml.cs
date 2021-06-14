using System.Windows;
using TetrisClient.Managers;

namespace TetrisClient
{
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();

            SoundButton.Content = SettingManager.MusicOn ? "🕪" : "🕨";
        }

        /// <summary>
        /// Handles the SinglePlayer Button key event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonSinglePlayer(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            Close();
        }

        /// <summary>
        /// Handles the MultiPlayer Button key event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonMultiPlayer(object sender, RoutedEventArgs e)
        {
            MultiplayerWindow multiplayerWindow = new();
            multiplayerWindow.Show();
            Close();
        }

        /// <summary>
        /// Handles the Sound Button key event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
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
