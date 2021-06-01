using System;
using System.Windows.Media;

namespace TetrisClient
{
    public class Block : ICloneable
    {
        public Tetromino tetromino { get; set; }
        public SolidColorBrush color { get; set; }

        public Matrix shape { get; set; }

        public int xCord { get; set; }
        public int yCord { get; set; }

        public Block()
        {
            xCord = 4;

            tetromino = BlockManager.GetRandomTetromino();
            shape = BlockManager.GetTetrominoShape(tetromino);
            color = BlockManager.GetTetrominoColor(tetromino);
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


        /// <summary>
        /// Places the Block into the tetris well.
        /// </summary>
        /// <param name="tetrisWell">The tetris well to place the block into.</param>
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

    public static class BlockManager
    {
        /// <summary>
        /// Get a random Tetromino shape.
        /// </summary>
        public static Tetromino GetRandomTetromino()
        {
            Array values = Enum.GetValues(typeof(Tetromino));
            return (Tetromino)values.GetValue(BoardManager.randStatus.Next(values.Length));
        }

        /// <summary>
        /// Get the shape Matrix of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the shape of.</param>
        public static Matrix GetTetrominoShape(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.IBlock:
                    return new Matrix(new int[,]
                       {
                                { 1, 1, 1, 1 },
                                { 0, 0, 0, 0 },
                                { 0, 0, 0, 0 },
                                { 0, 0, 0, 0 },
                       }
                   );
                case Tetromino.JBlock:
                    return new Matrix(new int[,]
                        {
                                { 1, 1, 1 },
                                { 0, 0, 1 },
                                { 0, 0, 0 },
                        }
                    );
                case Tetromino.LBlock:
                    return new Matrix(new int[,]
                        {
                                { 0, 0, 1 },
                                { 1, 1, 1 },
                                { 0, 0, 0 },
                        }
                    );
                case Tetromino.OBlock:
                    return new Matrix(new int[,]
                        {
                                { 1, 1 },
                                { 1, 1 },
                        }
                    );
                case Tetromino.SBlock:
                    return new Matrix(new int[,]
                        {
                                { 0, 1, 1 },
                                { 1, 1, 0 },
                                { 0, 0, 0 },
                        }
                    );
                case Tetromino.ZBlock:
                    return new Matrix(new int[,]
                        {
                                { 1, 1, 0 },
                                { 0, 1, 1 },
                                { 0, 0, 0 },
                        }
                    );
                case Tetromino.TBlock:
                    return new Matrix(new int[,]
                        {
                                { 1, 1, 1 },
                                { 0, 1, 0 },
                                { 0, 0, 0 },
                        }
                    );
                default:
                    return new Matrix(new int[,]
                        {
                                { 0, 0, 0 },
                                { 0, 0, 0 },
                                { 0, 0, 0 },
                        }
                    );
            }
        }

        /// <summary>
        /// Get the color of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the color of.</param>
        public static SolidColorBrush GetTetrominoColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.IBlock:
                    return Brushes.LightBlue;
                case Tetromino.JBlock:
                    return Brushes.Blue;
                case Tetromino.LBlock:
                    return Brushes.Orange;
                case Tetromino.OBlock:
                    return Brushes.Yellow;
                case Tetromino.SBlock:
                    return Brushes.Green;
                case Tetromino.ZBlock:
                    return Brushes.Red;
                case Tetromino.TBlock:
                    return Brushes.Purple;
                default:
                    return Brushes.White;
            }
        }
    }
}
