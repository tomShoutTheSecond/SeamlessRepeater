using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SeamlessRepeater.Helper.ImageUtilities;

namespace SeamlessRepeater.Helper
{
    public class CustomScrollbar : Grid
    {
        public EventHandler<double> ValueChanged;

        public double Value
        {
            get { return _value; }
            set
            {
                var oldValue = _value;
                if (oldValue != value)
                {
                    _value = value;
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        private Image _imageControl;
        private double _value;
        private bool _dragging;
        private double _sliderSize = 64;

        public CustomScrollbar()
        {
            _imageControl = new Image() { HorizontalAlignment = HorizontalAlignment.Stretch };
            Children.Add(_imageControl);

            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;

            //Application.Current.MainWindow.
            SizeChanged += (s, e) => Draw();

            Draw();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_dragging) return;

            var mousePoint = e.GetPosition(this);
            /*
            if (mousePoint.X < _sliderSize * 0.5)
                mousePoint.X = _sliderSize * 0.5;

            if (mousePoint.X > ActualWidth - _sliderSize * 0.5)
                mousePoint.X = ActualWidth - _sliderSize * 0.5;
                */
            double sliderRangeDistance = ActualWidth - _sliderSize;
            mousePoint.X -= _sliderSize * 0.5;

            var newValue = mousePoint.X / sliderRangeDistance;
            if (newValue < 0)
                Value = 0;
            else if (newValue > 1)
                Value = 1;
            else
                Value = newValue;

            Draw();
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragging = true;
            CaptureMouse();
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragging = false;
            ReleaseMouseCapture();
        }

        private void Draw()
        {
            var drawingGroup = new DrawingGroup();
            
            //create transparent background drawing to fill canvas
            var backgroundRect = new Rect(0, 0, ActualWidth, ActualHeight);
            var backgroundDrawing = RectToDrawing(backgroundRect, Brushes.Transparent, false);
            drawingGroup.Children.Add(backgroundDrawing);

            //create bar drawing 
            double sliderRangeDistance = ActualWidth - _sliderSize;
            double sliderCenter = Value * sliderRangeDistance;
            var rect = new Rect(sliderCenter, 0, _sliderSize, 16);
            var drawing = RectToDrawing(rect, CustomBrushes.VeryLightGray, false);
            drawingGroup.Children.Add(drawing);

            _imageControl.Source = new DrawingImage(drawingGroup);
        }
    }
}
