using System;
using System.Windows.Media;

namespace TetrisClient
{
    public class Block : ICloneable
    {
        public BoardManager bm { get; set; }

        public Tetromino tetromino { get; set; }
        public SolidColorBrush color { get; set; }

        public Matrix shape { get; set; }

        public int xCord { get; set; }
        public int yCord { get; set; }

        public Block(BoardManager bm)
        {
            this.bm = bm;
            xCord = (bm.tetrisWell.GetUpperBound(1) - bm.tetrisWell.GetLowerBound(1)) / 2 - 1; // Center the Block

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
            Block tempBlock = (Block) this.Clone();
            tempBlock.yCord += 1;
            if (BlockManager.CanMove(bm.tetrisWell, tempBlock))
            {
                this.yCord += 1;
            }
            else
            {
                this.Place();
            }
        }
        public void Rotate()
        {
            shape = shape.Rotate90();
        }

        /// <summary>
        /// Places the Block into the tetris well.
        /// </summary>
        public void Place()
        {
            if (BlockManager.CanMove(bm.tetrisWell, this))
            {
                System.Diagnostics.Debug.WriteLine(ColorArrayToString(bm.tetrisWell));
                for (int i = 0; i < shape.Value.GetLength(0); i++)
                {
                    for (int j = 0; j < shape.Value.GetLength(1); j++)
                    {
                        if (shape.Value[i, j] != 1) continue;
                        bm.tetrisWell[i + yCord, j + xCord] = color;
                    }
                }
                bm.NextTurn();
            } else
            {
                bm.EndGame();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public String ColorArrayToString(SolidColorBrush[,] colorArray)
        {
            String output = "[";
            for (int row = 0; row < colorArray.GetLength(0); row++)
            {
                output += "[";
                for (int col = 0; col < colorArray.GetLength(1); col++)
                {
                    if (colorArray[row, col] != null)
                    {
                        output += colorArray[row, col].Color.ToString();
                    } else
                    {
                        output += "#FFFFFFFF";
                    }
                    if (col != colorArray.GetUpperBound(1)) output += ",";
                }
                output += "]\n";
                if (row != colorArray.GetUpperBound(0)) output += ",";
            }
            output += "]";

            return output;
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
        /// Checks if the Block can be moved to the specified location in the tetris well.
        /// </summary>
        /// <param name="tetrisWell">The tetris well to check the block on.</param>
        /// <param name="block">The block to check on.</param>
        /// <returns>A boolean stating if the block can be moved.</returns>
        public static Boolean CanMove(SolidColorBrush[,] tetrisWell, Block block)
        {
            Boolean willCollide = false;

            for (int i = 0; i < block.shape.Value.GetLength(0); i++)
            {
                for (int j = 0; j < block.shape.Value.GetLength(1); j++)
                {
                    if (block.shape.Value[i, j] != 1) continue;

                    // Grid borders
                    if (block.yCord + i < tetrisWell.GetLowerBound(0) 
                        || block.yCord + i >= tetrisWell.GetUpperBound(0) + 1 
                        || block.xCord + j < tetrisWell.GetLowerBound(1) 
                        || block.xCord + j >= tetrisWell.GetUpperBound(1) + 1)
                    {
                        willCollide = true;
                    }
                    // Overlapping cells
                    else if (tetrisWell[block.yCord + i, block.xCord + j] != null)
                    {
                        willCollide = true;
                    }
                }
            }

            return !willCollide;
        }

        /// <summary>
        /// Get a random Tetromino shape.
        /// </summary>
        /// <returns>A random Tetromino.</returns>
        public static Tetromino GetRandomTetromino()
        {
            // ToDo: Make it select the 7 different tetrominos every 7 turns, but randomize the order.
            Array values = Enum.GetValues(typeof(Tetromino));
            return (Tetromino)values.GetValue(BoardManager.randStatus.Next(values.Length));
        }

        /// <summary>
        /// Get the shape Matrix of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the shape of.</param>
        /// <returns>The shape Matrix the given Tetromino.</returns>
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
                                { 1, 0, 0 },
                                { 1, 1, 1 },
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
        /// <returns>The color of the given Tetromino.</returns>
        public static SolidColorBrush GetTetrominoColor(Tetromino tetromino)
        {
            switch (tetromino)
            {
                case Tetromino.IBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#78F0F0"));
                case Tetromino.JBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#7878F0"));
                case Tetromino.LBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0C878"));
                case Tetromino.OBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0F078"));
                case Tetromino.SBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#78F078"));
                case Tetromino.ZBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#F07878"));
                case Tetromino.TBlock:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#C878F0"));
                default:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#C878F0"));
            }
        }
    }
}
