using System;
using System.ComponentModel;

namespace ForRobot.Libr
{
    /// <summary>
    /// Предоставляет данные для события <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/>.
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        #region Public variables

        /// <summary>
        /// Gets the index in the collection for which the property change has occurred.
        /// </summary>
        /// <value>
        /// Index in parent collection.
        /// </value>
        public int CollectionIndex { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemPropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="index">Индекс изменённого элемента.</param>
        /// <param name="name">Имя изменённого свойства.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name) { CollectionIndex = index; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ItemPropertyChangedEventArgs"/>.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="args">Экземпляр <see cref="PropertyChangedEventArgs"/> содержащий данные о событии</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName) { }

        #endregion
    }
}
