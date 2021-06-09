using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;

namespace TetrisClient
{
    public partial class MultiplayerWindow : Window
    {
        private HubConnection _connection;
        private Random P1Random;
        private Random P2Random;

        private bool P1Ready = false;
        private bool P2Ready = false;

        public BoardManager Bm { get; set; }

        public MultiplayerWindow()
        {
            InitializeComponent();
        }
        // Events kunnen `async` zijn in WPF:
        private async void StartGame_OnClick(object sender, RoutedEventArgs e)
        {
            // Als de connectie nog niet is geïnitialiseerd, dan kan er nog niks verstuurd worden:
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            Status.Content = "Connected!";
            int seed = Guid.NewGuid().GetHashCode();
            
            P1Random = new Random(seed);
            P1Ready = true;
            if (P2Ready)
            {
                InitGame();
            }

            // Het aanroepen van de TetrisHub.cs methode `ReadyUp`.
            // Hier geven we de int mee die de methode `ReadyUp` verwacht.
            await _connection.InvokeAsync("ReadyUp", seed);
        }
        private void GoBackButton(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
            Close();
        }
        private void ConnectButton(object sender, RoutedEventArgs e)
        {
            string url = "http://" + InputField.Text + "/TetrisHub";
            System.Diagnostics.Debug.WriteLine(url);

            try
            {
                _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

                // De eerste paramater moet gelijk zijn met de methodenaam in TetrisHub.cs
                // Wat er tussen de <..> staat bepaald wat de type van de paramater `seed` is.
                // Op deze manier loopt het onderstaande gelijk met de methode in TetrisHub.cs.
                _connection.On<int>("ReadyUp", seed =>
                {
                    // Seed van de andere client:
                    P2Random = new Random(seed);
                    MessageBox.Show(seed.ToString());
                    P2Ready = true;
                    if (P1Ready)
                    {
                        InitGame();
                    }
                });

                // Let op: het starten van de connectie moet *nadat* alle event listeners zijn gezet!
                // Als de methode waarin dit voorkomt al `async` (asynchroon) is, dan kan `Task.Run` weggehaald worden.
                // In het startersproject staat dit in de constructor, daarom is dit echter wel nodig:
                Task.Run(async () => await _connection.StartAsync());

                if (_connection.State.Equals(HubConnectionState.Connected))
                {
                    Status.Content = "Connected!";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(_connection.State);
                    Status.Content = "Can't connect to given IP";
                }

            }
            catch (Exception er)
            {
                Status.Content = "Not a valid IP";
            }
        }

        /// <summary>
        /// Initializes the game by preparing the board and resetting variables.
        /// </summary>
        public void InitGame()
        {
            Bm = new BoardManager(this);
            string url = "http://127.0.0.1:5000/TetrisHub";
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

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
            if (Bm == null) return;
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
            TetrisGridP1.Children.Clear();
            TetrisGridP2.Children.Clear();
            NextBlockGrid.Children.Clear();
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        public void DrawGrids()
        {
            //string tetrisWellString = BlockManager.ColorArrayToString(Bm.TetrisWell);
            SolidColorBrush[,] temp = Bm.TetrisWell;
            _connection.On<object>("UpdateWell", temp =>
            {
                MessageBox.Show(temp.ToString());
            });
            ClearGrids();

            // Fill the grid by looping through the tetris well.
            for (int i = 0; i < Bm.TetrisWell.GetLength(0); i++)
            {
                for (int j = 0; j < Bm.TetrisWell.GetLength(1); j++)
                {
                    Rectangle rectangle = CreateRectangle(Bm.TetrisWell[i, j], null);

                    TetrisGridP1.Children.Add(rectangle);
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

                    TetrisGridP1.Children.Add(rectangle);
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

                    TetrisGridP1.Children.Add(rectangle);
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
