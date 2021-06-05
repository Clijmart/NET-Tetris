using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
            Bm.EndGame();
        }

        /// <summary>
        /// Handles the KeyEvents sent by the player.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="KeyEventArgs">The Arguments that are sent with the KeyEvent.</param>
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
                        Block tempBlock = Bm.CurrentBlock.Clone();
                        tempBlock.MoveLeft();
                        if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
                        {
                            Bm.CurrentBlock.MoveLeft();
                            Bm.GhostBlock = Bm.CurrentBlock.CalculateGhost();
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Right:
                    {
                        Block tempBlock = Bm.CurrentBlock.Clone();
                        tempBlock.MoveRight();
                        if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
                        {
                            Bm.CurrentBlock.MoveRight();
                            Bm.GhostBlock = Bm.CurrentBlock.CalculateGhost();
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
                        Block tempBlock = Bm.CurrentBlock.Clone();
                        tempBlock.Rotate();
                        if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
                        {
                            Bm.CurrentBlock.Rotate();
                            Bm.GhostBlock = Bm.CurrentBlock.CalculateGhost();
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
                        EndGame();
                        InitGame();
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
        /// Clears all Block Grids.
        /// </summary>
        public void ClearGrids()
        {
            TetrisGrid.Children.Clear();
            NextBlockGrid.Children.Clear();
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        public void DrawGrids()
        {
            ClearGrids();

            // Fill the grid by looping through the tetris well.
            for (int i = 0; i < Bm.TetrisWell.GetLength(0); i++)
            {
                for (int j = 0; j < Bm.TetrisWell.GetLength(1); j++)
                {
                    Rectangle rectangle = CreateRectangle(Bm.TetrisWell[i, j], null);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }

            // Add the ghost block to the grid.
            int ghostY = Bm.GhostBlock.Y;
            int ghostX = Bm.GhostBlock.X;
            int[,] ghostBlock = Bm.GhostBlock.Shape.Value;
            for (int i = 0; i < ghostBlock.GetLength(0); i++)
            {
                for (int j = 0; j < ghostBlock.GetLength(1); j++)
                {
                    if (ghostBlock[i, j] != 1)
                    {
                        continue;
                    }

                    Rectangle rectangle = CreateRectangle(null, Bm.GhostBlock.Color);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i + ghostY);
                    Grid.SetColumn(rectangle, j + ghostX);
                }
            }

            // Add the current block to the grid.
            int currentY = Bm.CurrentBlock.Y;
            int currentX = Bm.CurrentBlock.X;
            int[,] currentBlock = Bm.CurrentBlock.Shape.Value;
            for (int i = 0; i < currentBlock.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.GetLength(1); j++)
                {
                    if (currentBlock[i, j] != 1)
                    {
                        continue;
                    }

                    Rectangle rectangle = CreateRectangle(Bm.CurrentBlock.Color, null);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i + currentY);
                    Grid.SetColumn(rectangle, j + currentX);
                }
            }

            // Add the next block to the grid.
            int[,] nextBlock = Bm.NextBlock.Shape.Value;
            for (int i = 0; i < nextBlock.GetLength(0); i++)
            {
                for (int j = 0; j < nextBlock.GetLength(1); j++)
                {
                    if (nextBlock[i, j] != 1)
                    {
                        continue;
                    }

                    Rectangle rectangle = CreateRectangle(Bm.NextBlock.Color, null);

                    NextBlockGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        /// <summary>
        /// Creates a Rectangle that can be used to draw a cell in the grid.
        /// </summary>
        /// <param name="fill">The color used to fill the Rectangle.</param>
        /// <param name="border">The color used for the border of the Rectangle.</param>
        /// <returns>A new Rectangle with the given colors.</returns>
        private static Rectangle CreateRectangle(SolidColorBrush fill, SolidColorBrush border)
        {
            if (border == null)
            {
                border = Brushes.White;
            }

            Rectangle rectangle = new()
            {
                Width = 25,
                Height = 25,
                Stroke = border,
                StrokeThickness = 1,
                Fill = fill,
            };

            return rectangle;
        }
    }
}
