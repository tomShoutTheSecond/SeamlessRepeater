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
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public Color Color;

        public ColorPickerWindow(Color currentColor)
        {
            InitializeComponent();

            Color = currentColor;
            HexTextBox.Text = Color.ToString();

            WindowGrid.Background = CustomBrushes.WindowBackground;
            ColorPreviewOuterBorder.Background = CustomBrushes.VeryDarkGray;
            ColorPreviewBackground.Background = CustomBrushes.CheckerBoardDarkSmall;
            AlphaSlider.Value = currentColor.A / 2.55;
            AlphaLabel.Content = $"Alpha: {Math.Round(AlphaSlider.Value)}%";

            MouseDown += (s, e) => { OnHexTextChanged(); };
            KeyDown += OnKeyDown;
        }

        private void OnAlphaSliderMove(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateSelectedColor(Color);

            if (AlphaLabel != null)
                AlphaLabel.Content = $"Alpha: {Math.Round(e.NewValue)}%";
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnColorClick(object sender, MouseButtonEventArgs e)
        {
            var colorDialog1 = new System.Windows.Forms.ColorDialog();
            if(colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var windowsFormsColor = colorDialog1.Color;
                var backgroundColor = Color.FromArgb(windowsFormsColor.A, windowsFormsColor.R, windowsFormsColor.G, windowsFormsColor.B);
                UpdateSelectedColor(backgroundColor);
            }
        }

        private void UpdateSelectedColor(Color color, bool fromHexInput = false)
        {
            if (fromHexInput)
            {
                //update alpha if you changed the alpha hex value
                if (color.A != Color.A)
                    AlphaSlider.Value = color.A / 2.55;

                //set alpha to 100% if it was on 0%
                if (AlphaSlider.Value == 0)
                    AlphaSlider.Value = 100;
            }

            double alpha = Math.Round(AlphaSlider.Value * 2.55);
            var newColor = Color.FromArgb((byte)alpha, color.R, color.G, color.B);

            Color = newColor;
            ColorPreviewBorder.Background = new SolidColorBrush(newColor);

            HexTextBox.Text = Color.ToString();
        }

        private void OnHexTextChanged()
        {
            Keyboard.ClearFocus();

            string oldText = HexTextBox.Text;
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(HexTextBox.Text);
                UpdateSelectedColor(color, true);
            }
            catch(FormatException)
            {
                ErrorHandler.Handle("Invalid color string");
                UpdateSelectedColor(Color);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                OnHexTextChanged();
        }
    }
}
