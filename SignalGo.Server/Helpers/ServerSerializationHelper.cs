using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Helpers
{
    public static class ServerSerializationHelper
    {
        public static string SerializeObject(this object obj, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            if (obj == null)
                return "";
            if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.OutgoingCall, customDataExchanger) { Server = serverBase, Client = client } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static T Deserialize<T>(this string json, ServerBase serverBase = null, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            return (T)Deserialize(json, typeof(T), serverBase, customDataExchanger: customDataExchanger, client: client);
        }

        public static object Deserialize(this string json, ServerBase serverBase = null, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            return Deserialize<object>(json, serverBase, customDataExchanger: customDataExchanger,client:client);
        }

        public static object Deserialize(this string json, Type type, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
                return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger) { Server = serverBase, Client = client } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
