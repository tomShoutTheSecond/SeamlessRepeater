using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeamlessRepeater.Helper
{
    public class OriginPointer
    { 
        //fraction of the size of the image (0 - 1)
        private double _x = 0.5;
        private double _y = 0.5;

        private bool _moving;
        private Canvas _canvas;
        private Window _window;
        private Panel _panel;

        public OriginPointer(Window window, Panel panel)
        {
            _window = window;
            _panel = panel;

            _panel.MouseDown += OnMouseDown;
            _panel.MouseUp += OnMouseUp;
            _panel.MouseMove += OnMouseMove;
        }

        public void Draw()
        {
            _window.Dispatcher.Invoke(() =>
            {
                _panel.Children.Remove(_canvas);
                _canvas = new Canvas();
                _canvas.MouseUp += OnMouseUp;

                var brush = new SolidColorBrush() { Color = Colors.White };

                int pointerSize = 20;

                var horizontalLine = new Line();
                horizontalLine.X1 = _panel.ActualWidth * _x - pointerSize;
                horizontalLine.X2 = _panel.ActualWidth * _x + pointerSize;
                horizontalLine.Y1 = _panel.ActualHeight * _y;
                horizontalLine.Y2 = horizontalLine.Y1;

                horizontalLine.StrokeThickness = 2;
                horizontalLine.Stroke = brush;
                horizontalLine.HorizontalAlignment = HorizontalAlignment.Left;
                horizontalLine.VerticalAlignment = VerticalAlignment.Top;

                _canvas.Children.Add(horizontalLine);

                var verticalLine = new Line();
                verticalLine.X1 = _panel.ActualWidth * _x;
                verticalLine.X2 = verticalLine.X1;
                verticalLine.Y1 = _panel.ActualHeight * _y - pointerSize;
                verticalLine.Y2 = _panel.ActualHeight * _y + pointerSize;

                verticalLine.StrokeThickness = 2;
                verticalLine.Stroke = brush;
                verticalLine.HorizontalAlignment = HorizontalAlignment.Left;
                verticalLine.VerticalAlignment = VerticalAlignment.Top;

                _canvas.Children.Add(verticalLine);

                _panel.Children.Add(_canvas);
            });
        }

        public Point GetPosition()
        {
            return new Point(_x, _y);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _moving = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _moving = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_moving) return;

            var (newX, newY) = CoordinatesToFractions(Mouse.GetPosition(_panel));
            _x = newX;
            _y = newY;

            Draw();
        }

        private (double x, double y) CoordinatesToFractions(Point position)
        {
            return (position.X / _panel.ActualWidth, position.Y / _panel.ActualHeight);
        }
    }
}
