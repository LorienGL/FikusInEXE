using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FikusIn
{
    public static class MathUtils
    {
        public static readonly Matrix3D ZeroMatrix = new Matrix3D(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

        public static readonly Vector3D XAxis = new Vector3D(1.0, 0.0, 0.0);

        public static readonly Vector3D YAxis = new Vector3D(0.0, 1.0, 0.0);

        public static readonly Vector3D ZAxis = new Vector3D(0.0, 0.0, 1.0);

        public static double GetAspectRatio(Size size)
        {
            return size.Width / size.Height;
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        private static Matrix3D GetViewMatrix(ProjectionCamera camera)
        {
            Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");
            Vector3D vector3D = -camera.LookDirection;
            vector3D.Normalize();
            Vector3D vector3D2 = Vector3D.CrossProduct(camera.UpDirection, vector3D);
            vector3D2.Normalize();
            Vector3D vector = Vector3D.CrossProduct(vector3D, vector3D2);
            Vector3D vector2 = (Vector3D)camera.Position;
            double offsetX = 0.0 - Vector3D.DotProduct(vector3D2, vector2);
            double offsetY = 0.0 - Vector3D.DotProduct(vector, vector2);
            double offsetZ = 0.0 - Vector3D.DotProduct(vector3D, vector2);
            return new Matrix3D(vector3D2.X, vector.X, vector3D.X, 0.0, vector3D2.Y, vector.Y, vector3D.Y, 0.0, vector3D2.Z, vector.Z, vector3D.Z, 0.0, offsetX, offsetY, offsetZ, 1.0);
        }

        public static Matrix3D GetViewMatrix(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (camera is ProjectionCamera camera2)
            {
                return GetViewMatrix(camera2);
            }

            if (camera is MatrixCamera matrixCamera)
            {
                return matrixCamera.ViewMatrix;
            }

            throw new ArgumentException($"Unsupported camera type '{camera.GetType().FullName}'.", "camera");
        }

        private static Matrix3D GetProjectionMatrix(OrthographicCamera camera, double aspectRatio)
        {
            Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");
            double width = camera.Width;
            double height = width / aspectRatio;
            //double nearPlaneDistance = camera.NearPlaneDistance;
            //double farPlaneDistance = camera.FarPlaneDistance;
            //double num2 = 1.0; // 1.0 / (nearPlaneDistance - farPlaneDistance); // Near and far can be inf, so its 1
            //double offsetZ = 1; // nearPlaneDistance * num2;
            return new Matrix3D(
                2.0 / width,    0.0,            0.0,        -1.0, 
                0.0,            2.0 / height,   0.0,        -1.0, 
                0.0,            0.0,            1.0,        -1.0, 
                0.0,            0.0,            0.0,        1.0);
        }

        private static Matrix3D GetProjectionMatrix(PerspectiveCamera camera, double aspectRatio)
        {
            Debug.Assert(camera != null, "Caller needs to ensure camera is non-null.");
            double num = DegreesToRadians(camera.FieldOfView);
            double nearPlaneDistance = camera.NearPlaneDistance;
            double farPlaneDistance = camera.FarPlaneDistance;
            double num2 = 1.0 / Math.Tan(num / 2.0);
            double m = aspectRatio * num2;
            double num3 = ((farPlaneDistance == double.PositiveInfinity) ? (-1.0) : (farPlaneDistance / (nearPlaneDistance - farPlaneDistance)));
            double offsetZ = nearPlaneDistance * num3;
            return new Matrix3D(num2, 0.0, 0.0, 0.0, 0.0, m, 0.0, 0.0, 0.0, 0.0, num3, -1.0, 0.0, 0.0, offsetZ, 0.0);
        }

        public static Matrix3D GetProjectionMatrix(Camera camera, double aspectRatio)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (camera is PerspectiveCamera camera2)
            {
                return GetProjectionMatrix(camera2, aspectRatio);
            }

            if (camera is OrthographicCamera camera3)
            {
                return GetProjectionMatrix(camera3, aspectRatio);
            }

            if (camera is MatrixCamera matrixCamera)
            {
                return matrixCamera.ProjectionMatrix;
            }

            throw new ArgumentException($"Unsupported camera type '{camera.GetType().FullName}'.", "camera");
        }

        private static Matrix3D GetHomogeneousToViewportTransform(Rect viewport)
        {
            double num = viewport.Width / 2.0;
            double num2 = viewport.Height / 2.0;
            double offsetX = viewport.X + num;
            double offsetY = viewport.Y + num2;
            return new Matrix3D(num, 0.0, 0.0, 0.0, 0.0, 0.0 - num2, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, offsetX, offsetY, 0.0, 1.0);
        }

        public static Matrix3D TryWorldToViewportTransform(Viewport3DVisual visual, out bool success)
        {
            success = false;
            Matrix3D result = TryWorldToCameraTransform(visual, out success);
            if (success)
            {
                Rect viewport = visual.Viewport;
                if (viewport == Rect.Empty)
                {
                    Window w = Window.GetWindow(visual.Parent);
                    if (w == null || w.Width == 0 || w.Height == 0)
                    {
                        success = false;
                        return ZeroMatrix;
                    }

                    viewport = new Rect(0.0, 0.0, w.Width, w.Height);
                }

                result.Append(GetProjectionMatrix(visual.Camera, GetAspectRatio(viewport.Size)));
                result.Append(GetHomogeneousToViewportTransform(visual.Viewport));
                success = true;
            }

            return result;
        }

        public static Matrix3D TryWorldToCameraTransform(Viewport3DVisual visual, out bool success)
        {
            success = false;
            Matrix3D identity = Matrix3D.Identity;
            Camera camera = visual.Camera;
            if (camera == null)
            {
                return ZeroMatrix;
            }

            Transform3D transform = camera.Transform;
            if (transform != null)
            {
                Matrix3D value = transform.Value;
                if (!value.HasInverse)
                {
                    return ZeroMatrix;
                }

                value.Invert();
                identity.Append(value);
            }

            identity.Append(GetViewMatrix(camera));
            success = true;
            return identity;
        }

        private static Matrix3D GetWorldTransformationMatrix(DependencyObject visual, out Viewport3DVisual viewport)
        {
            Matrix3D mat = Matrix3D.Identity;
            viewport = new Viewport3DVisual();
            if (!(visual is Visual3D))
            {
                throw new ArgumentException("Must be of type Visual3D.", "visual");
            }

            while (visual != null && visual is ModelVisual3D)
            {
                Transform3D transform3D = (Transform3D)visual.GetValue(ModelVisual3D.TransformProperty);
                if (transform3D != null)
                {
                    mat.Append(transform3D.Value);
                }

                visual = VisualTreeHelper.GetParent(visual);
            }

            if (visual as Viewport3DVisual == null)
            {
                if (visual != null)
                {
                    throw new ApplicationException($"Unsupported type: '{visual.GetType().FullName}'.  Expected tree of ModelVisual3Ds leading up to a Viewport3DVisual.");
                }

                return ZeroMatrix;
            }
            else
                viewport = (Viewport3DVisual)visual;

            return mat;
        }

        public static Matrix3D TryTransformTo2DAncestor(DependencyObject visual, out Viewport3DVisual viewport, out bool success)
        {
            Matrix3D worldTransformationMatrix = GetWorldTransformationMatrix(visual, out viewport);
            worldTransformationMatrix.Append(TryWorldToViewportTransform(viewport, out success));
            if (!success)
            {
                return ZeroMatrix;
            }

            return worldTransformationMatrix;
        }

        public static Matrix3D TryTransformToCameraSpace(DependencyObject visual, out Viewport3DVisual viewport, out bool success)
        {
            Matrix3D worldTransformationMatrix = GetWorldTransformationMatrix(visual, out viewport);
            worldTransformationMatrix.Append(TryWorldToCameraTransform(viewport, out success));
            if (!success)
            {
                return ZeroMatrix;
            }

            return worldTransformationMatrix;
        }

        public static Rect3D TransformBounds(Rect3D bounds, Matrix3D transform)
        {
            double x = bounds.X;
            double y = bounds.Y;
            double z = bounds.Z;
            double x2 = bounds.X + bounds.SizeX;
            double y2 = bounds.Y + bounds.SizeY;
            double z2 = bounds.Z + bounds.SizeZ;
            Point3D[] array = new Point3D[8]
            {
            new Point3D(x, y, z),
            new Point3D(x, y, z2),
            new Point3D(x, y2, z),
            new Point3D(x, y2, z2),
            new Point3D(x2, y, z),
            new Point3D(x2, y, z2),
            new Point3D(x2, y2, z),
            new Point3D(x2, y2, z2)
            };
            transform.Transform(array);
            Point3D point3D = array[0];
            x = (x2 = point3D.X);
            y = (y2 = point3D.Y);
            z = (z2 = point3D.Z);
            for (int i = 1; i < array.Length; i++)
            {
                point3D = array[i];
                x = Math.Min(x, point3D.X);
                y = Math.Min(y, point3D.Y);
                z = Math.Min(z, point3D.Z);
                x2 = Math.Max(x2, point3D.X);
                y2 = Math.Max(y2, point3D.Y);
                z2 = Math.Max(z2, point3D.Z);
            }

            return new Rect3D(x, y, z, x2 - x, y2 - y, z2 - z);
        }

        public static bool TryNormalize(ref Vector3D v)
        {
            double length = v.Length;
            if (length != 0.0)
            {
                v /= length;
                return true;
            }

            return false;
        }

        public static Point3D GetCenter(Rect3D box)
        {
            return new Point3D(box.X + box.SizeX / 2.0, box.Y + box.SizeY / 2.0, box.Z + box.SizeZ / 2.0);
        }
    }
}