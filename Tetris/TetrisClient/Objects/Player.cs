using System;
using System.Collections.Generic;

namespace TetrisClient.Objects
{
    public class Player
    {
        private static List<Player> Players = new List<Player>();

        public Guid PlayerID { get; set; }
        public string[,] TetrisWell { get; set; }
        public int Score { get; set; }
        public int LinesCleared { get; set; }
        public int Time { get; set; }

        public bool Ready { get; set; }

        public Player (Guid playerID)
        {
            PlayerID = playerID;
            Score = 0;
            LinesCleared = 0;
            Time = 0;

            Ready = false;
        }

        public static Player FindPlayer(Guid playerID)
        {
            foreach (Player player in Players)
            {
                if (player.PlayerID == playerID)
                {
                    return player;
                }
            }
            Player p = new(playerID);
            Players.Add(p);
            return p;
        }

        public static bool AllReady()
        {
            foreach (Player player in Players)
            {
                if (!player.Ready)
                {
                    return false;
                }
            }
            return true;
        }

        public static List<Player> GetPlayers()
        {
            return Players;
        }
    }
}
