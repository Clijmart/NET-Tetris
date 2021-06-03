using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace TetrisClient
{
    public class Block
    {
        public BoardManager Bm { get; set; }

        public Tetromino Tetromino { get; set; }
        public SolidColorBrush Color { get; set; }

        public Matrix Shape { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public Block(BoardManager bm)
        {
            Bm = bm;
            X = ((bm.TetrisWell.GetUpperBound(1) - bm.TetrisWell.GetLowerBound(1)) / 2) - 1; // Center the Block

            Tetromino = BlockManager.GetRandomTetromino();
            Shape = BlockManager.GetTetrominoShape(Tetromino);
            Color = BlockManager.GetTetrominoColor(Tetromino);
        }

        /// <summary>
        /// Moves the Block right by 1.
        /// </summary>
        /// <returns>A boolean stating if the block was moved.</returns>
        public void MoveRight()
        {
            X += 1;
        }

        /// <summary>
        /// Moves the Block left by 1.
        /// </summary>
        /// <returns>A boolean stating if the block was moved.</returns>
        public void MoveLeft()
        {
            X -= 1;
        }

        /// <summary>
        /// Moves the Block down by 1.
        /// </summary>
        /// <returns>A boolean stating if the block was moved.</returns>
        public bool MoveDown()
        {
            Block tempBlock = Clone();
            tempBlock.Y += 1;
            if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
            {
                Y += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rotates the Block.
        /// </summary>
        public void Rotate()
        {
            Shape = Shape.Rotate90();
        }

        /// <summary>
        /// Places the Block into the tetris well.
        /// </summary>
        public void Place()
        {
            if (BlockManager.CanMove(Bm.TetrisWell, this))
            {
                System.Diagnostics.Debug.WriteLine(ColorArrayToString(Bm.TetrisWell));
                for (int i = 0; i < Shape.Value.GetLength(0); i++)
                {
                    for (int j = 0; j < Shape.Value.GetLength(1); j++)
                    {
                        if (Shape.Value[i, j] != 1)
                        {
                            continue;
                        }

                        Bm.TetrisWell[i + Y, j + X] = Color;
                    }
                }
                Bm.NextTurn();
            }
            else
            {
                Bm.EndGame();
            }
        }

        /// <summary>
        /// Creates a clone of the Block.
        /// </summary>
        /// <returns>A new copy of the Block instance.</returns>
        public Block Clone()
        {
            return (Block) MemberwiseClone();
        }

        /// <summary>
        /// Returns the ghost of the Block.
        /// </summary>
        /// <returns>A new Block instance.</returns>
        public Block CalculateGhost()
        {
            bool reachedEnd = false;
            Block ghostBlock = Clone();
            while (!reachedEnd)
            {
                if (!ghostBlock.MoveDown())
                {
                    reachedEnd = true;
                }
            }
            return ghostBlock;
        }

        /// <summary>
        /// Turns an array of colors into a neatly formatted string.
        /// </summary>
        /// <param name="colorArray">An array of Brush colors.</param>
        /// <returns>A string.</returns>
        public static string ColorArrayToString(SolidColorBrush[,] colorArray)
        {
            string output = "[";
            for (int row = 0; row < colorArray.GetLength(0); row++)
            {
                output += "[";
                for (int col = 0; col < colorArray.GetLength(1); col++)
                {
                    if (colorArray[row, col] != null)
                    {
                        output += colorArray[row, col].Color.ToString();
                    }
                    else
                    {
                        output += "#FFFFFFFF";
                    }
                    if (col != colorArray.GetUpperBound(1))
                    {
                        output += ",";
                    }
                }
                output += "]\n";
                if (row != colorArray.GetUpperBound(0))
                {
                    output += ",";
                }
            }
            output += "]";

            return output;
        }
    }

    /// <summary>
    /// An enum of all available Tetromino shapes.
    /// </summary>
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
        public static bool CanMove(SolidColorBrush[,] tetrisWell, Block block)
        {
            bool willCollide = false;

            for (int i = 0; i < block.Shape.Value.GetLength(0); i++)
            {
                for (int j = 0; j < block.Shape.Value.GetLength(1); j++)
                {
                    if (block.Shape.Value[i, j] != 1)
                    {
                        continue;
                    }

                    // Grid borders
                    if (block.Y + i < tetrisWell.GetLowerBound(0)
                        || block.Y + i >= tetrisWell.GetUpperBound(0) + 1
                        || block.X + j < tetrisWell.GetLowerBound(1)
                        || block.X + j >= tetrisWell.GetUpperBound(1) + 1)
                    {
                        willCollide = true;
                    }
                    // Overlapping cells
                    else if (tetrisWell[block.Y + i, block.X + j] != null)
                    {
                        willCollide = true;
                    }
                }
            }

            return !willCollide;
        }

        private static List<Tetromino> BlockBag = new List<Tetromino>(Enum.GetValues(typeof(Tetromino)).Length);

        /// <summary>
        /// Get a random Tetromino shape.
        /// </summary>
        /// <returns>A random Tetromino.</returns>
        public static Tetromino GetRandomTetromino()
        {
            //BlockBag.Sort(x => BoardManager.randStatus.Next());

            // ToDo: Make it select the 7 different tetrominos every 7 turns, but randomize the order.
            Array values = Enum.GetValues(typeof(Tetromino));
            return (Tetromino) values.GetValue(BoardManager.randStatus.Next(values.Length));
        }

        /// <summary>
        /// Get the shape Matrix of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the shape of.</param>
        /// <returns>The shape Matrix the given Tetromino.</returns>
        public static Matrix GetTetrominoShape(Tetromino tetromino)
        {
            return tetromino switch
            {
                Tetromino.IBlock => new Matrix(new int[,]
                                      {
                                { 0, 0, 0, 0 },
                                { 1, 1, 1, 1 },
                                { 0, 0, 0, 0 },
                                { 0, 0, 0, 0 },
                                      }
                                  ),
                Tetromino.JBlock => new Matrix(new int[,]
                   {
                                { 1, 0, 0 },
                                { 1, 1, 1 },
                                { 0, 0, 0 },
                   }
               ),
                Tetromino.LBlock => new Matrix(new int[,]
                  {
                                { 0, 0, 1 },
                                { 1, 1, 1 },
                                { 0, 0, 0 },
                  }
              ),
                Tetromino.OBlock => new Matrix(new int[,]
                  {
                                { 1, 1 },
                                { 1, 1 },
                  }
              ),
                Tetromino.SBlock => new Matrix(new int[,]
                  {
                                { 0, 1, 1 },
                                { 1, 1, 0 },
                                { 0, 0, 0 },
                  }
              ),
                Tetromino.ZBlock => new Matrix(new int[,]
                  {
                                { 1, 1, 0 },
                                { 0, 1, 1 },
                                { 0, 0, 0 },
                  }
              ),
                Tetromino.TBlock => new Matrix(new int[,]
                  {
                                { 1, 1, 1 },
                                { 0, 1, 0 },
                                { 0, 0, 0 },
                  }
              ),
                _ => new Matrix(new int[,]
                  {
                                { 0, 0, 0 },
                                { 0, 0, 0 },
                                { 0, 0, 0 },
                  }
              ),
            };
        }

        /// <summary>
        /// Get the color of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the color of.</param>
        /// <returns>The color of the given Tetromino.</returns>
        public static SolidColorBrush GetTetrominoColor(Tetromino tetromino)
        {
            return tetromino switch
            {
                Tetromino.IBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#78F0F0"),
                Tetromino.JBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#7878F0"),
                Tetromino.LBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#F0C878"),
                Tetromino.OBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#F0F078"),
                Tetromino.SBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#78F078"),
                Tetromino.ZBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#F07878"),
                Tetromino.TBlock => (SolidColorBrush) new BrushConverter().ConvertFrom("#C878F0"),
                _ => (SolidColorBrush) new BrushConverter().ConvertFrom("#C878F0"),
            };
        }
    }
}
