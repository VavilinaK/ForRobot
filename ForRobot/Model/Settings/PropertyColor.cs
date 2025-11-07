using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace ForRobot.Model.File3D
{
    /// <summary>
    /// Модель представления имени элемента и его цвета
    /// </summary>
    public class PropertyColor
    {
        public string PropertyName { get; set; }

        public Color Color { get; set; }

        public PropertyColor(string name, Color color)
        {
            this.PropertyName = name;
            this.Color = color;
        }

        /// <summary>
        /// Возвращает цвет элемента из класса <see cref="ForRobot.Model.File3D.Colors"/>
        /// </summary>
        /// <returns></returns>
        private Color GetColor()
        {
            string propName = string.Empty;
            foreach (var f in typeof(ForRobot.Themes.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
                if (attribute.PropertyName == this.PropertyName)
                    return (System.Windows.Media.Color)f.GetValue(null);
            }
            return System.Windows.Media.Colors.Transparent;
        }

        /// <summary>
        /// Задаёт цвет свойству элемента в классе <see cref="ForRobot.Model.File3D.Colors"/>
        /// </summary>
        /// <param name="color">Новое значение свойства</param>
        private void SetColor(Color color)
        {
            foreach (var f in typeof(ForRobot.Themes.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
                if (attribute.PropertyName == this.PropertyName)
                    f.SetValue(null, color);
            }
        }
    }
}
