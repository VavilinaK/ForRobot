using System;
using System.Net;
using System.Collections;
using System.IO;
using System.Text;

using Newtonsoft.Json.Linq;

namespace JsonRPCTest.Classes
{
    public class JsonRpcClient
    {
        #region Private variables

        private const int CallIdMaximum = 100000;
        private const string JsonResponseError = "error";
        private const string JsonResponseResult = "result";

        private int callId;

        private readonly Uri uri;
        private readonly string username;
        private readonly string password;

        private int timeout;

        #endregion

        #region Events

        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<JsonRpcLogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<JsonRpcLogErrorEventArgs> LogError;

        #endregion

        #region Public variables

        internal Uri Uri { get => this.uri; }
        internal string Username { get => this.username; }
        internal string Password { get => this.password; }
        internal int Timeout
        {
            get=> this.timeout;
            set
            {
                if (value < 1000)
                {
                    value = 1000;
                }

                this.timeout = value;
            }
        }

        #endregion

        #region Constructors

        public JsonRpcClient(Uri uri, string username, string password)
        {
            this.uri = uri ?? throw new ArgumentNullException("uri");
            this.username = username;
            this.password = password;

            this.timeout = 5000;
        }

        #endregion

        #region Public functions

        public object Call(string method) => this.Call(method, (object)null);

        public virtual object Call(string method, object args)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException();
            }

            this.LogMessage("Calling JSON RPC method \"" + method + "\"...");

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.uri);
                request.AllowWriteStreamBuffering = true;
                request.ContentType = "application/json";
                request.Credentials = new NetworkCredential(this.username, this.password);
                request.KeepAlive = false;
                request.Method = "POST";
                request.Timeout = this.timeout;

                using (Stream requestStream = request.GetRequestStream())
                {
                    using (StreamWriter requestWriter = new StreamWriter(requestStream, Encoding.ASCII))
                    //using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
                    {
                    if (this.callId >= CallIdMaximum)
                    {
                        this.callId = 0;
                    }
                    this.callId += 1;

                    JObject call = new JObject();
                    //call.Add(new JProperty("jsonrpc", "2.0"));
                    call.Add(new JProperty("method", method));
                    if (args != null)
                    {
                        call.Add(new JProperty("params", args));
                    }
                    call.Add(new JProperty("id", this.callId));

                    this.LogMessage("JSON RPC call: " + call.ToString());

                    call.First.ToString().Replace("\"", "'");

                    requestWriter.Write("{" + call.First.ToString() + ",\"params\":[\"" + call.SelectToken("params").First.Last.ToString() + "\"],\"id\":1}");
                    //requestWriter.Write(call.ToString());
                }
                }

                //var httpResponse = (HttpWebResponse)request.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                //    var result = streamReader.ReadToEnd();
                //}

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            return this.ParseResponse(responseReader);
                        }
                    }
                }
        }
            catch (Exception ex)
            {
                this.LogErrorMessage("Error while calling JSON RPC method \"" + method + "\"", ex);
                return null;
            }
}

        public object Call(string method, IDictionary args) => this.Call(method, (object)args);

        public object Call(string method, params object[] args)
        {
            return this.Call(method, (object)args);
        }

        public static TType GetField<TType>(JObject obj, string field) => GetField<TType>(obj, field, default(TType));

        public static TType GetField<TType>(JObject obj, string field, TType defaultValue)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentException();
            }

            try
            {
                return (TType)Convert.ChangeType(obj[field].Value<JValue>().Value, typeof(TType));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        #endregion

        #region Internal functions

        internal void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Log == null)
            {
                return;
            }

            this.Log(this, new JsonRpcLogEventArgs(message));
        }

        internal void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        internal void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
            {
                return;
            }

            this.LogError(this, new JsonRpcLogErrorEventArgs(message, exception));
        }

        #endregion

        #region Private functions

        private object ParseResponse(StreamReader reader)
        {
            string response = reader.ReadToEnd();
            this.LogMessage("JSON RPC response: " + response);

            JObject responseObject = JObject.Parse(response);
            foreach (JProperty property in responseObject.Properties())
            {
                if (string.CompareOrdinal(property.Name, JsonResponseError) == 0)
                {
                    this.ParseError(property.Value as JObject);
                }

                if (string.CompareOrdinal(property.Name, JsonResponseResult) == 0)
                {
                    if (property.Value.HasValues == true)
                    {
                        return property.Value as JObject;
                    }
                    if (property.Value.Type == JTokenType.Integer)
                    {
                        return (int)property.Value.Value<JValue>();
                    }
                    if (property.Value.Type == JTokenType.Float)
                    {
                        return (float)property.Value.Value<JValue>();
                    }
                    if (property.Value.Type == JTokenType.String)
                    {
                        return property.Value.Value<JValue>().Value.ToString();
                    }

                    return property.Value.Value<JValue>();
                }
            }

            this.LogErrorMessage("Invalid JSON RPC response: " + response);
            throw new InvalidJsonRpcResponseException(response);
        }

        private void ParseError(JObject error)
        {
            this.LogErrorMessage("JSON RPC error received: " + error != null ? error.ToString() : "unknown");

            if (error == null)
            {
                throw new UnknownJsonRpcErrorException();
            }

            throw new JsonRpcErrorException(GetField<int>(error, "code"), GetField<string>(error, "message"));
        }

        #endregion
    }
}
