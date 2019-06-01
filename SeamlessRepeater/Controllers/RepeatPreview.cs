using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SeamlessRepeater.Helper.ImageUtilities;
using static SeamlessRepeater.Helper.FileHelper;
using System.Windows.Media.Imaging;
using SeamlessRepeater.Windows;

namespace SeamlessRepeater.Helper
{
    public class RepeatPreview
    {
        public DrawingImage Image;
        public Image RepeatImage;

        private ZoomBorder _repeatZoomBorder;
        private Workspace _workspace;
        private MainWindow _window;
        private int _tilesPerRow = 5;

        public RepeatPreview(MainWindow window, Grid repeatPreviewHolder, Workspace workspace)
        {
            _window = window;
            _workspace = workspace;

            _window.RepeatOptionsBorder.Background = CustomBrushes.VeryDarkGray;
            _window.RepeatOptionsBorder.BorderBrush = Brushes.Gray;

            RepeatImage = new Image() { Width = Workspace.ImageGridSize, Height = Workspace.ImageGridSize };
            var repeatImageGrid = new Grid() { Width = Workspace.ImageGridSize, Height = Workspace.ImageGridSize, Background = CustomBrushes.CheckerBoardDark };
            repeatImageGrid.Children.Add(RepeatImage);
            _repeatZoomBorder = new ZoomBorder(_window, repeatPreviewHolder);
            _repeatZoomBorder.Child = repeatImageGrid;
            repeatPreviewHolder.Children.Add(_repeatZoomBorder);

            _window.PreviewZoomInButton.Click += (s, e) => PreviewZoom(true);
            _window.PreviewZoomOutButton.Click += (s, e) => PreviewZoom(false);
            _window.PreviewCenterButton.Click += (s, e) => _repeatZoomBorder.Reset();
            _window.PreviewRepeatButton.Click += (s, e) => DrawRepeat();
            _window.PreviewSizeUpButton.Click += (s, e) => OnPreviewSizeUp();
            _window.PreviewSizeDownButton.Click += (s, e) => OnPreviewSizeDown();

            _window.SaveRepeatButton.Click += (s, e) => SaveRepeat();

        }

        public void DrawRepeat()
        {
            var previewSource = RenderLayerPanelsInGrid(Workspace.ImageGrid, Workspace.ImageGridSize);
            
            Draw(previewSource, RepeatImage, _tilesPerRow);
        }

        private void Draw(DrawingImage source, Image target, int tilesPerRow)
        {
            var drawingGroup = new DrawingGroup();

            //draw transparent background to fill the drawing group (forces it to be the right size)
            drawingGroup.Children.Add(new GeometryDrawing(Workspace.BackgroundLayer.Background, null, new RectangleGeometry(new Rect(0, 0, target.Width, target.Height))));

            var imageToRepeat = source;
            var tileSize = Workspace.ImageGridSize / tilesPerRow;

            double imageWidth = 0, imageHeight = 0;
            if (source.Width != 0 || source.Height != 0) //avoid exception when removing last layer
            {
                (imageWidth, imageHeight) = ImageFit(imageToRepeat, tileSize, tileSize);
            }

            int horizontalTiles = tilesPerRow;
            int verticalTiles = tilesPerRow;
            for (int i = 0; i < horizontalTiles; i++)
            {
                for (int j = 0; j < verticalTiles; j++)
                {
                    var image = new ImageDrawing();
                    image.Rect = new Rect(imageWidth * i, imageHeight * j, imageWidth, imageHeight);
                    image.ImageSource = imageToRepeat;

                    drawingGroup.Children.Add(image);
                }
            }

            Image = new DrawingImage(drawingGroup);
            target.Source = Image;
        }

        public void PreviewZoom(bool isIn)
        {
            double scaleFactor = -0.2;
            if (isIn)
                scaleFactor = 0.2;

            _repeatZoomBorder.Zoom(scaleFactor);
        }
        
        private void SaveRepeat()
        {
            //get image output size from user
            var saveImageWindow = new SaveImageWindow();
            if (!saveImageWindow.ShowDialog() == true) return;

            var scale = saveImageWindow.ImageScale;

            //render layers and save them
            FileHelper.SaveAsPng(Image, scale);
        }

        private void OnPreviewSizeUp()
        {
            _tilesPerRow++;
            DrawRepeat();
        }

        private void OnPreviewSizeDown()
        {
            if (_tilesPerRow < 2) return;

            _tilesPerRow--;
            DrawRepeat();
        }
    }
}
