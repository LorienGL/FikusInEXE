using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace FikusIn.GraphicEngine
{
    internal class Camera
    {
        private OrthographicCamera orthoCamera;

        public delegate void PositionChanged();
        public delegate void ZoomChanged();

        public event PositionChanged OnPositionChanged;
        public event ZoomChanged OnZoomChanged;

        public Point3D Position
        {
            get { return orthoCamera.Position; }            
        }

        public Transform3D Transform
        {
            get { return orthoCamera.Transform; }
        }

        public Camera(OrthographicCamera cam, PositionChanged positionChanged, ZoomChanged zoomChanged)
        {
            orthoCamera = cam;
            OnPositionChanged += positionChanged;
            OnZoomChanged += zoomChanged;
        }

        public void Pan(Vector offset, double windowWidth, bool invert, bool microPrecision)
        {
            Vector3D up = orthoCamera.UpDirection;
            Vector3D right = Vector3D.CrossProduct(orthoCamera.LookDirection, up);

            orthoCamera.Position -= (right * offset.X - up * offset.Y) * (orthoCamera.Width / windowWidth * 2.0) * (invert? -1: 1) * (microPrecision ? 0.1: 1);
            OnPositionChanged();
        }

        public void Roll(Point start, Point end, double windowWidth, double windowHeight)
        {
            double startA = Vector.AngleBetween(new Vector(start.X - windowWidth / 2.0, start.Y - windowHeight / 2.0), new Vector(1, 0));
            double currentA = Vector.AngleBetween(new Vector(end.X - windowWidth / 2.0, end.Y - windowHeight / 2.0), new Vector(1, 0));

            double a = currentA - startA;

            AxisAngleRotation3D arot = new AxisAngleRotation3D(orthoCamera.LookDirection, a);
            RotateTransform3D rot = new RotateTransform3D(arot, orthoCamera.Position);
            orthoCamera.UpDirection = rot.Transform(orthoCamera.UpDirection);
            OnPositionChanged();
        }

        private readonly double ZOOM_FACTOR = 1.0 / (5.0 * 120.0);
        public void Zoom(Point cursorPos, double delta, double windowWidth, double windowHeight, bool invert, bool microPrecision)
        {
            double s2w = orthoCamera.Width / windowWidth;
            Vector3D up = orthoCamera.UpDirection;
            Vector3D right = Vector3D.CrossProduct(orthoCamera.LookDirection, up);

            // Move mouse cursor world position to 0,0
            Point mouseDispl = new((cursorPos.X - windowWidth / 2.0) * s2w, -(cursorPos.Y - windowHeight / 2.0) * s2w);
            orthoCamera.Position += (right * mouseDispl.X + up * mouseDispl.Y);

            // Zoom
            orthoCamera.Width *= 1 + ZOOM_FACTOR * delta * (invert? -1: 1) * (microPrecision? 0.1: 1);

            // Set new position according to mouse position
            double s2wAfter = orthoCamera.Width / windowWidth;
            Point mouseDisplAfter = new Point((cursorPos.X - windowWidth / 2.0) * s2wAfter, -(cursorPos.Y - windowHeight / 2.0) * s2wAfter);
            orthoCamera.Position -= right * mouseDisplAfter.X + up * mouseDisplAfter.Y;
            OnPositionChanged();
            OnZoomChanged();
        }

        public void Rotate(Vector offset, Point3D pivot)
        {
            // This is magic tbh :D
            Transform3DGroup txGrp = new();

            if(offset.X != 0)
                txGrp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(orthoCamera.UpDirection, -offset.X), pivot));

            Vector3D right = Vector3D.CrossProduct(orthoCamera.LookDirection, orthoCamera.UpDirection);
            if(offset.Y != 0)
                txGrp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(right, -offset.Y), pivot));

            // This is a workaround for a bug in WPF
            // If it uses only transform, it accumulates all transforms and crashes after a bit, thus we use only the value of last
            txGrp.Children.Add(new MatrixTransform3D(orthoCamera.Transform.Value)); 
            
            orthoCamera.Transform = txGrp;
            OnPositionChanged();
        }
    }
}
