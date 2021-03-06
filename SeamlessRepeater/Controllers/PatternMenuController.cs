﻿using System;
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
            var gridRectangles = GenerateRecursiveRectangles(density);

            switch (type)
            {
                case PatternType.Grid:
                    coordinates = GetCenterPoints(gridRectangles);
                    break;
                case PatternType.Hex:
                    coordinates = GetHexPoints(gridRectangles);
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

        /// <summary>
        /// Creates increasingly smaller grids by splitting each GridRectangle into four each time
        /// Then returns the central points for each rectangle
        /// </summary>
        /// <param name="iterations"></param>
        private GridRectangle[] GenerateRecursiveRectangles(int iterations)
        {
            var wholeGrid = new GridRectangle(0, 0, 1, 1);

            var rectanglesToSplit = new[] { wholeGrid };

            for (int i = 0; i < iterations; i++)
            {
                var quarters = SplitRectanglesInFour(rectanglesToSplit);
                rectanglesToSplit = quarters;
            }

            return rectanglesToSplit;
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

        private Point[] GetHexPoints(GridRectangle[] rectangles)
        {
            var hexPoints = new List<Point>();
            foreach(var rectangle in rectangles)
            {
                hexPoints.AddRange(rectangle.GetHexPoints(_mainWindow.PatternOffsetSlider.Value));
            }

            return hexPoints.ToArray();
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

            public Point GetCenterPoint()
            {
                return new Point(_x + 0.5 * _width, _y + 0.5 * _height);
            }

            public Point[] GetHexPoints(double offset)
            {
                //offset is 0-1
                double xOffset = offset * 0.5;

                return new[] { new Point(_x + xOffset * _width, _y), new Point(_x + xOffset + 0.5 * _width, _y), new Point(_x, _y + 0.5 * _height), new Point(_x + 0.5 * _width, _y + 0.5 * _height) };
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
