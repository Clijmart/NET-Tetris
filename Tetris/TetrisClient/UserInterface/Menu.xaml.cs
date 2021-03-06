using System;
using System.Media;
using System.Windows;
using TetrisClient.Managers;

namespace TetrisClient
{
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();

            // Sets the correct button icon depending on the saved setting.
            MusicButton.Content = SettingManager.MusicOn ? "🎜" : "𝄽";
            SoundButton.Content = SettingManager.GameSoundsOn ? "🕪" : "🕨";
        }

        /// <summary>
        /// Handles the SinglePlayer Button event.
        /// </summary>
        /// <param name="sender">The sender of the Event.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonSinglePlayer(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            Close();
        }

        /// <summary>
        /// Handles the MultiPlayer Button event.
        /// </summary>
        /// <param name="sender">The sender of the Event.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonMultiPlayer(object sender, RoutedEventArgs e)
        {
            MultiplayerWindow multiplayerWindow = new();
            multiplayerWindow.Show();
            Close();
        }

        /// <summary>
        /// Handles the Sound Button event.
        /// </summary>
        /// <param name="sender">The sender of the Event.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonSound(object sender, RoutedEventArgs e)
        {
            if ((bool) SoundButton.IsChecked)
            {
                SettingManager.GameSoundsOn = true;
                SoundButton.Content = "🕪";
            }
            else
            {
                SettingManager.GameSoundsOn = false;
                SoundButton.Content = "🕨";
            }
        }

        /// <summary>
        /// Handles the Music Button event.
        /// </summary>
        /// <param name="sender">The sender of the Event.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ButtonMusic(object sender, RoutedEventArgs e)
        {
            if ((bool) MusicButton.IsChecked)
            {
                SettingManager.MusicOn = true;
                MusicButton.Content = "🎜";
            }
            else
            {
                SettingManager.MusicOn = false;
                MusicButton.Content = "𝄽";
            }
        }
    }
}
