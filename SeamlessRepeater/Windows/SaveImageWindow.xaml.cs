using SeamlessRepeater.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeamlessRepeater.Windows
{
    /// <summary>
    /// Interaction logic for SaveImageWindow.xaml
    /// </summary>
    public partial class SaveImageWindow : Window
    {
        public double ImageSize;
        public double ImageScale;

        public SaveImageWindow()
        {
            InitializeComponent();

            WindowGrid.Background = CustomBrushes.WindowBackground;

            OkButton.Click += OkButton_Click;
            Slider.ValueChanged += Slider_ValueChanged;
            Slider.TickFrequency = 0.25;
            Slider.IsSnapToTickEnabled = true;
            Slider.Value = 5;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 0)
                OkButton.IsEnabled = false;
            else
                OkButton.IsEnabled = true;

            int newSize = (int)(Workspace.ImageGridSize * e.NewValue);
            ImageSizeLabel.Content = $"Tile size: {newSize} x {newSize}px";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ImageScale = Slider.Value;
            ImageSize = ImageScale * Workspace.ImageGridSize;

            DialogResult = true;
        }
    }
}
