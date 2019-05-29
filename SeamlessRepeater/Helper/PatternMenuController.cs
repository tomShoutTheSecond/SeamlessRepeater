using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            switch (type)
            {
                case PatternType.Grid:
                    coordinates = new[] { new Point(0.25, 0.25), new Point(0.75, 0.75), new Point(0.75, 0.25), new Point(0.25, 0.75)  };
                    break;
                case PatternType.Hex:
                    coordinates = new[] { new Point(0.25, 0), new Point(0.75, 0), new Point(0, 0.5), new Point(0.5, 0.5) };
                    break;
            }

            int iterations = Math.Min(_workspace.Layers.Count, coordinates.Length);
            for (int i = 0; i < iterations; i++)
            {
                var layer = _workspace.Layers[i];
                var coordinate = coordinates[i];
                layer.SetCenterToPoint(coordinate, true);

                if (layer.Selected) layer.DrawOutline();
            }
        }
    }
}
