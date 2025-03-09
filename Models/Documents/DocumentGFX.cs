using FikusIn.Model.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace FikusIn.Models.Documents
{
    public class DocumentGFX
    {
        private readonly D3DImage myD3DImage = new();

        private nint myColorSurf; // Direct3D color surface pointer

        private Document myDoc;

        private Size mySize = new Size(0, 0);

        private bool myRenderNeeded = true;
        private Stopwatch? myLastRenderStartStopwatch = Stopwatch.StartNew();
        private Stopwatch? myLastRenderEndStopwatch;

        public DocumentGFX(Document p_Doc)
        {
            myLastRenderStartStopwatch = Stopwatch.StartNew();
            myLastRenderEndStopwatch = Stopwatch.StartNew();

            myDoc = p_Doc;

            myD3DImage.IsFrontBufferAvailableChanged
              += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            BeginRenderingScene();
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
            Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}: DocumentGFX.BeginRenderingScene() =====>");
        }

        public void StopRenderingScene()
        {
            CompositionTarget.Rendering -= OnRendering;
            myColorSurf = nint.Zero;
            Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}: <===== DocumentGFX.StopRenderingScene()");
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            //Render();

            myRenderNeeded = true;
            TryRender();
        }

        public void TryRender()
        {
            if (!myRenderNeeded && (myDoc.GetOCDocument() == null || !myDoc.GetOCDocument().GetView().IsInvalidated()))
                return;

            if (myLastRenderStartStopwatch?.Elapsed.TotalMilliseconds > myCurrFPS2ms && myLastRenderEndStopwatch?.Elapsed.TotalMilliseconds > 7.0) // 45 FPS
            {
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:   DocumentGFX.TryRender() {myLastRenderStartStopwatch?.Elapsed.TotalMilliseconds} {myLastRenderEndStopwatch?.Elapsed.TotalMilliseconds} => ");
                myLastRenderStartStopwatch.Restart();
                Stopwatch renderSW = Stopwatch.StartNew();
                Render();
                renderSW.Stop();
                if(renderSW.Elapsed.TotalMilliseconds > 2.0 * myCurrFPS2ms)
                    myCurrFPS2ms = 1000.0 / 30.0;
                else if(renderSW.Elapsed.TotalMilliseconds < 0.5 * myCurrFPS2ms)
                    myCurrFPS2ms = 1000.0 / 45.0;
                myLastRenderEndStopwatch.Restart();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:   DocumentGFX.TryRender() <= ");
                //myRenderNeeded = false;
            }
        }

        private void Render()
        {
            if (!HasFailed
              && myD3DImage.IsFrontBufferAvailable
              && myColorSurf != nint.Zero
              && myD3DImage.PixelWidth != 0 && myD3DImage.PixelHeight != 0)
            {
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Lock()");
                myD3DImage.Lock();
                {
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => BEGIN RedrawView");
                    myDoc.GetOCDocument()?.GetView().RedrawView();
                    //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:       DocumentGFX.Render() => END   RedrawView");
                    myD3DImage.AddDirtyRect(new Int32Rect(0, 0, myD3DImage.PixelWidth, myD3DImage.PixelHeight));
                }
                myD3DImage.Unlock();
                //Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() => myD3DImage.Unlock()");
            }
            else
                Debug.WriteLine($"{DateTime.Now:H:mm:ss.fff}:     DocumentGFX.Render() ##### NOT UPDATED ===> {HasFailed} {myD3DImage.IsFrontBufferAvailable} {myColorSurf} {myD3DImage.PixelWidth} {myD3DImage.PixelHeight}");
        }

        public void Resize(int theSizeX, int theSizeY)
        {
            mySize = new Size(theSizeX, theSizeY);

            if (!HasFailed && myD3DImage.IsFrontBufferAvailable)
            {
                myD3DImage.Lock();
                {
                    myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, nint.Zero);
                    var ocDoc = myDoc.GetOCDocument();
                    if (ocDoc != null && ocDoc.GetView() != null)
                        myColorSurf = ocDoc.GetView().ResizeBridgeFBO(theSizeX, theSizeY);
                    myD3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, myColorSurf);
                }
                myD3DImage.Unlock();
            }
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
