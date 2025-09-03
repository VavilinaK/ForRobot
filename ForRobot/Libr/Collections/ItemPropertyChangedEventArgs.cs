using System;
using System.ComponentModel;

namespace ForRobot.Libr.Collections
{
    public class ItemPropertyChangedEventArgs : EventArgs
    {
        public int Index { get; }
        public PropertyChangedEventArgs PropertyChangedEventArgs { get; }

        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs e)
        {
            Index = index;
            PropertyChangedEventArgs = e;
        }
    }
}
