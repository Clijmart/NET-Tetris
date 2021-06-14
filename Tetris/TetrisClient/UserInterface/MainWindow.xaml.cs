using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using TetrisClient.GameManager;

namespace TetrisClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BoardManager Bm { get; set; }

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
            Bm = new BoardManager(this);

            DrawGrids();
        }

        /// <summary>
        /// Ends the game by clearing the board and resetting variables.
        /// </summary>
        public void EndGame()
        {
            MessageBox.Show("Game Over!");
            Menu menu = new();
            menu.Show();

            Close();
        }

        /// <summary>
        /// Handles the exit button event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            Bm.EndGame();
        }

        /// <summary>
        /// Handles the KeyEvents sent by the player.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the KeyEvent.</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
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
            Level.Text = Bm.CalculateLevel().ToString();
            Lines.Text = Bm.LinesCleared.ToString();
            Score.Text = Bm.Score.ToString();

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
