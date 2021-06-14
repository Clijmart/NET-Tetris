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
            PlacingBlock.Text = "#" + (placing + 1);

            ScoreBlock.Text = "Score: " + score;
            LevelBlock.Text = "Level: " + (level + 1);
            LinesClearedBlock.Text = "Lines: " + linesCleared;

            TimeSpan timeSpan = new(0, 0, time / 10);
            TimeBlock.Text = "Time: " + timeSpan.ToString(@"hh\:mm\:ss");

            if (SettingManager.GameSoundsOn)
            {
                new SoundPlayer(new Uri(Environment.CurrentDirectory + "/Resources/GameOver.wav", UriKind.Relative).ToString()).Play();
            }
        }

        private void ButtonMenu(object sender, RoutedEventArgs e)
        {
            Menu menu = new();
            menu.Show();
            Close();
        }
    }
}
