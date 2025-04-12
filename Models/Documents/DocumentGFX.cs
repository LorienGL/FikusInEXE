using FikusIn.Model.Documents;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace FikusIn.Models.Documents
{
    public class DocumentGFX: IDisposable
    {
        private readonly D3DImage d3dImage = new();

        private nint colorSurf; // Direct3D color surface pointer

        private readonly Document doc;

        private Size frameSize = new(0, 0);

        public DocumentGFX(Document p_Doc)
        {
            doc = p_Doc;

            d3dImage.IsFrontBufferAvailableChanged
              += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            BeginRenderingScene();
        }

        private OCView? GetDocOCView()
        {
            return doc.GetOCDocument()?.GetView();
        }


        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (d3dImage.IsFrontBufferAvailable)
                BeginRenderingScene();
            else
                StopRenderingScene(); // Device not avilable
        }

        private bool HasFailed = false;

        private void BeginRenderingScene()
        {
            if (HasFailed || !d3dImage.IsFrontBufferAvailable)
                return;

            var ocDoc = doc.GetOCDocument();
            if (ocDoc != null && !ocDoc.CreateView())
            {
                MessageBox.Show("Failed to initialize Direct3D interoperability!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                HasFailed = true;
                return;
            }

            // Need this so when the Direct3D device is available, we can start rendering (else surface isn't created)
            if (frameSize.Width != 0 && frameSize.Height != 0 && colorSurf == nint.Zero)
                Resize((int)frameSize.Width, (int)frameSize.Height);

            CompositionTarget.Rendering += OnRendering;
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}: DocumentGFX.BeginRenderingScene() =====>");
        }

        public void StopRenderingScene()
        {
            CompositionTarget.Rendering -= OnRendering;
            colorSurf = nint.Zero;
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}: <===== DocumentGFX.StopRenderingScene()");
        }

        // Ad a OnBeginRendering event & delegate to be able to perform actions before rendering
        public event EventHandler? BeginRendering;

        public event EventHandler? FirstRenderEnded;

        private void OnRendering(object? sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            if(_lastRender == args.RenderingTime)
                return;

            BeginRendering?.Invoke(this, EventArgs.Empty);
            if (Render())
                _lastRender = args.RenderingTime;
        }

        private bool _firstRender = true;
        private TimeSpan _lastRender;
        private Stopwatch? totalStopwatch = null;
        private int myFrameCount = 0;

        private bool Render()
        {
            if (!HasFailed
              && d3dImage.IsFrontBufferAvailable
              && colorSurf != nint.Zero
              && d3dImage.PixelWidth != 0 && d3dImage.PixelHeight != 0)
            {
                if (totalStopwatch == null)
                    totalStopwatch = Stopwatch.StartNew();
                var renderSW = Stopwatch.StartNew();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Lock()");
                d3dImage.Lock();
                {
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => BEGIN RedrawView");
                    GetDocOCView()?.RedrawView();
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => END   RedrawView");
                    d3dImage.AddDirtyRect(new Int32Rect(0, 0, d3dImage.PixelWidth, d3dImage.PixelHeight));
                }
                d3dImage.Unlock();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Unlock()");
                renderSW.Stop();
                ++myFrameCount;

                if(myFrameCount % 30 == 0)
                    Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:  DocumentGFX.Render() {(int)((double)myFrameCount / totalStopwatch.Elapsed.TotalSeconds)}FPS - {(int)renderSW.Elapsed.TotalMilliseconds}ms");

                if (_firstRender)
                    FirstRenderEnded?.Invoke(this, EventArgs.Empty);
                _firstRender = false;

                return true;
            }
            else
                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() ##### NOT UPDATED ===> {HasFailed} {d3dImage.IsFrontBufferAvailable} {colorSurf} {d3dImage.PixelWidth} {d3dImage.PixelHeight}");

            return false;
        }

        public void Resize(int theSizeX, int theSizeY)
        {
            frameSize = new System.Windows.Size(theSizeX, theSizeY);

            if (!HasFailed && d3dImage.IsFrontBufferAvailable)
            {
                d3dImage.Lock();
                {
                    d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, nint.Zero);
                    var ocView = GetDocOCView();
                    if (ocView != null)
                        colorSurf = ocView.ResizeBridgeFBO(theSizeX, theSizeY);
                    d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, colorSurf);                   
                }
                d3dImage.Unlock();
            }
        }

        public void Dispose()
        {
            d3dImage.IsFrontBufferAvailableChanged
              -= new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            StopRenderingScene();
        }

        public static Image? GetScreenshot(Grid view)
        {
            Size size = new(view.ActualWidth, view.ActualHeight);
            if (size.IsEmpty)
                return null;

            RenderTargetBitmap rtbmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            rtbmp.Render(drawingvisual);
            rtbmp.Freeze();

            Image image = new Image
            {
                Source = rtbmp,
                Width = size.Width,
                Height = size.Height
            };
            image.Source = rtbmp;

            return image;
        }



        public D3DImage Image
        {
            get
            {
                return d3dImage;
            }
        }
    }
}
