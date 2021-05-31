using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class Block : ICloneable
    {
        public Tetromino name { get; set; }
        public SolidColorBrush color { get; set; }

        public Matrix shape { get; set; }

        public int xCord { get; set; }
        public int yCord { get; set; }

        public Block()
        {
            xCord = 4;
        }
        public void MoveRight()
        {
            xCord += 1;
        }
        public void MoveLeft()
        {
            xCord -= 1;
        }
        public void MoveDown()
        {
            yCord += 1;
        }
        public void Rotate()
        {
            shape = shape.Rotate90();
        }
        public void Place(SolidColorBrush[,] tetrisWell)
        {
            for (int i = 0; i < shape.Value.GetLength(0); i++)
            {
                for (int j = 0; j < shape.Value.GetLength(1); j++)
                {
                    if (shape.Value[i, j] != 1) continue;
                    tetrisWell[i + yCord, j + xCord] = color;
                }
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
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

        public SolidColorBrush[,] tetrisWell {get; set; }
        public Block currentBlock { get; set; }
        public Block nextBlock { get; set; }
        public int level { get; set; }



        public BoardManager()
        {
            currentBlock = new Block();
            nextBlock = new Block();
            tetrisWell = new SolidColorBrush[16, 10];

            BuildBlock();
            
            SelectNextBlock();
            

        }


        public void BuildBlock()
        {
            Array values = Enum.GetValues(typeof(Tetromino));
            nextBlock.name = (Tetromino)values.GetValue(randStatus.Next(values.Length));
            switch (nextBlock.name)
            {
                case Tetromino.IBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 1, 1 },
                            { 0, 0, 0, 0 },
                            { 0, 0, 0, 0 },
                            { 0, 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.LimeGreen;
                    return;
                case Tetromino.JBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 1 },
                            { 0, 0, 1 },
                            { 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.Orange;
                    return;
                case Tetromino.LBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 0, 0, 1 },
                            { 1, 1, 1 },
                            { 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.Blue;
                    return;
                case Tetromino.OBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 1, 1 },
                            { 1, 1 },
                        }
                    );
                    nextBlock.color = Brushes.Purple;
                    return;
                case Tetromino.SBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 0, 1, 1 },
                            { 1, 1, 0 },
                            { 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.Red;
                    return;
                case Tetromino.ZBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 0 },
                            { 0, 1, 1 },
                            { 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.YellowGreen;
                    return;
                case Tetromino.TBlock:
                    nextBlock.shape = new Matrix(new int[,]
                        {
                            { 1, 1, 1 },
                            { 0, 1, 0 },
                            { 0, 0, 0 },
                        }
                    );
                    nextBlock.color = Brushes.Cyan;
                    return;
                default: return;
            }
        }
        public void SelectNextBlock()
        {
            currentBlock = (Block) nextBlock.Clone();
            BuildBlock();
        }
    }
}
