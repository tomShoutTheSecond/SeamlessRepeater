using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SeamlessRepeater.Helper
{
    public static class FileHelper
    {
        public static (bool success, BitmapImage image) OpenImage()
        {
            string fileName = null;

            var openFileDialog = new OpenFileDialog();
            if (!openFileDialog.ShowDialog() == true)
                return (false, null);

            fileName = openFileDialog.FileName;

            if (fileName == null)
                return (false, null);

            BitmapImage image = null;
            try
            {
                var uri = new Uri(fileName, UriKind.Absolute);//new Uri("Images/renaissance.jpg", UriKind.Relative);
                image = new BitmapImage(uri);
            }
            catch(ArgumentOutOfRangeException)
            {
                ErrorHandler.Handle("Image too large");
                return (false, null);
            }

            return (true, image);
        }

        public static bool SaveAsPng(DrawingImage drawing, double scale)
        {
            if (drawing == null || drawing.Width == 0 || drawing.Height == 0) return false;

            var drawingImage = new Image { Source = drawing };
            var width = drawing.Width * scale;
            var height = drawing.Height * scale;
            drawingImage.Arrange(new Rect(0, 0, width, height));

            var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingImage);

            return SaveAsPng(bitmap);
        }

        public static bool SaveAsPng(BitmapSource image)
        {
            if (image == null)
                return false;

            string fileName = null;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Image Files(*.png) | *.png";
            if (!saveFileDialog.ShowDialog() == true)
                return false;

            fileName = saveFileDialog.FileName;

            if (fileName == null)
                return false;

            var encoder = new PngBitmapEncoder();

            BitmapFrame frame = BitmapFrame.Create(image);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }

            return true;
        }
    }
}
