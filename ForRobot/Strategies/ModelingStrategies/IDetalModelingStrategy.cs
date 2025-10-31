using System;
using System.Windows.Media.Media3D;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

namespace ForRobot.Strategies.ModelingStrategies
{
    /// <summary>
    /// Базовый интерфейс стратегии построения 3Д модели детали
    /// </summary>
    public interface IDetalModelingStrategy
    {
        bool CanHandle(DetalType detalType);
        Model3DGroup Get3DScene(Detal detal);
    }
}
