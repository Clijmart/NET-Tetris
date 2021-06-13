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

        private void ExitButton(object sender, RoutedEventArgs e)
        {
            if (Bm != null)
            {
                Bm.EndGame();
            }
            else
            {
                _connection.DisposeAsync();
                Player.RemovePlayer(MainPlayer);
                Menu menu = new();
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
            SettingManager.StoredName = NameField.Text;
            SettingManager.StoredConnection = ConnectionField.Text;

            string url = "http://" + ConnectionField.Text + "/TetrisHub";
            System.Diagnostics.Debug.WriteLine(url);

            try
            {
                _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect()
                .Build();

                _ = _connection.On<string>("RequestStatus", playerId =>
                {
                    _connection.InvokeAsync("SendRequestedStatus", playerId, MainPlayer.Name, MainPlayer.Ready);
                });

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

                _ = _connection.On<object[]>("OnJoin", message =>
                {
                    string playerId = (string) message[0];
                    int seed = (int) ((long) message[1] % int.MaxValue);
                    int playerCount = (int) ((long) message[2] % int.MaxValue);

                    MainPlayer = Player.FindPlayer(playerId);
                    MainPlayer.Name = NameField.Text;
                    if (MainPlayer.Name == "" || MainPlayer.Name == null)
                    {
                        MainPlayer.Name = "Player " + playerCount;
                    }
                    Seed = seed;

                    Application.Current.Dispatcher.Invoke(delegate ()
                    {
                        UpdatePlayersText();
                    });

                    SettingManager.StoredName = MainPlayer.Name;

                    _connection.InvokeAsync("SendStatus", MainPlayer.Name, MainPlayer.Ready);
                });

                _ = _connection.On<object[]>("ReceiveStatus", message =>
                  {
                      string playerId = (string) message[0];
                      string name = (string) message[1];
                      bool ready = (bool) message[2];
                      System.Diagnostics.Debug.WriteLine("Received ReceiveStatus: " + playerId);

                      Player p = Player.FindPlayer(playerId);
                      p.Name = name;
                      p.Ready = ready;

                      Application.Current.Dispatcher.Invoke(delegate ()
                      {
                          UpdatePlayersText();
                      });

                      if (Player.AllReady())
                      {
                          InitGame();
                      }
                  });

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
            Menu menu = new();
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
            if (Bm == null)
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
                        EndGame();
                        InitGame();
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

            Level.Text = Bm.CalculateLevel().ToString();
            Lines.Text = Bm.LinesCleared.ToString();

            TimeSpan timeSpan = new(0, 0, Bm.Time / 10);
            Time.Text = timeSpan.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Draws all block grids.
        /// </summary>
        public void DrawGrids()
        {
            // Send the game info to the server.
            _connection.InvokeAsync("SendGameInfo", BlockManager.PlaceBlockInWell(Bm.TetrisWell, Bm.CurrentBlock), Bm.Score);

            // Receive the game info of other players from the server.
            _ = _connection.On<object[]>("ReceiveGameInfo", message =>
              {
                  Player p = Player.FindPlayer((string) message[0]);
                  p.TetrisWell = ((JArray) message[1]).ToObject<string[,]>();
                  p.Score = (long) message[2];
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
                    p.PlayerGrid.Children.Clear();
                    InterfaceManager.DrawWell(p.PlayerGrid, p.TetrisWell);
                }
            }
        }
    }
}
