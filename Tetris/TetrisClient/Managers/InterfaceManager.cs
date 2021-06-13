using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace TetrisClient.GameManager
{
    public class InterfaceManager
    {
        /// <summary>
        /// Draws a well on the grid.
        /// </summary>
        /// <param name="subGrid">The grid to draw the well onto.</param>
        /// <param name="well">The well to draw.</param>
        public static void DrawWell(Grid subGrid, string[,] well)
        {
            for (int i = 0; i < well.GetLength(0); i++)
            {
                for (int j = 0; j < well.GetLength(1); j++)
                {
                    DrawSquare(subGrid, well[i, j], j, i, false);
                }
            }
        }

        /// <summary>
        /// Draws a block on the grid.
        /// </summary>
        /// <param name="subGrid">The grid to draw the well onto.</param>
        /// <param name="block">The block to draw.</param>
        /// <param name="isGhost">Whether the block should be drawn as a ghost.</param>
        /// <param name="offset">Whether the block coordinates should be offset.</param>
        public static void DrawBlock(Grid subGrid, Block block, bool isGhost, bool offset)
        {
            int[,] shape = block.Shape.Value;
            for (int i = 0; i < shape.GetLength(0); i++)
            {
                for (int j = 0; j < shape.GetLength(1); j++)
                {
                    if (shape[i, j] == 1)
                    {
                        DrawSquare(subGrid, block.Color, offset ? j + block.X : j, offset ? i + block.Y : i, isGhost);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a square on the grid.
        /// </summary>
        /// <param name="subGrid">The grid to draw the square onto.</param>
        /// <param name="colorString">The color to draw the square.</param>
        /// <param name="x">The x coordinate to draw the square at.</param>
        /// <param name="y">The y coordinate to draw the square at.</param>
        /// <param name="isHollow">Whether the block should be made hollow.</param>
        public static void DrawSquare(Grid subGrid, string colorString, int x, int y, bool isHollow)
        {
            if (colorString == null)
            {
                return;
            }
            Color color = (Color) ColorConverter.ConvertFromString(colorString);
            Rectangle mainRectangle = CreateRectangle(color, isHollow);
            subGrid.Children.Add(mainRectangle);
            Grid.SetColumn(mainRectangle, x);
            Grid.SetRow(mainRectangle, y);

            // Creates the 3D effect if the square isn't hollow.
            if (!isHollow)
            {
                Polygon triangle = CreateTriangle(color);
                subGrid.Children.Add(triangle);
                Grid.SetColumn(triangle, x);
                Grid.SetRow(triangle, y);

                Rectangle innerRectangle = CreateInnerRectangle(color);
                subGrid.Children.Add(innerRectangle);
                Grid.SetColumn(innerRectangle, x);
                Grid.SetRow(innerRectangle, y);
            }
        }

        /// <summary>
        /// Creates a rectangle.
        /// </summary>
        /// <param name="color">The color to draw the rectangle.</param>
        /// <param name="isHollow">Whether the block should be made hollow.</param>
        /// <returns>A rectangle.</returns>
        public static Rectangle CreateRectangle(Color color, bool isHollow)
        {
            Rectangle rectangle = new()
            {
                Width = 25,
                Height = 25,
            };

            if (isHollow)
            {
                color = InterpolateColors(color, (Color) ColorConverter.ConvertFromString("#00FFFFFF"), 0.25f);
                rectangle.Stroke = new SolidColorBrush(color);
                rectangle.StrokeThickness = 2;
            }
            else
            {
                color = InterpolateColors(color, Brushes.White.Color, 0.25f);

                rectangle.Fill = new SolidColorBrush(color);
            }

            return rectangle;
        }

        /// <summary>
        /// Creates a triangle.
        /// </summary>
        /// <param name="color">The color to draw the triangle.</param>
        /// <returns>A polygon.</returns>
        public static Polygon CreateTriangle(Color color)
        {
            Polygon triangle = new()
            {
                Width = 25,
                Height = 25,
            };

            triangle.Fill = new SolidColorBrush(InterpolateColors(color, Brushes.Black.Color, 0.25f));
            triangle.Points = new PointCollection
            {
                new Point(0, 0), new Point(25, 25), new Point(0, 25)
            };

            return triangle;
        }

        /// <summary>
        /// Creates a small rectangle.
        /// </summary>
        /// <param name="color">The color to draw the rectangle.</param>
        /// <returns>A rectangle.</returns>
        public static Rectangle CreateInnerRectangle(Color color)
        {
            Rectangle rectangle = new()
            {
                Width = 17,
                Height = 17,
                Fill = new SolidColorBrush(color),
            };

            return rectangle;
        }

        /// <summary>
        /// Combines two colors.
        /// Primarily used to darken or lighten a given color.
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <param name="percentage">A float stating how much the second color should merge into the first.</param>
        /// <returns>A color.</returns>
        public static Color InterpolateColors(Color color1, Color color2, float percentage)
        {
            double a1 = color1.A / 255.0, r1 = color1.R / 255.0, g1 = color1.G / 255.0, b1 = color1.B / 255.0;
            double a2 = color2.A / 255.0, r2 = color2.R / 255.0, g2 = color2.G / 255.0, b2 = color2.B / 255.0;

            byte a3 = Convert.ToByte((a1 + (a2 - a1) * percentage) * 255);
            byte r3 = Convert.ToByte((r1 + (r2 - r1) * percentage) * 255);
            byte g3 = Convert.ToByte((g1 + (g2 - g1) * percentage) * 255);
            byte b3 = Convert.ToByte((b1 + (b2 - b1) * percentage) * 255);
            return Color.FromArgb(a3, r3, g3, b3);
        }

        /// <summary>
        /// Creates an info block.
        /// </summary>
        /// <param name="subGrid">The grid to add the block to.</param>
        /// <param name="text">Text to place in the block.</param>
        /// <param name="moveCount">Amount of times to move the block to the right.</param>
        /// <returns>A textblock.</returns>
        public static TextBlock CreateInfoBlock(Grid subGrid, string text, int moveCount)
        {
            TextBlock infoBlock = new()
            {
                Width = 250,
                Height = 50,
                Margin = new Thickness(275 * moveCount, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = text,
                FontWeight = FontWeights.Bold,
                FontFamily = (FontFamily) Application.Current.TryFindResource("MainFont"),
                FontSize = 30,
                Foreground = (SolidColorBrush) Application.Current.TryFindResource("Text"),
                TextAlignment = TextAlignment.Center,
            };

            subGrid.Children.Add(infoBlock);
            return infoBlock;
        }

        /// <summary>
        /// Creates a tetris grid.
        /// </summary>
        /// <param name="subGrid">The grid to add the tetris grid to.</param>
        /// <param name="moveCount">Amount of times to move the grid to the right.</param>
        /// <param name="rows">Amount of rows to add to the grid.</param>
        /// <param name="cols">Amount of columns to add to the grid.</param>
        /// <returns>A grid.</returns>
        public static Grid CreateTetrisGrid(Grid subGrid, int moveCount, int rows, int cols)
        {
            Grid tetrisGrid = new()
            {
                Width = 250,
                Height = 500,
                Margin = new Thickness(275 * moveCount, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = (SolidColorBrush) Application.Current.TryFindResource("Background"),
            };

            for (int i = 0; i < cols; i++)
            {
                ColumnDefinition gridCol = new();
                gridCol.Width = new GridLength(25);
                tetrisGrid.ColumnDefinitions.Add(gridCol);
            }

            for (int i = 0; i < rows; i++)
            {
                RowDefinition gridRow = new();
                gridRow.Height = new GridLength(25);
                tetrisGrid.RowDefinitions.Add(gridRow);
            }

            tetrisGrid.Effect = new DropShadowEffect()
            {
                BlurRadius = 10,
                Color = (Color) Application.Current.TryFindResource("TextColor"),
                ShadowDepth = 0,
                Opacity = 1
            };

            subGrid.Children.Add(tetrisGrid);
            return tetrisGrid;
        }
    }
}