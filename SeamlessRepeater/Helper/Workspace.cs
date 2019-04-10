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

namespace SeamlessRepeater.Helper
{
    public class Workspace
    {
        public const int ImageGridSize = 500;
        public static Grid BackgroundLayer;
        public static Grid ImageGrid;

        private List<LayerPanel> _layers { get; set; }
        private ZoomBorder _workspaceZoomBorder;
        private MainWindow _window;
        private string _oldAngleText;

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

            _window.MouseDown += (s, e) => OnWindowMouseDown();
            _window.KeyDown += OnWindowKeyDown;
            _window.AddLayerButton.Click += (s, e) => StartImageImport();
            _window.SaveImageButton.Click += (s, e) => SaveImage();
            _window.WorkspaceZoomInButton.Click += (s, e) => WorkspaceZoom(true);
            _window.WorkspaceZoomOutButton.Click += (s, e) => WorkspaceZoom(false);
            _window.LayerSizeDownButton.Click += (s, e) => LayerSizeDown();
            _window.LayerSizeUpButton.Click += (s, e) => LayerSizeUp();

            _layers = new List<LayerPanel>();
            _window.LayersListView.Background = CustomBrushes.VeryDarkGray;
            _window.LayersListView.ItemsSource = _layers;
            _window.LayersListView.SelectionChanged += OnLayerSelected;
            _window.LayersListView.MouseDoubleClick += OnLayerDoubleClick;

            _window.BackgroundColorBackground.Background = CustomBrushes.CheckerBoardDarkSmall;
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

            if (_layers.Count == 0)
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

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
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

            foreach (var layer in _layers)
                layer.DrawOutline();

            _window.RepeatPreview.DrawRepeat();
            _window.BackgroundColorPicker.Background = backgroundBrush;
        }

        private void LayerSizeDown()
        {
            if (_layers.Count < 1) return;

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
            selectedLayer.SizeDown();
        }

        private void LayerSizeUp()
        {
            if (_layers.Count < 1) return;

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
            selectedLayer.SizeUp();
        }

        private void SwapLayers(int firstIndex, int secondIndex)
        {
            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];

            //change indexes
            _layers[firstIndex].SetIndex(secondIndex);
            _layers[secondIndex].SetIndex(firstIndex);

            //swap position in the list (changes order in ListView)
            _layers.Swap(firstIndex, secondIndex);

            LayerPanel.SelectedLayerIndex = selectedLayer.Index;

            _window.LayersListView.Items.Refresh();
        }

        public void OnMoveDownClick(object sender, RoutedEventArgs e)
        {
            if (_layers.Count < 2) return;

            var button = ((Button)sender);
            var row = (LayerPanel)button.DataContext;

            var listIndex = _layers.IndexOf(row);
            var layerAboveListIndex = listIndex - 1;

            if (layerAboveListIndex < 0) return; //is already on top

            SwapLayers(listIndex, layerAboveListIndex);
        }

        public void OnMoveUpClick(object sender, RoutedEventArgs e)
        {
            if (_layers.Count < 2) return;

            var button = ((Button)sender);
            var row = (LayerPanel)button.DataContext;

            var listIndex = _layers.IndexOf(row);
            var layerAboveListIndex = listIndex + 1;

            if (layerAboveListIndex > _layers.Count - 1) return; //is already on top

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

            foreach (var layer in _layers)
                layer.ClearOutline();

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
            _window.LayerSettingsTitle.Content = selectedLayer.LayerName;
            selectedLayer.DrawOutline();

            //TODO: update slider position and angle text here with selectedLayer.Angle

        }

        private void OnLayerDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnLayerSelected(sender, null);

            if (_window.LayersListView.SelectedIndex == -1) return;

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
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
            _layers.Remove(layer);
            _window.LayersListView.Items.Refresh();

            //sort out layer indexes so they are consecutive
            foreach (var layerPanel in _layers)
            {
                int index = _layers.IndexOf(layerPanel);
                layerPanel.SetIndex(index);

                //clear outlines
                layerPanel.ClearOutline();
            }

            LayerPanel.SelectedLayerIndex = 0;
            LayerPanel.LastUsedIndex = _layers.Count;

            _window.RepeatPreview.DrawRepeat();

            if (_layers.Count == 0) //removed the last layer
            {
                _window.LayerRotationSlider.Value = 0;
                var angle = (float)_window.LayerRotationSlider.Value;
                _window.LayerAngleTextBox.Text = $"{angle}°";
                return;
            }

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
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
            setOriginWindow.Show();
        }

        public void AddNewLayer(BitmapImage image)
        {
            var newLayer = new LayerPanel(_window, ImageGrid, image)
            {
                Width = ImageGrid.Width,
                Height = ImageGrid.Height
            };

            //add to image
            ImageGrid.Children.Add(newLayer);

            LayerPanel.SelectedLayerIndex = newLayer.Index;
            _window.LayerSettingsTitle.Content = newLayer.LayerName;

            foreach (var layer in _layers)
                layer.ClearOutline();
            newLayer.DrawOutline();

            //add to list
            _layers.Add(newLayer);
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
            if (_layers.Count == 0)
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

            var selectedLayer = _layers[LayerPanel.SelectedLayerIndex];
            Task.Run(async () => await selectedLayer.Rotate(angle));

            _window.LayerAngleTextBox.Text = $"{angle}°";
        }
    }
}
