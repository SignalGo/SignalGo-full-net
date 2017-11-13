using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    /// Creates a ICollection object.
    /// this will help you to convert entity framework ICollections
    /// </summary>
    /// <typeparam>The object type to convert.</typeparam>
    public class CustomICollectionCreationConverter : JsonConverter
    {
        Type BaseType { get; set; }

        public CustomICollectionCreationConverter()
        {
            BaseType = typeof(ICollection<>);
        }
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("CustomCreationConverter should only be used while deserializing.");
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var value = Create(objectType);
            if (value == null)
            {
                throw new JsonSerializationException("No object created.");
            }

            serializer.Populate(reader, value);
            return value;
        }

        /// <summary>
        /// Creates an object which will then be populated by the serializer.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The created object.</returns>
        public object Create(Type objectType)
        {
            var generic = objectType.GetListOfGenericArguments().FirstOrDefault();
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(generic));
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType.GetIsGenericType() && objectType.GetGenericTypeDefinition() == BaseType)
                return true;
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite
        {
            get { return false; }
        }
    }

    /// <summary>
    /// data exchanger of json serialize or deserializer
    /// </summary>
    public class DataExchangeConverter : JsonConverter
    {
        /// <summary>
        /// server of signalGo that called exchanger
        /// </summary>
        public object Server { get; set; }
        /// <summary>
        /// client of signalGo that called exchanger
        /// </summary>
        public object Client { get; set; }
        /// <summary>
        /// exchange types
        /// </summary>
        private CustomDataExchangerAttribute[] ExchangerTypes { get; set; }

        /// <summary>
        /// constructor of this attrib neeed your strategy mode
        /// </summary>
        /// <param name="mode">strategy mode</param>
        /// <param name="exchangerTypes">exchange types</param>
        public DataExchangeConverter(LimitExchangeType mode, params CustomDataExchangerAttribute[] exchangerTypes)
        {
            Mode = mode;
            ExchangerTypes = exchangerTypes;
        }

        /// <summary>
        /// your strategy mode for serialize and deserialize
        /// </summary>
        public LimitExchangeType Mode { get; set; }

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
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            var implementICollection = (SkipDataExchangeAttribute)objectType.GetTypeInfo().GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#else
            var implementICollection = (SkipDataExchangeAttribute)objectType.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();

#endif
            bool? canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(null, null, null, objectType, implementICollection);
            if (canIgnore.HasValue)
            {
                if (canIgnore.Value)
                {
                    return null;
                }
            }
            else if (implementICollection != null && (implementICollection.Mode == LimitExchangeType.Both || implementICollection.Mode == Mode))
            {
                return null;
            }

            var jToken = JToken.Load(reader);
            JsonSerializer sz = new JsonSerializer();
            sz.Converters.Add(new CustomICollectionCreationConverter());
            var obj = jToken.ToObject(objectType, sz);
            if (obj == null)
                obj = JsonConvert.DeserializeObject(jToken.ToString(), objectType);
            GenerateProperties(obj);
            return obj;
        }

        void MergeExchangeTypes(Type type, LimitExchangeType limitExchangeType)
        {
            if (ExchangerTypes == null || ExchangerTypes.Length == 0 || ExchangerTypes.Any(x => x.Type == type))
            {
                var customDataExchanger = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).ToList();
                foreach (var exchanger in customDataExchanger)
                {
                    exchanger.Type = type;
                }
                customDataExchanger.RemoveAll(x => (x.LimitationMode != LimitExchangeType.Both && x.LimitationMode != limitExchangeType) || !x.IsEnabled(Client, Server, null, type) || !x.GetExchangerByUserCustomization(Client));
                if (ExchangerTypes != null)
                    customDataExchanger.AddRange(ExchangerTypes);
                if (customDataExchanger.Count > 0)
                    ExchangerTypes = customDataExchanger.ToArray();
            }
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
            MergeExchangeTypes(type, LimitExchangeType.IncomingCall);
            if (SerializeHelper.GetTypeCodeOfObject(instance.GetType()) != SerializeObjectType.Object)
            {
                return;
            }
            foreach (var property in type.GetListOfProperties())
            {
                if (property.CanRead)
                {
                    var implementICollection = (SkipDataExchangeAttribute)property.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
                    var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, null, type, implementICollection);
                    bool isIgnored = false;
                    if (canIgnore.HasValue)
                    {
                        if (canIgnore.Value)
                        {
                            isIgnored = true;
                        }
                    }
                    else if (implementICollection != null && (implementICollection.Mode == LimitExchangeType.Both || implementICollection.Mode == Mode))
                    {
                        isIgnored = true;
                    }
                    if (!isIgnored)
                    {
                        if (ExchangerTypes != null)
                        {
                            var find = ExchangerTypes.FirstOrDefault(x => x.Type == type && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == LimitExchangeType.IncomingCall));
                            if (find != null && find.Properties != null)
                            {
                                if (find.CustomDataExchangerType == CustomDataExchangerType.Take)
                                {
                                    if (find.Properties != null && !find.Properties.Contains(property.Name) && find.IsEnabled(Client, Server, property.Name, type))
                                        isIgnored = true;
                                }
                                else
                                {
                                    if (find.Properties != null && find.Properties.Contains(property.Name) && find.IsEnabled(Client, Server, property.Name, type))
                                        isIgnored = true;
                                }
                            }
                        }
                    }
                    if (isIgnored)
                    {
                        property.SetValue(instance, null, null);
                    }
                    else
                    {
                        bool isPropertyArray = typeof(IEnumerable).GetIsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string);
                        bool isPropertyDictionary = typeof(IDictionary).GetIsAssignableFrom(property.PropertyType);
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
                            {
                                foreach (object item in (IEnumerable)value)
                                {
                                    GenerateProperties(item);
                                }
                            }
                            else if (property.PropertyType == typeof(ICollection<>))
                            {

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
                MergeExchangeTypes(type, LimitExchangeType.OutgoingCall);

#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                if (type.GetTypeInfo().BaseType != null && type.Namespace == "System.Data.Entity.DynamicProxies")
                {
                    type = type.GetTypeInfo().BaseType;
                }
#else
                if (type.GetBaseType() != null && type.Namespace == "System.Data.Entity.DynamicProxies")
                {
                    type = type.GetBaseType();
                }
#endif

#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                var implementICollection = (SkipDataExchangeAttribute)type.GetTypeInfo().GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#else
                var implementICollection = (SkipDataExchangeAttribute)type.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
#endif
                bool? canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(value, null, null, type, implementICollection);
                if (canIgnore.HasValue)
                {
                    if (canIgnore.Value)
                    {
                        return;
                    }
                }
                else if (implementICollection != null && (implementICollection.Mode == LimitExchangeType.Both || implementICollection.Mode == Mode))
                {
                    return;
                }

                bool isArray = typeof(IEnumerable).GetIsAssignableFrom(type) && !(value is string);
                bool isDictionary = typeof(IDictionary).GetIsAssignableFrom(type);

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
                        bool isPropertyArray = typeof(IEnumerable).GetIsAssignableFrom(itemType) && itemType != typeof(string);
                        bool isPropertyDictionary = typeof(IDictionary).GetIsAssignableFrom(itemType);
                        if (isPropertyArray || isPropertyDictionary)
                            serializer.Serialize(writer, item);
                        else
                        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                            bool canWriteFast = itemType == typeof(string) || !(itemType.GetTypeInfo().IsClass || itemType.GetTypeInfo().IsInterface);
#else
                            bool canWriteFast = itemType == typeof(string) || !(itemType.GetIsClass() || itemType.GetIsInterface());
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
                foreach (var property in baseType.GetListOfProperties())
                {
                    GenerateValue(property, null);
                }

                foreach (var field in baseType.GetListOfFields())
                {
                    GenerateValue(null, field);
                }
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "WriteData 4");
            }

            void GenerateValue(PropertyInfo property, FieldInfo field)
            {
                if ((property != null && property.CanRead) || field != null)
                {
                    SkipDataExchangeAttribute implementICollection = null;
                    if (property != null)
                        implementICollection = (SkipDataExchangeAttribute)property.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();
                    else if (field != null)
                        implementICollection = (SkipDataExchangeAttribute)field.GetCustomAttributes(typeof(SkipDataExchangeAttribute), true).FirstOrDefault();

                    var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, field, baseType, implementICollection);
                    bool isIgnored = false;
                    if (canIgnore.HasValue)
                    {
                        if (canIgnore.Value)
                        {
                            isIgnored = true;
                        }
                    }
                    else if (implementICollection != null && (implementICollection.Mode == LimitExchangeType.Both || implementICollection.Mode == Mode))
                    {
                        isIgnored = true;
                    }
                    if (!isIgnored)
                    {
                        if (ExchangerTypes != null)
                        {
                            var find = ExchangerTypes.FirstOrDefault(x => x.Type == baseType && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == LimitExchangeType.OutgoingCall));
                            if (find != null && find.Properties != null)
                            {
                                if (find.CustomDataExchangerType == CustomDataExchangerType.Take)
                                {
                                    if (property != null)
                                    {
                                        if (find.Properties != null && !find.Properties.Contains(property.Name) && find.IsEnabled(Client, Server, property.Name, baseType))
                                            isIgnored = true;
                                    }

                                    else if (field != null)
                                    {
                                        if (find.Properties != null && !find.Properties.Contains(field.Name) && find.IsEnabled(Client, Server, field.Name, baseType))
                                            isIgnored = true;
                                    }
                                }
                                else
                                {
                                    if (property != null)
                                    {
                                        if (find.Properties != null && find.Properties.Contains(property.Name) && find.IsEnabled(Client, Server, property.Name, baseType))
                                            isIgnored = true;
                                    }
                                    else if (field != null)
                                    {
                                        if (find.Properties != null && find.Properties.Contains(field.Name) && find.IsEnabled(Client, Server, field.Name, baseType))
                                            isIgnored = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!isIgnored)
                    {
                        bool isPropertyArray = false;
                        if (property != null)
                            isPropertyArray = typeof(IEnumerable).GetIsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string);
                        else if (field != null)
                            isPropertyArray = typeof(IEnumerable).GetIsAssignableFrom(field.FieldType) && field.FieldType != typeof(string);

                        bool isPropertyDictionary = false;
                        if (property != null)
                            isPropertyDictionary = typeof(IDictionary).GetIsAssignableFrom(property.PropertyType);
                        else if (field != null)
                            isPropertyDictionary = typeof(IDictionary).GetIsAssignableFrom(field.FieldType);

                        if (isPropertyArray || isPropertyDictionary)
                        {
                            object propValue = null;
                            try
                            {
                                if (property != null)
                                    propValue = property.GetValue(instance, null);
                                else if (field != null)
                                    propValue = field.GetValue(instance);
                            }
                            catch (Exception ex)
                            {
                                AutoLogger.LogError(ex, "WriteData 1");
                            }
                            if (propValue != null)
                            {
                                try
                                {
                                    if (property != null)
                                        writer.WritePropertyName(property.Name);
                                    else if (field != null)
                                        writer.WritePropertyName(field.Name);
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
                                object value = null;
                                if (property != null)
                                {
                                    value = property.GetValue(instance, null);
                                    writer.WritePropertyName(property.Name);
                                }
                                else if (field != null)
                                {
                                    value = field.GetValue(instance);
                                    writer.WritePropertyName(field.Name);
                                }
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
    }
}
