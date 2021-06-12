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
                    DrawSquare(subGrid, well[i, j], null, j, i);
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
                        DrawSquare(subGrid, isGhost ? null : block.Color, isGhost ? block.Color : null, offset ? j + block.X : j, offset ? i + block.Y : i);
                    }
                }
            }
        } 

        public static void DrawSquare(Grid subGrid, string fill, string border, int x, int y)
        {
            Rectangle mainRectangle = CreateRectangle(fill, border);
            subGrid.Children.Add(mainRectangle);
            Grid.SetColumn(mainRectangle, x);
            Grid.SetRow(mainRectangle, y);

            if (fill != null)
            {
                Polygon triangle = CreateTriangle(fill);
                subGrid.Children.Add(triangle);
                Grid.SetColumn(triangle, x);
                Grid.SetRow(triangle, y);

                Rectangle innerRectangle = CreateInnerRectangle(fill);
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
        public static Rectangle CreateRectangle(string fill, string border)
        {
            Rectangle rectangle = new()
            {
                Width = 25,
                Height = 25,
                Stroke = (SolidColorBrush) new BrushConverter().ConvertFrom(border ?? "#FFFFFFFF"),
                StrokeThickness = fill == null && border != null ? 3 : 1,
                Fill = fill == null ? null : (SolidColorBrush) new BrushConverter().ConvertFrom(fill),
            };

            return rectangle;
        }

        public static Polygon CreateTriangle(string fill)
        {
            Polygon triangle = new()
            {
                Width = 23,
                Height = 23,
            };

            SolidColorBrush innerColor = (SolidColorBrush) new BrushConverter().ConvertFrom(fill);
            innerColor.Color = InterpolateColors(innerColor.Color, Brushes.Black.Color, 0.25f);
            triangle.Fill = innerColor;
            triangle.Points = new PointCollection
            {
                new Point(1, 1), new Point(24, 24), new Point(1, 24)
            };
            
            return triangle;
        }

        public static Rectangle CreateInnerRectangle(string fill)
        {
            Rectangle rectangle = new()
            {
                Width = 15,
                Height = 15,
            };

            if (fill != null)
            {
                SolidColorBrush innerColor = (SolidColorBrush) new BrushConverter().ConvertFrom(fill);
                innerColor.Color = InterpolateColors(innerColor.Color, Brushes.White.Color, 0.25f);
                rectangle.Fill = innerColor;
            }

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
