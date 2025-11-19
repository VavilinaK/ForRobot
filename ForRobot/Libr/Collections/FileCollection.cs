using System;
using System.Collections.Generic;

using ForRobot.Models.Controls;

namespace ForRobot.Libr.Collections
{
    /// <summary>
    /// Класс действий с коллекцией файлов
    /// </summary>
    public static class FileCollection
    {
        /// <summary>
        /// Поиск элемента в подмножестве
        /// </summary>
        /// <param name="root">Корневой элемент поиска</param>
        /// <param name="nameToSearchFor">Имя для поиска</param>
        /// <returns></returns>
        public static IFile Search(IFile root, string nameToSearchFor)
        {
            Queue<IFile> Q = new Queue<IFile>();
            HashSet<IFile> S = new HashSet<IFile>();
            Q.Enqueue(root);
            S.Add(root);

            while (Q.Count > 0)
            {
                IFile e = Q.Dequeue();
                if (e.Name.ToLower() == nameToSearchFor.ToLower())
                    return e;

                foreach (IFile friend in e.Children)
                {
                    if (!S.Contains(friend))
                    {
                        Q.Enqueue(friend);
                        S.Add(friend);
                    }
                }
            }
            return null;
        }
    }
}
