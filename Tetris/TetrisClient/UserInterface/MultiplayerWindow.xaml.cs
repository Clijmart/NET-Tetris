using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using TetrisClient.GameManager;
using TetrisClient.Managers;
using TetrisClient.Objects;

namespace TetrisClient
{
    public partial class MultiplayerWindow : Window
    {
        private HubConnection _connection;
        public Player MainPlayer;

        public Random RandomSeeded;

        private int Seed;

        public BoardManager Bm { get; set; }

        public MultiplayerWindow()
        {
            InitializeComponent();
            ConnectionField.Text = SettingManager.StoredConnection;
            NameField.Text = SettingManager.StoredName;

            ReadyUpButton.Visibility = Visibility.Hidden;
            PlayersText.Visibility = Visibility.Hidden;

            GameSidebar.Visibility = Visibility.Hidden;
            NextBlockGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the connect button event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private async void ConnectButtonMethod(object sender, RoutedEventArgs e)
        {
            // Fill and store the contents of the input fields.
            if (ConnectionField.Text == "")
            {
                ConnectionField.Text = "127.0.0.1:5000";
            }
            SettingManager.StoredName = NameField.Text;
            SettingManager.StoredConnection = ConnectionField.Text;
            string url = "http://" + ConnectionField.Text + "/TetrisHub";

            // Try to create a connection.
            try
            {
                _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect()
                .Build();

                // Receive a status request from the server and send the status back.
                _ = _connection.On<string>("RequestStatus", playerId =>
                {
                    _connection.InvokeAsync("SendRequestedStatus", playerId, MainPlayer.Name, MainPlayer.Ready);
                });

                // Receive a leave notification from the server, so the player can be removed clientside too.
                _ = _connection.On<string>("OnLeave", playerId =>
                {
                    if (Bm == null)
                    {
                        Player.RemovePlayer(Player.FindPlayer(playerId));

                        Application.Current.Dispatcher.Invoke(delegate ()
                        {
                            UpdatePlayersText();
                        });
                    }
                });

                // Receive a join succesful notification from the server, containing a seed and the connected player count.
                _ = _connection.On<object[]>("OnJoin", message =>
                {
                    string playerId = (string) message[0];
                    int seed = (int) ((long) message[1] % int.MaxValue);
                    int playerCount = (int) ((long) message[2] % int.MaxValue);

                    // Create the main player object.
                    MainPlayer = Player.FindPlayer(playerId);
                    MainPlayer.Name = NameField.Text;
                    if (MainPlayer.Name == "" || MainPlayer.Name == null)
                    {
                        MainPlayer.Name = "Player " + playerCount;
                    }
                    SettingManager.StoredName = MainPlayer.Name;

                    Seed = seed;

                    Application.Current.Dispatcher.Invoke(delegate ()
                    {
                        UpdatePlayersText();
                    });

                    // Send the current status to the server.
                    _connection.InvokeAsync("SendStatus", MainPlayer.Name, MainPlayer.Ready);
                });
                
                // Receive the status of a player from the server.
                _ = _connection.On<object[]>("ReceiveStatus", message =>
                {
                    // Create the player object.
                    string playerId = (string) message[0];
                    string name = (string) message[1];
                    bool ready = (bool) message[2];

                    Player p = Player.FindPlayer(playerId);
                    p.Name = name;
                    p.Ready = ready;

                    Application.Current.Dispatcher.Invoke(delegate ()
                    {
                        UpdatePlayersText();
                    });

                    // Start the game if everyone is ready.
                    if (Player.AllReady())
                    {
                        InitGame();
                    }
                });

                // Actually create the connection.
                await _connection.StartAsync();

                if (_connection.State.Equals(HubConnectionState.Connected))
                {
                    Status.Content = "Connected!";

                    ReadyUpButton.Visibility = Visibility.Visible;
                    PlayersText.Visibility = Visibility.Visible;
                    InputGrid.Visibility = Visibility.Hidden;
                    Status.Visibility = Visibility.Hidden;
                    ConnectButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(_connection.State);
                    Status.Content = "Can't connect!";
                }
            }
            catch (Exception er)
            {
                System.Diagnostics.Debug.WriteLine(er);
                Status.Content = "Invalid IP!";
            }
        }

        /// <summary>
        /// Handles the ready up button event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
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

            _connection.InvokeAsync("SendStatus", MainPlayer.Name, MainPlayer.Ready);

            if (Player.AllReady())
            {
                InitGame();
            }
        }

        /// <summary>
        /// Handles the exit button event.
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="e">The Arguments that are sent with the Event.</param>
        private void ExitButton(object sender, RoutedEventArgs e)
        {
            if (Bm != null)
            {
                if (Bm.Running)
                {
                    EndGame();
                }
                CloseGame();
            }
            else
            {
                Menu menu = new();
                menu.Show();

                Close();
            }
        }

        /// <summary>
        /// Initializes the game by preparing the board, toggling fields and resetting variables.
        /// </summary>
        public void InitGame()
        {
            void action()
            {
                PrepareGrids();

                Bm = new BoardManager(this);
                DrawGrids();

                Focus();

                ConnectionSidebar.Visibility = Visibility.Hidden;
                PlayersText.Visibility = Visibility.Hidden;
                GameSidebar.Visibility = Visibility.Visible;
                NextBlockGrid.Visibility = Visibility.Visible;
            }
            Dispatcher.Invoke(action);
        }

        /// <summary>
        /// Dynamically creates all extra grids and info blocks.
        /// </summary>
        public void PrepareGrids()
        {
            int opponents = 0;
            foreach (Player p in Player.GetPlayers())
            {
                if (p != MainPlayer)
                {
                    p.PlayerGrid = InterfaceManager.CreateTetrisGrid(OpponentGrid, opponents, MainTetrisGrid.RowDefinitions.Count, MainTetrisGrid.ColumnDefinitions.Count);
                    p.TetrisWell = new string[p.PlayerGrid.RowDefinitions.Count, p.PlayerGrid.ColumnDefinitions.Count];

                    p.NameBlock = InterfaceManager.CreateInfoBlock(NameGrid, p.Name, opponents);
                    p.ScoreBlock = InterfaceManager.CreateInfoBlock(ScoreGrid, "" + p.Score, opponents);

                    opponents++;
                }
            }

            MainPlayer.NameBlock = InterfaceManager.CreateInfoBlock(NameGrid, MainPlayer.Name, opponents);
            MainPlayer.ScoreBlock = InterfaceManager.CreateInfoBlock(ScoreGrid, "" + MainPlayer.Score, opponents);

            Width = 25 + (275 * (opponents + 1)) + 150;
        }

        /// <summary>
        /// Ends the game and goes back to main menu.
        /// </summary>
        public void EndGame()
        {
            MainPlayer.Alive = false;
            Bm.Running = false;

            // Send the game info to the server.
            _connection.InvokeAsync("SendGameInfo", BlockManager.PlaceBlockInWell(Bm.TetrisWell, Bm.CurrentBlock), Bm.Score, Bm.LinesCleared, Bm.Time, Bm.Running);
        }

        public async void CloseGame()
        {
            // Disconnect from the server.
            await _connection.DisposeAsync();

            Bm.Timer.StopTimer();
            if (SettingManager.MusicOn)
            {
                Bm.SoundManager.StopMusic();
            }

            Player.RemovePlayer(MainPlayer);

            Menu menu = new();
            menu.Show();
            Close();
        }

        public void ShowResults()
        {
            MessageBox.Show(string.Format("You placed #{0} with a score of {1}!", Player.GetPlacing(MainPlayer) + 1, MainPlayer.Score));
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

            // Make sure the key event does not work between blocks swaps.
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
                            DrawGrids();
                        }
                        return;
                    }
                case Key.Escape:
                    {
                        EndGame();
                        CloseGame();
                        return;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates all lobby text.
        /// </summary>
        public void UpdatePlayersText()
        {
            string text = "";
            foreach (Player p in Player.GetPlayers())
            {
                text += p.Ready ? "✓ " : "✗ ";
                text += p.Name;
                text += "\n";
            }
            PlayersText.Text = text;
        }

        /// <summary>
        /// Updates all text.
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

            Level.Text = "" + Bm.CalculateLevel();
            Lines.Text = "" + Bm.LinesCleared;

            TimeSpan timeSpan = new(0, 0, Bm.Time / 10);
            Time.Text = timeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Draws all block grids.
        /// </summary>
        public void DrawGrids()
        {
            // Send the game info to the server.
            _connection.InvokeAsync("SendGameInfo", BlockManager.PlaceBlockInWell(Bm.TetrisWell, Bm.CurrentBlock), Bm.Score, Bm.LinesCleared, Bm.Time, Bm.Running);

            // Receive the game info of other players from the server.
            _ = _connection.On<object[]>("ReceiveGameInfo", message =>
              {
                  Player p = Player.FindPlayer((string) message[0]);
                  p.TetrisWell = ((JArray) message[1]).ToObject<string[,]>();
                  p.Score = (long) message[2];
                  p.LinesCleared = (int) ((long) message[3] % int.MaxValue);
                  p.Time = (int) ((long) message[4] % int.MaxValue);
                  p.Alive = (bool) message[5];
              });

            // Draw the main well and blocks.
            MainTetrisGrid.Children.Clear();
            NextBlockGrid.Children.Clear();
            InterfaceManager.DrawWell(MainTetrisGrid, Bm.TetrisWell);
            InterfaceManager.DrawBlock(MainTetrisGrid, Bm.GhostBlock, true, true);
            InterfaceManager.DrawBlock(MainTetrisGrid, Bm.CurrentBlock, false, true);
            InterfaceManager.DrawBlock(NextBlockGrid, Bm.NextBlock, false, true);

            // Draw the well and blocks of all other players. 
            foreach (Player p in Player.GetPlayers())
            {
                if (p != MainPlayer && p.TetrisWell != null && p.PlayerGrid != null)
                {
                    if (p.Alive)
                    {
                        p.PlayerGrid.Children.Clear();
                        InterfaceManager.DrawWell(p.PlayerGrid, p.TetrisWell);
                    }
                    else
                    {
                        p.PlayerGrid.Background = (SolidColorBrush) Application.Current.TryFindResource("PlayerNotAlive");
                    }

                }
            }
        }
    }
}
