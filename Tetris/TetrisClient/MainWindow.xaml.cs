using System;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
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
        public BoardManager bm { get; set; }

        public MainWindow()
        {

            InitializeComponent();

            //ToDo: Voeg soundtrack toe
            /*
            SoundPlayer player = new SoundPlayer("C:/Users/martc/OneDrive/Documents/HBO_ICT/NET/Projecten/Practicum5/practicum-5-net-2020-ex-gamechane-engineers/Tetris/TetrisClient/resources/Tetris_theme.wav");
            player.Load();
            player.Play();

            bool soundFinished = true;
            if (soundFinished)
            {
                soundFinished = false;
                Task.Factory.StartNew(() => { player.PlaySync(); soundFinished = true; });
            }
            */
            InitGame(bm);
        }

        /// <summary>
        /// Initializes the game by preparing the board and resetting variables.
        /// </summary>
        /// <param name="bm">The BoardManager used in this game.</param>
        void InitGame(BoardManager bm)
        {
            this.bm = new BoardManager(this);

            DrawGrids();
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
                        Block tempBlock = (Block) bm.currentBlock.Clone();
                        tempBlock.MoveLeft();
                        if (BlockManager.CanMove(bm.tetrisWell, tempBlock))
                        {
                            bm.currentBlock.MoveLeft();
                            bm.ghostBlock = bm.currentBlock.CalculateGhost();
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Right:
                    {
                        Block tempBlock = (Block) bm.currentBlock.Clone();
                        tempBlock.MoveRight();
                        if (BlockManager.CanMove(bm.tetrisWell, tempBlock))
                        {
                            bm.currentBlock.MoveRight();
                            bm.ghostBlock = bm.currentBlock.CalculateGhost();
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Down:
                    {
                        if (!bm.currentBlock.MoveDown())
                        {
                            bm.currentBlock.Place();
                        }

                        DrawGrids();
                        return;
                    }
                case Key.Up:
                    {
                        Block tempBlock = (Block)bm.currentBlock.Clone();
                        tempBlock.Rotate();
                        if (BlockManager.CanMove(bm.tetrisWell, tempBlock))
                        {
                            bm.currentBlock.Rotate();
                            bm.ghostBlock = bm.currentBlock.CalculateGhost();
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Space:
                    {
                        bm.currentBlock = bm.ghostBlock;
                        if (!bm.currentBlock.MoveDown())
                        {
                            bm.currentBlock.Place();
                        }
                        DrawGrids();
                        return;
                    }
            }
        }

        public void UpdateText()
        {
            Score.Text = bm.score.ToString();
            Time.Text = (bm.time / 10).ToString();
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
            for (int i = 0; i < bm.tetrisWell.GetLength(0); i++)
            {
                for (int j = 0; j < bm.tetrisWell.GetLength(1); j++)
                {
                    Rectangle rectangle = createRectangle(bm.tetrisWell[i, j], null);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }

            // Add the ghost block to the grid.
            int ghostY = bm.ghostBlock.yCord;
            int ghostX = bm.ghostBlock.xCord;
            int[,] ghostBlock = bm.ghostBlock.shape.Value;
            for (int i = 0; i < ghostBlock.GetLength(0); i++)
            {
                for (int j = 0; j < ghostBlock.GetLength(1); j++)
                {
                    if (ghostBlock[i, j] != 1) continue;

                    Rectangle rectangle = createRectangle(null, bm.ghostBlock.color);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i + ghostY);
                    Grid.SetColumn(rectangle, j + ghostX);
                }
            }

            // Add the current block to the grid.
            int currentY = bm.currentBlock.yCord;
            int currentX = bm.currentBlock.xCord;
            int[,] currentBlock = bm.currentBlock.shape.Value;
            for (int i = 0; i < currentBlock.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.GetLength(1); j++)
                {
                    if (currentBlock[i, j] != 1) continue;

                    Rectangle rectangle = createRectangle(bm.currentBlock.color, null);

                    TetrisGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i + currentY);
                    Grid.SetColumn(rectangle, j + currentX);
                }
            }

            // Add the next block to the grid.
            int[,] nextBlock = bm.nextBlock.shape.Value;
            for (int i = 0; i < nextBlock.GetLength(0); i++)
            {
                for (int j = 0; j < nextBlock.GetLength(1); j++)
                {
                    if (nextBlock[i, j] != 1) continue;

                    Rectangle rectangle = createRectangle(bm.nextBlock.color, null);

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
        /// <returns>A new Rectangle with the given color.</returns>
        private Rectangle createRectangle(SolidColorBrush fill, SolidColorBrush border)
        {
            if (border == null)
            {
                border = Brushes.White;
            }
            Rectangle rectangle = new Rectangle()
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
