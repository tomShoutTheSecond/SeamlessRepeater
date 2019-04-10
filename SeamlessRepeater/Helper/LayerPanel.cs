using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static SeamlessRepeater.Helper.ImageUtilities;

namespace SeamlessRepeater.Helper
{
    /// <summary>
    /// Panel that contains the drawing of a layer
    /// </summary>
    public class LayerPanel : Grid
    {
        public int Index { get; private set; }
        public string LayerName { get; set; }
        public bool Selected => Index == SelectedLayerIndex;
        public double Angle;
        public DrawingGroup DrawingGroup;
        public static Brush SelectionColor = Brushes.White;
        public static int LastUsedIndex = 0;
        public static int SelectedLayerIndex { get; set; } = 0;
        public Visibility SelectedIndicatorVisibility
        {
            get
            {
                if (Selected)
                    return Visibility.Visible;

                return Visibility.Hidden;
            }
            private set { }
        }

        private bool _isFirstDraw = true;
        private bool _isFirstOutlineDraw = true;
        private double _imageSize;
        private Image _imageControl;
        private Image _outlineImageControl;
        private BitmapImage _originalBitmapImage;
        private BitmapImage _drawableBitmapImage;
        private List<Drawing> _oldLayers;
        private List<GeometryDrawing> _oldRects;
        private MainWindow _window;
        private (ImageDrawing, Rect?) _nullnull = (null, null);
        private DrawingGroup _outlineDrawingGroup;
        private Random _random = new Random();

        //for drag n drop
        private bool _isDragging;
        private double _x;
        private double _y;
        private double _oldX;
        private double _oldY;
        private double _offsetX;
        private double _offsetY;

        private enum Corner { None, TopLeft, TopRight, BottomLeft, BottomRight };

        public LayerPanel(MainWindow window, Panel parent, BitmapImage bitmapImage)
        {
            //set Index and Name
            Index = LastUsedIndex;
            LastUsedIndex++;
            LayerName = $"Layer {Index}";

            //set layer depth
            SetZIndex(this, Index);

            //set image size
            _imageSize = parent.Width * 0.4;

            //create drawing area

            DrawingGroup = new DrawingGroup();
            _outlineDrawingGroup = new DrawingGroup();
            _originalBitmapImage = bitmapImage;
            _originalBitmapImage.Freeze();
            _drawableBitmapImage = _originalBitmapImage;
            _window = window;

            _imageControl = new Image();
            _imageControl.Stretch = Stretch.None;
            _imageControl.HorizontalAlignment = HorizontalAlignment.Left;
            _imageControl.VerticalAlignment = VerticalAlignment.Top;

            Children.Add(_imageControl);

            //create outline drawing area

            _outlineImageControl = new Image();
            _outlineImageControl.Stretch = Stretch.None;
            _outlineImageControl.HorizontalAlignment = HorizontalAlignment.Left;
            _outlineImageControl.VerticalAlignment = VerticalAlignment.Top;

            Children.Add(_outlineImageControl);

            //create storage for layers that need to be cleared on next draw
            _oldLayers = new List<Drawing>();
            _oldRects = new List<GeometryDrawing>();

            //add mouse listeners
            parent.MouseDown += (s, e) => OnMouseDown();
            _window.MouseLeave += (s, e) => OnMouseUp();
            _window.MouseUp += (s, e) => OnMouseUp();
            _window.MouseMove += (s, e) => OnMouseMove();
        }

        public async Task Rotate(float angle)
        {
            Angle = angle;
            if(angle == 0)
            {
                _drawableBitmapImage = _originalBitmapImage;
                Dispatcher.Invoke(() =>
                {
                    DrawOutline();
                    Draw();
                });
                return;
            }

            Dispatcher.Invoke(() =>
            {
                _window.LoadingLabel.Visibility = Visibility.Visible;
                _window.Cursor = Cursors.Wait;
            });

            //TODO: this code is absolutely wank, size changes at random with rotation
            /*
            var oldSize = Math.Max(_drawableBitmapImage.PixelWidth, _drawableBitmapImage.PixelHeight);*/
            _drawableBitmapImage = await RotateImage(_originalBitmapImage, angle);
            /*var newSize = Math.Max(_drawableBitmapImage.PixelWidth, _drawableBitmapImage.PixelHeight);
            var sizeDifference = (double)newSize / (double)oldSize;
            _imageSize *= sizeDifference;*/

            Dispatcher.Invoke(() =>
            {
                DrawOutline();
                Draw();
            });
        }

        public void SizeDown()
        {
            var sizeChange = 50;

            if (_imageSize - sizeChange < Width * 0.1)
            {
                var oldImageSize = _imageSize;
                _imageSize = Width * 0.1;

                //keep central
                var sizeDifference = oldImageSize - _imageSize;
                _x += sizeDifference * 0.5;
                _y += sizeDifference * 0.5;

                Draw();
                DrawOutline();
                return;
            }

            _imageSize -= sizeChange;

            //keep central
            _x += sizeChange * 0.5;
            _y += sizeChange * 0.5;

            Draw();
            DrawOutline();
        }

        public void SizeUp()
        {
            var sizeChange = 50;

            if (_imageSize + sizeChange >= Width)
            {
                var oldImageSize = _imageSize;

                _imageSize = Width;

                //keep central
                var sizeDifference = oldImageSize - _imageSize;
                _x += sizeDifference * 0.5;
                _y += sizeDifference * 0.5;

                Draw();
                DrawOutline();
                return;
            }

            _imageSize += sizeChange;

            //keep central
            _x -= sizeChange * 0.5;
            _y -= sizeChange * 0.5;

            Draw();
            DrawOutline();
        }

        public void ClearOutline()
        {
            //clear rects from canvas
            foreach (var oldRect in _oldRects)
            {
                _outlineDrawingGroup.Children.Remove(oldRect);
            }
            _oldRects = new List<GeometryDrawing>();
        }

        public void DrawOutline()
        {
            try
            {
                //draw transparent background to fill the drawing group
                //this is required to stop image getting stuck in top left corner
                if (_isFirstOutlineDraw)
                {
                    _outlineDrawingGroup.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, Width, Height))));
                    _isFirstOutlineDraw = false;
                }

                ClearOutline();

                var mainLayer = new ImageDrawing();
                var (totalImageWidth, totalImageHeight) = ImageFit(_drawableBitmapImage, _imageSize, _imageSize);
                double leftCrop = 0;
                double rightCrop = 0;
                double topCrop = 0;
                double bottomCrop = 0;

                //calculate horizontal crop
                if (_x < 0)
                {
                    leftCrop = -_x;
                }
                else if (_x > Width - totalImageWidth)
                {
                    //rightCrop = -(Width - totalImageWidth) - _x;
                    rightCrop = _x - (Width - totalImageWidth);
                }

                //calculate vertical crop
                if (_y < 0)
                {
                    topCrop = -_y;
                }
                else if (_y > Height - totalImageHeight)
                {
                    //bottomCrop = -(Height - totalImageHeight) - _y;
                    bottomCrop = _y - (Height - totalImageHeight);
                }

                //crop image at canvas edges

                double bitmapPixelsPerScreenPixel = (double)_drawableBitmapImage.PixelWidth / (double)totalImageWidth;

                double croppedWidth = totalImageWidth - leftCrop - rightCrop;
                double croppedHeight = totalImageHeight - topCrop - bottomCrop;

                if (croppedWidth > 0 && croppedHeight > 0)
                {
                    //var layerSource = SafeMakeCroppedBitmap(_bitmapImage, (int)(leftCrop * bitmapPixelsPerScreenPixel), (int)(topCrop * bitmapPixelsPerScreenPixel), (int)(croppedWidth * bitmapPixelsPerScreenPixel), (int)(croppedHeight * bitmapPixelsPerScreenPixel));
                    mainLayer.Rect = new Rect(ClipToCanvasEdges(_x, _y), new Size(croppedWidth, croppedHeight));
                    //mainLayer.ImageSource = layerSource;

                    //draw rectangle around main layer
                    var rectangleDrawing = RectToDrawing(mainLayer.Rect, SelectionColor, true);
                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    //add main layer
                    //_outlineDrawingGroup.Children.Add(mainLayer);

                    //save reference to current layer
                    _oldRects.Add(rectangleDrawing);
                }
                else
                {
                    if (croppedWidth < 0)
                        croppedWidth = 0;
                    if (croppedHeight < 0)
                        croppedHeight = 0;
                }

                //add wrapped image sections

                var (_, leftEdgeRect) = GetLeftWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, topCrop, bitmapPixelsPerScreenPixel);
                if (leftEdgeRect != null)
                {
                    var rectangleDrawing = RectToDrawing(leftEdgeRect.Value, SelectionColor, true);

                    //add new layer
                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    //save for deletion on redraw
                    _oldRects.Add(rectangleDrawing);
                }

                var (_, rightEdgeRect) = GetRightWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, topCrop, bitmapPixelsPerScreenPixel);
                if (rightEdgeRect != null)
                {
                    var rectangleDrawing = RectToDrawing(rightEdgeRect.Value, SelectionColor, true);

                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    _oldRects.Add(rectangleDrawing);
                }

                var (_, topEdgeRect) = GetTopWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, leftCrop, bitmapPixelsPerScreenPixel);
                if (topEdgeRect != null)
                {
                    var rectangleDrawing = RectToDrawing(topEdgeRect.Value, SelectionColor, true);

                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    _oldRects.Add(rectangleDrawing);
                }

                var (_, bottomEdgeRect) = GetBottomWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, leftCrop, bitmapPixelsPerScreenPixel);
                if (bottomEdgeRect != null)
                {
                    var rectangleDrawing = RectToDrawing(bottomEdgeRect.Value, SelectionColor, true);

                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    _oldRects.Add(rectangleDrawing);
                }

                var (_, oppositeCornerRect) = GetOppositeCornerWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, bitmapPixelsPerScreenPixel);
                if (oppositeCornerRect != null)
                {
                    var rectangleDrawing = RectToDrawing(oppositeCornerRect.Value, SelectionColor, true);

                    _outlineDrawingGroup.Children.Add(rectangleDrawing);

                    _oldRects.Add(rectangleDrawing);
                }

                var imageSource = new DrawingImage(_outlineDrawingGroup);

                _outlineImageControl.Source = imageSource;
                _outlineImageControl.Width = Width;
                _outlineImageControl.Height = Height;
            }
            catch (ArgumentException exception)
            {
                ErrorHandler.Handle("An exception occured while trying to draw something", true, exception);
            }
        }

        public async void Draw()
        {
            //wait for initialization to finish
            while (_drawableBitmapImage == null)
                await Task.Delay(100);

            _window.LoadingLabel.Visibility = Visibility.Visible;

            _window.ForceCursor = true;
            _window.Cursor = Cursors.Wait;

            //allow UI to update
            await Task.Delay(5);

            try
            {
                //draw transparent background to fill the drawing group
                //this is required to stop image getting stuck in top left corner
                if (_isFirstDraw)
                {
                    DrawingGroup.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, Width, Height))));

                    _isFirstDraw = false;
                }

                //clear images from canvas
                foreach (var oldLayer in _oldLayers)
                {
                    DrawingGroup.Children.Remove(oldLayer);
                }
                _oldLayers = new List<Drawing>();

                var mainLayer = new ImageDrawing();
                var (totalImageWidth, totalImageHeight) = ImageFit(_drawableBitmapImage, _imageSize, _imageSize);
                double leftCrop = 0;
                double rightCrop = 0;
                double topCrop = 0;
                double bottomCrop = 0;

                //calculate horizontal crop
                if (_x < 0)
                {
                    leftCrop = -_x;
                }
                else if (_x > Width - totalImageWidth)
                {
                    //rightCrop = -(Width - totalImageWidth) - _x;
                    rightCrop = _x - (Width - totalImageWidth);
                }

                //calculate vertical crop
                if (_y < 0)
                {
                    topCrop = -_y;
                }
                else if (_y > Height - totalImageHeight)
                {
                    //bottomCrop = -(Height - totalImageHeight) - _y;
                    bottomCrop = _y - (Height - totalImageHeight);
                }

                //crop image at canvas edges

                double bitmapPixelsPerScreenPixel = (double)_drawableBitmapImage.PixelWidth / (double)totalImageWidth;

                double croppedWidth = totalImageWidth - leftCrop - rightCrop;
                double croppedHeight = totalImageHeight - topCrop - bottomCrop;

                if (croppedWidth > 0 && croppedHeight > 0)
                {
                    var layerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, (int)(leftCrop * bitmapPixelsPerScreenPixel), (int)(topCrop * bitmapPixelsPerScreenPixel), (int)(croppedWidth * bitmapPixelsPerScreenPixel), (int)(croppedHeight * bitmapPixelsPerScreenPixel));
                    mainLayer.Rect = new Rect(ClipToCanvasEdges(_x, _y), new Size(croppedWidth, croppedHeight));
                    mainLayer.ImageSource = layerSource;

                    //add main layer
                    DrawingGroup.Children.Add(mainLayer);

                    //save reference to current layer
                    _oldLayers.Add(mainLayer);
                }
                else
                {
                    if (croppedWidth < 0)
                        croppedWidth = 0;
                    if (croppedHeight < 0)
                        croppedHeight = 0;
                }

                //add wrapped image sections

                var (leftEdgeLayer, _) = GetLeftWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, topCrop, bitmapPixelsPerScreenPixel);
                if (leftEdgeLayer != null)
                {
                    //add new layer
                    DrawingGroup.Children.Add(leftEdgeLayer);

                    //save for deletion on redraw
                    _oldLayers.Add(leftEdgeLayer);
                }

                var (rightEdgeLayer, _) = GetRightWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, topCrop, bitmapPixelsPerScreenPixel);
                if (rightEdgeLayer != null)
                {
                    DrawingGroup.Children.Add(rightEdgeLayer);

                    _oldLayers.Add(rightEdgeLayer);
                }

                var (topEdgeLayer, _) = GetTopWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, leftCrop, bitmapPixelsPerScreenPixel);
                if (topEdgeLayer != null)
                {
                    DrawingGroup.Children.Add(topEdgeLayer);

                    _oldLayers.Add(topEdgeLayer);
                }

                var (bottomEdgeLayer, _) = GetBottomWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, leftCrop, bitmapPixelsPerScreenPixel);
                if (bottomEdgeLayer != null)
                {
                    DrawingGroup.Children.Add(bottomEdgeLayer);

                    _oldLayers.Add(bottomEdgeLayer);
                }

                var (oppositeCornerLayer, _) = GetOppositeCornerWrappedLayer(totalImageWidth, totalImageHeight, croppedWidth, croppedHeight, bitmapPixelsPerScreenPixel);
                if (oppositeCornerLayer != null)
                {
                    DrawingGroup.Children.Add(oppositeCornerLayer);

                    _oldLayers.Add(oppositeCornerLayer);
                }

                var imageSource = new DrawingImage(DrawingGroup);

                _imageControl.Source = imageSource;
                _imageControl.Width = Width;
                _imageControl.Height = Height;


                _window.Cursor = Cursors.Arrow;
            }
            catch (ArgumentException exception)
            {
                ErrorHandler.Handle("An exception occured while trying to draw something", true, exception);
            }
            finally
            {
                _window.LoadingLabel.Visibility = Visibility.Hidden;
                _window.Cursor = Cursors.Arrow;
            }
        }

        public void SetIndex(int newIndex)
        {
            Index = newIndex;
            SetZIndex(this, newIndex);
        }

        private (ImageDrawing image, Rect? layerRect) GetLeftWrappedLayer(double totalImageWidth, double totalImageHeight, double croppedWidth, double croppedHeight, double topCrop, double bitmapPixelsPerScreenPixel)
        {
            if (_x > Width - totalImageWidth)
                return _nullnull;

            //make layer for bit that wraps around left edge
            var leftEdgeLayer = new ImageDrawing();

            double displayWidth = totalImageWidth - croppedWidth;
            double displayHeight = croppedHeight;

            if (displayWidth <= 0 || displayHeight <= 0) return _nullnull;

            int bitmapWidth = (int)(displayWidth * bitmapPixelsPerScreenPixel);
            int bitmapHeight = (int)(displayHeight * bitmapPixelsPerScreenPixel);

            int bitmapTopCrop = (int)(topCrop * bitmapPixelsPerScreenPixel);

            var leftLayerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, 0, bitmapTopCrop, bitmapWidth, bitmapHeight);
            var layerRect = new Rect(ClipToCanvasEdges(Width + _x, _y), new Size(displayWidth, displayHeight));
            leftEdgeLayer.Rect = layerRect;
            leftEdgeLayer.ImageSource = leftLayerSource;

            return (leftEdgeLayer, layerRect);
        }

        private (ImageDrawing image, Rect? layerRect) GetRightWrappedLayer(double totalImageWidth, double totalImageHeight, double croppedWidth, double croppedHeight, double topCrop, double bitmapPixelsPerScreenPixel)
        {
            if (_x < 0)
                return _nullnull;

            //make layer for bit that wraps around right edge
            var rightEdgeLayer = new ImageDrawing();

            double displayWidth = totalImageWidth - croppedWidth;
            double displayHeight = croppedHeight;

            if (displayWidth <= 0 || displayHeight <= 0) return _nullnull;

            int bitmapWidth = (int)(displayWidth * bitmapPixelsPerScreenPixel);
            int bitmapHeight = (int)(displayHeight * bitmapPixelsPerScreenPixel);

            int bitmapLeftCrop = (int)(croppedWidth * bitmapPixelsPerScreenPixel);
            int bitmapTopCrop = (int)(topCrop * bitmapPixelsPerScreenPixel);

            var rightLayerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, bitmapLeftCrop, bitmapTopCrop, bitmapWidth, bitmapHeight);
            double xPosition = _x - Width;
            if (xPosition > Width - displayWidth) //if touching right edge of canvas
                xPosition = Width - displayWidth; //stop it going any further

            var layerRect = new Rect(ClipToCanvasEdges(xPosition, _y), new Size(displayWidth, displayHeight));
            rightEdgeLayer.Rect = layerRect; 
            rightEdgeLayer.ImageSource = rightLayerSource;

            return (rightEdgeLayer, layerRect);
        }

        private (ImageDrawing image, Rect? layerRect) GetTopWrappedLayer(double totalImageWidth, double totalImageHeight, double croppedWidth, double croppedHeight, double leftCrop, double bitmapPixelsPerScreenPixel)
        {
            if (_y > Height - totalImageHeight)
                return _nullnull;

            //make layer for bit that wraps around top edge
            var topEdgeLayer = new ImageDrawing();

            double displayWidth = croppedWidth;
            double displayHeight = totalImageHeight - croppedHeight;

            if (displayWidth <= 0 || displayHeight <= 0) return _nullnull;

            int bitmapWidth = (int)(displayWidth * bitmapPixelsPerScreenPixel);
            int bitmapHeight = (int)(displayHeight * bitmapPixelsPerScreenPixel);

            int bitmapLeftCrop = (int)(leftCrop * bitmapPixelsPerScreenPixel);

            var topLayerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, bitmapLeftCrop, 0, bitmapWidth, bitmapHeight);
            var layerRect = new Rect(ClipToCanvasEdges(_x, Height + _y), new Size(displayWidth, displayHeight));
            topEdgeLayer.Rect = layerRect;
            topEdgeLayer.ImageSource = topLayerSource;

            return (topEdgeLayer, layerRect);
        }

        private (ImageDrawing image, Rect? layerRect) GetBottomWrappedLayer(double totalImageWidth, double totalImageHeight, double croppedWidth, double croppedHeight, double leftCrop, double bitmapPixelsPerScreenPixel)
        {
            if (_y < 0)
                return _nullnull;

            //make layer for bit that wraps around bottom edge
            var bottomEdgeLayer = new ImageDrawing();

            double displayWidth = croppedWidth;
            double displayHeight = totalImageHeight - croppedHeight;

            if (displayWidth <= 0 || displayHeight <= 0) return _nullnull;

            int bitmapWidth = (int)(displayWidth * bitmapPixelsPerScreenPixel);
            int bitmapHeight = (int)(displayHeight * bitmapPixelsPerScreenPixel);

            int bitmapLeftCrop = (int)(leftCrop * bitmapPixelsPerScreenPixel);
            int bitmapTopCrop = (int)(croppedHeight * bitmapPixelsPerScreenPixel);

            var bottomLayerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, bitmapLeftCrop, bitmapTopCrop, bitmapWidth, bitmapHeight);
            double yPosition = _y - Height;
            if (yPosition > Height - displayHeight) //if touching bottom edge of canvas
                yPosition = Height - displayHeight; //stop it going any further

            var layerRect = new Rect(ClipToCanvasEdges(_x, yPosition), new Size(displayWidth, displayHeight));
            bottomEdgeLayer.Rect = layerRect;
            bottomEdgeLayer.ImageSource = bottomLayerSource;

            return (bottomEdgeLayer, layerRect);
        }

        private (ImageDrawing image, Rect? layerRect) GetOppositeCornerWrappedLayer(double totalImageWidth, double totalImageHeight, double croppedWidth, double croppedHeight, double bitmapPixelsPerScreenPixel)
        {
            var oppositeCornerLayer = new ImageDrawing();

            double displayWidth = totalImageWidth - croppedWidth;
            double displayHeight = totalImageHeight - croppedHeight;

            if (displayWidth <= 0 || displayHeight <= 0) return _nullnull;

            int bitmapWidth = (int)(displayWidth * bitmapPixelsPerScreenPixel);
            int bitmapHeight = (int)(displayHeight * bitmapPixelsPerScreenPixel);

            //find out which corner the image went off
            var whichCorner = Corner.None;
            if (_x < 0 && _y < 0)
                whichCorner = Corner.TopLeft;
            else if (_x < 0 && _y > Height - totalImageHeight)
                whichCorner = Corner.BottomLeft;
            else if (_x > Width - totalImageWidth && _y < 0)
                whichCorner = Corner.TopRight;
            else if (_x > Width - totalImageWidth && _y > Height - totalImageHeight)
                whichCorner = Corner.BottomRight;

            CroppedBitmap layerSource = null;
            Rect? layerRect = null;
            switch (whichCorner)
            {
                case Corner.None:
                    return _nullnull;
                case Corner.TopLeft:
                    {
                        layerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, 0, 0, bitmapWidth, bitmapHeight);
                        layerRect = new Rect(ClipToCanvasEdges(Width + _x, Height + _y), new Size(displayWidth, displayHeight));
                        oppositeCornerLayer.Rect = layerRect.Value;
                        oppositeCornerLayer.ImageSource = layerSource;
                    }
                    break;
                case Corner.TopRight:
                    {
                        layerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, (int)(totalImageWidth * bitmapPixelsPerScreenPixel - bitmapWidth), 0, bitmapWidth, bitmapHeight);
                        double xPosition = _x - Width;
                        if (xPosition > Width - displayWidth) //if touching right edge of canvas
                            xPosition = Width - displayWidth; //stop it going any further
                        layerRect = new Rect(ClipToCanvasEdges(xPosition, Height + _y), new Size(displayWidth, displayHeight));
                        oppositeCornerLayer.Rect = layerRect.Value;
                        oppositeCornerLayer.ImageSource = layerSource;
                    }
                    break;
                case Corner.BottomLeft:
                    {
                        layerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, 0, (int)(totalImageHeight * bitmapPixelsPerScreenPixel - bitmapHeight), bitmapWidth, bitmapHeight);
                        double yPosition = _y - Height;
                        if (yPosition > Height - displayHeight) //if touching bottom edge of canvas
                            yPosition = Height - displayHeight; //stop it going any further
                        layerRect = new Rect(ClipToCanvasEdges(Width + _x, yPosition), new Size(displayWidth, displayHeight));
                        oppositeCornerLayer.Rect = layerRect.Value;
                        oppositeCornerLayer.ImageSource = layerSource;
                        break;
                    }
                case Corner.BottomRight:
                    {
                        layerSource = SafeMakeCroppedBitmap(_drawableBitmapImage, (int)(totalImageWidth * bitmapPixelsPerScreenPixel - bitmapWidth), (int)(totalImageHeight * bitmapPixelsPerScreenPixel - bitmapHeight), bitmapWidth, bitmapHeight);
                        double xPosition = _x - Width;
                        if (xPosition > Width - displayWidth) //if touching right edge of canvas
                            xPosition = Width - displayWidth; //stop it going any further
                        double yPosition = _y - Height;
                        if (yPosition > Height - displayHeight) //if touching bottom edge of canvas
                            yPosition = Height - displayHeight; //stop it going any further
                        layerRect = new Rect(ClipToCanvasEdges(xPosition, yPosition), new Size(displayWidth, displayHeight));
                        oppositeCornerLayer.Rect = layerRect.Value;
                        oppositeCornerLayer.ImageSource = layerSource;
                    }
                    break;
            }

            return (oppositeCornerLayer, layerRect);
        }

        private CroppedBitmap SafeMakeCroppedBitmap(BitmapImage image, int x, int y, int width, int height)
        {
            if (width <= 0)
                return null;

            if (width > image.PixelWidth)
                width = image.PixelWidth;

            if (height <= 0)
                return null;

            if (height > image.PixelHeight)
                height = image.PixelHeight;

            var rect = new Int32Rect(x, y, width, height);
            return new CroppedBitmap(image, rect);
        }

        private Point ClipToCanvasEdges(double x, double y)
        {
            if (x < 0)
                x = 0;
            else if (x > Width)
                x = Width;

            if (y < 0)
                y = 0;
            else if (y > Height)
                y = Height;

            return new Point(x, y);
        }

        private void OnMouseDown()
        {
            if (!Selected) return;

            _isDragging = true;

            _oldX = _x;
            _oldY = _y;

            var point = Mouse.GetPosition(this);

            _offsetX = point.X - _x;
            _offsetY = point.Y - _y;

            _window.CaptureMouse();
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;

            _isDragging = false;

            _window.ReleaseMouseCapture();

            if(_oldX != _x || _oldY != _y) //if you moved the layer
                Draw();
        }

        private void OnMouseMove()
        {
            if (!Selected) return;

            if (!_isDragging) return;

            var point = Mouse.GetPosition(this);

            _x = point.X - _offsetX;
            _y = point.Y - _offsetY;

            DrawOutline();

            if (_window.RedrawCheckbox.IsChecked == true)
                Draw();
        }
    }
}
