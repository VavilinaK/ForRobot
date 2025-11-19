using System;
using System.Windows.Media.Media3D;

using ForRobot.Models.Detals;
using ForRobot.Models.File3D;

namespace ForRobot.Libr.Strategies.ModelingStrategies
{
    /// <summary>
    /// Базовый интерфейс стратегии построения 3Д модели детали
    /// </summary>
    public interface IDetalModelingStrategy
    {
        bool CanHandle(DetalType detalType);
        Model3DGroup CreateModel3D(Detal detal);
    }
}
