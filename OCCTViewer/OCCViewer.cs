using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Win32;
using System.Windows;

namespace FikusIn.OCCTViewer
{
    public enum View
    {
        FitAll,
        Axo,
        Front,
        Top,
        Left,
        Back,
        Right,
        Bottom
    }

    public enum FileFormat
    {
        NONE,
        FIN,
        DXF,
        BREP,
        STEP,
        IGES,
        VRML,
        STL,
        IMAGE
    }

    public enum DisplayMode
    {
        Wireframe,
        Shading
    }

    public class OCCViewer
    {
        public event EventHandler? ZoomingFinished;
        protected void RaiseZoomingFinished()
        {
            ZoomingFinished?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? AvaliabiltyOfOperationsChanged;
        protected void RaiseAvaliabiltyOfOperationsChanged()
        {
            AvaliabiltyOfOperationsChanged?.Invoke(this, EventArgs.Empty);
        }

        public OCCTProxyD3D View { get; private set; }
        private bool IsRectVisible { get; set; }
        public bool DegenerateMode { get; private set; }

        public bool IsWireframeEnabled { get; private set; }
        public bool IsShadingEnabled { get; private set; }
        public bool IsTransparencyEnabled { get; private set; }
        public bool IsColorEnabled { get; private set; }
        public bool IsMaterialEnabled { get; private set; }
        public bool IsDeleteEnabled { get; private set; }

        private float myCurZoom;
        private int myXmin;
        private int myYmin;
        private int myXmax;
        private int myYmax;
        private int myButtonDownX;
        private int myButtonDownY;
        public OCCViewer()
        {
            View = new OCCTProxyD3D();
            View.InitOCCTProxy();
            IsRectVisible = false;
            DegenerateMode = true;
        }

        public bool InitViewer()
        {
            bool res = View.InitViewer();        

            return res;
        }


        public void FitAll()
        {
            View.ZoomAllView();
        }

        public void AxoView()
        {
            View.AxoView();
        }

        public void FrontView()
        {
            View.FrontView();
        }

        public void TopView()
        {
            View.TopView();
        }

        public void LeftView()
        {
            View.LeftView();
        }

        public void BackView()
        {
            View.BackView();
        }

        public void RightView()
        {
            View.RightView();
        }

        public void Reset()
        {
            View.Reset();
        }

        public void BottomView()
        {
            View.BottomView();
        }

        public void HiddenOff()
        {
            View.SetDegenerateModeOff();
            DegenerateMode = false;
        }

        public void HiddenOn()
        {
            View.SetDegenerateModeOn();
            DegenerateMode = true;
        }

        public void SelectionChanged()
        {
            switch (View.DisplayMode())
            {
                case -1:
                    IsShadingEnabled = false;
                    IsWireframeEnabled = false;
                    break;
                case 0:
                    IsWireframeEnabled = false;
                    IsShadingEnabled = true;
                    IsTransparencyEnabled = false;
                    break;
                case 1:
                    IsWireframeEnabled = true;
                    IsShadingEnabled = false;
                    IsTransparencyEnabled = true;
                    break;
                case 10:
                    IsWireframeEnabled = true;
                    IsShadingEnabled = true;
                    IsTransparencyEnabled = true;
                    break;
                default:
                    break;
            }

            if (View.IsObjectSelected())
            {
                IsColorEnabled = true;
                IsMaterialEnabled = true;
                IsDeleteEnabled = true;
            }
            else
            {
                IsColorEnabled = false;
                IsMaterialEnabled = false;
                IsTransparencyEnabled = false;
                IsDeleteEnabled = false;
            }

            RaiseAvaliabiltyOfOperationsChanged();
        }



        public void Delete()
        {
            View.EraseObjects();
            SelectionChanged();
        }

        protected void MultiDragEvent(int x, int y, int theState)
        {
            if (theState == -1) //mouse is down
            {
                myButtonDownX = x;
                myButtonDownY = y;
            }
            else if (theState == 1) //mouse is up
            {
                View.ShiftSelect(Math.Min(myButtonDownX, x), Math.Min(myButtonDownY, y),
                                  Math.Max(myButtonDownX, x), Math.Max(myButtonDownY, y));
            }
        }

        protected void DragEvent(int x, int y, int theState)
        {
            if (theState == -1) //mouse is down
            {
                myButtonDownX = x;
                myButtonDownY = y;
            }
            else if (theState == 1) //mouse is up
            {
                View.Select(Math.Min(myButtonDownX, x), Math.Min(myButtonDownY, y),
                             Math.Max(myButtonDownX, x), Math.Max(myButtonDownY, y));
            }
        }

    }
}
