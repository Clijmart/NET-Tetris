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
}
