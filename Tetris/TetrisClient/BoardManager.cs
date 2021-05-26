using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TetrisClient
{
    public class Block
    {
        public Tetromino name { get; set; }
        public SolidColorBrush color { get; set; }

        public Matrix shape { get; set; }

        public Block()
        {

        }

    }
    public enum Tetromino
    {
        JBlock,
        LBlock,
        TBlock,
        OBlock,
        IBlock,
        SBlock,
        ZBlock
    }

    public class BoardManager
    {
        private static Random randStatus = new Random();
        public Block currentBlock { get; set; }
        public Block nextBlock { get; set; }

        public BoardManager()
        {
            currentBlock = new Block();
            nextBlock = new Block();

            Array values = Enum.GetValues(typeof(Tetromino));
            Random random = new Random();
            currentBlock.name = (Tetromino)values.GetValue(random.Next(values.Length));
            nextBlock.name = (Tetromino)values.GetValue(random.Next(values.Length));
            BuildBlock(currentBlock);
            BuildBlock(nextBlock);

        }


        public void BuildBlock(Block block)
        {
            switch (block.name)
            {
                case Tetromino.IBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 0, 0, 0, 0 },
                            { 1, 1, 1, 1 },
                            { 0, 0, 0, 0 },
                            { 0, 0, 0, 0 },
                        }
                    );
                    block.color = Brushes.LimeGreen;
                    return;
                case Tetromino.JBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 0, 0, 0 },
                            { 1, 1, 1 },
                            { 0, 0, 1 },
                        }
                    );
                    block.color = Brushes.Orange;
                    return;
                case Tetromino.LBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 0, 0, 1 },
                            { 1, 1, 1 },
                            { 0, 0, 0 },
                        }
                    );
                    block.color = Brushes.Blue;
                    return;
                case Tetromino.OBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 1, 1 },
                            { 1, 1 },
                        }
                    );
                    block.color = Brushes.Purple;
                    return;
                case Tetromino.SBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 0, 1, 1 },
                            { 1, 1, 0 },
                            { 0, 0, 0 },
                        }
                    );
                    block.color = Brushes.Red;
                    return;
                case Tetromino.ZBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 0 },
                            { 0, 1, 1 },
                            { 0, 0, 0 },
                        }
                    );
                    block.color = Brushes.YellowGreen;
                    return;
                case Tetromino.TBlock:
                    block.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 1 },
                            { 0, 1, 0 },
                            { 0, 0, 0 },
                        }
                    );
                    block.color = Brushes.Cyan;
                    return;
                default: return;
            }
        }
    }
}
