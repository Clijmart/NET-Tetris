using System;
using System.Collections.Generic;
using System.Media;
using TetrisClient.Managers;

namespace TetrisClient
{
    public class Block
    {
        public BoardManager Bm { get; set; }

        public Tetromino Tetromino { get; set; }
        public string Color { get; set; }

        public Matrix Shape { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public Block(BoardManager bm)
        {
            Bm = bm;

            Tetromino = BlockManager.GetRandomTetromino();
            Shape = BlockManager.GetTetrominoShape(Tetromino);
            Color = BlockManager.GetTetrominoColor(Tetromino);

            X = ((bm.TetrisWell.GetUpperBound(1) - bm.TetrisWell.GetLowerBound(1)) / 2) - ((Shape.Value.GetLength(1) - 1) / 2); // Center the Block
        }

        /// <summary>
        /// Moves the Block right by 1.
        /// </summary>
        /// <returns>A boolean stating if the block was moved.</returns>
        public bool MoveRight()
        {
            Block tempBlock = Clone();
            tempBlock.X += 1;
            if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
            {
                X += 1;
                Bm.GhostBlock = CalculateGhost();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Moves the Block left by 1.
        /// </summary>
        /// <returns>A boolean stating if the block was moved.</returns>
        public bool MoveLeft()
        {
            Block tempBlock = Clone();
            tempBlock.X -= 1;
            if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
            {
                X -= 1;
                Bm.GhostBlock = CalculateGhost();
                return true;
            }
            return false;
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
        /// <returns>A boolean stating if the block was rotated.</returns>
        public bool Rotate()
        {
            bool foundRotation = false;

            // Wall kicking
            // If a block can not be rotated, try to find a different solution.
            List<int> tryX = new() { 0, 1, -1 };
            if (Tetromino == Tetromino.IBlock)
            {
                tryX.AddRange(new int[] { 2, -2 });
            }

            Block tempBlock = Clone();
            tempBlock.Shape = tempBlock.Shape.Rotate90();

            foreach (int x in tryX)
            {
                tempBlock.X = X + x;
                if (BlockManager.CanMove(Bm.TetrisWell, tempBlock))
                {
                    foundRotation = true;
                    break;
                }
            }

            // If a solution was found, rotate the block.
            if (foundRotation)
            {
                if (SettingManager.GameSoundsOn)
                {
                    new SoundPlayer(new Uri(Environment.CurrentDirectory + "/Resources/RotateBlock.wav", UriKind.Relative).ToString()).Play();
                }
                X = tempBlock.X;
                Shape = tempBlock.Shape;
                Bm.GhostBlock = CalculateGhost();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Places the Block into the tetris well.
        /// </summary>
        public void Place()
        {
            if (BlockManager.CanMove(Bm.TetrisWell, this))
            {
                if (SettingManager.GameSoundsOn)
                {
                    new SoundPlayer(new Uri(Environment.CurrentDirectory + "/Resources/PlaceBlock.wav", UriKind.Relative).ToString()).Play();
                }
                Bm.TetrisWell = BlockManager.PlaceBlockInWell(Bm.TetrisWell, this);
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
