using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace ForRobot.Model.File3D
{
    /// <summary>
    /// Модель представления имени элемента и цвета
    /// </summary>
    public class PropertyColor
    {
        private Color _color;

        public string PropertyName { get; }

        //public Color Color { get; set; }
        public Color Color
        {
            get => this._color == System.Windows.Media.Colors.Transparent ? (this._color = this.GetColor()) : this._color;
            set => this.SetColor(value);
        }

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
            foreach (var f in typeof(ForRobot.Model.File3D.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
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
            foreach (var f in typeof(ForRobot.Model.File3D.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                var attribute = f.GetCustomAttributes(typeof(ForRobot.Libr.Attributes.PropertyNameAttribute), false).FirstOrDefault() as ForRobot.Libr.Attributes.PropertyNameAttribute;
                if (attribute.PropertyName == this.PropertyName)
                    f.SetValue(null, color);
            }
        }
    }
}
