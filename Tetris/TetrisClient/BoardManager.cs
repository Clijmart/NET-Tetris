using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TetrisClient
{
   
    public class BoardManager
    {
        public static Random randStatus = new Random();

        public SolidColorBrush[,] tetrisWell {get; set; }
        public Block currentBlock { get; set; }
        public Block nextBlock { get; set; }
        public int level { get; set; }
        private SoundManager soundManager { get; set; }


        public BoardManager()
        {
            tetrisWell = new SolidColorBrush[16, 10];
            currentBlock = new Block(tetrisWell);
            nextBlock = new Block(tetrisWell);
            soundManager = new SoundManager();

            soundManager.PlayMusic();
            NextTurn();
        }

        public void NextTurn()
        {
            // ToDo: Implement Filled Row code below, contains bug: After removing filled rows, the next placed block disappears.
            /*
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
            */
            SelectNextBlock();
        }

        /// <summary>
        /// Select the next block and update the current block.
        /// </summary>
        public void SelectNextBlock()
        {
            currentBlock = (Block) nextBlock.Clone();
            nextBlock = new Block(tetrisWell);
        }
        
    }
}
