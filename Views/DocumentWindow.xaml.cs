using FikusIn.Model.Documents;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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

            //mouseMoveTimer = new System.Windows.Threading.DispatcherTimer();
            //mouseMoveTimer.Tick += new EventHandler(MouseMoveTimer_Tick);
            //mouseMoveTimer.Interval = TimeSpan.FromMilliseconds(1000 / 30);
            //mouseMoveTimer.Start();
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

        //private void Window_MouseMove(object sender, MouseEventArgs e)
        //{
        //    Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} Window_MouseMove ===>");

        //    var p = Mouse.GetPosition(gridD3D); //e.GetPosition(gridD3D);

        //    try
        //    {
        //        if (canvasSelectionBox.Visibility != Visibility.Collapsed 
        //            && (dragStartingPosition == null || e.LeftButton == MouseButtonState.Released || e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
        //            canvasSelectionBox.Visibility = Visibility.Collapsed;

        //        // No button pressed, we release dragstart and we do just picking/highlighting
        //        if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
        //        {
        //            dragStartingPosition = null;
        //            GetDocument()?.GetOCDocument()?.GetView()?.MoveTo((int)p.X, (int)p.Y);
        //            Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pick ({(int)p.X}, {(int)p.Y})");
        //            return;
        //        }

        //        Point dragCurrentPosition = p;
        //        bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        //        // Left click: Selection (& box sel)
        //        if (e.LeftButton == MouseButtonState.Pressed && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
        //        {
        //            if (dragStartingPosition == null)
        //            {
        //                dragStartingPosition = p;
        //                return;
        //            }

        //            // Box selection
        //            canvasSelectionBox.Width = Math.Abs(dragStartingPosition.Value.X - dragCurrentPosition.X);
        //            canvasSelectionBox.Height = Math.Abs(dragStartingPosition.Value.Y - dragCurrentPosition.Y);
        //            Canvas.SetLeft(canvasSelectionBox, Math.Min(dragStartingPosition.Value.X, dragCurrentPosition.X));
        //            Canvas.SetTop(canvasSelectionBox, Math.Min(dragStartingPosition.Value.Y, dragCurrentPosition.Y));
        //            canvasSelectionBox.Visibility = Visibility.Visible;

        //            if (dragCurrentPosition.X > dragStartingPosition.Value.X)
        //                canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 1, 0 };
        //            else
        //                canvasSelectionBox.StrokeDashArray = new DoubleCollection() { 2, 2 };

        //            GetDocument()?.GetOCDocument()?.GetView()?.MoveTo((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
        //            Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Select Box ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
        //        }
        //        // Middle click: Pan
        //        else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
        //        {
        //            if (dragStartingPosition != null)
        //            {
        //                GetDocument()?.GetOCDocument()?.GetView()?.Pan(dragCurrentPosition.X - dragStartingPosition.Value.X, dragStartingPosition.Value.Y - dragCurrentPosition.Y);
        //                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pan ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
        //            }

        //            dragStartingPosition = dragCurrentPosition;
        //        }
        //        // Right click: Rotate & Roll
        //        else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
        //        {
        //            if (dragStartingPosition == null)
        //            {
        //                dragStartingPosition = p;
        //                GetDocument()?.GetOCDocument()?.GetView()?.StartRotation(dragStartingPosition.Value.X, dragStartingPosition.Value.Y, isCtrlDown);
        //                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Start Rotation ({(int)p.X}, {(int)p.Y})");
        //            }
        //            else
        //            {
        //                GetDocument()?.GetOCDocument()?.GetView()?.Rotation(dragCurrentPosition.X, dragCurrentPosition.Y);
        //                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Rotation ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
        //            }
        //        }
        //        else
        //        {
        //            dragStartingPosition = null;
        //        }
        //    }
        //    finally
        //    {
        //        Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} <=== Window_MouseMove");
        //        //GetDocument()?.GFX?.TryRender();
        //        //ImageBrush anImage = new(GetDocument()?.GFX?.Image);
        //        //gridD3D.Background = anImage;
        //    }
        //}

        //System.Windows.Threading.DispatcherTimer mouseMoveTimer;
        //private Timer? mouseMoveTimer;



        //private void MouseMoveTimer_Tick(object? sender)
        //{
        //    if (!Dispatcher.CheckAccess())
        //        Dispatcher.Invoke(() => MouseMoveTimer_Tick(sender));
        //    else
        //        MouseMove();
        //}

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        public static bool IsKeyDown(Key key)
        {
            return (GetAsyncKeyState((ushort)KeyInterop.VirtualKeyFromKey(key)) & 0x8000) != 0;
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => (GetAsyncKeyState(0x01) & 0x8000) != 0,
                MouseButton.Middle => (GetAsyncKeyState(0x04) & 0x8000) != 0,
                MouseButton.Right => (GetAsyncKeyState(0x02) & 0x8000) != 0,
                _ => false
            };
        }


        private void TrackMouseMovement(object? sender, EventArgs e)
        {
            //var p = Mouse.GetPosition(gridD3D); //e.GetPosition(gridD3D);
            var p = gridD3D.PointFromScreen(GetMousePosition());
            if(p.X < 0 || p.Y < 0 || p.X > gridD3D.ActualWidth || p.Y > gridD3D.ActualHeight)
                return;

            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} TrackMouseMovement ({(int)p.X},{(int)p.Y}) ===>");

            bool lb = IsMouseButtonDown(MouseButton.Left);
            bool mb = IsMouseButtonDown(MouseButton.Middle);
            bool rb = IsMouseButtonDown(MouseButton.Right);
            bool ctrl = IsKeyDown(Key.LeftCtrl) || IsKeyDown(Key.RightCtrl);
            bool shift = IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);

            try
            {
                if (canvasSelectionBox.Visibility != Visibility.Collapsed
                    && (dragStartingPosition == null || !lb || mb || rb))
                    canvasSelectionBox.Visibility = Visibility.Collapsed;

                // No button pressed, we release dragstart and we do just picking/highlighting
                if (!lb && !mb && !rb)
                {
                    dragStartingPosition = null;
                    GetDocument()?.GetOCDocument()?.GetView()?.MoveTo((int)p.X, (int)p.Y);
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pick ({(int)p.X}, {(int)p.Y})");
                    return;
                }

                Point dragCurrentPosition = p;

                // Left click: Selection (& box sel)
                if (lb && !mb && !rb)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = p;
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

                    GetDocument()?.GetOCDocument()?.GetView()?.MoveTo((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Select Box ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                }
                // Middle click: Pan
                else if (!lb && mb && !rb)
                {
                    if (dragStartingPosition != null)
                    {
                        GetDocument()?.GetOCDocument()?.GetView()?.Pan(dragCurrentPosition.X - dragStartingPosition.Value.X, dragStartingPosition.Value.Y - dragCurrentPosition.Y);
                        //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pan ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                    }

                    dragStartingPosition = dragCurrentPosition;
                }
                // Right click: Rotate & Roll
                else if (!lb && !mb && rb)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = p;
                        GetDocument()?.GetOCDocument()?.GetView()?.StartRotation(dragStartingPosition.Value.X, dragStartingPosition.Value.Y, ctrl);
                        //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Start Rotation ({(int)p.X}, {(int)p.Y})");
                    }
                    else
                    {
                        GetDocument()?.GetOCDocument()?.GetView()?.Rotation(dragCurrentPosition.X, dragCurrentPosition.Y);
                        //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Rotation ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                    }
                }
                else
                {
                    dragStartingPosition = null;
                }
            }
            finally
            {
                DoEvents(); // Allow the mouse wheel events to be processed

                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} <=== Window_MouseMove");
                //GetDocument()?.GFX?.TryRender();
                //ImageBrush anImage = new(GetDocument()?.GFX?.Image);
                //gridD3D.Background = anImage;
            }
        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(
                    delegate (object f)
                    {
                        ((DispatcherFrame)f).Continue = false;
                        return null;
                    }), frame);
            Dispatcher.PushFrame(frame);
        }


        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // No drag started, just select whatever is under the mouse (MoveTo already called in MouseMove)
            if (!dragStartingPosition.HasValue) 
            {
                GetDocument()?.GetOCDocument()?.GetView()?.Select((int)Mouse.GetPosition(gridD3D).X, (int)Mouse.GetPosition(gridD3D).Y);
                return;
            }

            canvasSelectionBox.Visibility = Visibility.Collapsed;

            Point dragEndingPosition = Mouse.GetPosition(gridD3D);

            // Left click: Selection (& box sel)
            GetDocument()?.GetOCDocument()?.GetView()?.Select((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragEndingPosition.X, (int)dragEndingPosition.Y);

            dragStartingPosition = null;
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Zoom ({e.Delta})");

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                    GetDocument()?.GetOCDocument()?.GetView()?.NextDetected();
                else
                    GetDocument()?.GetOCDocument()?.GetView()?.PreviousDetected();
            }
            else
            {
                if (e.Delta > 0)
                    GetDocument()?.GetOCDocument()?.GetView()?.ZoomOut();
                else
                    GetDocument()?.GetOCDocument()?.GetView()?.ZoomIn();
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
            GetDocument()?.InitGFX(this);
            ImageBrush anImage = new(GetDocument()?.GFX?.Image);
            gridD3D.Background = anImage;
            GetDocument()?.GFX?.Resize(Convert.ToInt32(gridD3D.ActualWidth), Convert.ToInt32(gridD3D.ActualHeight));
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GetDocument()?.GFX?.Resize(Convert.ToInt32(e.NewSize.Width), Convert.ToInt32(e.NewSize.Height));
        }

        private void gridD3D_MouseEnter(object sender, MouseEventArgs e)
        {
            var anim = new DoubleAnimation(1.0, 0.99, TimeSpan.FromSeconds(1));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;

            Timeline.SetDesiredFrameRate(anim, 30);
            gridD3D.BeginAnimation(UIElement.OpacityProperty, anim);
            if (GetDocument() != null && GetDocument()?.GFX != null)
                GetDocument().GFX.BeginRendering += TrackMouseMovement;
        }

        private void gridD3D_MouseLeave(object sender, MouseEventArgs e)
        {
            gridD3D.BeginAnimation(UIElement.OpacityProperty, null);
            if (GetDocument() != null && GetDocument()?.GFX != null)
                GetDocument().GFX.BeginRendering -= TrackMouseMovement;
        }
    }
}
