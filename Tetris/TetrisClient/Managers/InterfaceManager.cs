using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TetrisClient.GameManager
{
    public class InterfaceManager
    {
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
        /// Creates a Rectangle that can be used to draw a cell in the grid.
        /// </summary>
        /// <param name="fill">The color used to fill the Rectangle.</param>
        /// <param name="border">The color used for the border of the Rectangle.</param>
        /// <returns>A new Rectangle with the given colors.</returns>
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
    }
}