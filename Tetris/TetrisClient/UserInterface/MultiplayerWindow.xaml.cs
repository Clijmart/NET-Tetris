using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TetrisClient.GameManager;
using TetrisClient.Objects;

namespace TetrisClient
{
    public partial class MultiplayerWindow : Window
    {
        private HubConnection _connection;
        public Player MainPlayer;

        public Random RandomSeeded;

        public Grid tetrisGrid;

        private int Seed;

        public BoardManager Bm { get; set; }

        public MultiplayerWindow()
        {
            InitializeComponent();
            ReadyUpButton.Visibility = Visibility.Hidden;
            PlayersText.Visibility = Visibility.Hidden;

            GameSidebar.Visibility = Visibility.Hidden;
            NextBlockGrid.Visibility = Visibility.Hidden;

            MainPlayer = Player.FindPlayer(Guid.NewGuid());
            MainPlayer.Name = NameField.Text;
            if (MainPlayer.Name == "" || MainPlayer.Name == null)
            {
                MainPlayer.Name = "Player " + (Player.GetPlayers().IndexOf(MainPlayer) + 1);
            }
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

            MainPlayer.Ready = !MainPlayer.Ready;

            ReadyUpButton.Background = (SolidColorBrush) new BrushConverter().ConvertFrom(MainPlayer.Ready ? "#FF2FCE7F" : "#FF2F7FDE");
            UpdatePlayersText();

            // Het aanroepen van de TetrisHub.cs methode `ReadyUp`.
            // Hier geven we de int mee die de methode `ReadyUp` verwacht.
            _connection.InvokeAsync("SendStatus", new object[] { MainPlayer.PlayerID, MainPlayer.Name, MainPlayer.Ready, Seed });

            if (Player.AllReady())
            {
                InitGame();
            }
        }

        private void ExitButton(object sender, RoutedEventArgs e)
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
            if (ConnectionField.Text == "")
            {
                ConnectionField.Text = "127.0.0.1:5000";
            }
            string url = "http://" + ConnectionField.Text + "/TetrisHub";
            System.Diagnostics.Debug.WriteLine(url);

            try
            {
                _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect()
                .Build();

                _connection.On<object[]>("Join", message =>
                {
                    Player p = Player.FindPlayer(Guid.Parse((string) message[0]));
                    p.Name = (string) message[1];
                    if (p.Name == "" || p.Name == null)
                    {
                        p.Name = "Player " + (Player.GetPlayers().IndexOf(p) + 1);
                    }

                    Application.Current.Dispatcher.Invoke(delegate ()
                    {
                        UpdatePlayersText();
                    });

                    _connection.InvokeAsync("SendStatus", new object[] { MainPlayer.PlayerID, MainPlayer.Name, MainPlayer.Ready, Seed });
                });

                _connection.On<object[]>("SendStatus", message =>
                {
                    Player p = Player.FindPlayer(Guid.Parse((string) message[0]));
                    p.Name = (string) message[1];
                    if (p.Name == "" || p.Name == null)
                    {
                        p.Name = "Player " + (Player.GetPlayers().IndexOf(p) + 1);
                    }

                    p.Ready = (bool) message[2];

                    Application.Current.Dispatcher.Invoke(delegate() {
                        UpdatePlayersText();
                    });

                    int seed = (int) ((long) message[3] % int.MaxValue);
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
                    InputGrid.Visibility = Visibility.Hidden;
                    Status.Visibility = Visibility.Hidden;
                    ConnectButton.Visibility = Visibility.Hidden;

                    MainPlayer.Name = NameField.Text;
                    await _connection.InvokeAsync("Join", new object[] { MainPlayer.PlayerID, MainPlayer.Name });
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
                PrepareGrids();

                Bm = new BoardManager(this);
                DrawGrids();

                Focus();

                ConnectionSidebar.Visibility = Visibility.Hidden;
                PlayersText.Visibility = Visibility.Hidden;
                GameSidebar.Visibility = Visibility.Visible;
                NextBlockGrid.Visibility = Visibility.Visible;
            };
            Dispatcher.Invoke(action);
        }

        public void PrepareGrids()
        {
            int opponents = 0;
            foreach (Player p in Player.GetPlayersMinus(MainPlayer))
            {
                tetrisGrid = new Grid
                {
                    Width = 250,
                    Height = 500,
                    Margin = new Thickness(275 * opponents, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = (SolidColorBrush) Application.Current.TryFindResource("Background"),
                    Effect = new DropShadowEffect()
                    {
                        BlurRadius = 10,
                        Color = (Color) Application.Current.TryFindResource("TextColor"),
                        ShadowDepth = 0,
                    }
                };

                for (int i = 0; i < 16; i++)
                {
                    ColumnDefinition gridCol = new ColumnDefinition();
                    gridCol.Width = new GridLength(25);
                    tetrisGrid.ColumnDefinitions.Add(gridCol);
                }

                for (int i = 0; i < 20; i++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(25);
                    tetrisGrid.RowDefinitions.Add(gridRow);
                }
                p.TetrisWell = new string[tetrisGrid.RowDefinitions.Count, tetrisGrid.ColumnDefinitions.Count];
                OpponentGrid.Children.Add(tetrisGrid);
                p.PlayerGrid = tetrisGrid;

                TextBlock nameBlock = new()
                {
                    Width = 250,
                    Height = 50,
                    Margin = new Thickness(275 * opponents, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Text = p.Name,
                    FontWeight = FontWeights.Bold,
                    FontFamily = (FontFamily) Application.Current.TryFindResource("MainFont"),
                    FontSize = 30,
                    Foreground = (SolidColorBrush) Application.Current.TryFindResource("Text"),
                    TextAlignment = TextAlignment.Center,
                };
                NameGrid.Children.Add(nameBlock);
                p.NameBlock = nameBlock;

                TextBlock scoreBlock = new()
                {
                    Width = 250,
                    Height = 50,
                    Margin = new Thickness(275 * opponents, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Text = "" + p.Score,
                    FontWeight = FontWeights.Bold,
                    FontFamily = (FontFamily) Application.Current.TryFindResource("MainFont"),
                    FontSize = 30,
                    Foreground = (SolidColorBrush) Application.Current.TryFindResource("Text"),
                    TextAlignment = TextAlignment.Center,
                };
                ScoreGrid.Children.Add(scoreBlock);
                p.ScoreBlock = scoreBlock;

                opponents++;
            }

            TextBlock mainNameBlock = new()
            {
                Width = 250,
                Height = 50,
                Margin = new Thickness(275 * opponents, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = MainPlayer.Name,
                FontWeight = FontWeights.Bold,
                FontFamily = (FontFamily) Application.Current.TryFindResource("MainFont"),
                FontSize = 30,
                Foreground = (SolidColorBrush) Application.Current.TryFindResource("Text"),
                TextAlignment = TextAlignment.Center,
            };
            NameGrid.Children.Add(mainNameBlock);
            MainPlayer.NameBlock = mainNameBlock;

            TextBlock mainScoreBlock = new()
            {
                Width = 250,
                Height = 50,
                Margin = new Thickness(275 * opponents, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = "" + MainPlayer.Score,
                FontWeight = FontWeights.Bold,
                FontFamily = (FontFamily) Application.Current.TryFindResource("MainFont"),
                FontSize = 30,
                Foreground = (SolidColorBrush) Application.Current.TryFindResource("Text"),
                TextAlignment = TextAlignment.Center,
            };
            ScoreGrid.Children.Add(mainScoreBlock);
            MainPlayer.ScoreBlock = mainScoreBlock;

            Width = 25 + 275 * (opponents + 1) + 150;
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
            foreach (Player p in Player.GetPlayers())
            {
                if (p.Name == "" || p.Name == null)
                {
                    p.Name = "Player " + (Player.GetPlayers().IndexOf(p) + 1);
                }

                text += p.Ready ? "✓ " : "✗ ";
                text += p.Name;
                text += "\n";
            }
            PlayersText.Text = text;
        }

        /// <summary>
        /// Updates all Text.
        /// </summary>
        public void UpdateText()
        {
            MainPlayer.Score = Bm.Score;
            foreach (Player p in Player.GetPlayers())
            {
                if (p.ScoreBlock != null)
                {
                    p.ScoreBlock.Text = "" + p.Score;
                }
            }

            Level.Text = Bm.CalculateLevel().ToString();
            Lines.Text = Bm.LinesCleared.ToString();

            TimeSpan timeSpan = new(0, 0, Bm.Time / 10);
            Time.Text = timeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Draws all Block Grids.
        /// </summary>
        public void DrawGrids()
        {
            _connection.InvokeAsync<object[]>("UpdatePlayer", new object[] { MainPlayer.PlayerID, BlockManager.PlaceBlockInWell(Bm.TetrisWell, Bm.CurrentBlock), Bm.Score });

            _connection.On<object[]>("UpdatePlayer", message =>
            {
                Player p = Player.FindPlayer(Guid.Parse((string) message[0]));
                p.TetrisWell = ((Newtonsoft.Json.Linq.JArray) message[1]).ToObject<string[,]>();
                p.Score = (long) message[2];
            });

            MainTetrisGrid.Children.Clear();
            NextBlockGrid.Children.Clear();
            InterfaceManager.DrawWell(MainTetrisGrid, Bm.TetrisWell);
            InterfaceManager.DrawBlock(MainTetrisGrid, Bm.GhostBlock, true, true);
            InterfaceManager.DrawBlock(MainTetrisGrid, Bm.CurrentBlock, false, true);
            InterfaceManager.DrawBlock(NextBlockGrid, Bm.NextBlock, false, true);

            foreach (Player p in Player.GetPlayersMinus(MainPlayer))
            {
                if (p.TetrisWell != null && p.PlayerGrid != null)
                {
                    p.PlayerGrid.Children.Clear();
                    InterfaceManager.DrawWell(p.PlayerGrid, p.TetrisWell);
                }
            }
        }
    }
}
