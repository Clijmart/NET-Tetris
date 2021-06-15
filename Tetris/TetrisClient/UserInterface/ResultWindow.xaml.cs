using System;
using System.Media;
using System.Windows;
using TetrisClient.Managers;

namespace TetrisClient.UserInterface
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        public ResultWindow(int placing, long score, int level, int linesCleared, int time)
        {
            InitializeComponent();

            if (placing == -1)
            {
                PlacingBlock.Visibility = Visibility.Hidden;
            }
            PlacingBlock.Text = "You placed #" + (placing + 1);

            ScoreBlock.Text = score.ToString();
            LevelBlock.Text = (level + 1).ToString();
            LinesBlock.Text = linesCleared.ToString();

            TimeBlock.Text = new TimeSpan(0, 0, time / 10).ToString(@"hh\:mm\:ss");

            if (SettingManager.GameSoundsOn)
            {
                new SoundPlayer(new Uri(Environment.CurrentDirectory + "/Resources/GameOver.wav", UriKind.Relative).ToString()).Play();
            }
        }

        /// <summary>
        /// Handles the Menu Button key event.
        /// </summary>
        /// <param name="sender">The sender of the Event.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void MenuButtonMethod(object sender, RoutedEventArgs e)
        {
            Menu menu = new();
            menu.Show();
            Close();
        }
    }
}
