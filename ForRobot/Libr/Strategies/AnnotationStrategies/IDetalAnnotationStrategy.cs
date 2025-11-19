using System;
using System.Collections.ObjectModel;

using ForRobot.Models.Detals;
using ForRobot.Models.File3D;

namespace ForRobot.Libr.Strategies.AnnotationStrategies
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
