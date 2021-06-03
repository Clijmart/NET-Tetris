using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace TetrisClient
{
    public class BoardManager
    {
        public MainWindow mainWindow { get; set; }

        public static Random randStatus = new Random();

        public SolidColorBrush[,] tetrisWell { get; set; }
        public Block currentBlock { get; set; }
        public Block nextBlock { get; set; }
        public Block ghostBlock { get; set; }

        public Boolean running { get; set; }
        public int time { get; set; }
        public int linesCleared { get; set; }
        public int score { get; set; }

        private SoundManager soundManager { get; set; }


        public DispatcherTimer dispatcherTimer;

        public BoardManager(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            tetrisWell = new SolidColorBrush[mainWindow.TetrisGrid.RowDefinitions.Count, mainWindow.TetrisGrid.ColumnDefinitions.Count];
            currentBlock = new Block(this);
            nextBlock = new Block(this);

            ghostBlock = currentBlock.CalculateGhost();

            time = 0;
            linesCleared = 0;
            score = 0;

            soundManager = new SoundManager();
            soundManager.PlayMusic();
            NextTurn();

            //  DispatcherTimer setup
            this.dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();

            running = true;
        }

        /// <summary>
        /// Calculates the level based on the amount of lines cleared.
        /// </summary>
        /// <returns>An integer with the current level.</returns>
        public int CalculateLevel()
        {
            return linesCleared / 10;
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
        /// Runs every timer tick.
        /// Moves the current block down and updates the window if necessary. 
        /// </summary>
        /// <param name="sender">The sender of the KeyEvent.</param>
        /// <param name="EventArgs">The Arguments that are sent with the TimerTick.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (running)
            {
                time++;

                if (time % (10 - Math.Min(8, CalculateLevel() * 2)) == 0)
                {
                    if (!currentBlock.MoveDown())
                    {
                        currentBlock.Place();
                    }
                    mainWindow.DrawGrids();
                }

                mainWindow.UpdateText();
            }
        }

        /// <summary>
        /// Prepares the game for the next turn.
        /// Checks if any rows need to be cleared, increases score and selects the next block.
        /// </summary>
        public void NextTurn()
        {
            int rowsFilled = 0;
            for (int row = 0; row < tetrisWell.GetLength(0); row++)
            {
                Boolean rowFilled = true;
                for (int cell = 0; cell < tetrisWell.GetLength(1); cell++)
                {
                    if (tetrisWell[row, cell] == null) rowFilled = false;
                }
                if (rowFilled)
                {
                    SolidColorBrush[,] newTetrisWell = new SolidColorBrush[tetrisWell.GetLength(0), tetrisWell.GetLength(1)];
                    for (int i = tetrisWell.GetUpperBound(0), k = tetrisWell.GetUpperBound(0); i >= 0; i--)
                    {
                        if (i == row) continue;

                        for (int j = 0; j < tetrisWell.GetLength(1); j++)
                        {
                            newTetrisWell[k, j] = tetrisWell[i, j];
                        }
                        k--;
                    }
                    tetrisWell = newTetrisWell;
                    rowsFilled++;
                }
            }

            score += CalculateScore(rowsFilled, CalculateLevel());

            linesCleared += rowsFilled;
            SelectNextBlock();
        }

        /// <summary>
        /// Select the next block and update the current block.
        /// </summary>
        public void SelectNextBlock()
        {
            currentBlock = (Block) nextBlock.Clone();
            ghostBlock = currentBlock.CalculateGhost();
            currentBlock.bm.tetrisWell = tetrisWell;
            nextBlock = new Block(this);
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        public void EndGame()
        {
            running = false;
            dispatcherTimer.Stop();
            soundManager.StopMusic();
        }
    }
}
