using FikusIn.Model.Documents;
using FikusIn.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        private Document? GetDocument()
        {
            try
            {
                return (Document)DataContext;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DocumentWindow()
        {
            InitializeComponent();

            pnlSubMenu.Visibility = Visibility.Collapsed;
        }

        ~DocumentWindow()
        {
            myRenderTimer?.Dispose();
            myRenderTimer = null;
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
            //dragStartingPosition = Mouse.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} Window_MouseMove ===>");

            try
            {
                // No button pressed, we release dragstart and we do just picking/highlighting
                if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
                {
                    dragStartingPosition = null;
                    if(canvasSelectionBox.Visibility != Visibility.Collapsed)
                        canvasSelectionBox.Visibility = Visibility.Collapsed;
                    GetDocument().GetOCDocument()?.GetView()?.MoveTo((int)Mouse.GetPosition(this).X, (int)Mouse.GetPosition(this).Y);
                    return;
                }

                Point dragCurrentPosition = Mouse.GetPosition(this);
                bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

                // Left click: Selection (& box sel)
                if (e.LeftButton == MouseButtonState.Pressed && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = Mouse.GetPosition(this);
                        return;
                    }

                    // Box selection
                    canvasSelectionBox.Width = Math.Abs(dragStartingPosition.Value.X - dragCurrentPosition.X);
                    canvasSelectionBox.Height = Math.Abs(dragStartingPosition.Value.Y - dragCurrentPosition.Y);
                    Canvas.SetLeft(canvasSelectionBox, Math.Min(dragStartingPosition.Value.X, dragCurrentPosition.X));
                    Canvas.SetTop(canvasSelectionBox, Math.Min(dragStartingPosition.Value.Y, dragCurrentPosition.Y));
                    canvasSelectionBox.Visibility = Visibility.Visible;

                    if (dragCurrentPosition.X > dragStartingPosition.Value.X)
                        canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 1, 0 };
                    else
                        canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 2, 2 };

                    GetDocument().GetOCDocument()?.GetView()?.MoveTo((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
                }
                // Middle click: Pan
                else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
                {
                    if (dragStartingPosition != null)
                    {
                        GetDocument().GetOCDocument()?.GetView()?.Pan(dragCurrentPosition.X - dragStartingPosition.Value.X, dragStartingPosition.Value.Y - dragCurrentPosition.Y);
                        if (canvasSelectionBox.Visibility != Visibility.Collapsed)
                            canvasSelectionBox.Visibility = Visibility.Collapsed;
                    }

                    dragStartingPosition = dragCurrentPosition;
                }
                // Right click: Rotate & Roll
                else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = Mouse.GetPosition(this);
                        GetDocument().GetOCDocument()?.GetView()?.StartRotation(dragStartingPosition.Value.X, dragStartingPosition.Value.Y, isCtrlDown);
                        if (canvasSelectionBox.Visibility != Visibility.Collapsed)
                            canvasSelectionBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        GetDocument().GetOCDocument()?.GetView()?.Rotation(dragCurrentPosition.X, dragCurrentPosition.Y);
                        Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Rotation({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                    }
                }
                else
                {
                    dragStartingPosition = null;
                    if (canvasSelectionBox.Visibility != Visibility.Collapsed)
                        canvasSelectionBox.Visibility = Visibility.Collapsed;
                }
            }
            finally
            {
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} <=== Window_MouseMove");
                GetDocument()?.GFX?.TryRender();
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // No drag started, just select whatever is under the mouse (MoveTo already called in MouseMove)
            if (!dragStartingPosition.HasValue) 
            {
                GetDocument().GetOCDocument()?.GetView()?.Select((int)Mouse.GetPosition(this).X, (int)Mouse.GetPosition(this).Y);
                return;
            }

            canvasSelectionBox.Visibility = Visibility.Collapsed;

            Point dragEndingPosition = Mouse.GetPosition(this);

            // Left click: Selection (& box sel)
            GetDocument().GetOCDocument()?.GetView()?.Select((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragEndingPosition.X, (int)dragEndingPosition.Y);

            dragStartingPosition = null;
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                    GetDocument().GetOCDocument()?.GetView()?.NextDetected();
                else
                    GetDocument().GetOCDocument()?.GetView()?.PreviousDetected();
            }
            else
            {
                if (e.Delta > 0)
                    GetDocument().GetOCDocument()?.GetView()?.ZoomOut();
                else
                    GetDocument().GetOCDocument()?.GetView()?.ZoomIn();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMenuOrientation();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GetDocument().InitGFX();
            ImageBrush anImage = new(GetDocument().GFX?.Image);
            gridD3D.Background = anImage;
            GetDocument().GFX?.Resize(Convert.ToInt32(gridD3D.ActualWidth), Convert.ToInt32(gridD3D.ActualHeight));

            myRenderTimer = new Timer(OnRenderTimer, null, 0, 1000 / 30); // 30 FPS when iddle (45 when moving)

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            myRenderTimer?.Dispose();
            myRenderTimer = null;
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetDocument()?.GFX?.Resize(Convert.ToInt32(e.NewSize.Width), Convert.ToInt32(e.NewSize.Height));
        }

        private Timer? myRenderTimer;
        private void OnRenderTimer(object? state)
        {
            try
            {
                Dispatcher?.Invoke(() => GetDocument()?.GFX?.TryRender());
            }
            catch (Exception)
            {
            }
        }
    }
}
