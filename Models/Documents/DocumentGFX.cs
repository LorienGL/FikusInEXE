using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace FikusIn.Models.Documents
{
    public class DocumentGFX: IDisposable
    {
        private readonly D3DImage myD3DImage = new();
        private readonly DispatcherObject myDispatcher;

        private nint myColorSurf; // Direct3D color surface pointer

        private Document myDoc;

        private System.Windows.Size mySize = new System.Windows.Size(0, 0);

        private bool myRenderNeeded = true;
        private Stopwatch myLastRenderStartStopwatch;
        private Stopwatch myLastRenderEndStopwatch;

        public DocumentGFX(Document p_Doc, DispatcherObject theDispatcher)
        {
            myLastRenderStartStopwatch = Stopwatch.StartNew();
            myLastRenderEndStopwatch = Stopwatch.StartNew();

            myDoc = p_Doc;
            myDispatcher = theDispatcher;

            myD3DImage.IsFrontBufferAvailableChanged
              += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            BeginRenderingScene();

            //myRenderTimer = new Timer(OnRenderTimer, null, 0, 1000 / 45); // 30 FPS when iddle (45 when moving)
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
        private double myCurrFPS2ms = 1000.0 / 45.0;

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


        private void OnRendering(object? sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            if(_lastRender == args.RenderingTime)
                return;

            BeginRendering?.Invoke(this, EventArgs.Empty);
            if (Render())
                _lastRender = args.RenderingTime;
        }

        private bool ShouldRender()
        {
            //if(myRenderNeeded || (myDoc.GetOCDocument() != null && myDoc.GetOCDocument().GetView().IsInvalidated())) // Need panting (paint request done or model view changed)
                if (myLastRenderStartStopwatch?.Elapsed.TotalMilliseconds > myCurrFPS2ms && myLastRenderEndStopwatch?.Elapsed.TotalMilliseconds > 5.0) // 45 FPS min 3 mili between frames
                //if (myLastRenderEndStopwatch?.Elapsed.TotalMilliseconds > 3.0) // min 3 ms between frames
                    return true;

            return false;
        }

        private List<double> myRenderTimes = new();
        private List<double> myFPSs = new();
        private double myAvgFPS = 0.0;
        private double myFPS = 0.0;
        private TimeSpan _lastRender;
        private Stopwatch? totalStopwatch = null;
        private int myFrameCount = 0;

        public void TryRender()
        {
            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:  => DocumentGFX.TryRender() {ShouldRender()}");

            if (!ShouldRender())
                return;

            //double ms = myLastRenderEndStopwatch == null? 0: myLastRenderEndStopwatch.Elapsed.TotalMilliseconds;

            Stopwatch renderStart = Stopwatch.StartNew();
            Stopwatch renderSW = Stopwatch.StartNew();
            if (!Render())
                return; 
            renderSW.Stop();

            //myRenderTimes.Add(renderSW.Elapsed.TotalMilliseconds);
            //myAvgFPS = 1000.0 / myRenderTimes.Average();
            //myFPSs.Add(ms);

            if (myCurrFPS2ms > 1000.0 / 30.0 && renderSW.Elapsed.TotalMilliseconds > 2.0 * myCurrFPS2ms)
                myCurrFPS2ms = 1000.0 / 30.0;
            else if(myCurrFPS2ms < 1000.0 / 45.0 && renderSW.Elapsed.TotalMilliseconds < 0.5 * myCurrFPS2ms)
                myCurrFPS2ms = 1000.0 / 45.0;

            myLastRenderStartStopwatch = renderStart;
            myLastRenderEndStopwatch?.Restart();

            myRenderNeeded = false;

            //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:  <= DocumentGFX.TryRender()");
        }

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

        public void SaveAsPNGFile(string fileName)
        {
            if (myColorSurf == nint.Zero)
                return;

            myD3DImage.Lock();
            {
                myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, nint.Zero);
                myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, myColorSurf);
            }
            myD3DImage.Unlock();
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
