using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Libr.Strategies.AnnotationStrategies;
using ForRobot.Models.Detals;
using ForRobot.Models.File3D;

namespace ForRobot.Libr.Services
{
    /// <summary>
    /// Класс сервис для составления коллекции <see cref="Annotation"/>, т.е. параметров детали
    /// </summary>
    public class AnnotationService
    {
        private readonly IEnumerable<IDetalAnnotationStrategy> _strategies;

        public AnnotationService(IEnumerable<IDetalAnnotationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public ObservableCollection<Annotation> GetAnnotations(Detal detal)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(DetalTypes.StringToEnum(detal.DetalType)));
            return strategy?.CreateAnnotations(detal) ?? null;
        }

        /// <summary>
        /// Возвращвет <see cref="Annotation"/> или <see cref="List{Annotation}"/>
        /// </summary>
        /// <param name="detal"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetAnnotation(Detal detal, string propertyName)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(DetalTypes.StringToEnum(detal.DetalType)));

            var method = strategy.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                           .FirstOrDefault(m => m.GetCustomAttribute<ForRobot.Libr.Attributes.PropertyNameAttribute>()?.PropertyName == propertyName);
            
            return method.Invoke(strategy, new object[] { detal });
        }
    }
}
