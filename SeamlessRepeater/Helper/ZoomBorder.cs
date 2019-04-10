using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SeamlessRepeater.Helper
{
    public class ZoomBorder : Border
    {
        private bool _holdingCtrl;
        private Grid _parent;
        private UIElement _child = null;
        private Point _origin;
        private Point _start;
        private Window _window;

        public ZoomBorder(Window window, Grid parent)
        {
            ClipToBounds = true;
            _window = window;
            _parent = parent;

            _parent.Background = CustomBrushes.VeryDarkGray;
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this._child = element;
            if (_child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                _child.RenderTransform = group;
                _child.RenderTransformOrigin = new Point(0.0, 0.0);
                _parent.MouseWheel += OnMouseWheel;
                _parent.MouseRightButtonDown += OnMouseRightButtonDown;
                _parent.MouseRightButtonUp += OnMouseRightButtonUp;
                _parent.MouseMove += OnMouseMove;

                //for ctrl to pan
                _window.KeyDown += OnKeyDown;
                _window.KeyUp += OnKeyUp;
                _parent.MouseLeftButtonDown += OnMouseLeftButtonDown;
                _parent.MouseLeftButtonUp += OnMouseLeftButtonUp;
            }
        }

        public void Reset()
        {
            if (_child != null)
            {
                // reset zoom
                var st = GetScaleTransform(_child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(_child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        public void Zoom(double zoom, double? translationX = null, double? translationY = null)
        {
            var scaleTransform = GetScaleTransform(_child);
            var translateTransform = GetTranslateTransform(_child);

            double oldScaleX = scaleTransform.ScaleX;
            double oldScaleY = scaleTransform.ScaleY;

            //zoom to center if no translation is given
            if (translationX == null || translationY == null)
            {
                var parent = Parent as Panel;

                var centerPoint = new Point(parent.ActualWidth * 0.5, parent.ActualWidth * 0.5);
                var pointRelativeToChild = TranslatePoint(centerPoint, _child);
                translationX = pointRelativeToChild.X;
                translationY = pointRelativeToChild.Y;
            }

            //don't zoom out if scale is less than .4
            if (!(zoom > 0) && (scaleTransform.ScaleX < .4 || scaleTransform.ScaleY < .4))
                return;

            //make zoom amount be relative to the current magnification
            zoom *= scaleTransform.ScaleX;

            scaleTransform.ScaleX += zoom;
            scaleTransform.ScaleY += zoom;

            double absoluteX = translationX.Value * oldScaleX + translateTransform.X;
            double absoluteY = translationY.Value * oldScaleY + translateTransform.Y;

            translateTransform.X = absoluteX - translationX.Value * scaleTransform.ScaleX;
            translateTransform.Y = absoluteY - translationY.Value * scaleTransform.ScaleY;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _parent.Cursor = Cursors.Hand;
                _holdingCtrl = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _parent.Cursor = Cursors.Arrow;
                _holdingCtrl = false;
            }
        }

        #region Mouse Events

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child != null)
            {
                var scaleTransform = GetScaleTransform(_child);
                var translateTransform = GetTranslateTransform(_child);

                double zoom = e.Delta > 0 ? .1 : -.1;

                Point relative = e.GetPosition(_child);

                Zoom(zoom, relative.X, relative.Y);
            }
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _parent.Cursor = Cursors.Hand;

                var tt = GetTranslateTransform(_child);
                _start = e.GetPosition(this);
                _origin = new Point(tt.X, tt.Y);
                _child.CaptureMouse();
            }
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _parent.Cursor = Cursors.Arrow;
                _child.ReleaseMouseCapture();
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_holdingCtrl) return;

            OnMouseRightButtonDown(sender, e);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_holdingCtrl) return;

            OnMouseRightButtonUp(sender, e);
        }
        /*
        void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }
        */
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_child != null)
            {
                if (_child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(_child);
                    Vector v = _start - e.GetPosition(this);
                    tt.X = _origin.X - v.X;
                    tt.Y = _origin.Y - v.Y;
                }
            }
        }

        #endregion
    }
}