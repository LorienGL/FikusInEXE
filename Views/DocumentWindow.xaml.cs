using FikusIn.OCCTViewer;
using FikusIn.ViewModel;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FikusIn.Views
{
    /// <summary>
    /// Interaction logic for DocumentWindow.xaml
    /// </summary>
    public partial class DocumentWindow : UserControl
    {
        private D3DViewer? aViewer = null;

        public DocumentWindow()
        {
            InitializeComponent();

            pnlSubMenu.Visibility = Visibility.Collapsed;

            // DataContext is the opened document
        }

        private void SetMenuOrientation()
        {
            if (Width <= Height)
            {
                pnlMenu.HorizontalAlignment = HorizontalAlignment.Stretch;
                pnlMenu.VerticalAlignment = VerticalAlignment.Top;
                pnlMenu.Height = 70;
                pnlMenu.Width = double.NaN;
                DockPanel.SetDock(pnlMenu, Dock.Right);

                DockPanel.SetDock(btnMenu, Dock.Left);

                pnlSubMenu.HorizontalAlignment = HorizontalAlignment.Stretch;
                pnlSubMenu.VerticalAlignment = VerticalAlignment.Top;
                pnlSubMenu.Height = 70;
                pnlSubMenu.Width = double.NaN;
                pnlSubMenu.Orientation = Orientation.Horizontal;
            }
            else
            {
                pnlMenu.HorizontalAlignment = HorizontalAlignment.Left;
                pnlMenu.VerticalAlignment = VerticalAlignment.Stretch;
                pnlMenu.Width = 70;
                pnlMenu.Height = double.NaN;
                DockPanel.SetDock(pnlMenu, Dock.Bottom);

                DockPanel.SetDock(btnMenu, Dock.Top);

                pnlSubMenu.HorizontalAlignment = HorizontalAlignment.Left;
                pnlSubMenu.VerticalAlignment = VerticalAlignment.Top;
                pnlSubMenu.Width = 70;
                pnlSubMenu.Height = double.NaN;
                pnlSubMenu.Orientation = Orientation.Vertical;
            }
        }

        private void ShowMenu()
        {
            SetMenuOrientation();
            pnlSubMenu.Visibility = Visibility.Visible;
        }

        private void btnMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowMenu();
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            if (pnlSubMenu.Visibility == Visibility.Visible)
                pnlSubMenu.Visibility = Visibility.Collapsed;
            else
                ShowMenu();
        }

        private Point? dragStartingPosition;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           pnlSubMenu.Visibility = Visibility.Collapsed;
            dragStartingPosition = Mouse.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            // No button pressed, we release dragstart
            if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                dragStartingPosition = null;
                return;
            }

            if ((e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) && !dragStartingPosition.HasValue)
            {
                dragStartingPosition = Mouse.GetPosition(this);
                if(e.RightButton == MouseButtonState.Pressed)
                    aViewer?.Viewer?.View.Rotation((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y);

                return;
            }

            if (!dragStartingPosition.HasValue)
                return;

            Point dragCurrentPosition = Mouse.GetPosition(this);
            Vector dragOffset = dragCurrentPosition - dragStartingPosition.Value;

            // Left click: Selection (& box sel)
            if (e.LeftButton == MouseButtonState.Pressed && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released && dragStartingPosition.HasValue)
            {
                canvasSelectionBox.Width = Math.Abs(dragStartingPosition.Value.X - dragCurrentPosition.X);
                canvasSelectionBox.Height = Math.Abs(dragStartingPosition.Value.Y - dragCurrentPosition.Y);
                Canvas.SetLeft(canvasSelectionBox, Math.Min(dragStartingPosition.Value.X, dragCurrentPosition.X));
                Canvas.SetTop(canvasSelectionBox, Math.Min(dragStartingPosition.Value.Y, dragCurrentPosition.Y));
                canvasSelectionBox.Visibility = Visibility.Visible;

                if (dragCurrentPosition.X > dragStartingPosition.Value.X)
                    canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 1, 0 };
                else
                    canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 2, 2 };

                //v3dMain.InvalidateVisual();
            }
            else
            {
                canvasSelectionBox.Visibility = Visibility.Collapsed;

                // Middle click: Pan
                if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released && dragStartingPosition.HasValue)
                {
                    //gfxEngine.Camera.Pan(dragOffset, Width, (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0, (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0);
                }
                // Right click: Rotate
                else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed && dragStartingPosition.HasValue)
                {
                    aViewer?.Viewer?.View.Rotation((int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
                    //gfxEngine.Camera.Rotate(dragOffset, new Point3D(0, 0, 0));
                }
                // Left + Right click: Camera Roll
                else if (e.LeftButton == MouseButtonState.Pressed && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed && dragStartingPosition.HasValue)
                {
                    //gfxEngine.Camera.Roll(dragStartingPosition.Value, dragCurrentPosition, Width, Height);
                }

                dragStartingPosition = dragCurrentPosition;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!dragStartingPosition.HasValue)
                return;

            canvasSelectionBox.Visibility = Visibility.Collapsed;

            Point dragEndingPosition = Mouse.GetPosition(this);
            Vector dragOffset = dragEndingPosition - dragStartingPosition.Value;

            //TODO: Add code to do the box selection
            if (dragOffset.X < 5)
                dragOffset.X = 5;
            if (dragOffset.Y < 5)
                dragOffset.Y = 5;

            dragStartingPosition = null;
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (e.Delta != 0)
                //gfxEngine.Camera.Zoom(Mouse.GetPosition(this), e.Delta, Width, Height, (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0, (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0);
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void v3dMesh_MouseEnter(object sender, MouseEventArgs e)
        {

        }


        private void wDoc_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMenuOrientation();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            aViewer = new D3DViewer();
            ImageBrush anImage = new(aViewer.Image);
            gridD3D.Background = anImage;

            //aViewer.Viewer?.ImportModel(ModelFormat.STEP);

            aViewer.Resize((int)gridD3D.ActualWidth, (int)gridD3D.ActualHeight);
        }

        private void gridD3D_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            aViewer?.Resize((int)gridD3D.ActualWidth, (int)gridD3D.ActualHeight);
        }
    }
}
