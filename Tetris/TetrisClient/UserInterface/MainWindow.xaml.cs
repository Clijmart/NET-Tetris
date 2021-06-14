using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TetrisClient.GameManager;
using TetrisClient.Managers;
using TetrisClient.UserInterface;

namespace TetrisClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BoardManager Bm { get; set; }

        private TextBlock ScoreBlock { get; set; }

        public MainWindow()
        {

            InitializeComponent();

            InitGame();
        }

        /// <summary>
        /// Initializes the game by preparing the board and resetting variables.
        /// </summary>
        public void InitGame()
        {
            PrepareGrids();
            Bm = new BoardManager(this);

            DrawGrids();
        }

        public void PrepareGrids()
        {
            ScoreBlock = InterfaceManager.CreateInfoBlock(ScoreGrid, "0", 0);
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        public void EndGame()
        {
            Bm.Running = false;
            TetrisGrid.Background = (SolidColorBrush) Application.Current.TryFindResource("PlayerNotAlive");

            if (SettingManager.MusicOn)
            {
                Bm.SoundManager.StopMusic();
            }

            ResultWindow results = new(-1, Bm.Score, Bm.CalculateLevel(), Bm.LinesCleared, Bm.Time);
            results.Show();
            Close();
        }

        /// <summary>
        /// Handles the exit button event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            Bm.Timer.StopTimer();
            if (SettingManager.MusicOn)
            {
                Bm.SoundManager.StopMusic();
            }

            Menu menu = new();
            menu.Show();
            Close();
        }

        /// <summary>
        /// Handles the KeyEvents sent by the player.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the KeyEvent.</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Bm == null || !Bm.Running)
            {
                return;
            }

            if (e.IsRepeat && Bm.BlockRepeat)
            {
                return;
            }
            Bm.BlockRepeat = false;

            switch (e.Key)
            {
                case Key.Left:
                    {
                        if (Bm.CurrentBlock.MoveLeft())
                        {
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Right:
                    {
                        if (Bm.CurrentBlock.MoveRight())
                        {
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Down:
                    {
                        if (!Bm.CurrentBlock.MoveDown())
                        {
                            Bm.CurrentBlock.Place();
                        }

                        DrawGrids();
                        return;
                    }
                case Key.Up:
                    {
                        if (Bm.CurrentBlock.Rotate())
                        {
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Space:
                    {
                        Bm.CurrentBlock = Bm.GhostBlock;
                        if (!Bm.CurrentBlock.MoveDown())
                        {
                            Bm.CurrentBlock.Place();
                        }
                        DrawGrids();
                        return;
                    }
                case Key.Escape:
                    {
                        Bm.EndGame();
                        return;
                    }

                default:
                    break;
            }
        }

        /// <summary>
        /// Updates all Text.
        /// </summary>
        public void UpdateText()
        {
            Level.Text = "" + Bm.CalculateLevel();
            Lines.Text = "" + Bm.LinesCleared;
            ScoreBlock.Text = "" + Bm.Score;

            TimeSpan timeSpan = new(0, 0, Bm.Time / 10);
            Time.Text = timeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        public void DrawGrids()
        {
            TetrisGrid.Children.Clear();
            NextBlockGrid.Children.Clear();

            InterfaceManager.DrawWell(TetrisGrid, Bm.TetrisWell);
            InterfaceManager.DrawBlock(TetrisGrid, Bm.GhostBlock, true, true);
            InterfaceManager.DrawBlock(TetrisGrid, Bm.CurrentBlock, false, true);
            InterfaceManager.DrawBlock(NextBlockGrid, Bm.NextBlock, false, true);
        }
    }
}
