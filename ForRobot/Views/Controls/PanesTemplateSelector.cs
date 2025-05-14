using System.Windows.Controls;
using System.Windows;

namespace ForRobot.Views.Controls
{
    using AvalonDock.Layout;

    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector() { }

        public DataTemplate FileViewTemplate { get; set; }

        public DataTemplate FileStatsViewTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is Model.File3D.File3D)
                return FileViewTemplate;

            if (item is Model.File3D.File3D)
                return FileStatsViewTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
