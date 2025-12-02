using System;
using System.Windows.Media.Media3D;

namespace ForRobot.Libr.Collections
{
    public static class Transform3DBuilder
    {
        public static Transform3DGroup Create()
        {
            return new Transform3DGroup();
        }
                
        public static Transform3DGroup Scale(this Transform3DGroup transform, double x, double y, double z)
        {
            transform.Children.Add(new ScaleTransform3D(x, y, z));
            return transform;
        }

        public static Transform3DGroup Scale(this Transform3DGroup transform, double scale) => Scale(transform, scale, scale, scale);
        
        public static Transform3DGroup Translate(this Transform3DGroup transform, double x, double y, double z)
        {
            transform.Children.Add(new TranslateTransform3D(x, y, z));
            return transform;
        }

        public static Transform3DGroup Translate(this Transform3DGroup transform, Vector3D vector3D) => Translate(transform, vector3D.X, vector3D.Y, vector3D.Z);

        public static Transform3DGroup Rotate(this Transform3DGroup transform, Vector3D axis, double angle, Point3D center = new Point3D())
        {
            transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(axis, angle), center));
            return transform;
        }

        public static Transform3DGroup Rotate(this Transform3DGroup transform, AxisAngleRotation3D axisAngleRotation3D, Point3D center = new Point3D())
        {
            transform.Children.Add(new RotateTransform3D(axisAngleRotation3D, center));
            return transform;
        }
    }
}
