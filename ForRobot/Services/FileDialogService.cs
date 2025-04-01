using System;
using System.IO;

using Microsoft.Win32;

namespace ForRobot.Services
{
    public interface IFileDialogService
    {
        string OpenFileDialog(string initialDirectory, string defaultPath, string filter, string defaultExtension);

        string SaveFileDialog(string initialDirectory, string defaultPath, string filter);
    }

    public sealed class FileDialogService : IFileDialogService
    {
        public string OpenFileDialog(string initialDirectory, string defaultPath, string filter, string defaultExtension)
        {
            //if (initialDirectory == null || defaultPath == null)
            //    return null;

            var d = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                FileName = defaultPath,
                Filter = filter,
                DefaultExt = defaultExtension
            };

            if (d.ShowDialog() != true)
                return null;

            return d.FileName;
        }

        public string SaveFileDialog(string initialDirectory, string defaultPath, string filter)
        {
            //if (initialDirectory == null || defaultPath == null)
            //    return null;

            var d = new SaveFileDialog
            {
                InitialDirectory = initialDirectory,
                FileName = Path.GetFileNameWithoutExtension(defaultPath),
                Filter = filter,
                DefaultExt = Path.GetExtension(defaultPath)
            };

            if (d.ShowDialog() != true)
            {
                return null;
            }

            return d.FileName;
        }
    }
}
