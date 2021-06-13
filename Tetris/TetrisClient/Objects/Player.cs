using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TetrisClient.Objects
{
    public class Player
    {
        private static List<Player> Players = new List<Player>();

        public string PlayerId { get; set; }
        public string Name { get; set; }
        public string[,] TetrisWell { get; set; }
        public long Score { get; set; }
        public int LinesCleared { get; set; }
        public int Time { get; set; }

        public Grid PlayerGrid { get; set; }
        public TextBlock NameBlock { get; set; }
        public TextBlock ScoreBlock { get; set; }
        public bool Ready { get; set; }

        public Player (string playerID)
        {
            PlayerId = playerID;
            Score = 0;
            LinesCleared = 0;
            Time = 0;

            Ready = false;
        }

        public static Player FindPlayer(string playerID)
        {
            foreach (Player player in Players)
            {
                if (player.PlayerId == playerID)
                {
                    return player;
                }
            }
            Player p = new(playerID);
            Players.Add(p);
            Players = Players.OrderBy(x=>x.Name).ToList();
            return p;
        }

        public static bool AllReady()
        {
            if (Players.Count < 2)
            {
                return false;
            }

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

        public static void RemovePlayer(Player player)
        {
            Players.Remove(player);
        }
    }
}
