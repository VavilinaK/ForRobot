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
        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            switch (type)
            {
                case typeof(System.Windows.Interop.InteropBitmap)
            }
        }
    }
}
