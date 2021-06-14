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
        public bool Alive { get; set; }

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
            Alive = true;

            Ready = false;
        }

        /// <summary>
        /// Find a player with a given identifier.
        /// If the player can not be found, create it.
        /// </summary>
        /// <param name="playerID">The player identifier to look for.</param>
        /// <returns>The found or created player object.</returns>
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

        public static int GetPlacing(Player player)
        {
            List<Player> playersSorted = Players.OrderByDescending(x => x.Score).ThenByDescending(x => x.LinesCleared).ThenBy(x => x.Time).ToList();
            return playersSorted.IndexOf(player);
        }

        /// <summary>
        /// Checks if there's enough players and if they are all ready.
        /// </summary>
        /// <returns>Whether all players are ready.</returns>
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

        /// <summary>
        /// Check if all players are dead.
        /// </summary>
        /// <returns>Whether all players are dead.</returns>
        public static bool AllDead()
        {
            foreach (Player player in Players)
            {
                if (player.Alive)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get all connected players.
        /// </summary>
        /// <returns>A list of players.</returns>
        public static List<Player> GetPlayers()
        {
            return Players;
        }

        /// <summary>
        /// Removes a player from the list.
        /// </summary>
        /// <param name="player">The player to remove.</param>
        public static void RemovePlayer(Player player)
        {
            Players.Remove(player);
        }
    }
}
