using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ForRobot.Libr.Json
{
    public class SettingsResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (typeof(System.Windows.Media.ImageSource).IsAssignableFrom(property.PropertyType))
            {

            }
            //switch (property.PropertyType.FullName)
            //{
            //    case System.Windows.Media.ImageSource:
            //        break;
            //}

            return property;
        }
    }
}
