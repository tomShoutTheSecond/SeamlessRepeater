using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SeamlessRepeater.Helper.ArrayExtensions;

namespace SeamlessRepeater.Helper
{
    public class PatternMenuController
    {
        private enum PatternType { Grid, Hex }
        private MainWindow _mainWindow;
        private Workspace _workspace;

        public PatternMenuController(MainWindow mainWindow, Workspace workspace)
        {
            _mainWindow = mainWindow;
            _workspace = workspace;

            mainWindow.PatternMenuGrid.Click += (s, e) => CreatePattern(PatternType.Grid);
            mainWindow.PatternMenuHex.Click += (s, e) => CreatePattern(PatternType.Hex);
        }

        private void CreatePattern(PatternType type)
        {
            Point[] coordinates = new Point[0];

            int density = (int)_mainWindow.PatternDensitySlider.Value;

            switch (type)
            {
                case PatternType.Grid:
                    coordinates = RecursivePointsGrid(density);//new[] { new Point(0.25, 0.25), new Point(0.75, 0.75), new Point(0.75, 0.25), new Point(0.25, 0.75)  };
                    break;
                case PatternType.Hex:
                    coordinates = new[] { new Point(0.25, 0), new Point(0.75, 0), new Point(0, 0.5), new Point(0.5, 0.5) };
                    break;
            }

            for (int i = 0; i < _workspace.Layers.Count; i++)
            {
                var layer = _workspace.Layers[i];
                var coordinate = GetCoordinate(coordinates, i);
                layer.SetCenterToPoint(coordinate, true);

                if (layer.Selected) layer.DrawOutline();
            }

            _workspace.SnapPoints = coordinates;
            _workspace.DrawSnapPoints();
        }

        private Point[] GeneratePointsGrid()
        {
            //cut the grid into four parts
            var topLeftRect = new GridRectangle(0, 0, 0.5, 0.5);
            var topRightRect = new GridRectangle(0.5, 0, 0.5, 0.5);
            var bottomLeftRect = new GridRectangle(0, 0.5, 0.5, 0.5);
            var bottomRightRect = new GridRectangle(0.5, 0.5, 0.5, 0.5);
            /*
            var topLeftCoords = topLeftRect.GetQuadrantCenterPoints();
            var topRightCoords = topRightRect.GetQuadrantCenterPoints();
            var bottomLeftCoords = bottomLeftRect.GetQuadrantCenterPoints();
            var bottomRightCoords = bottomRightRect.GetQuadrantCenterPoints();*/

            var topLeftCenter = topLeftRect.GetCenterPoint();
            var topRightCenter = topRightRect.GetCenterPoint();
            var bottomLeftCenter = bottomLeftRect.GetCenterPoint();
            var bottomRightCenter = bottomRightRect.GetCenterPoint();


            return new[] { topLeftCenter, topRightCenter, bottomLeftCenter, bottomRightCenter };//topLeftCoords.Append(topRightCoords).Append(bottomLeftCoords).Append(bottomRightCoords);
        }


        /// <summary>
        /// Creates increasingly smaller grids by splitting each GridRectangle into four each time
        /// Then returns the central points for each rectangle
        /// </summary>
        /// <param name="iterations"></param>
        private Point[] RecursivePointsGrid(int iterations)
        {
            var wholeGrid = new GridRectangle(0, 0, 1, 1);

            var rectanglesToSplit = new[] { wholeGrid };

            for (int i = 0; i < iterations; i++)
            {
                var quarters = SplitRectanglesInFour(rectanglesToSplit);
                rectanglesToSplit = quarters;
            }

            return GetCenterPoints(rectanglesToSplit);
        }

        private GridRectangle[] SplitRectanglesInFour(GridRectangle[] rectangles)
        {
            var centerPoints = new List<GridRectangle>();
            foreach(var rectangle in rectangles)
            {
                var quarters = rectangle.SplitIntoFour();
                centerPoints.AddRange(quarters);
            }

            return centerPoints.ToArray();
        }

        private Point[] GetCenterPoints(GridRectangle[] rectangles)
        {
            var centerPoints = new Point[rectangles.Length];
            for (int i = 0; i < rectangles.Length; i++)
            {
                var thisRectangle = rectangles[i];
                centerPoints[i] = thisRectangle.GetCenterPoint();
            }

            return centerPoints;
        }

        /// <summary>
        /// Gets a coordinate from the array for a specific layer
        /// </summary>
        private Point GetCoordinate(Point[] coordinates, int layerIndex)
        {
            //loop around the coordinates array when we reach the end so we can position infinite layers
            while (layerIndex > coordinates.Length - 1)
                layerIndex -= coordinates.Length;

            return coordinates[layerIndex];
        }

        /// <summary>
        /// Represents a rectangular portion of the ImageGrid
        /// Can calculate points of the center of each quadrant to produce the grid pattern coordinates
        /// _x, _y, _width and _height are a proportion of the ImageGrid's equivalent properties
        /// </summary>
        private class GridRectangle
        {
            private double _x;
            private double _y;
            private double _width;
            private double _height;

            public GridRectangle(double x, double y, double width, double height)
            {
                _x = x;
                _y = y;
                _width = width;
                _height = height;
            }
            /*
            /// <summary>
            /// Returns the central point for each quarter portion of this rectangle
            /// </summary>
            public Point[] GetQuadrantCenterPoints()
            {
                var topLeftPoint = new Point(_x + 0.25 * _width, _y + 0.25 * _height);
                var topRightPoint = new Point(_x + 0.75 * _width, _y + 0.25 * _height);
                var bottomLeftPoint = new Point(_x + 0.25 * _width, _y + 0.75 * _height);
                var bottomRightPoint = new Point(_x + 0.75 * _width, _y + 0.75 * _height);

                return new[] { topLeftPoint, topRightPoint, bottomLeftPoint, bottomRightPoint };
            }
            */

            public Point GetCenterPoint()
            {
                return new Point(_x + 0.5 * _width, _y + 0.5 * _height);
            }

            /// <summary>
            /// Returns a GridRectangle for each quarter of this GridRectangle
            /// </summary>
            public GridRectangle[] SplitIntoFour()
            {
                double halfWidth = _width * 0.5;
                double halfHeight = _height * 0.5;

                var topLeftRect = new GridRectangle(_x, _y, halfWidth, halfHeight);
                var topRightRect = new GridRectangle(_x + halfWidth, _y, halfWidth, halfHeight);
                var bottomLeftRect = new GridRectangle(_x, _y + halfHeight, halfWidth, halfHeight);
                var bottomRightRect = new GridRectangle(_x + halfWidth, _y + halfHeight, halfWidth, halfHeight);

                return new[] { topLeftRect, topRightRect, bottomLeftRect, bottomRightRect };
            }
        }
    }
}
