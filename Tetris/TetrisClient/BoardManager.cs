using System;
using System.Windows.Media;

namespace TetrisClient
{
    public class BoardManager
    {
        public MainWindow MainWindow { get; set; }

        public static readonly Random randStatus = new();

        public SolidColorBrush[,] TetrisWell { get; set; }
        public Block CurrentBlock { get; set; }
        public Block NextBlock { get; set; }
        public Block GhostBlock { get; set; }

        public bool Running { get; set; }
        public int Time { get; set; }
        public Timer Timer { get; set; }
        public int LinesCleared { get; set; }
        public int Score { get; set; }

        private SoundManager SoundManager { get; set; }


        public BoardManager(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            TetrisWell = new SolidColorBrush[mainWindow.TetrisGrid.RowDefinitions.Count, mainWindow.TetrisGrid.ColumnDefinitions.Count];
            CurrentBlock = new Block(this);
            NextBlock = new Block(this);

            GhostBlock = CurrentBlock.CalculateGhost();

            Time = 0;
            LinesCleared = 0;
            Score = 0;

            SoundManager = new SoundManager();
            SoundManager.PlayMusic();
            NextTurn();

            //  DispatcherTimer setup
            Timer = new(this);
            Timer.StartTimer();
            Running = true;
        }

        /// <summary>
        /// Calculates the level based on the amount of lines cleared.
        /// </summary>
        /// <returns>An integer with the current level.</returns>
        public int CalculateLevel()
        {
            return LinesCleared / 10;
        }

        /// <summary>
        /// Calculates the score using the amount of filled lines and the current level.
        /// </summary>
        /// <param name="linesfilled">The amount of lines to calculate the schore for.</param>
        /// <param name="level">The current level.</param>
        /// <returns>An integer with the calculated score.</returns>
        public static int CalculateScore(int linesFilled, int level)
        {
            int s = 0;
            s += linesFilled switch
            {
                1 => 40 * (level + 1),
                2 => 100 * (level + 1),
                3 => 300 * (level + 1),
                4 => 1200 * (level + 1),
                _ => 420 * linesFilled * (level + 1),
            };
            return s;
        }

        /// <summary>
        /// Prepares the game for the next turn.
        /// Checks if any rows need to be cleared, increases score and selects the next block.
        /// </summary>
        public void NextTurn()
        {
            int rowsFilled = 0;
            for (int row = 0; row < TetrisWell.GetLength(0); row++)
            {
                bool rowFilled = true;
                for (int cell = 0; cell < TetrisWell.GetLength(1); cell++)
                {
                    if (TetrisWell[row, cell] == null)
                    {
                        rowFilled = false;
                    }
                }
                if (rowFilled)
                {
                    SolidColorBrush[,] newTetrisWell = new SolidColorBrush[TetrisWell.GetLength(0), TetrisWell.GetLength(1)];
                    for (int i = TetrisWell.GetUpperBound(0), k = TetrisWell.GetUpperBound(0); i >= 0; i--)
                    {
                        if (i == row)
                        {
                            continue;
                        }

                        for (int j = 0; j < TetrisWell.GetLength(1); j++)
                        {
                            newTetrisWell[k, j] = TetrisWell[i, j];
                        }
                        k--;
                    }
                    TetrisWell = newTetrisWell;
                    rowsFilled++;
                }
            }

            Score += CalculateScore(rowsFilled, CalculateLevel());

            LinesCleared += rowsFilled;
            SelectNextBlock();
        }

        /// <summary>
        /// Select the next block and update the current block.
        /// </summary>
        public void SelectNextBlock()
        {
            CurrentBlock = NextBlock.Clone();
            GhostBlock = CurrentBlock.CalculateGhost();
            CurrentBlock.Bm.TetrisWell = TetrisWell;
            NextBlock = new Block(this);
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        public void EndGame()
        {
            Running = false;
            Timer.StopTimer();
            SoundManager.StopMusic();
        }
    }
}
