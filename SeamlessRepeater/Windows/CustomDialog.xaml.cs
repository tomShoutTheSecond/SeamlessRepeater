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
    public enum CustomDialogType
    {
        OK = 1,
        YesNo = 2,
        TextInput = 3
    }
    /// <summary>
    /// Dialog that gets a string from the user
    /// </summary>
    public partial class CustomDialog : Window
    {
        public string DialogTitle { get; set; }
        public string DialogMessage { get; set; }
        public string DialogInputText { get; set; }

        public CustomDialog(string title, string message, CustomDialogType dialogType, string placeholderText = null)
        {
            DialogTitle = title;
            DialogMessage = message;
            DialogInputText = placeholderText;

            InitializeComponent();

            InputTextBox.Focus();
            InputTextBox.CaretIndex = DialogInputText == null ? 0 : DialogInputText.Length;

            Background = CustomBrushes.WindowBackground;
            ButtonGrid.Background = CustomBrushes.DarkGray;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            switch(dialogType)
            {
                case CustomDialogType.OK:
                    InputTextBox.Visibility = Visibility.Collapsed;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case CustomDialogType.YesNo:
                    InputTextBox.Visibility = Visibility.Collapsed;
                    OkButton.Content = "Yes";
                    CancelButton.Content = "No";
                    break;
                case CustomDialogType.TextInput:
                    break;
            }
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                DialogResult = true;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
