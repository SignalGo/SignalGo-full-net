using Newtonsoft.Json;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalGo.Server.Helpers
{
    /// <summary>
    /// static servialize and deserialize system
    /// </summary>
    public static class ServerSerializationHelper
    {
        /// <summary>
        /// serialize an object
        /// </summary>
        /// <param name="obj">object that you want to serialize to json</param>
        /// <param name="serverBase">server provider</param>
        /// <param name="nullValueHandling"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string SerializeObject(this object obj, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            if (obj == null)
                return "";
            if (serverBase != null && serverBase.ProviderSetting.IsEnabledDataExchanger)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new CustomICollectionCreationConverter(), new DataExchangeConverter(LimitExchangeType.OutgoingCall, customDataExchanger) { CurrentTaskId = Task.CurrentId, Server = serverBase, Client = client, IsEnabledReferenceResolver = serverBase.ProviderSetting.IsEnabledReferenceResolver, IsEnabledReferenceResolverForArray = serverBase.ProviderSetting.IsEnabledReferenceResolverForArray, } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// deserialize json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="serverBase"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string json, ServerBase serverBase = null, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            return (T)Deserialize(json, typeof(T), serverBase, customDataExchanger: customDataExchanger, client: client);
        }

        /// <summary>
        /// deserialize json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="serverBase"></param>
        /// <param name="nullValueHandling"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static object Deserialize(this string json, Type type, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (serverBase != null && serverBase.ProviderSetting.IsEnabledDataExchanger)
                return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new CustomICollectionCreationConverter(), new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger) { CurrentTaskId = Task.CurrentId, ValidationRuleInfoManager = serverBase?.ValidationRuleInfoManager, Server = serverBase, Client = client, IsEnabledReferenceResolver = serverBase.ProviderSetting.IsEnabledReferenceResolver, IsEnabledReferenceResolverForArray = serverBase.ProviderSetting.IsEnabledReferenceResolverForArray } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// desrialize json by validation
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="serverBase"></param>
        /// <param name="nullValueHandling"></param>
        /// <param name="customDataExchanger"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static object DeserializeByValidate(this string json, Type type, ServerBase serverBase = null, NullValueHandling nullValueHandling = NullValueHandling.Ignore, CustomDataExchangerAttribute[] customDataExchanger = null, ClientInfo client = null)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            if (!IsValidJson(json))
                json = SerializeObject(json, serverBase, nullValueHandling, customDataExchanger, client);
            if (serverBase != null && serverBase.ProviderSetting.IsEnabledDataExchanger)
                return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Converters = new List<JsonConverter>() { new CustomICollectionCreationConverter(), new DataExchangeConverter(LimitExchangeType.IncomingCall, customDataExchanger) { CurrentTaskId = Task.CurrentId, ValidationRuleInfoManager = serverBase?.ValidationRuleInfoManager, Server = serverBase, Client = client, IsEnabledReferenceResolver = serverBase.ProviderSetting.IsEnabledReferenceResolver, IsEnabledReferenceResolverForArray = serverBase.ProviderSetting.IsEnabledReferenceResolverForArray } }, Formatting = Formatting.None, NullValueHandling = nullValueHandling });
            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings() { Formatting = Formatting.None, NullValueHandling = nullValueHandling, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// check if the string is json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
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
