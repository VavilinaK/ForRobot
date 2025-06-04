using System;

namespace ForRobot.Libr.Attributes
{
    public class PropertyNameAttribute : Attribute
    {
        public string PropertyName { get; private set; }

        public PropertyNameAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }
    }
}
