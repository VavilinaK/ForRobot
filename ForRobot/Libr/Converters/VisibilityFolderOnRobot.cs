using System;
using System.Linq;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Controls;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Скрывает папки на роботе, запрещённые настройками
    /// </summary>
    public class VisibilityFolderOnRobot : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<ForRobot.Model.Controls.File> files = values[0] as ObservableCollection<ForRobot.Model.Controls.File>; // Коллекция файлов на роботе.
            SortedDictionary<string, bool> settings = values[1] as SortedDictionary<string, bool>; // Коллекция доступных для блокировки папок из настроек.
            
            if(files != null)
            {
                var set = settings.Where(x => !x.Value).Select(s => s.Key).ToList<string>();

                for(int i = 0; i < files.Count(); i++)
                {
                    var file = files.ToArray<ForRobot.Model.Controls.File>()[i];

                    if (set.Contains(file.Name))
                    {
                        files.Remove(file);
                        i--;
                        continue;
                    }

                    var q = new Queue<ForRobot.Model.Controls.File>();
                    q.Enqueue(file);

                    while (q.Count > 0)
                    {
                        var node = q.Dequeue();

                        for (int y = 0; y < node.Children.Count; y++)
                        {
                            q.Enqueue(node.Children[y] as ForRobot.Model.Controls.File);

                            if (set.Contains(node.Children[y].Name))
                            {
                                node.Children.Remove(node.Children[y] as ForRobot.Model.Controls.File);
                                y--;
                            }
                        }
                    }
                }
            }

            return files;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
