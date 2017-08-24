using Newtonsoft.Json;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Helpers
{
    public static class ServerSerializationHelper
    {
        public static string SerializeObject(this object obj, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (obj == null)
                return "";
            if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(SkipExchangeType.OutgoingCall) }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static T Deserialize<T>(this string json, ServerBase serverBase = null)
        {
            return (T)Deserialize(json, typeof(T), serverBase);
        }

        public static object Deserialize(this string json, ServerBase serverBase = null)
        {
            return Deserialize<object>(json, serverBase);
        }

        public static object Deserialize(this string json, Type type, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
                return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(SkipExchangeType.IncomingCall) }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
