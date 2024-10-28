using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Json
{
    public class PlitaWithRibsAttributesResolver : DefaultContractResolver
    {
        private string _scoseType;
        private bool _diferentDistance;
        private bool _paralleleRibs;
        private bool _diferentDissolutionLeft;
        private bool _diferentDissolutionRight;

        public PlitaWithRibsAttributesResolver(string sScoseType, bool bDiferentDistance = false, bool bParalleleRibs = true, bool bDiferentDissolutionLeft = false, bool bDiferentDissolutionRight = false) : base()
        {
            this._scoseType = sScoseType;
            this._diferentDistance = bDiferentDistance;
            this._paralleleRibs = bParalleleRibs;
            this._diferentDissolutionLeft = bDiferentDissolutionLeft;
            this._diferentDissolutionRight = bDiferentDissolutionRight;
        }

        protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<Newtonsoft.Json.Serialization.JsonProperty> props = base.CreateProperties(type, memberSerialization);

            if(props.Count > 0 && props[0].DeclaringType.FullName == "ForRobot.Model.Detals.Plita")
            {
                if (this._scoseType == ScoseTypes.Rect)
                {
                    props.Where(item => item.UnderlyingName == "Wight").First().Ignored = true;
                    props.Where(item => item.UnderlyingName == "BevelToLeft").First().Ignored = true;
                    props.Where(item => item.UnderlyingName == "BevelToRight").First().Ignored = true;
                }
            }

            //if (props.Count > 0 && props[0].DeclaringType.FullName == "ForRobot.Model.Detals.Rib")
            //{
            //    //if (this._paralleleRibs)
            //    //{
            //    //    props.Where(item => item.UnderlyingName == "DistanceLeft").First().Ignored = true;
            //    //}
            //    //else
            //    //{
            //    //    props.Where(item => item.UnderlyingName == "Distance").First().Ignored = true;
            //    //}
            //}

            return props;
        }
    }
}
