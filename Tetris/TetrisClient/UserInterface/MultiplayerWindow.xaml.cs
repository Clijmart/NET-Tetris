using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TetrisClient.Objects;

namespace TetrisClient
{
    public partial class MultiplayerWindow : Window
    {
        private HubConnection _connection;
        public Player MainPlayer;

        public Random RandomSeeded;

        public Grid TetrisGrid;

        private int Seed;


        public BoardManager Bm { get; set; }

        public MultiplayerWindow()
        {
            InitializeComponent();
            ReadyUpButton.Visibility = Visibility.Hidden;
            PlayersText.Visibility = Visibility.Hidden;

            MainPlayer = Player.FindPlayer(Guid.NewGuid());
            UpdatePlayersText();
        }

        private void StartGame_OnClick(object sender, RoutedEventArgs e)
        {
            // Als de connectie nog niet is geïnitialiseerd, dan kan er nog niks verstuurd worden:
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            if (Seed == 0)
            {
                Seed = new Random().Next();
            }
            RandomSeeded = new Random(Seed);

            MainPlayer.Ready = true;
            UpdatePlayersText();

            // Het aanroepen van de TetrisHub.cs methode `ReadyUp`.
            // Hier geven we de int mee die de methode `ReadyUp` verwacht.
            _connection.InvokeAsync("SendStatus", new object[] { MainPlayer.PlayerID, MainPlayer.Ready, Seed });

            if (Player.AllReady())
            {
                InitGame();
            }
        }

        private void GoBackButton(object sender, RoutedEventArgs e)
        {
           if (Bm != null)
            {
                Bm.EndGame();

            }
            else
            {
                Menu menu = new Menu();
                menu.Show();

                Close();
            }
        }

        private async void ConnectButtonMethod(object sender, RoutedEventArgs e)
        {
            string url = "http://" + InputField.Text + "/TetrisHub";
            System.Diagnostics.Debug.WriteLine(url);

            try
            {
                _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect()
                .Build();

                _connection.On<Guid>("Join", playerID =>
                {
                    Player p = Player.FindPlayer(playerID);

                    Application.Current.Dispatcher.Invoke(delegate() {
                        UpdatePlayersText();
                    });

                    _connection.InvokeAsync("SendStatus", new object[] { MainPlayer.PlayerID, MainPlayer.Ready, Seed });
                });

                _connection.On<object[]>("SendStatus", message =>
                {
                    Player p = Player.FindPlayer(Guid.Parse((string) message[0]));
                    p.Ready = (bool) message[1];

                    Application.Current.Dispatcher.Invoke(delegate() {
                        UpdatePlayersText();
                    });

                    int seed = (int) ((long) message[2] % int.MaxValue);
                    if (seed != 0)
                    {
                        Seed = seed;
                    }

                    if (Player.AllReady())
                    {
                        InitGame();
                    }
                });

                // De eerste paramater moet gelijk zijn met de methodenaam in TetrisHub.cs
                // Wat er tussen de <..> staat bepaald wat de type van de paramater `seed` is.
                // Op deze manier loopt het onderstaande gelijk met de methode in TetrisHub.cs.


                await Task.Run(async () => await _connection.StartAsync());
                // Let op: het starten van de connectie moet *nadat* alle event listeners zijn gezet!
                // Als de methode waarin dit voorkomt al `async` (asynchroon) is, dan kan `Task.Run` weggehaald worden.
                // In het startersproject staat dit in de constructor, daarom is dit echter wel nodig:

                if (_connection.State.Equals(HubConnectionState.Connected))
                {
                    Status.Content = "Connected!";
                    ReadyUpButton.Visibility = Visibility.Visible;
                    PlayersText.Visibility = Visibility.Visible;
                    await _connection.InvokeAsync("Join", MainPlayer.PlayerID);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(_connection.State);
                    Status.Content = "Can't connect to given IP";
                }

            }
            catch (Exception er)
            {
                System.Diagnostics.Debug.WriteLine(er);
                Status.Content = "Not a valid IP";
            }
        }

        /// <summary>
        /// Initializes the game by preparing the board and resetting variables.
        /// </summary>
        public void InitGame()
        {
            Action action = () =>
            {
                foreach (Player p in Player.GetPlayersMinus(MainPlayer))
                {
                    TetrisGrid = new Grid();
                    TetrisGrid.Width = 250;
                    TetrisGrid.Height = 500;
                    TetrisGrid.HorizontalAlignment = HorizontalAlignment.Left;
                    TetrisGrid.VerticalAlignment = VerticalAlignment.Bottom;
                    TetrisGrid.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#F4F4F4");
                    for (int i = 0; i < 16; i++)
                    {
                        ColumnDefinition gridCol = new ColumnDefinition();
                        gridCol.Width = new GridLength(25);
                        TetrisGrid.ColumnDefinitions.Add(gridCol);
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        RowDefinition gridRow = new RowDefinition();
                        gridRow.Height = new GridLength(25);
                        TetrisGrid.RowDefinitions.Add(gridRow);
                    }
                    OpponentGrids.Children.Add(TetrisGrid);

                    p.TetrisWell = new string[TetrisGrid.RowDefinitions.Count, TetrisGrid.ColumnDefinitions.Count];
                    p.PlayerGrid = TetrisGrid;
                }

                Bm = new BoardManager(this);
                DrawGrids();

                Focus();
                ConnectButton.Visibility = Visibility.Hidden;
                ReadyUpButton.Visibility = Visibility.Hidden;
                InputField.Visibility = Visibility.Hidden;
                PlayersText.Visibility = Visibility.Hidden;
                Status.Visibility = Visibility.Hidden;

            };
            Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Ends the game by clearing the board and resetting variables.
        /// </summary>
        public void EndGame()
        {
            Menu menu = new Menu();
            menu.Show();
            Close();
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

        public void UpdatePlayersText()
        {
            string text = "";
            int playerNumber = 1;
            foreach (Player p in Player.GetPlayers())
            {
                text += "Player" + playerNumber + " ";
                text += p.Ready ? "Ready" : "Not Ready";
                text += "\n";
                playerNumber++;
            }
            PlayersText.Text = text;
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
            NextBlockGrid.Children.Clear();
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        public void DrawGrids()
        {
            string[,] newWell = (string[,]) Bm.TetrisWell.Clone();
            for (int i = 0; i < Bm.CurrentBlock.Shape.Value.GetLength(0); i++)
            {
                for (int j = 0; j < Bm.CurrentBlock.Shape.Value.GetLength(1); j++)
                {
                    if (Bm.CurrentBlock.Shape.Value[i, j] != 1)
                    {
                        continue;
                    }

                    newWell[i + Bm.CurrentBlock.Y, j + Bm.CurrentBlock.X] = Bm.CurrentBlock.Color;
                }
            }

            _connection.InvokeAsync<object[]>("UpdateWell", new object[] { MainPlayer.PlayerID, newWell });

            _connection.On<object[]>("UpdateWell", message =>
            {
                Player p = Player.FindPlayer(Guid.Parse((string) message[0]));
                p.TetrisWell = ((Newtonsoft.Json.Linq.JArray) message[1]).ToObject<string[,]>();
            });

            ClearGrids();

            foreach (Player p in Player.GetPlayersMinus(MainPlayer))
            {
                if (p.TetrisWell != null && p.PlayerGrid != null)
                {
                    p.PlayerGrid.Children.Clear();
                    for (int i = 0; i < p.TetrisWell.GetLength(0); i++)
                    {
                        for (int j = 0; j < p.TetrisWell.GetLength(1); j++)
                        {
                            Rectangle rectangle = CreateRectangle(p.TetrisWell[i, j], null);

                            p.PlayerGrid.Children.Add(rectangle);
                            Grid.SetRow(rectangle, i);
                            Grid.SetColumn(rectangle, j);
                        }
                    }
                }
            }

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
        private static Rectangle CreateRectangle(string fill, string border)
        {
            Rectangle rectangle = new()
            {
                Width = 25,
                Height = 25,
                Stroke = (SolidColorBrush) new BrushConverter().ConvertFrom(border ?? "#FFFFFFFF"),
                StrokeThickness = 1,
                Fill = fill == null ? null : (SolidColorBrush) new BrushConverter().ConvertFrom(fill),
            };

            return rectangle;
        }
    }
}
