using System;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

namespace ForRobot.Strategies.AnnotationStrategies
{
    /// <summary>
    /// Базовый интерфейс стратегии построения отображения параметров
    /// </summary>
    public interface IDetalAnnotationStrategy
    {
        bool CanHandle(DetalType detalType);
        ObservableCollection<Annotation> CreateAnnotations(Detal detal);
        string ToString(decimal param);
    }
}
