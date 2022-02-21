using Newtonsoft.Json;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;

namespace SignalGo.Client
{
    public static class ClientSerializationHelper
    {
        public static JsonSettingHelper JsonSettingHelper { get; set; } = new JsonSettingHelper();
        /// <summary>
        /// enable refrence serializing when duplicate object detected
        /// </summary>
        public static bool IsEnabledReferenceResolver { get; set; } = true;
        /// <summary>
        /// enable refrence serializing when duplicate list of objects detected
        /// </summary>
        public static bool IsEnabledReferenceResolverForArray { get; set; } = true;
        public static string SerializeObject(this object obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null)
        {
            if (obj == null)
                return "";
            if (!IsEnabledReferenceResolver && !IsEnabledReferenceResolverForArray)
            {
                return JsonConvert.SerializeObject(obj, JsonSettingHelper.GlobalJsonSetting);
            }
            //if (serverBase != null && serverBase.InternalSetting.IsEnabledDataExchanger)
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Converters = JsonSettingHelper.GetConverters(new DataExchangeConverter(LimitExchangeType.OutgoingCall, customDataExchanger)
                {
                    IsEnabledReferenceResolver = IsEnabledReferenceResolver,
                    IsEnabledReferenceResolverForArray = IsEnabledReferenceResolverForArray
                }),
                Formatting = Formatting.None,
                NullValueHandling = nullValueHandling
            });
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
            if (!IsEnabledReferenceResolver && !IsEnabledReferenceResolverForArray)
            {
                return JsonConvert.DeserializeObject(json, type, JsonSettingHelper.GlobalJsonSetting);
            }
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                NullValueHandling = nullValueHandling,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Converters = JsonSettingHelper.GetConverters(new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger)
                {
                    IsEnabledReferenceResolver = IsEnabledReferenceResolver,
                    IsEnabledReferenceResolverForArray = IsEnabledReferenceResolverForArray
                })
            });
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
