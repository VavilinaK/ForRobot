using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Model;

namespace ForRobot.ViewModels
{
    public class NavigationTreeViewModel : BaseClass
    {
        /// <summary>
        /// Обработчик исключений асинхронных комманд
        /// </summary>
        private static readonly Action<Exception> _exceptionCallback = new Action<Exception>(e =>
        {
            try
            {
                throw e;
            }
            catch (DivideByZeroException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        });

        #region Public variables

        #region Commands

        public ICommand HomeCommand { get; set; } = new RelayCommand(obj => SelectHomeDirection(obj as Robot));

        public ICommand UpdateFilesCommandAsync { get; set; } = new AsyncRelayCommand(async obj => await (obj as Robot)?.GetFilesAsync(), _exceptionCallback);

        public ICommand DownladeFilesCommandAsync { get; } = new AsyncRelayCommand(async obj => await DownladeFiles(obj as Robot), _exceptionCallback);

        public ICommand DropFilesCommandAsync { get; } = new AsyncRelayCommand(async obj => await DropFilesAsync(obj as Robot), _exceptionCallback);

        public ICommand DeleteFilesCommandAsync { get; } = new AsyncRelayCommand(async obj => await DeleteFilesAsync(obj as Robot), _exceptionCallback);

        #endregion Commands

        #endregion Public variables

        #region Private functions

        private static void SelectHomeDirection(Robot robot)
        {
            robot.PathControllerFolder = ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot;
            //RaisePropertyChanged()
        }

        /// <summary>
        /// Выборка отмеченных файлов
        /// </summary>
        /// <param name="files">Корневой каталог</param>
        /// <returns></returns>
        private static List<ForRobot.Model.Controls.IFile> SelectCheckedFiles(ForRobot.Model.Controls.IFile files)
        {
            List<ForRobot.Model.Controls.IFile> checkedFiles = new List<ForRobot.Model.Controls.IFile>(); // Список отмеченных файлов
            foreach (var file in files.Children)
            {
                Stack<ForRobot.Model.Controls.IFile> stack = new Stack<Model.Controls.IFile>();
                stack.Push(file);
                ForRobot.Model.Controls.IFile current;
                do
                {
                    current = stack.Pop();
                    IEnumerable<ForRobot.Model.Controls.IFile> children = current.Children;

                    if (current.IsCheck)
                        checkedFiles.Add(current);

                    foreach (var f in children)
                        stack.Push(f);
                }
                while (stack.Count > 0);
            }
            return checkedFiles;
        }

        #region Async

        /// <summary>
        /// Асинхронная выборка отмеченных файлов
        /// </summary>
        /// <param name="files">Коренной каталог</param>
        /// <returns></returns>
        private static async ValueTask<List<ForRobot.Model.Controls.IFile>> SelectCheckedFilesAsync(ForRobot.Model.Controls.IFile files) => await Task.Run(() => SelectCheckedFiles(files));

        /// <summary>
        /// Асинхронная отправка файлов
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task DropFilesAsync(Robot robot)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Source Code or Data files (*.src, *.dat)|*.src;*.dat|Data files (*.dat)|*.dat|Source Code File (*.src)|*src",
                Title = $"Отправка файла/ов на {robot.Name}",
                Multiselect = true
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel && (string.IsNullOrEmpty(openFileDialog.FileName) || string.IsNullOrEmpty(openFileDialog.FileNames[0])))
                    return;

                foreach (var path in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(path);

                    string tempFile = System.IO.Path.Combine(Robot.PathOfTempFolder, fileName);

                    if (!robot.CopyToPC(path, tempFile))
                        continue;

                    if (!robot.Copy(tempFile, System.IO.Path.Combine(robot.PathControllerFolder, fileName)))
                        continue;
                }

                await robot.GetFilesAsync();

                foreach (var file in robot.Files.Children)
                {
                    foreach (var path in openFileDialog.FileNames)
                    {
                        var searchFile = file.Search(Path.GetFileName(path));
                        if (searchFile == null) continue;
                        file.Search(Path.GetFileName(path)).IsCopy = true;
                    }
                }
            }
        }

        /// <summary>
        /// Скачивание выбранных файлов
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task DownladeFiles(Robot robot)
        {
            string path;
            using (var fbd = new FolderBrowserDialog() { Description = "Сохранить файлы в:" })
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    path = fbd.SelectedPath;
                }
                else
                    return;
            }

            var checkedFiles = await SelectCheckedFilesAsync(robot.Files);

            foreach(var file in checkedFiles)
            {
                var searchFile = robot.Files.Search(Path.GetFileName(file.Path));
                if (searchFile == null) continue;
                robot.DownladeFile(file.Path, path);
            }
        }

        /// <summary>
        /// Асинхронное удаление файлов
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task DeleteFilesAsync(Robot robot)
        {
            var checkedFiles = await SelectCheckedFilesAsync(robot.Files);

            await Task.Run(async () =>
            {
                foreach (var file in checkedFiles)
                {
                    await Task.Run(() => robot.DeleteFile(file.Path));
                }
            });
            await robot.GetFilesAsync();
        }

        #endregion Async

        #endregion Private functions
    }
}
