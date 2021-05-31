using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        void InitGame(BoardManager bm)
        {
            DrawBlocks(bm);
        }

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    {
                        bm.SelectNextBlock();
                        DrawBlocks(bm);
                        return;
                    }
                case Key.Left:
                    {
                        bm.currentBlock.MoveLeft();
                        DrawBlocks(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Right:
                    {
                        bm.currentBlock.MoveRight();
                        DrawBlocks(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Up:
                    {
                        bm.currentBlock.Rotate();
                        DrawBlocks(bm);
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        return;
                    }
                case Key.Down:
                    {
                        System.Diagnostics.Debug.WriteLine(e.Key);
                        bm.currentBlock.Place(bm.tetrisWell);
                        DrawBlocks(bm);
                        return;
                    }
            }
        }
        void ClearGrid()
        {
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 10; j++)
                {

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = Brushes.White, // Achtergrondkleur
                    };

                    TetrisGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de 
                    Grid.SetRow(rectangle, i); // Zet de rij
                    Grid.SetColumn(rectangle, j); // Zet de kolom
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = Brushes.White, // Achtergrondkleur
                    };

                    NextBlockGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de 
                    Grid.SetRow(rectangle, i); // Zet de rij
                    Grid.SetColumn(rectangle, j); // Zet de kolom
                }
            }
        }
        void DrawBlocks(BoardManager bm)
        {
            ClearGrid();

            for (int i = 0; i < bm.tetrisWell.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < bm.tetrisWell.GetUpperBound(1) + 1; j++)
                {
                    if (bm.tetrisWell[i, j] != 1) continue;

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = Brushes.Red, // Achtergrondkleur
                    };

                    TetrisGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de 
                    Grid.SetRow(rectangle, i); // Zet de rij
                    Grid.SetColumn(rectangle, j); // Zet de kolom
                }
            }

            int offsetY = bm.currentBlock.yCord;
            int offsetX = bm.currentBlock.xCord;
            int[,] currentBlock = bm.currentBlock.shape.Value;
            int[,] nextBlock = bm.nextBlock.shape.Value;
            for (int i = 0; i < currentBlock.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.GetLength(1); j++)
                {
                    // Als de waarde niet gelijk is aan 1,
                    // dan hoeft die niet getekent te worden:
                    if (currentBlock[i, j] != 1) continue;

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = bm.currentBlock.color, // Achtergrondkleur
                    };

                    TetrisGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de Grid
                    Grid.SetRow(rectangle, i + offsetY); // Zet de rij
                    Grid.SetColumn(rectangle, j + offsetX); // Zet de kolom
                }
            }
            for (int i = 0; i < nextBlock.GetLength(0); i++)
            {
                for (int j = 0; j < nextBlock.GetLength(1); j++)
                {
                    // Als de waarde niet gelijk is aan 1,
                    // dan hoeft die niet getekent te worden:
                    if (nextBlock[i, j] != 1) continue;

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = bm.nextBlock.color, // Achtergrondkleur
                    };

                    NextBlockGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de Grid
                    Grid.SetRow(rectangle, i); // Zet de rij
                    Grid.SetColumn(rectangle, j); // Zet de kolom
                }
            }
        }
    }
}
