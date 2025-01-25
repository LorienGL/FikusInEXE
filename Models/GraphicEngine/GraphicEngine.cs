using _3DTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FikusIn.Model.GraphicEngine
{
    internal class GraphicEngine
    {
        private Viewport3D viewport3D;
        private PointLight[] lights;
        private Vector3D[] lightsInitialPositions;

        public GraphicEngine(Viewport3D view, OrthographicCamera cam, PointLight[] ls)
        {
            viewport3D = view;

            lights = ls;
            lightsInitialPositions = new Vector3D[ls.Length];
            for (int i = 0; i < ls.Length; i++)
                lightsInitialPositions[i] = ls[i].Position - cam.Position;

            Camera = new Camera(cam, OnCameraPositionChanged, OnCameraZoomChanged);
            OnCameraPositionChanged();
        }

        public void PaintCube(float side)
        {
            float s = side / 2.0f;
            Point3D[] bp = [new(-s, -s, s), new(s, -s, s), new(s, s, s), new(-s, s, s), new(-s, -s, -s), new(s, -s, -s), new(s, s, -s), new(-s, s, -s)];
            Vector3D[] bn = [new(0, 0, 1), new(0, 0, -1), new(0, 1, 0), new(0, -1, 0), new(1, 0, 0), new(-1, 0, 0)];

            Point3D[] points = {
                bp[0], bp[1], bp[2], bp[3], // Top
                bp[4], bp[5], bp[6], bp[7], // Bottom
                bp[0], bp[4], bp[7], bp[3], // Left
                bp[1], bp[5], bp[6], bp[2], // Right
                bp[0], bp[1], bp[4], bp[5], // Front
                bp[2], bp[3], bp[6], bp[7]  // Back
            };

            Vector3D[] normals = {
                bn[0], bn[0], bn[0], bn[0], // Top
                bn[1], bn[1], bn[1], bn[1], // Bottom
                bn[5], bn[5], bn[5], bn[5], // Left
                bn[4], bn[4], bn[4], bn[4], // Right
                bn[3], bn[3], bn[3], bn[3], // Front
                bn[2], bn[2], bn[2], bn[2]  // Back
            };

            int[] indices = {
                0, 1, 2, 0, 2, 3,       // Top
                4, 6, 5, 4, 7, 6,       // Bottom
                8, 10, 9, 8, 11, 10,    // Left
                12, 13, 14, 12, 14, 15, // Right
                16, 18, 17, 17, 18, 19, // Front
                21, 20, 22, 21, 22, 23  // Back
            };

            MeshGeometry3D mesh = new() { Positions = new Point3DCollection(points), Normals = new Vector3DCollection(normals), TriangleIndices = new Int32Collection(indices) };
            mesh.Freeze();

            MaterialGroup materialGroup = new();
            //materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0xA7, 0xB9, 0xCC))));
            var r = new Random();
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb((byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256)))));
            materialGroup.Children.Add(new SpecularMaterial(Brushes.White, 70));
            materialGroup.Freeze();

            GeometryModel3D model = new() { Geometry = mesh, Material = materialGroup };
            model.Freeze();
            ModelVisual3D visual = new() { Content = model };

            viewport3D.Children.Add(visual);

            // Wireframe edges

            Point3D[] pts = [bp[0], bp[1], bp[1], bp[2], bp[2], bp[3], bp[3], bp[0],
                bp[0], bp[4], bp[1], bp[5], bp[2], bp[6], bp[3], bp[7],
                bp[4], bp[5], bp[5], bp[6], bp[6], bp[7], bp[7], bp[4]];

            // TODO: Uncomment this to paint lines
            //PaintLines(pts, 2, Colors.Black);

            //Point3DCollection pc = [bp[0], bp[1], bp[1], bp[2], bp[2], bp[3], bp[3], bp[0], bp[0], bp[4], bp[1], bp[5], bp[2], bp[6], bp[3], bp[7], bp[4], bp[5], bp[5], bp[6], bp[6], bp[7], bp[7], bp[4]];
            //Point3DCollection pc = [ new(0,0,0), new(100,0,0)];
            //Lines3D edges = new() { Thickness = 3, Color = Colors.Yellow };
            //edges.Points.Add(new(0, 0, 0));
            //edges.Points.Add(new(100, 0, 0));
            //edges.Transform = Transform3D.Identity;

            //viewport3D.Children.Add(edges);
        }


        private Dictionary<MeshGeometry3D, Tuple<Point3D[], double>> wireframeObjects = new();

        /// <summary>
        /// Paints lines in 3D space
        /// </summary>
        /// <param name="ps">points in pairs (else, last is ignored)</param>
        /// <param name="thickness">in pixels</param>
        /// <param name="c">color of the line (they never have shading)</param>
        public void PaintLines(Point3D[] ps, double thickness, Color c)
        {
            OrthographicCamera? oc = viewport3D.Camera as OrthographicCamera;
            if (oc == null)
                return;

            double d = thickness / 2.0;
            double t = oc.Width / Window.GetWindow(viewport3D).Width;
            double r = d * t;

            Point3DCollection pts = new();
            Int32Collection indices = new();
            for (int i = 0; i < ps.Length - 1; i += 2)
            {
                Vector3D dir = ps[i + 1] - ps[i];
                dir.Normalize();

                // Get the line direction vector projected to the plane of the camera
                Vector3D vector3D = Vector3D.CrossProduct(oc.LookDirection, Vector3D.CrossProduct(dir, oc.LookDirection));
                if (vector3D.LengthSquared == 0)
                    vector3D = oc.UpDirection;

                // Get the perpendicular vector regarding view axis
                Vector3D perp = Vector3D.CrossProduct(vector3D, oc.LookDirection);

                pts.Add(ps[i] - perp * r);
                pts.Add(ps[i] + perp * r);
                pts.Add(ps[i + 1] + perp * r);
                pts.Add(ps[i + 1] - perp * r);

                indices.Add(i * 4 + 1);
                indices.Add(i * 4);
                indices.Add(i * 4 + 2);

                indices.Add(i * 4 + 2);
                indices.Add(i * 4);
                indices.Add(i * 4 + 3);

                // Get the perpendicular vector regarding dir
                Vector3D perp2 = Vector3D.CrossProduct(perp, dir);

                pts.Add(ps[i] - perp2 * r);
                pts.Add(ps[i] + perp2 * r);
                pts.Add(ps[i + 1] + perp2 * r);
                pts.Add(ps[i + 1] - perp2 * r);

                indices.Add(i * 4 + 5);
                indices.Add(i * 4 + 4);
                indices.Add(i * 4 + 6);

                indices.Add(i * 4 + 6);
                indices.Add(i * 4 + 4);
                indices.Add(i * 4 + 7);
            }
            pts.Freeze();
            indices.Freeze();


            MeshGeometry3D mesh = new() { Positions = pts, TriangleIndices = indices };
            wireframeObjects.Add(mesh, new(ps, thickness));
            //mesh.Freeze();

            MaterialGroup materialGroup = new();
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            materialGroup.Children.Add(new SpecularMaterial(new SolidColorBrush(Colors.Black), 0));
            materialGroup.Children.Add(new EmissiveMaterial(new SolidColorBrush(c)));
            materialGroup.Freeze();

            GeometryModel3D model = new() { Geometry = mesh, Material = materialGroup, BackMaterial = materialGroup };
            ModelVisual3D visual = new() { Content = model };

            viewport3D.Children.Add(visual);
        }

        public Camera Camera { get; private set; }

        private void OnCameraPositionChanged()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].Position = Camera.Position + lightsInitialPositions[i];
                lights[i].Transform = Camera.Transform;
            }
        }

        private void OnCameraZoomChanged()
        {
            OrthographicCamera? oc = viewport3D.Camera as OrthographicCamera;
            if (oc == null)
                return;

            foreach (var wf in wireframeObjects)
            {
                var mesh = wf.Key;
                var ps = wf.Value.Item1;
                double thickness = wf.Value.Item2;

                double d = thickness / 2.0;
                double t = oc.Width / Window.GetWindow(viewport3D).Width;
                double r = d * t;

                Point3DCollection pts = new();

                for (int i = 0; i < ps.Length - 1; i += 2)
                {
                    Vector3D dir = ps[i + 1] - ps[i];
                    dir.Normalize();

                    // Get the line direction vector projected to the plane of the camera
                    Vector3D vector3D = Vector3D.CrossProduct(oc.LookDirection, Vector3D.CrossProduct(dir, oc.LookDirection));
                    if (vector3D.LengthSquared == 0)
                        vector3D = oc.UpDirection;

                    // Get the perpendicular vector regarding view axis
                    Vector3D perp = Vector3D.CrossProduct(vector3D, oc.LookDirection);

                    pts.Add(ps[i] - perp * r);
                    pts.Add(ps[i] + perp * r);
                    pts.Add(ps[i + 1] + perp * r);
                    pts.Add(ps[i + 1] - perp * r);

                    // Get the perpendicular vector regarding dir
                    Vector3D perp2 = Vector3D.CrossProduct(perp, dir);

                    pts.Add(ps[i] - perp2 * r);
                    pts.Add(ps[i] + perp2 * r);
                    pts.Add(ps[i + 1] + perp2 * r);
                    pts.Add(ps[i + 1] - perp2 * r);
                }

                mesh.Positions = pts;
            }
        }


    }
}
