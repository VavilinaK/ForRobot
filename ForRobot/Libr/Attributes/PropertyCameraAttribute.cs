using System;
using System.Windows.Media.Media3D;

namespace ForRobot.Libr.Attributes
{
    /// <summary>
    /// Атрибут присвоения своству параметров камеры
    /// </summary>
    public class PropertyCameraAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        public Vector3D LookDirection { get; private set; }
        public Vector3D UpDirection { get; private set; }

        public PropertyCameraAttribute(string propertyName, Vector3D lookDirection, Vector3D upDirection)
        {
            this.PropertyName = propertyName;
            this.LookDirection = lookDirection;
            this.UpDirection = upDirection;
        }
    }
}
