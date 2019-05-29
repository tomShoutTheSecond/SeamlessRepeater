using SeamlessRepeater.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SetOriginWindow.xaml
    /// </summary>
    public partial class SetOriginWindow : Window
    {
        private OriginPointer _originPointer;

        public SetOriginWindow()
        {
            InitializeComponent();

            InitializeWindow();
        }

        /// <summary>
        /// Called when window is shown
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            GetImageFromFile();
        }

        private void InitializeWindow()
        {
            SizeChanged += OnWindowResize;

            _originPointer = new OriginPointer(this, DrawableGrid);

            AcceptOriginButton.Click += (s, e) => Finish();
        }

        private void Finish()
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.FinishImageImport((BitmapImage)OriginPreviewImage.Source, _originPointer.GetPosition());
            Close();
        }

        private void OnWindowResize(object sender, SizeChangedEventArgs e)
        {
            _originPointer.Draw();
        }

        private void GetImageFromFile()
        {
            var (success, image) = FileHelper.OpenImage();
            if (!success)
            {
                Close();
                return;
            }

            OriginPreviewImage.Source = image;
        }
    }
}
