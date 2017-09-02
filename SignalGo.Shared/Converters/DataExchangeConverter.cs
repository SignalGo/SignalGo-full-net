using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Converters
{
    /// <summary>
    /// data exchanger of json serialize or deserializer
    /// </summary>
    public class DataExchangeConverter : JsonConverter
    {
        /// <summary>
        /// constructor of this attrib neeed your strategy mode
        /// </summary>
        /// <param name="mode">strategy mode</param>
        public DataExchangeConverter(SkipExchangeType mode)
        {
            Mode = mode;
        }
        /// <summary>
        /// your strategy mode for serialize and deserialize
        /// </summary>
        public SkipExchangeType Mode { get; set; }

        /// <summary>
        /// can convert or not
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// read json for deseralize object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            var implementICollection = (SkipDataExchangeAttribute)objectType.GetTypeInfo().GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#else
            var implementICollection = (SkipDataExchangeAttribute)objectType.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();

#endif
            bool? canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(null, null, objectType, implementICollection);
            if (canIgnore.HasValue)
            {
                if (canIgnore.Value)
                {
                    return null;
                }
            }
            else if (implementICollection != null && (implementICollection.Mode == SkipExchangeType.Both || implementICollection.Mode == Mode))
            {
                return null;
            }

            var jToken = JToken.Load(reader);
            var obj = jToken.ToObject(objectType);
            GenerateProperties(obj);
            return obj;
        }

        /// <summary>
        /// generate properties of object for deserialze
        /// </summary>
        /// <param name="instance"></param>
        void GenerateProperties(object instance)
        {
            if (instance == null)
                return;
            var type = instance.GetType();
            if (SerializeHelper.GetTypeCodeOfObject(instance.GetType()) != SerializeObjectType.Object)
            {
                return;
            }
            foreach (var property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    var implementICollection = (SkipDataExchangeAttribute)property.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
                    var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, type, implementICollection);
                    bool isIgnore = false;
                    if (canIgnore.HasValue)
                    {
                        if (canIgnore.Value)
                        {
                            isIgnore = true;
                        }
                    }
                    else if (implementICollection != null && (implementICollection.Mode == SkipExchangeType.Both || implementICollection.Mode == Mode))
                    {
                        isIgnore = true;
                    }
                    if (isIgnore)
                    {
                        property.SetValue(instance, null, null);
                    }
                    else
                    {
                        bool isPropertyArray = typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string);
                        bool isPropertyDictionary = typeof(IDictionary).IsAssignableFrom(property.PropertyType);
                        if (isPropertyDictionary)
                        {
                            var value = property.GetValue(instance, null);
                            if (value != null)
                                foreach (DictionaryEntry item in (IDictionary)value)
                                {
                                    GenerateProperties(item.Key);
                                    GenerateProperties(item.Value);
                                }
                        }
                        else if (isPropertyArray)
                        {
                            var value = property.GetValue(instance, null);
                            if (value != null)
                                foreach (object item in (IEnumerable)value)
                                {
                                    GenerateProperties(item);
                                }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// write json for serialize object
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                if (SerializeHelper.GetTypeCodeOfObject(value.GetType()) != SerializeObjectType.Object)
                {
                    writer.WriteValue(value);
                    return;
                }
                var type = value.GetType();
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                if (type.GetTypeInfo().BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
                {
                    type = type.GetTypeInfo().BaseType;
                }
#else
                 if (type.BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
                {
                    type = type.BaseType;
                }
#endif

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                var implementICollection = (SkipDataExchangeAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#else
                var implementICollection = (SkipDataExchangeAttribute)type.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#endif
                bool? canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(value, null, type, implementICollection);
                if (canIgnore.HasValue)
                {
                    if (canIgnore.Value)
                    {
                        return;
                    }
                }
                else if (implementICollection != null && (implementICollection.Mode == SkipExchangeType.Both || implementICollection.Mode == Mode))
                {
                    return;
                }

                bool isArray = typeof(IEnumerable).IsAssignableFrom(type) && !(value is string);
                bool isDictionary = typeof(IDictionary).IsAssignableFrom(type);

                if (isArray && !isDictionary)
                    writer.WriteStartArray();
                else
                    writer.WriteStartObject();
                if (isDictionary)
                {
                    foreach (DictionaryEntry item in (IDictionary)value)
                    {
                        JToken jTokenKey = JToken.FromObject(item.Key);
                        JToken jTokenValue = JToken.FromObject(item.Value);

                        writer.WritePropertyName(item.Key.ToString());

                        if (jTokenValue.Type == JTokenType.Object || jTokenValue.Type == JTokenType.Array)
                        {
                            serializer.Serialize(writer, item.Value);
                        }
                        else
                        {
                            writer.WriteValue(item.Value);
                        }
                    }
                }
                else if (isArray)
                {
                    foreach (var item in (IEnumerable)value)
                    {
                        if (item == null)
                            continue;
                        var itemType = item.GetType();
                        bool isPropertyArray = typeof(IEnumerable).IsAssignableFrom(itemType) && itemType != typeof(string);
                        bool isPropertyDictionary = typeof(IDictionary).IsAssignableFrom(itemType);
                        if (isPropertyArray || isPropertyDictionary)
                            serializer.Serialize(writer, item);
                        else
                        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                            bool canWriteFast = itemType == typeof(string) || !(itemType.GetTypeInfo().IsClass || itemType.GetTypeInfo().IsInterface);
#else
                            bool canWriteFast = itemType == typeof(string) || !(itemType.IsClass || itemType.IsInterface);
#endif
                            if (canWriteFast)
                                writer.WriteValue(item);
                            else
                                serializer.Serialize(writer, item);
                        }
                    }
                }
                else
                {
                    WriteData(type, value, writer, serializer);
                }

                if (isArray && !isDictionary)
                {
                    writer.WriteEndArray();
                }
                else
                    writer.WriteEndObject();
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "WriteJson");
            }

        }

        /// <summary>
        /// write data and convert to json for serialize
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="instance"></param>
        /// <param name="writer"></param>
        /// <param name="serializer"></param>
        void WriteData(Type baseType, object instance, JsonWriter writer, JsonSerializer serializer)
        {
            try
            {
                foreach (var property in baseType.GetProperties())
                {
                    if (property.CanRead)
                    {
                        var implementICollection = (SkipDataExchangeAttribute)property.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
                        var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, baseType, implementICollection);
                        bool isIgnored = false;
                        if (canIgnore.HasValue)
                        {
                            if (canIgnore.Value)
                            {
                                isIgnored = true;
                            }
                        }
                        else if (implementICollection != null && (implementICollection.Mode == SkipExchangeType.Both || implementICollection.Mode == Mode))
                        {
                            isIgnored = true;
                        }

                        if (!isIgnored)
                        {
                            bool isPropertyArray = typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string);
                            bool isPropertyDictionary = typeof(IDictionary).IsAssignableFrom(property.PropertyType);
                            if (isPropertyArray || isPropertyDictionary)
                            {
                                object propValue = null;
                                try
                                {
                                    propValue = property.GetValue(instance, null);
                                }
                                catch (Exception ex)
                                {
                                    AutoLogger.LogError(ex, "WriteData 1");
                                }
                                if (propValue != null)
                                {
                                    try
                                    {
                                        writer.WritePropertyName(property.Name);
                                        serializer.Serialize(writer, propValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        AutoLogger.LogError(ex, "WriteData 2");
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    var value = property.GetValue(instance, null);
                                    writer.WritePropertyName(property.Name);
                                    serializer.Serialize(writer, value);
                                }
                                catch (Exception ex)
                                {
                                    AutoLogger.LogError(ex, "WriteData 3");
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "WriteData 4");
            }
        }
    }
}
