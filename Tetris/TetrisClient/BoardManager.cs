using System;
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

        public BoardManager()
        {
            currentBlock = new Block();
            nextBlock = new Block();
            tetrisWell = new SolidColorBrush[16, 10];
            
            SelectNextBlock();
        }

        /// <summary>
        /// Select the next block and update the current block.
        /// </summary>
        public void SelectNextBlock()
        {
            currentBlock = (Block) nextBlock.Clone();
            nextBlock = new Block();
        }
    }
}
