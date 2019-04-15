using Microsoft.Win32;
using SeamlessRepeater.Helper;
using SeamlessRepeater.Windows;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SeamlessRepeater.Helper.ImageUtilities;
using static SeamlessRepeater.Helper.ListExtensions;

namespace SeamlessRepeater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RepeatPreview RepeatPreview;

        private Workspace _workspace;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Initialize()
        {
            _workspace = new Workspace(this);
            RepeatPreview = new RepeatPreview(this, RepeatPreviewHolder, _workspace);
        }

        private void OnLayerMoveDownClick(object sender, RoutedEventArgs e)
        {
            _workspace.OnMoveDownClick(sender, e);
        }

        private void OnLayerMoveUpClick(object sender, RoutedEventArgs e)
        {
            _workspace.OnMoveUpClick(sender, e);
        }

        private void OnLayerDeleteClick(object sender, RoutedEventArgs e)
        {
            _workspace.OnDeleteClick(sender, e);
        }

        public void FinishImageImport(BitmapImage newImage)
        {
            _workspace.AddNewLayer(newImage);
        }

        private void OnResetWorkspaceView(object sender, RoutedEventArgs e)
        {
            _workspace.ResetWorkspaceView();
        }

        private void OnBackgroundColorClick(object sender, MouseButtonEventArgs e)
        {
            _workspace.ShowBackgroundColorPicker();
        }

        private void OnLayerRotateSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _workspace.OnLayerRotateSlider(sender, e);
        }

        private void OnLayerRotationEnd(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _workspace.OnLayerRotationEnd(sender, e);
        }
    }
}
