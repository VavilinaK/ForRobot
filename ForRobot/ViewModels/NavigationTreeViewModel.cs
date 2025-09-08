using System;
using System.Windows.Input;

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

        public ICommand HomeCommand { get; set; } = new RelayCommand(obj => SelectHomeDirection(obj as Robot));

        public ICommand UpdateFilesCommandAsync { get; set; } = new AsyncRelayCommand(async obj => await (obj as Robot)?.GetFilesAsync(), _exceptionCallback);

        private static void SelectHomeDirection(Robot robot) => robot.PathControllerFolder = ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot;
    }
}
