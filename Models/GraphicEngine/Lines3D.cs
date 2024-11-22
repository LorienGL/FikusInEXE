using _3DTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace FikusIn.Model.GraphicEngine
{
    internal class Lines3D : ModelVisual3D
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ScreenSpaceLines3D), new PropertyMetadata(Colors.White, OnColorChanged));

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register("Thickness", typeof(double), typeof(ScreenSpaceLines3D), new PropertyMetadata(1.0, OnThicknessChanged));

        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(Point3DCollection), typeof(ScreenSpaceLines3D), new PropertyMetadata(null, OnPointsChanged));

        private Matrix3D _visualToScreen;

        private Matrix3D _screenToVisual;

        private readonly GeometryModel3D _model;

        private readonly MeshGeometry3D _mesh;

        private double viewWidth = 0;

        public Color Color
        {
            get
            {
                return (Color)GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }

        public double Thickness
        {
            get
            {
                return (double)GetValue(ThicknessProperty);
            }
            set
            {
                SetValue(ThicknessProperty, value);
            }
        }

        public Point3DCollection Points
        {
            get
            {
                return (Point3DCollection)GetValue(PointsProperty);
            }
            set
            {
                SetValue(PointsProperty, value);
            }
        }

        public Lines3D()
        {
            _mesh = new MeshGeometry3D();
            _model = new GeometryModel3D();
            _model.Geometry = _mesh;
            SetColor(Color);
            Content = _model;
            Points = new Point3DCollection();
            CompositionTarget.Rendering += OnRender;
        }

        private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Lines3D)sender).SetColor((Color)args.NewValue);
        }

        private void SetColor(Color color)
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            materialGroup.Children.Add(new EmissiveMaterial(new SolidColorBrush(color)));
            materialGroup.Freeze();
            _model.Material = materialGroup;
            _model.BackMaterial = materialGroup;
        }

        private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Lines3D)sender).GeometryDirty();
        }

        private static void OnPointsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((Lines3D)sender).GeometryDirty();
        }

        private void OnRender(object? sender, EventArgs e)
        {
            if ((Points.Count != 0 || _mesh.Positions.Count != 0) && UpdateTransforms())
            {
                RebuildGeometry();
            }
        }

        private void GeometryDirty()
        {
            _visualToScreen = View3DMathUtils.ZeroMatrix;
        }

        private void RebuildGeometry()
        {
            double halfThickness = Thickness / 2.0;
            int num = Points.Count / 2;
            Point3DCollection point3DCollection = new Point3DCollection(num * 4);
            for (int i = 0; i < num; i++)
            {
                int num2 = i * 2;
                Point3D startPoint = Points[num2];
                Point3D endPoint = Points[num2 + 1];
                AddSegment(point3DCollection, startPoint, endPoint, halfThickness);
            }
            point3DCollection.Freeze();
            _mesh.Positions = point3DCollection;
            Int32Collection int32Collection = new Int32Collection(Points.Count * 3);
            for (int j = 0; j < Points.Count / 2; j++)
            {
                int32Collection.Add(j * 4 + 2);
                int32Collection.Add(j * 4 + 1);
                int32Collection.Add(j * 4);
                int32Collection.Add(j * 4 + 2);
                int32Collection.Add(j * 4 + 3);
                int32Collection.Add(j * 4 + 1);
            }
            int32Collection.Freeze();
            _mesh.TriangleIndices = int32Collection;
        }

        private void AddSegment(Point3DCollection positions, Point3D startPoint, Point3D endPoint, double halfThickness)
        {
            Vector3D vector3D = endPoint * _visualToScreen - startPoint * _visualToScreen;
            vector3D.Z = 0.0;
            vector3D.Normalize();
            Vector delta = new Vector(0.0 - vector3D.Y, vector3D.X);
            delta *= halfThickness;
            Widen(startPoint, delta, out var pOut, out var pOut2);
            positions.Add(pOut);
            positions.Add(pOut2);
            Widen(endPoint, delta, out pOut, out pOut2);
            positions.Add(pOut);
            positions.Add(pOut2);
        }

        private void Widen(Point3D pIn, Vector delta, out Point3D pOut1, out Point3D pOut2)
        {
            Point4D point4D = (Point4D)pIn;
            Point4D point4D2 = point4D * _visualToScreen;
            Point4D point4D3 = point4D2;
            point4D2.X += delta.X * point4D2.W;
            point4D2.Y += delta.Y * point4D2.W;
            point4D3.X -= delta.X * point4D3.W;
            point4D3.Y -= delta.Y * point4D3.W;
            point4D2 *= _screenToVisual;
            point4D3 *= _screenToVisual;
            pOut1 = new Point3D(point4D2.X / point4D2.W, point4D2.Y / point4D2.W, point4D2.Z / point4D2.W);
            pOut2 = new Point3D(point4D3.X / point4D3.W, point4D3.Y / point4D3.W, point4D3.Z / point4D3.W);
        }


        private bool UpdateTransforms()
        {
            Viewport3DVisual viewport;

            DependencyObject v = this;
            while (v != null && v is not Viewport3DVisual)
                v = VisualTreeHelper.GetParent(v);

            if (v == null)
                return false;

            viewport = (Viewport3DVisual)v;

            OrthographicCamera? oc = viewport.Camera as OrthographicCamera;

            // If line width does not need to change (no zoom), we can skip redoing the lines
            if (oc != null)
            {
                if (viewWidth == oc.Width)
                    return false;

                viewWidth = oc.Width;
            }

            Matrix3D matrix3D = View3DMathUtils.TryTransformTo2DAncestor(this, out viewport, out bool success);
            if (!success || !matrix3D.HasInverse)
            {
                _mesh.Positions = null;
                return false;
            }

            if (matrix3D == _visualToScreen)
                return false;


            _visualToScreen = _screenToVisual = matrix3D;
            _screenToVisual.Invert();
            return true;
        }

        public void MakeWireframe(Model3D model)
        {
            Points.Clear();
            if (model != null)
            {
                Matrix3DStack matrix3DStack = new Matrix3DStack();
                matrix3DStack.Push(Matrix3D.Identity);
                WireframeHelper(model, matrix3DStack);
            }
        }

        private void WireframeHelper(Model3D model, Matrix3DStack matrixStack)
        {
            Transform3D transform = model.Transform;
            if (transform != null && transform != Transform3D.Identity)
            {
                matrixStack.Prepend(model.Transform.Value);
            }
            try
            {
                if (model is Model3DGroup group)
                {
                    WireframeHelper(group, matrixStack);
                }
                else if (model is GeometryModel3D model2)
                {
                    WireframeHelper(model2, matrixStack);
                }
            }
            finally
            {
                if (transform != null && transform != Transform3D.Identity)
                {
                    matrixStack.Pop();
                }
            }
        }

        private void WireframeHelper(Model3DGroup group, Matrix3DStack matrixStack)
        {
            foreach (Model3D child in group.Children)
            {
                WireframeHelper(child, matrixStack);
            }
        }

        private void WireframeHelper(GeometryModel3D model, Matrix3DStack matrixStack)
        {
            Geometry3D geometry = model.Geometry;
            if (!(geometry is MeshGeometry3D meshGeometry3D))
            {
                return;
            }
            Point3D[] array = new Point3D[meshGeometry3D.Positions.Count];
            meshGeometry3D.Positions.CopyTo(array, 0);
            matrixStack.Peek().Transform(array);
            Int32Collection triangleIndices = meshGeometry3D.TriangleIndices;
            if (triangleIndices.Count > 0)
            {
                int num = array.Length - 1;
                int i = 2;
                for (int count = triangleIndices.Count; i < count; i += 3)
                {
                    int num2 = triangleIndices[i - 2];
                    int num3 = triangleIndices[i - 1];
                    int num4 = triangleIndices[i];
                    if (0 > num2 || num2 > num || 0 > num3 || num3 > num || 0 > num4 || num4 > num)
                    {
                        break;
                    }
                    AddTriangle(array, num2, num3, num4);
                }
            }
            else
            {
                int j = 2;
                for (int num5 = array.Length; j < num5; j += 3)
                {
                    int i2 = j - 2;
                    int i3 = j - 1;
                    int i4 = j;
                    AddTriangle(array, i2, i3, i4);
                }
            }
        }

        private void AddTriangle(Point3D[] positions, int i0, int i1, int i2)
        {
            Points.Add(positions[i0]);
            Points.Add(positions[i1]);
            Points.Add(positions[i1]);
            Points.Add(positions[i2]);
            Points.Add(positions[i2]);
            Points.Add(positions[i0]);
        }
    }
}
