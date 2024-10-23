using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ForRobot.Libr.Json
{
    public class BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttributesResolver : DefaultContractResolver
    {
        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<Newtonsoft.Json.Serialization.JsonProperty> props = base.CreateProperties(type, memberSerialization);
            foreach (var prop in props)
            {
                if (Attribute.IsDefined(type.GetProperty(prop.UnderlyingName), typeof(BeveledPlitaDifferentDistanceBetweenNotParallelRibsAttribute)))
                {
                    prop.Ignored = false;
                }
            }
            return props;
        }
    }
}
