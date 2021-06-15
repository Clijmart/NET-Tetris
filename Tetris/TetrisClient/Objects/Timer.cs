using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TetrisClient.Objects;

namespace TetrisClient
{
    public class Timer
    {
        private DispatcherTimer DispatcherTimer { get; set; }

        public BoardManager Bm { get; set; }

        public Timer(BoardManager bm)
        {
            Bm = bm;
            DispatcherTimer = new DispatcherTimer(DispatcherPriority.Send);
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public async void StartTimer()
        {
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                    DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                    DispatcherTimer.Start();
                }));
            });
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            DispatcherTimer.Stop();
        }

        /// <summary>
        /// Runs every timer tick.
        /// Moves the current block down and updates the window if necessary. 
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="EventArgs">The Arguments that are sent with the TimerTick.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Checks if timer needs to do something
            if (Bm.Running || !Player.AllDead())
            {
                if (Bm.Running)
                {
                    Bm.Time++;
                }

                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                  {
                      // Depending on the levelspeed, updates the core game mechanics.
                      if (Bm.Time % (10 - Math.Min(8, Bm.CalculateLevel() * 2)) == 0)
                      {
                          if (Bm.Running)
                          {
                              // Tries to move or place the current block.
                              if (!Bm.CurrentBlock.MoveDown())
                              {
                                  Bm.CurrentBlock.Place();
                              }

                              // Updates and draws all main UI elements.
                              if (Bm.MainWindow != null)
                              {
                                  Bm.MainWindow.DrawGrids();
                                  Bm.MainWindow.UpdateText();
                              }
                              else
                              {
                                  Bm.MultiplayerWindow.UpdateGrid();
                                  Bm.MultiplayerWindow.UpdateText();
                              }
                          }
                      }
                  }));
            }

            // Checks if the game should end.
            if (Bm.MultiplayerWindow != null && Player.AllDead())
            {
                Bm.MultiplayerWindow.GoToResults();
            }
        }
    }
}
