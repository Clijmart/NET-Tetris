using System;
using System.Collections.Generic;

namespace TetrisClient
{
    public static class BlockManager
    {
        /// <summary>
        /// Places a block in the tetris well.
        /// </summary>
        /// <param name="tetrisWell">The tetris well to place the block in.</param>
        /// <param name="block">The block to place.</param>
        /// <returns>The tetriswell including the placed block.</returns>
        public static string[,] PlaceBlockInWell(string[,] tetrisWell, Block block)
        {
            string[,] newWell = (string[,]) tetrisWell.Clone();
            for (int i = 0; i < block.Shape.Value.GetLength(0); i++)
            {
                for (int j = 0; j < block.Shape.Value.GetLength(1); j++)
                {
                    if (block.Shape.Value[i, j] != 1)
                    {
                        continue;
                    }

                    newWell[i + block.Y, j + block.X] = block.Color;
                }
            }

            return newWell;
        }

        /// <summary>
        /// Checks if the Block can be moved to the specified location in the tetris well.
        /// </summary>
        /// <param name="tetrisWell">The tetris well to check the block on.</param>
        /// <param name="block">The block to check on.</param>
        /// <returns>A boolean stating if the block can be moved.</returns>
        public static bool CanMove(string[,] tetrisWell, Block block)
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

        // A list filled with tetrominos.
        private static readonly List<Tetromino> BlockBag = new(Enum.GetValues(typeof(Tetromino)).Length);

        /// <summary>
        /// Get a random Tetromino shape from the BlockBag.
        /// </summary>
        /// <returns>A Tetromino.</returns>
        public static Tetromino GetRandomTetromino()
        {
            if (BlockBag.Count == 0)
            {
                foreach (Tetromino a in Enum.GetValues(typeof(Tetromino)))
                {
                    BlockBag.Add(a);
                }
            }
            int i = BoardManager.randStatus.Next(BlockBag.Count);
            Tetromino tetromino = BlockBag[i];
            BlockBag.RemoveAt(i);

            return tetromino;
        }

        /// <summary>
        /// Get the shape Matrix of a given Tetromino.
        /// </summary>
        /// <param name="tetromino">The Tetromino to find the shape of.</param>
        /// <returns>The shape Matrix of the given Tetromino.</returns>
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
        public static string GetTetrominoColor(Tetromino tetromino)
        {
            return tetromino switch
            {
                Tetromino.IBlock => "#00F0F0",
                Tetromino.JBlock => "#0000F0",
                Tetromino.LBlock => "#F0A000",
                Tetromino.OBlock => "#F0F000",
                Tetromino.SBlock => "#00F000",
                Tetromino.TBlock => "#A000F0",
                Tetromino.ZBlock => "#F00000",
                _ => "#A0A0F0",
            };
        }
    }
}
