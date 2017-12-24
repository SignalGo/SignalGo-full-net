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
            object value = null;
            if (objectType.GetIsGenericType() && objectType.GetGenericTypeDefinition() == BaseType)
            {
                value = Create(objectType);
                if (value == null)
                {
                    throw new JsonSerializationException("No object created.");
                }
                serializer.Populate(reader, value);
            }
            else
            {
                if (SerializeHelper.HandleDeserializingObjectList.TryGetValue(objectType, out SerializeDelegateHandler serializehandling))
                {
                    try
                    {
                        var json = JToken.Load(reader);
                        var instance = json.ToObject(serializehandling.ParameterType);
                        value = serializehandling.Delegate.DynamicInvoke(instance);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

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
            if (objectType.GetIsGenericType() && objectType.GetGenericTypeDefinition() == BaseType || SerializeHelper.HandleDeserializingObjectList.ContainsKey(objectType))
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

        List<object> SerializedObjects = new List<object>();


        bool? CanIgnoreCustomDataExchanger(Type type, object instance)
        {
            CustomDataExchangerAttribute implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both)).FirstOrDefault();
            if (implementICollection == null)
                implementICollection = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();

            bool? canIgnore = implementICollection == null ? null : implementICollection.CanIgnore(instance, null, null, type, Server, Client);
            if ((canIgnore ?? false))
                return true;
            else if (implementICollection != null && implementICollection.ExchangeType == CustomDataExchangerType.Ignore && implementICollection.Properties == null && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                return true;
            return false;
        }

        CustomDataExchangerAttribute GetCustomDataExchanger(Type type, object instance)
        {
            CustomDataExchangerAttribute implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both)).FirstOrDefault();
            if (implementICollection == null)
                implementICollection = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();

            return implementICollection;
        }

        bool? CanIgnoreCustomDataExchanger(Type type, PropertyInfo property, object instance)
        {
            CustomDataExchangerAttribute implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both)).FirstOrDefault();
            if (implementICollection == null)
                implementICollection = property.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();

            bool? canIgnore = implementICollection == null ? null : implementICollection.CanIgnore(instance, property, null, type, Server, Client);
            if ((canIgnore ?? false))
                return true;
            else if (implementICollection != null && implementICollection.ExchangeType == CustomDataExchangerType.Ignore && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                return true;
            return false;
        }

        bool? CanIgnoreCustomDataExchanger(Type type, FieldInfo fieldInfo, object instance)
        {
            CustomDataExchangerAttribute implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both)).FirstOrDefault();
            if (implementICollection == null)
                implementICollection = fieldInfo.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();

            bool? canIgnore = implementICollection == null ? null : implementICollection.CanIgnore(instance, null, fieldInfo, type, Server, Client);
            if ((canIgnore ?? false))
                return true;
            else if (implementICollection != null && implementICollection.ExchangeType == CustomDataExchangerType.Ignore && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                return true;
            return false;
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
            if ((CanIgnoreCustomDataExchanger(objectType, existingValue) ?? false))
                return null;

            var jToken = JToken.Load(reader);
            JsonSerializer sz = new JsonSerializer()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            sz.Converters.Add(new CustomICollectionCreationConverter() { });

            object obj = null;

            try
            {
                obj = jToken.ToObject(objectType, sz);
            }
            catch (Exception ex)
            {

            }
            if (obj == null)
                obj = JsonConvert.DeserializeObject(jToken.ToString(), objectType);
            GenerateProperties(obj);
            return obj;
        }

        //void MergeExchangeTypes(Type type, LimitExchangeType limitExchangeType)
        //{
        //    if (ExchangerTypes == null || ExchangerTypes.Length == 0 || ExchangerTypes.Any(x => x.Type == type))
        //    {
        //        var customDataExchanger = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).ToList();
        //        foreach (var exchanger in customDataExchanger)
        //        {
        //            exchanger.Type = type;
        //        }
        //        customDataExchanger.RemoveAll(x => (x.LimitationMode != LimitExchangeType.Both && x.LimitationMode != limitExchangeType) || !(x.CanIgnore(null, null, null, type, Client, Server) ?? false) || !x.GetExchangerByUserCustomization(Client));
        //        if (ExchangerTypes != null)
        //            customDataExchanger.AddRange(ExchangerTypes);
        //        if (customDataExchanger.Count > 0)
        //            ExchangerTypes = customDataExchanger.ToArray();
        //    }
        //}

        /// <summary>
        /// generate properties of object for deserialze
        /// </summary>
        /// <param name="instance"></param>
        void GenerateProperties(object instance)
        {
            if (instance == null)
                return;
            var type = instance.GetType();
            //MergeExchangeTypes(type, Mode);
            if (SerializeHelper.GetTypeCodeOfObject(instance.GetType()) != SerializeObjectType.Object)
            {
                return;
            }
            foreach (var property in type.GetListOfProperties())
            {
                if (property.CanRead)
                {
                    //var implementICollection = property.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();
                    //var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, null, type, Server, Client);
                    //bool isIgnored = false;
                    //if (canIgnore.HasValue)
                    //{
                    //    if (canIgnore.Value)
                    //    {
                    //        isIgnored = true;
                    //    }
                    //}
                    //else if (implementICollection != null && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                    //{
                    //    isIgnored = true;
                    //}
                    var isIgnored = CanIgnoreCustomDataExchanger(type, property, instance) ?? false;

                    if (!isIgnored)
                    {
                        if (ExchangerTypes != null)
                        {
                            var find = ExchangerTypes.FirstOrDefault(x => x.Type == type && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == Mode));
                            if (find != null && find.Properties != null)
                            {
                                var manualCanIngnore = find.CanIgnore(instance, property, null, type, Client, Server);
                                if (find.ExchangeType == CustomDataExchangerType.Take)
                                {
                                    if (find.Properties != null && !find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                                        isIgnored = true;
                                }
                                else
                                {
                                    if (find.Properties != null && find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
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
                        }
                    }
                }
            }
        }

        bool HasJsonIgnore(Type type)
        {
            return type != null && type.GetCustomAttributes<JsonIgnoreAttribute>(true).Any();
        }

        bool HasJsonIgnore(PropertyInfo property)
        {
            return property != null && AttributeHelper.GetCustomAttributes<JsonIgnoreAttribute>(property, true).Any();
        }

        bool HasJsonIgnore(FieldInfo field)
        {
            return field != null && AttributeHelper.GetCustomAttributes<JsonIgnoreAttribute>(field, true).Any();
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
                var type = value.GetType();
                if (HasJsonIgnore(type))
                    return;
                if (SerializeHelper.GetTypeCodeOfObject(type) != SerializeObjectType.Object)
                {
                    writer.WriteValue(value);
                    return;
                }
                //else
                //{
                //    if (SerializedObjects.Contains(value))
                //        return;
                //    else
                //        SerializedObjects.Add(value);
                //}
                SerializeHelper.HandleSerializingObjectList.TryGetValue(type, out Delegate serializeHandler);
                if (serializeHandler != null)
                {
                    value = serializeHandler.DynamicInvoke(value);
                    type = value.GetType();
                    if (SerializeHelper.GetTypeCodeOfObject(value.GetType()) != SerializeObjectType.Object)
                    {
                        writer.WriteValue(value);
                        return;
                    }
                }


                //MergeExchangeTypes(type, Mode);

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

                //var implementICollection = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();
                //bool? canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(value, null, null, type, Server, Client);
                //if (canIgnore.HasValue)
                //{
                //    if (canIgnore.Value)
                //    {
                //        return;
                //    }
                //}
                //else if (implementICollection != null && implementICollection.Type == type && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                //{
                //    return;
                //}
                if ((CanIgnoreCustomDataExchanger(type, value) ?? false))
                {
                    if (writer.WriteState == WriteState.Property)
                        writer.WriteValue((object)null);
                    //    writer.WriteEnd();
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
                    GenerateDictionary(value, writer, serializer);
                }
                else if (isArray)
                {
                    GenerateArray(value, writer, serializer);
                }
                else
                {
                    WriteData(type, value, writer, serializer);
                }
                //writer.WriteEnd();
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

        void GenerateDictionary(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<DictionaryEntry> items = new List<DictionaryEntry>();
            foreach (DictionaryEntry item in (IDictionary)value)
            {
                if (item.Value == null)
                    continue;
                var itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.Value.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (SerializedObjects.Contains(item.Value))
                        continue;
                    else
                        SerializedObjects.Add(item.Value);
                    items.Add(item);
                }
                else
                {
                    writer.WritePropertyName(item.Key.ToString());
                    writer.WriteValue(item.Value);

                }
            }
            foreach (var item in items)
            {
                serializer.Serialize(writer, item.Value);
            }
        }

        void GenerateArray(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<object> objects = new List<object>();
            foreach (var item in (IEnumerable)value)
            {
                if (item == null)
                    continue;
                var itemType = item.GetType();
                var itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (SerializedObjects.Contains(item))
                        continue;
                    else
                        SerializedObjects.Add(item);
                    objects.Add(item);
                }
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
            foreach (var item in objects)
            {
                serializer.Serialize(writer, item);
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
                //#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                //                if (baseType.GetTypeInfo().BaseType != null && baseType.Namespace == "System.Data.Entity.DynamicProxies")
                //                {
                //                    baseType = baseType.GetTypeInfo().BaseType;
                //                }
                //#else
                //                if (baseType.GetBaseType() != null && baseType.Namespace == "System.Data.Entity.DynamicProxies")
                //                {
                //                    baseType = baseType.GetBaseType();
                //                }
                //#endif
                //var implementICollection = baseType.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();
                //var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, null, null, baseType, Server, Client);
                //if (canIgnore.HasValue)
                //{
                //    if (canIgnore.Value)
                //        return;
                //}
                //else if (implementICollection != null && implementICollection.Type == baseType && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                //{
                //    return;
                //}
                var implementICollection = GetCustomDataExchanger(baseType, instance);
                if ((CanIgnoreCustomDataExchanger(baseType, instance) ?? false))
                    return;

                foreach (var property in baseType.GetListOfProperties())
                {
                    if (implementICollection != null)
                    {
                        if (implementICollection.ExchangeType == CustomDataExchangerType.Ignore && implementICollection.ContainsProperty(property.Name))
                            continue;
                        else if (implementICollection.ExchangeType == CustomDataExchangerType.Take && !implementICollection.ContainsProperty(property.Name))
                            continue;
                    }

                    GenerateValue(property, null);
                }
                foreach (var field in baseType.GetListOfFields())
                {
                    if (implementICollection != null)
                    {
                        if (implementICollection.ExchangeType == CustomDataExchangerType.Ignore && implementICollection.ContainsProperty(field.Name))
                            continue;
                        else if (implementICollection.ExchangeType == CustomDataExchangerType.Take && !implementICollection.ContainsProperty(field.Name))
                            continue;
                    }
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
                    if (HasJsonIgnore(property) || HasJsonIgnore(field))
                        return;
                    bool isIgnored = false;
                    if (property != null)
                        isIgnored = CanIgnoreCustomDataExchanger(baseType, property, instance) ?? false;
                    if (field != null)
                        isIgnored = CanIgnoreCustomDataExchanger(baseType, field, instance) ?? false;
                    //CustomDataExchangerAttribute implementICollection = null;
                    //if (property != null)
                    //    implementICollection = property.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();
                    //else if (field != null)
                    //    implementICollection = field.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.LimitationMode == Mode || x.LimitationMode == LimitExchangeType.Both).FirstOrDefault();

                    //var canIgnore = implementICollection == null ? (bool?)null : implementICollection.CanIgnore(instance, property, field, baseType, Server, Client);
                    //bool isIgnored = false;
                    //if (canIgnore.HasValue)
                    //{
                    //    if (canIgnore.Value)
                    //    {
                    //        isIgnored = true;
                    //    }
                    //}
                    //else if (implementICollection != null && (implementICollection.LimitationMode == LimitExchangeType.Both || implementICollection.LimitationMode == Mode))
                    //{
                    //    isIgnored = true;
                    //}
                    if (!isIgnored)
                    {
                        if (ExchangerTypes != null)
                        {
                            var find = ExchangerTypes.FirstOrDefault(x => x.Type == baseType && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == Mode));
                            if (find != null && find.Properties != null)
                            {
                                var manualCanIngnore = find.CanIgnore(instance, property, field, baseType, Client, Server);
                                if (find.ExchangeType == CustomDataExchangerType.Take)
                                {
                                    if (property != null)
                                    {
                                        if (find.Properties != null && !find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                                            isIgnored = true;
                                    }

                                    else if (field != null)
                                    {
                                        if (find.Properties != null && !find.Properties.Contains(field.Name) && (manualCanIngnore ?? false))
                                            isIgnored = true;
                                    }
                                }
                                else
                                {
                                    if (property != null)
                                    {
                                        if (find.Properties != null && find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                                            isIgnored = true;
                                    }
                                    else if (field != null)
                                    {
                                        if (find.Properties != null && find.Properties.Contains(field.Name) && (manualCanIngnore ?? false))
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
                                    var itemJsonType = SerializeHelper.GetTypeCodeOfObject(propValue.GetType());
                                    if (itemJsonType == SerializeObjectType.Object)
                                    {
                                        if (SerializedObjects.Contains(propValue))
                                            return;
                                        else
                                            SerializedObjects.Add(propValue);
                                    }
                                    if (property != null)
                                        writer.WritePropertyName(property.Name);
                                    else if (field != null)
                                        writer.WritePropertyName(field.Name);
                                    SerializeHelper.HandleSerializingObjectList.TryGetValue(property.PropertyType, out Delegate serializeHandler);
                                    if (serializeHandler != null)
                                        propValue = serializeHandler.DynamicInvoke(propValue);
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
                                    SerializeHelper.HandleSerializingObjectList.TryGetValue(property.PropertyType, out Delegate serializeHandler);
                                    if (serializeHandler != null)
                                        value = serializeHandler.DynamicInvoke(value);
                                    if (value == null)
                                        return;
                                    var itemJsonType = SerializeHelper.GetTypeCodeOfObject(value.GetType());
                                    if (itemJsonType == SerializeObjectType.Object)
                                    {
                                        if (SerializedObjects.Contains(value))
                                            return;
                                        else
                                            SerializedObjects.Add(value);
                                    }
                                    //if (SerializeHelper.GetTypeCodeOfObject(value.GetType()) != SerializeObjectType.Object)
                                    writer.WritePropertyName(property.Name);
                                }
                                else if (field != null)
                                {
                                    value = field.GetValue(instance);
                                    SerializeHelper.HandleSerializingObjectList.TryGetValue(field.FieldType, out Delegate serializeHandler);
                                    if (serializeHandler != null)
                                        value = serializeHandler.DynamicInvoke(value);
                                    if (value == null)
                                        return;
                                    var itemJsonType = SerializeHelper.GetTypeCodeOfObject(value.GetType());
                                    if (itemJsonType == SerializeObjectType.Object)
                                    {
                                        if (SerializedObjects.Contains(value))
                                            return;
                                        else
                                            SerializedObjects.Add(value);
                                    }
                                    // if (SerializeHelper.GetTypeCodeOfObject(value.GetType()) != SerializeObjectType.Object)
                                    writer.WritePropertyName(field.Name);

                                }
                                //if (value != instance)//loop handling
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
