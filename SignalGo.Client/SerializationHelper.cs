using Newtonsoft.Json;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;

namespace SignalGo.Client
{
    public static class ClientSerializationHelper
    {
        public static string SerializeObject(this object obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null)
        {
            if (obj == null)
                return "";
            //if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.OutgoingCall, customDataExchanger) }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            //return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static T DeserializeObject<T>(this string json, CustomDataExchangerAttribute[] customDataExchanger = null)
        {
            return (T)DeserializeObject(json, typeof(T), customDataExchanger: customDataExchanger);
        }

        public static object DeserializeObject(this string json, CustomDataExchangerAttribute[] customDataExchanger = null)
        {
            return DeserializeObject<object>(json, customDataExchanger: customDataExchanger);
        }

        public static object DeserializeObject(this string json, Type type, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger) } });
        }

        //public static object DeserializeByValidate(this string json, Type type, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null)
        //{
        //    if (string.IsNullOrEmpty(json))
        //        return null;
        //    if (!IsValidJson(json))
        //        json = SerializeObject(json, nullValueHandling, customDataExchanger);
        //    if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
        //        return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger) { Server = serverBase, Client = client } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
        //    return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        //}

        public static bool IsValidJson(this string json)
        {
            json = json.Trim();
            if ((json.StartsWith("{") && json.EndsWith("}")) ||
                (json.StartsWith("[") && json.EndsWith("]")) ||
                (json.StartsWith("\"") && json.EndsWith("\"")))
            {
                return true;
            }
            return false;
        }
    }
}
