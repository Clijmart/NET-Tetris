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
        BoardManager bm = new BoardManager();

        public MainWindow()
        {
            InitializeComponent();

            //ToDo: Voeg soundtrack toe
            
            /* SoundPlayer player = new SoundPlayer("C:/Users/ieman/source/repos/practicum-5-net-2020-ex-gamechane-engineers/Tetris/TetrisClient/resources/Tetris_theme.wav");
            player.Load();
            player.Volume = 10;
            player.Play();

            bool soundFinished = true;
            if (soundFinished)
            {
                soundFinished = false;
                Task.Factory.StartNew(() => { player.PlaySync(); soundFinished = true; });
            }*/

            InitGame(bm);
        }

        /// <summary>
        /// Initializes the game by preparing the board and resetting variables.
        /// </summary>
        /// <param name="bm">The BoardManager used in this game.</param>
        void InitGame(BoardManager bm)
        {
            DrawGrids(bm);
        }

        /// <summary>
        /// Handles the KeyEvents sent by the player.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="KeyEventArgs">The Arguments that are sent with the KeyEvent.</param>
        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    {
                        bm.currentBlock.MoveLeft();
                        DrawGrids(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Right:
                    {
                        bm.currentBlock.MoveRight();
                        DrawGrids(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Down:
                    {
                        bm.currentBlock.MoveDown();
                        DrawGrids(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Up:
                    {
                        bm.currentBlock.Rotate();
                        DrawGrids(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Space:
                    {
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        bm.currentBlock.Place(bm.tetrisWell);
                        bm.SelectNextBlock();
                        DrawGrids(bm);
                        return;
                    }
            }
        }

        /// <summary>
        /// Clears all Block Grids.
        /// </summary>
        void ClearGrids()
        {
            TetrisGrid.Children.Clear();
            NextBlockGrid.Children.Clear();
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        /// <param name="bm">The BoardManager used to draw the grid.</param>
        void DrawGrids(BoardManager bm)
        {
            ClearGrids();

            // Fill the grid by looping through the tetris well.
            for (int i = 0; i < bm.tetrisWell.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < bm.tetrisWell.GetUpperBound(1) + 1; j++)
                {
                    Rectangle rectangle = createRectangle(bm.tetrisWell[i, j]);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }

            // Add the current block to the grid.
            int offsetY = bm.currentBlock.yCord;
            int offsetX = bm.currentBlock.xCord;
            int[,] currentBlock = bm.currentBlock.shape.Value;
            for (int i = 0; i < currentBlock.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.GetLength(1); j++)
                {
                    if (currentBlock[i, j] != 1) continue;

                    Rectangle rectangle = createRectangle(bm.currentBlock.color);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i + offsetY);
                    Grid.SetColumn(rectangle, j + offsetX);
                }
            }

            // Add the next block to the grid.
            int[,] nextBlock = bm.nextBlock.shape.Value;
            for (int i = 0; i < nextBlock.GetLength(0); i++)
            {
                for (int j = 0; j < nextBlock.GetLength(1); j++)
                {
                    if (nextBlock[i, j] != 1) continue;

                    Rectangle rectangle = createRectangle(bm.nextBlock.color);

                    NextBlockGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        /// <summary>
        /// Creates a Rectangle that can be used to draw a cell in the grid.
        /// </summary>
        /// <param name="color">The color used to draw the Rectangle.</param>
        private Rectangle createRectangle(SolidColorBrush color)
        {
            Rectangle rectangle = new Rectangle()
            {
                Width = 25,
                Height = 25,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Fill = color,
            };

            return rectangle;
        }
    }
}
