using SeamlessRepeater.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using System.Windows.Shapes;

namespace SeamlessRepeater.Helper
{
    public class Workspace
    {
        public const int ImageGridSize = 500;
        public static Grid BackgroundLayer;
        public static Grid ImageGrid;
        public Point[] SnapPoints;
        public List<LayerPanel> Layers { get; set; }

        private ZoomBorder _workspaceZoomBorder;
        private MainWindow _window;
        private PatternMenuController _patternController;
        private string _oldAngleText;
        private List<Ellipse> _snapPointMarkers = new List<Ellipse>();

        public Workspace(MainWindow window)
        {
            _window = window;

            //set window colors
            _window.WindowGrid.Background = CustomBrushes.WindowBackground;
            _window.BackgroundLayerOption.Background = CustomBrushes.VeryDarkGray;
            _window.LayerSettingsSection.Background = CustomBrushes.VeryDarkGray;
            _window.WorkspaceButtonsBorder.Background = CustomBrushes.VeryDarkGray;

            ImageGrid = new Grid() { Background = CustomBrushes.CheckerBoardDark, Width = ImageGridSize, Height = ImageGridSize };
            BackgroundLayer = new Grid();
            ImageGrid.Children.Add(BackgroundLayer);
            _workspaceZoomBorder = new ZoomBorder(_window, _window.WorkspaceHolder);
            _workspaceZoomBorder.Child = ImageGrid;
            _window.WorkspaceHolder.Children.Add(_workspaceZoomBorder);
            ImageGrid.ClipToBounds = true;

            _window.MouseDown += (s, e) => OnWindowMouseDown();
            _window.KeyDown += OnWindowKeyDown;
            _window.AddLayerButton.Click += (s, e) => StartImageImport();
            _window.SaveImageButton.Click += (s, e) => SaveImage();
            _window.WorkspaceZoomInButton.Click += (s, e) => WorkspaceZoom(true);
            _window.WorkspaceZoomOutButton.Click += (s, e) => WorkspaceZoom(false);
            _window.LayerSizeDownButton.Click += (s, e) => LayerSizeDown();
            _window.LayerSizeUpButton.Click += (s, e) => LayerSizeUp();

            Layers = new List<LayerPanel>();
            _window.LayersListView.Background = CustomBrushes.VeryDarkGray;
            _window.LayersListView.ItemsSource = Layers;
            _window.LayersListView.SelectionChanged += OnLayerSelected;
            _window.LayersListView.MouseDoubleClick += OnLayerDoubleClick;

            _window.BackgroundColorBackground.Background = CustomBrushes.CheckerBoardDarkSmall;

            _patternController = new PatternMenuController(window, this);
        }

        public void WorkspaceZoom(bool isIn)
        {
            double scaleFactor = -0.2;
            if (isIn)
                scaleFactor = 0.2;

            _workspaceZoomBorder.Zoom(scaleFactor);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            //update layer angle if text changed (on enter key press)
            if (e.Key == Key.Return)
                OnWindowMouseDown();
        }

        private void OnWindowMouseDown()
        {
            UpdateAngleFromTextBox();

            Keyboard.ClearFocus();
        }

        private void UpdateAngleFromTextBox()
        {
            //update layer angle if text changed

            if (Layers.Count == 0)
            {
                ResetAngleTextBox();
                return;
            }

            //remove the ° if the user entered one
            string angleText = _window.LayerAngleTextBox.Text;

            if (_oldAngleText == angleText) return;
            _oldAngleText = angleText;

            if (angleText.EndsWith("°"))
                angleText = angleText.Substring(0, _window.LayerAngleTextBox.Text.Length - 1);

            if (!((float.TryParse(angleText, out float newAngle) && newAngle > 0 && newAngle <= 360))) 
            {
                ResetAngleTextBox();
                return;
            }

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            Task.Run(async () => await selectedLayer.Rotate(newAngle));

            _window.LayerRotationSlider.Value = newAngle;

            //put the ° back
            _window.LayerAngleTextBox.Text = $"{newAngle}°";
        }

        private void ResetAngleTextBox()
        {
            var angle = (float)_window.LayerRotationSlider.Value;
            _window.LayerAngleTextBox.Text = $"{angle}°";
        }

        public void ShowBackgroundColorPicker()
        {
            var currentBackgroundBrush = (SolidColorBrush)BackgroundLayer.Background;
            var currentBackgroundColor = currentBackgroundBrush == null ? Colors.Transparent : currentBackgroundBrush.Color;
            var colorDialog = new ColorPickerWindow(currentBackgroundColor);
            if (colorDialog.ShowDialog() == true)
            {
                ChangeBackgroundColor(colorDialog.Color);
            }
        }

        private void ChangeBackgroundColor(Color newColor)
        {
            var backgroundBrush = new SolidColorBrush(newColor);
            BackgroundLayer.Background = backgroundBrush;

            LayerPanel.SelectionColor = GetNewLayerSelectionColor(newColor);

            foreach (var layer in Layers)
                layer.DrawOutline();

            _window.RepeatPreview.DrawRepeat();
            _window.BackgroundColorPicker.Background = backgroundBrush;
        }

        private void LayerSizeDown()
        {
            if (Layers.Count < 1) return;

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            selectedLayer.SizeDown();
        }

        private void LayerSizeUp()
        {
            if (Layers.Count < 1) return;

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            selectedLayer.SizeUp();
        }

        private void SwapLayers(int firstIndex, int secondIndex)
        {
            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];

            //change indexes
            Layers[firstIndex].SetIndex(secondIndex);
            Layers[secondIndex].SetIndex(firstIndex);

            //swap position in the list (changes order in ListView)
            Layers.Swap(firstIndex, secondIndex);

            LayerPanel.SelectedLayerIndex = selectedLayer.Index;

            _window.LayersListView.Items.Refresh();
        }

        public void OnMoveDownClick(object sender, RoutedEventArgs e)
        {
            if (Layers.Count < 2) return;

            var button = ((Button)sender);
            var row = (LayerPanel)button.DataContext;

            var listIndex = Layers.IndexOf(row);
            var layerAboveListIndex = listIndex - 1;

            if (layerAboveListIndex < 0) return; //is already on top

            SwapLayers(listIndex, layerAboveListIndex);
        }

        public void OnMoveUpClick(object sender, RoutedEventArgs e)
        {
            if (Layers.Count < 2) return;

            var button = ((Button)sender);
            var row = (LayerPanel)button.DataContext;

            var listIndex = Layers.IndexOf(row);
            var layerAboveListIndex = listIndex + 1;

            if (layerAboveListIndex > Layers.Count - 1) return; //is already on top

            SwapLayers(listIndex, layerAboveListIndex);
        }

        public void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomDialog("Dialog", "Delete layer?", CustomDialogType.YesNo);
            if (dialog.ShowDialog() != true) return;

            var button = ((Button)sender);
            var row = (LayerPanel)button.DataContext;
            DeleteLayer(row);
        }

        private void OnLayerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_window.LayersListView.SelectedIndex < 0) return;

            LayerPanel.SelectedLayerIndex = _window.LayersListView.SelectedIndex;

            _window.LayersListView.Items.Refresh();

            foreach (var layer in Layers)
                layer.ClearOutline();

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            _window.LayerSettingsTitle.Content = selectedLayer.LayerName;
            selectedLayer.DrawOutline();

            //TODO: update slider position and angle text here with selectedLayer.Angle

        }

        private void OnLayerDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnLayerSelected(sender, null);

            if (_window.LayersListView.SelectedIndex == -1) return;

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            RenameLayer(selectedLayer);
        }

        private void RenameLayer(LayerPanel layer)
        {
            var dialog = new CustomDialog("Rename", "Rename layer", CustomDialogType.TextInput, layer.LayerName);
            if (dialog.ShowDialog() == true)
            {
                layer.LayerName = dialog.DialogInputText;
                _window.LayersListView.Items.Refresh();
            }
        }

        public void ResetWorkspaceView()
        {
            _workspaceZoomBorder.Reset();
        }

        public void DrawSnapPoints()
        {
            foreach(var marker in _snapPointMarkers)
            {
                ImageGrid.Children.Remove(marker);
            }

            foreach(var snapPoint in SnapPoints)
            {
                AddSnapPointMarker(snapPoint);

                var opposingSnapPoint = GetOpposingPointIfOnTheEdge(snapPoint);
                if (opposingSnapPoint != null)
                    AddSnapPointMarker(opposingSnapPoint.Value);
            }

            foreach (var layer in Layers)
            {
                layer.Draw();
            }
        }

        private void AddSnapPointMarker(Point snapPoint)
        {
            int markerSize = 8;

            var drawPosition = new Point(snapPoint.X * ImageGridSize, snapPoint.Y * ImageGridSize);

            var circle = new Ellipse();

            circle.Fill = Brushes.LightGray;
            circle.StrokeThickness = 1;
            circle.Stroke = Brushes.Black;

            circle.Width = markerSize;
            circle.Height = markerSize;

            circle.HorizontalAlignment = HorizontalAlignment.Left;
            circle.VerticalAlignment = VerticalAlignment.Top;

            circle.Margin = new Thickness(drawPosition.X - markerSize * 0.5, drawPosition.Y - markerSize * 0.5, 0, 0);

            ImageGrid.Children.Add(circle);
            _snapPointMarkers.Add(circle);

            Grid.SetZIndex(circle, -100);
        }

        /// <summary>
        /// Returns the corresponding point on the other side of the ImageGrid if the snap point is on an edge of the ImageGrid
        /// Returns null if not
        /// </summary>
        private Point? GetOpposingPointIfOnTheEdge(Point snapPoint)
        {
            if (snapPoint.X <= 0)
                return new Point(snapPoint.X + 1, snapPoint.Y);

            if (snapPoint.X > ImageGridSize)
                return new Point(snapPoint.X - 1, snapPoint.Y);

            if (snapPoint.Y <= 0)
                return new Point(snapPoint.X, snapPoint.Y + 1);

            if (snapPoint.Y > ImageGridSize)
                return new Point(snapPoint.X, snapPoint.Y - 1);

            return null;
        }

        private Brush GetNewLayerSelectionColor(Color backgroundColor)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
            float hue = color.GetHue();
            float saturation = color.GetSaturation();
            float lightness = color.GetBrightness();

            if (backgroundColor.A < 64) //transparent color
                return Brushes.White;

            if (lightness < 0.5) //dark color
                return Brushes.White;

            //light color
            return CustomBrushes.VeryDarkGray;
        }

        private void DeleteLayer(LayerPanel layer)
        {
            //remove from image
            ImageGrid.Children.Remove(layer);

            //remove from list
            Layers.Remove(layer);
            _window.LayersListView.Items.Refresh();

            //sort out layer indexes so they are consecutive
            foreach (var layerPanel in Layers)
            {
                int index = Layers.IndexOf(layerPanel);
                layerPanel.SetIndex(index);

                //clear outlines
                layerPanel.ClearOutline();
            }

            LayerPanel.SelectedLayerIndex = 0;
            LayerPanel.LastUsedIndex = Layers.Count;

            _window.RepeatPreview.DrawRepeat();

            if (Layers.Count == 0) //removed the last layer
            {
                _window.LayerRotationSlider.Value = 0;
                var angle = (float)_window.LayerRotationSlider.Value;
                _window.LayerAngleTextBox.Text = $"{angle}°";
                return;
            }

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            selectedLayer.DrawOutline();
        }

        private void SaveImage()
        {
            //get image output size from user
            var saveImageWindow = new SaveImageWindow();
            if (!saveImageWindow.ShowDialog() == true) return;

            var size = saveImageWindow.ImageSize;

            //render layers and save them
            var imageToSave = ImageUtilities.RenderLayerPanelsInGrid(ImageGrid, size);
            FileHelper.SaveAsPng(imageToSave, 1);
        }

        private void StartImageImport()
        {
            var setOriginWindow = new SetOriginWindow();
            setOriginWindow.ShowDialog();
        }

        public void AddNewLayer(BitmapImage image, Point origin)
        {
            var newLayer = new LayerPanel(_window, this, ImageGrid, image, origin)
            {
                Width = ImageGrid.Width,
                Height = ImageGrid.Height
            };

            //add to image
            ImageGrid.Children.Add(newLayer);

            LayerPanel.SelectedLayerIndex = newLayer.Index;
            _window.LayerSettingsTitle.Content = newLayer.LayerName;

            foreach (var layer in Layers)
                layer.ClearOutline();
            newLayer.DrawOutline();

            //add to list
            Layers.Add(newLayer);
            _window.LayersListView.Items.Refresh();

            _window.RepeatPreview.DrawRepeat();

            //TODO: update slider position and angle text here with selectedLayer.Angle?
        }

        public void OnLayerRotateSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;
        }

        public void OnLayerRotationEnd(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Layers.Count == 0)
            {
                _window.LayerRotationSlider.Value = 0;
                return;
            }

            _window.LayerRotationSlider.Value = Math.Round(_window.LayerRotationSlider.Value);

            var angle = (float)_window.LayerRotationSlider.Value;

            var closest90 = Math.Round(angle / 90) * 90;

            //snap to multiples of 90 degrees
            int degreesMargin = 15;
            if (Math.Abs(angle - closest90) <= degreesMargin) _window.LayerRotationSlider.Value = closest90;

            angle = (float)_window.LayerRotationSlider.Value;

            var selectedLayer = Layers[LayerPanel.SelectedLayerIndex];
            Task.Run(async () => await selectedLayer.Rotate(angle));

            _window.LayerAngleTextBox.Text = $"{angle}°";
        }
    }
}
