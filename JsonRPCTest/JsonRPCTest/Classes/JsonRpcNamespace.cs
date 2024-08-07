using System;
using Newtonsoft.Json.Linq;

namespace JsonRPCTest.Classes
{
    public class JsonRpcNamespace
    {
        #region Private variables

        protected JsonRpcClient client;

        #endregion

        #region Constructor

        protected JsonRpcNamespace(JsonRpcClient client)
        {
            this.client = client ?? throw new ArgumentNullException("client");
        }

        #endregion

        #region Helper functions

        protected TType getInfo<TType>(string label)
        {
            return this.getInfo<TType>(label, default(TType));
        }

        protected TType getInfo<TType>(string label, TType defaultValue)
        {
            this.client.LogMessage("System.GetInfoLabels(" + label + ")");

            JObject result = this.client.Call("System.GetInfoLabels", new string[] { label }) as JObject;
            if (result == null || result[label] == null)
            {
                this.client.LogErrorMessage("System.GetInfoLabels(" + label + "): Invalid response");

                return defaultValue;
            }

            return JsonRpcClient.GetField<TType>(result, label, defaultValue);
        }

        #endregion
    }
}
