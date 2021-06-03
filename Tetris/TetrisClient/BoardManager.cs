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
        public int level { get; set; }
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
            level = 1;

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

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (running)
            {
                time++;

                if (time % (10 - Math.Min(5, (level + 1) / 10)) == 0)
                {
                    if (!currentBlock.MoveDown())
                    {
                        currentBlock.Place();
                    }
                    mainWindow.DrawGrids();
                }
            }
        }

        public void NextTurn()
        {
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
                }
            }

            level++;
            SelectNextBlock();
        }

        /// <summary>
        /// Select the next block and update the current block.
        /// </summary>
        public void SelectNextBlock()
        {
            currentBlock = (Block)nextBlock.Clone();
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
