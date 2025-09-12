using System;
using System.ComponentModel;

namespace ForRobot.Libr.Collections
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Индекс элемента в родительской коллекции
        /// </summary>
        public int CollectionIndex { get; }

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemPropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="index">Индекс изменённого элемента.</param>
        /// <param name="name">Имя изменённого свойства.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name)
        {
            CollectionIndex = index;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemPropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="args">Экземпляр <see cref="PropertyChangedEventArgs"/> содержащий данные о событии</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName) { }

        #endregion
    }
}
