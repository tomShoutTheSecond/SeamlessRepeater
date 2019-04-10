using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SeamlessRepeater.Helper
{
    public static class CustomBrushes
    {
        /// <summary>
        /// 90% Gray
        /// </summary>
        public static Brush VeryLightGray => new SolidColorBrush(Color.FromRgb(230, 230, 230));

        /// <summary>
        /// 10% Gray
        /// </summary>
        public static Brush VeryDarkGray => new SolidColorBrush(Color.FromRgb(26, 26, 26));

        /// <summary>
        /// 15% Gray
        /// </summary>
        public static Brush DarkGray => new SolidColorBrush(Color.FromRgb(38, 38, 38));

        /// <summary>
        /// 20% Gray
        /// </summary>
        public static Brush WindowBackground => new SolidColorBrush(Color.FromRgb(51, 51, 51));

        public static Brush CheckerBoard
        {
            get
            {
                // Create a DrawingBrush and use it to
                // paint the rectangle.
                DrawingBrush checkerBoardBrush = new DrawingBrush();

                GeometryDrawing backgroundSquare =
                    new GeometryDrawing(
                        Brushes.White,
                        null,
                        new RectangleGeometry(new Rect(0, 0, 100, 100)));

                GeometryGroup aGeometryGroup = new GeometryGroup();
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 50, 50)));
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(50, 50, 50, 50)));

                var checkerBrush = Brushes.LightGray;//new LinearGradientBrush();
                                                     //checkerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                                                     //checkerBrush.GradientStops.Add(new GradientStop(Colors.Gray, 1.0));

                GeometryDrawing checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

                DrawingGroup checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(backgroundSquare);
                checkersDrawingGroup.Children.Add(checkers);

                double size = 0.05;
                checkerBoardBrush.Drawing = checkersDrawingGroup;
                checkerBoardBrush.Viewport = new Rect(0, 0, size, size);
                checkerBoardBrush.TileMode = TileMode.Tile;

                return checkerBoardBrush;
            }
            set { }
        }

        public static Brush CheckerBoardDark
        {
            get
            {
                // Create a DrawingBrush and use it to
                // paint the rectangle.
                DrawingBrush checkerBoardBrush = new DrawingBrush();

                GeometryDrawing backgroundSquare =
                    new GeometryDrawing(
                        CustomBrushes.WindowBackground,
                        null,
                        new RectangleGeometry(new Rect(0, 0, 100, 100)));

                GeometryGroup aGeometryGroup = new GeometryGroup();
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 50, 50)));
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(50, 50, 50, 50)));

                var checkerBrush = CustomBrushes.VeryDarkGray;//new LinearGradientBrush();
                                                     //checkerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                                                     //checkerBrush.GradientStops.Add(new GradientStop(Colors.Gray, 1.0));

                GeometryDrawing checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

                DrawingGroup checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(backgroundSquare);
                checkersDrawingGroup.Children.Add(checkers);

                double size = 0.05;
                checkerBoardBrush.Drawing = checkersDrawingGroup;
                checkerBoardBrush.Viewport = new Rect(0, 0, size, size);
                checkerBoardBrush.TileMode = TileMode.Tile;

                return checkerBoardBrush;
            }
            set { }
        }

        public static Brush CheckerBoardDarkSmall
        {
            get
            {
                int gridSize = 100;

                // Create a DrawingBrush and use it to
                // paint the rectangle.
                DrawingBrush checkerBoardBrush = new DrawingBrush();

                GeometryDrawing backgroundSquare =
                    new GeometryDrawing(
                        CustomBrushes.WindowBackground,
                        null,
                        new RectangleGeometry(new Rect(0, 0, gridSize, gridSize)));

                GeometryGroup aGeometryGroup = new GeometryGroup();
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, gridSize * 0.5, gridSize * 0.5)));
                aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(gridSize * 0.5, gridSize * 0.5, gridSize * 0.5, gridSize * 0.5)));

                var checkerBrush = CustomBrushes.VeryDarkGray;//new LinearGradientBrush();
                                                              //checkerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                                                              //checkerBrush.GradientStops.Add(new GradientStop(Colors.Gray, 1.0));

                GeometryDrawing checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

                DrawingGroup checkersDrawingGroup = new DrawingGroup();
                checkersDrawingGroup.Children.Add(backgroundSquare);
                checkersDrawingGroup.Children.Add(checkers);

                double size = 1;//0.05;
                checkerBoardBrush.Drawing = checkersDrawingGroup;
                checkerBoardBrush.Viewport = new Rect(0, 0, size, size);
                checkerBoardBrush.TileMode = TileMode.Tile;

                return checkerBoardBrush;
            }
            set { }
        }
    }
}
