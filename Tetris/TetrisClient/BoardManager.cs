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

            tetrisWell = new SolidColorBrush[16, 10];
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
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();

            running = true;
        }

        public int getLevel()
        {
            return linesCleared / 10;
        }

        public static int CalculateScore(int linesFilled, int level)
        {
            int s = 0;
            switch (linesFilled)
            {
                case 1:
                    s += 40 * (level + 1);
                    break;
                case 2:
                    s += 100 * (level + 1);
                    break;
                case 3:
                    s += 300 * (level + 1);
                    break;
                case 4:
                    s += 1200 * (level + 1);
                    break;
            }
            return s;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (running)
            {
                time++;

                if (time % (10 - Math.Min(8, getLevel() * 2)) == 0)
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

            score += CalculateScore(rowsFilled, getLevel());

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

        public void EndGame()
        {
            running = false;
            dispatcherTimer.Stop();
        }
    }
}
