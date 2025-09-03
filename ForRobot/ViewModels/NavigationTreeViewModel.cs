using System;
using System.Windows.Input;

using ForRobot.Model;

namespace ForRobot.ViewModels
{
    public class NavigationTreeViewModel : BaseClass
    {
        public ICommand HomeCommand { get; set; } = new RelayCommand(obj => SelectHomeDirection(obj as Robot));

        private static void SelectHomeDirection(Robot robot) => robot.PathControllerFolder = ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot;
    }
}
