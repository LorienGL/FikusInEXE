using FikusIn.Model.Documents;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace FikusIn.Models.Documents
{
    public class DocumentGFX: IDisposable
    {
        private readonly D3DImage myD3DImage = new();

        private nint myColorSurf; // Direct3D color surface pointer

        private Document myDoc;

        private Size mySize = new(0, 0);

        public DocumentGFX(Document p_Doc)
        {
            myDoc = p_Doc;

            myD3DImage.IsFrontBufferAvailableChanged
              += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            BeginRenderingScene();
        }

        private OCView? GetDocOCView()
        {
            return myDoc.GetOCDocument()?.GetView();
        }


        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (myD3DImage.IsFrontBufferAvailable)
                BeginRenderingScene();
            else
                StopRenderingScene(); // Device not avilable
        }

        private bool HasFailed = false;

        private void BeginRenderingScene()
        {
            if (HasFailed || !myD3DImage.IsFrontBufferAvailable)
                return;

            var ocDoc = myDoc.GetOCDocument();
            if (ocDoc != null && !ocDoc.CreateView())
            {
                MessageBox.Show("Failed to initialize Direct3D interoperability!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                HasFailed = true;
                return;
            }

            // Need this so when the Direct3D device is available, we can start rendering (else surface isn't created)
            if (mySize.Width != 0 && mySize.Height != 0 && myColorSurf == nint.Zero)
                Resize((int)mySize.Width, (int)mySize.Height);

            CompositionTarget.Rendering += OnRendering;
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}: DocumentGFX.BeginRenderingScene() =====>");
        }

        public void StopRenderingScene()
        {
            CompositionTarget.Rendering -= OnRendering;
            myColorSurf = nint.Zero;
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
              && myD3DImage.IsFrontBufferAvailable
              && myColorSurf != nint.Zero
              && myD3DImage.PixelWidth != 0 && myD3DImage.PixelHeight != 0)
            {
                if (totalStopwatch == null)
                    totalStopwatch = Stopwatch.StartNew();
                var renderSW = Stopwatch.StartNew();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Lock()");
                myD3DImage.Lock();
                {
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => BEGIN RedrawView");
                    GetDocOCView()?.RedrawView();
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => END   RedrawView");
                    myD3DImage.AddDirtyRect(new Int32Rect(0, 0, myD3DImage.PixelWidth, myD3DImage.PixelHeight));
                }
                myD3DImage.Unlock();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Unlock()");
                renderSW.Stop();
                ++myFrameCount;

                if(myFrameCount % 30 == 0)
                    Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:  DocumentGFX.Render() {(int)((double)myFrameCount / totalStopwatch.Elapsed.TotalSeconds)}FPS - {(int)renderSW.Elapsed.TotalMilliseconds}ms");

                if(_firstRender)
                {
                    _firstRender = false;
                    FirstRenderEnded?.Invoke(this, EventArgs.Empty);
                }

                return true;
            }
            else
                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() ##### NOT UPDATED ===> {HasFailed} {myD3DImage.IsFrontBufferAvailable} {myColorSurf} {myD3DImage.PixelWidth} {myD3DImage.PixelHeight}");

            return false;
        }

        public void Resize(int theSizeX, int theSizeY)
        {
            mySize = new System.Windows.Size(theSizeX, theSizeY);

            if (!HasFailed && myD3DImage.IsFrontBufferAvailable)
            {
                myD3DImage.Lock();
                {
                    myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, nint.Zero);
                    var ocView = GetDocOCView();
                    if (ocView != null)
                        myColorSurf = ocView.ResizeBridgeFBO(theSizeX, theSizeY);
                    myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, myColorSurf);                   
                }
                myD3DImage.Unlock();
            }
        }

        public void Dispose()
        {
            myD3DImage.IsFrontBufferAvailableChanged
              -= new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            StopRenderingScene();
        }

        public PngBitmapEncoder? GetPNG()
        {
            if (myColorSurf == nint.Zero)
                return null;

            Image image = new Image();
            image.Source = myD3DImage;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(myD3DImage.PixelWidth, myD3DImage.PixelHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(image);
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            return pngBitmapEncoder;
        }

        public D3DImage Image
        {
            get
            {
                return myD3DImage;
            }
        }
    }
}
