using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SeamlessRepeater.Helper
{
    public static class ImageUtilities
    {
        /// <summary>
        /// Returns required size to fit "bitmap" into the desired area, preserving its aspect ratio
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static (double width, double height) ImageFit(BitmapSource bitmap, double maxWidth, double maxHeight)
        {
            if (bitmap == null)
                return (0, 0);

            double aspectRatio = (double)bitmap.PixelWidth / (double)bitmap.PixelHeight;

            if (aspectRatio > 1) //landscape image
            {
                double height = maxWidth / aspectRatio;
                return (maxWidth, height);
            }

            //portrait image
            double width = maxHeight * aspectRatio;

            return (width, maxHeight);
        }

        /// <summary>
        /// Returns required size to fit "bitmap" into the desired area, preserving its aspect ratio
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static (double width, double height) ImageFit(DrawingImage bitmap, double maxWidth, double maxHeight)
        {
            double aspectRatio = (double)bitmap.Width / (double)bitmap.Height;

            if (aspectRatio > 1) //landscape image
            {
                double height = maxWidth / aspectRatio;
                return (maxWidth, height);
            }

            //portrait image
            double width = maxHeight * aspectRatio;

            return (width, maxHeight);
        }

        /// <summary>
        /// Returns random image
        /// </summary>
        /// <returns>Random image</returns>
        public static BitmapImage GetImage()
        {
            //make new image
            string uriString = "pack://application:,,,/SeamlessRepeater;component/Images/LegoTrance.jpg";
            var uri = new Uri(uriString, UriKind.Absolute);
            return new BitmapImage(uri);
        }

        /// <summary>
        /// Returns an icon from the Icons folder
        /// </summary>
        /// <param name="iconName">Filename of icon, without directory or file extension (eg "circle-outline")</param>
        public static BitmapImage GetIcon(string iconName)
        {
            //make new image
            string uriString = $"pack://application:,,,/SeamlessRepeater;component/Icons/{iconName}.png";
            var uri = new Uri(uriString, UriKind.Absolute);
            return new BitmapImage(uri);
        }

        /// <summary>
        /// Renders all the layers held in a grid to a drawing
        /// </summary>
        /// <param name="grid">Grid holding LayerPanel(s)</param>
        /// <returns>Rendered drawing</returns>
        public static DrawingImage RenderLayerPanelsInGrid(Grid grid, double size)
        {
            var layerDrawings = new DrawingGroup();

            var imageRect = new Rect(0, 0, size, size);
            var backgroundLayerDrawing = RectToDrawing(imageRect, Workspace.BackgroundLayer.Background, false);
            layerDrawings.Children.Add(backgroundLayerDrawing);

            foreach (var child in grid.Children)
            {
                if (child is LayerPanel layer)
                {
                    layer.Draw();
                    var layerDrawing = new ImageDrawing(new DrawingImage(layer.DrawingGroup), imageRect);
                    layerDrawings.Children.Add(layerDrawing);
                }
            }

            var finalImage = new DrawingImage(layerDrawings);

            return finalImage;
        }

        /// <summary>
        /// This will produce a dirty image with dodgy render errors such as lines between tiles
        /// </summary>
        /// <param name="view"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static RenderTargetBitmap ControlToDirtyImage(FrameworkElement view, double scale = 1)
        {
            Size size = new Size(view.ActualWidth * scale, view.ActualHeight * scale);
            if (size.IsEmpty)
                return null;

            RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);
            return result;
        }

        /// <summary>
        /// Creates a GeometryDrawing from a Rect
        /// </summary>
        public static GeometryDrawing RectToDrawing(Rect rect, Brush brush, bool outline)
        {
            //draw rectangle around main layer
            var mainRectangle = new RectangleGeometry(rect);
            var rectangleDrawing = new GeometryDrawing(outline ? null : brush, outline ? new Pen(brush, 1) : null, mainRectangle);

            return rectangleDrawing;
        }

        public static Task<BitmapImage> RotateImage(BitmapImage bitmapImage, float angle)
        {
            return Task.Run(() =>
            {
                var longestSide = Math.Max(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                var size = new System.Drawing.Size(longestSide, longestSide);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (MemoryStream inStream = new MemoryStream())
                {
                    encoder.Save(inStream);

                    //reset stream back to start
                    inStream.Seek(0, SeekOrigin.Begin);

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            //Ideas to preserve image size,
                            //-crop to a square before rotating 
                            //-crop to original size after rotating

                            //-find a different way to determine size on LayerPanel.Draw()
                            //e.g calculate a "scale" when the image is loaded then display the image at this scale (regardless of dimensions)
                            //rather than using bounding box

                            imageFactory.Load(inStream)
                                        .Rotate(angle)
                                        .Format(new PngFormat())
                                        .Save(outStream);
                        }

                        var outputBitmap = new BitmapImage();
                        outputBitmap.BeginInit();
                        outputBitmap.StreamSource = outStream;
                        outputBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        outputBitmap.EndInit();
                        outputBitmap.Freeze();
                        return outputBitmap;
                    }
                }
            });
        }

        private static string ByteArrayToString(this byte[] array)
        {
            return BitConverter.ToString(array).Replace("-", "");
        }
    }
}
