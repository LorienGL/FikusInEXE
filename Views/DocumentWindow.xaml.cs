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
        private Document? Document
        {
            get
            {
                if (DataContext is var doc && doc != null && doc is Document)
                    return doc as Document;
                else
                    return null;
            }
        }

        private OCView? DocOCView
        {
            get
            {
                if (Document != null && Document.GetOCDocument() is var ocdoc && ocdoc != null)
                    return ocdoc.GetView();
                else
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
        //            DocOCView?.MoveTo((int)p.X, (int)p.Y);
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

        //            DocOCView?.MoveTo((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
        //            Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Select Box ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
        //        }
        //        // Middle click: Pan
        //        else if (e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
        //        {
        //            if (dragStartingPosition != null)
        //            {
        //                DocOCView?.Pan(dragCurrentPosition.X - dragStartingPosition.Value.X, dragStartingPosition.Value.Y - dragCurrentPosition.Y);
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
        //                DocOCView?.StartRotation(dragStartingPosition.Value.X, dragStartingPosition.Value.Y, isCtrlDown);
        //                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Start Rotation ({(int)p.X}, {(int)p.Y})");
        //            }
        //            else
        //            {
        //                DocOCView?.Rotation(dragCurrentPosition.X, dragCurrentPosition.Y);
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


        private bool leftButtonDown = false;
        private bool middleButtonDown = false;
        private bool rightButtonDown = false;
        Point m_CurrentMousePosition = new Point(0, 0);

        private void TrackMouseMovement(object? sender, EventArgs e)
        {
            //var p = Mouse.GetPosition(gridD3D); //e.GetPosition(gridD3D);
            var mp = gridD3D.PointFromScreen(GetMousePosition());
            m_CurrentMousePosition = mp;
            if (mp.X < 0 || mp.Y < 0 || mp.X > gridD3D.ActualWidth || mp.Y > gridD3D.ActualHeight)
                return;

            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff} TrackMouseMovement ({(int)p.X},{(int)p.Y}) ===>");

            bool lb = IsMouseButtonDown(MouseButton.Left);
            bool mb = IsMouseButtonDown(MouseButton.Middle);
            bool rb = IsMouseButtonDown(MouseButton.Right);
            bool ctrl = IsKeyDown(Key.LeftCtrl) || IsKeyDown(Key.RightCtrl);
            bool shift = IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);

            if(lb != leftButtonDown)
            {
                leftButtonDown = lb;
                if (lb)
                    Window_MouseLeftButtonDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                else
                    Window_MouseLeftButtonUp(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
            }

            if(mb != rightButtonDown) 
            {
                middleButtonDown = mb;
                if (mb)
                    Window_MouseRightButtonDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle));
                else
                    Window_MouseRightButtonUp(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle));
            }

            if (rb != rightButtonDown)
            {
                rightButtonDown = rb;
                if (rb)
                    Window_MouseRightButtonDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right));
                else
                    Window_MouseRightButtonUp(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right));
            }

            try
            {
                if (canvasSelectionBox.Visibility != Visibility.Collapsed
                    && (dragStartingPosition == null || !lb || mb || rb))
                    canvasSelectionBox.Visibility = Visibility.Collapsed;

                // No button pressed, we release dragstart and we do just picking/highlighting
                if (!lb && !mb && !rb)
                {
                    dragStartingPosition = null;
                    DocOCView?.MoveTo((int)mp.X, (int)mp.Y);
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pick ({(int)p.X}, {(int)p.Y})");
                    return;
                }

                Point dragCurrentPosition = mp;

                // Left click: Selection (& box sel)
                if (lb && !mb && !rb)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = mp;
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

                    DocOCView?.MoveTo((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)dragCurrentPosition.X, (int)dragCurrentPosition.Y);
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Select Box ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                }
                // Middle click: Pan
                else if (!lb && mb && !rb)
                {
                    if (dragStartingPosition != null)
                    {
                        DocOCView?.Pan(dragCurrentPosition.X - dragStartingPosition.Value.X, dragStartingPosition.Value.Y - dragCurrentPosition.Y);
                        //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Pan ({(int)dragCurrentPosition.X}, {(int)dragCurrentPosition.Y})");
                    }

                    dragStartingPosition = dragCurrentPosition;
                }
                // Right click: Rotate & Roll
                else if (!lb && !mb && rb)
                {
                    if (dragStartingPosition == null)
                    {
                        dragStartingPosition = mp;
                        DocOCView?.StartRotation(dragStartingPosition.Value.X, dragStartingPosition.Value.Y, ctrl);
                        //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Start Rotation ({(int)p.X}, {(int)p.Y})");
                    }
                    else
                    {
                        DocOCView?.Rotation(dragCurrentPosition.X, dragCurrentPosition.Y);
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

        private static readonly double dragThreshold = 4.0; // pixels
        private bool IsDragStarted()
        {
            return dragStartingPosition.HasValue && dragStartingPosition.Value.SquareDistance(m_CurrentMousePosition) >= dragThreshold * dragThreshold;
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragStarted())
            {
                canvasSelectionBox.Visibility = Visibility.Collapsed;

                if(dragStartingPosition.HasValue)
                    DocOCView?.Select((int)dragStartingPosition.Value.X, (int)dragStartingPosition.Value.Y, (int)m_CurrentMousePosition.X, (int)m_CurrentMousePosition.Y);

                dragStartingPosition = null;
            }
            else
            {
                DocOCView?.Select((int)m_CurrentMousePosition.X, (int)m_CurrentMousePosition.Y);
            }
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}     Zoom ({e.Delta})");

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                    DocOCView?.NextDetected();
                else
                    DocOCView?.PreviousDetected();
            }
            else
            {
                if (e.Delta > 0)
                    DocOCView?.ZoomOut();
                else
                    DocOCView?.ZoomIn();
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
            Document?.InitGFX(this);
            ImageBrush anImage = new(Document?.GFX?.Image);
            gridD3D.Background = anImage;
            Document?.GFX?.Resize(Convert.ToInt32(gridD3D.ActualWidth), Convert.ToInt32(gridD3D.ActualHeight));
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Document?.GFX?.Resize(Convert.ToInt32(e.NewSize.Width), Convert.ToInt32(e.NewSize.Height));
        }

        private void gridD3D_MouseEnter(object sender, MouseEventArgs e)
        {
            var anim = new DoubleAnimation(1.0, 0.99, TimeSpan.FromSeconds(1));
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;

            Timeline.SetDesiredFrameRate(anim, 30);
            gridD3D.BeginAnimation(UIElement.OpacityProperty, anim);
            if (Document is var doc && doc != null && doc.GFX is var gfx && gfx != null)
                gfx.BeginRendering += TrackMouseMovement;
        }

        private void gridD3D_MouseLeave(object sender, MouseEventArgs e)
        {
            gridD3D.BeginAnimation(UIElement.OpacityProperty, null);
            if (Document is var doc && doc != null && doc.GFX is var gfx && gfx != null)
                gfx.BeginRendering -= TrackMouseMovement;
        }
    }

    public static class PointExtensions
    {
        public static double SquareDistance(this Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
    }
}
