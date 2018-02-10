using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Converters
{
    internal class DefaultReferenceResolver
    {
        private int _referenceCount;

        //private BidirectionalDictionary<string, object> GetMappings(object context)
        //{
        //    JsonSerializerInternalBase internalSerializer = context as JsonSerializerInternalBase;
        //    if (internalSerializer == null)
        //    {
        //        JsonSerializerProxy proxy = context as JsonSerializerProxy;
        //        if (proxy != null)
        //        {
        //            internalSerializer = proxy.GetInternalSerializer();
        //        }
        //        else
        //        {
        //            throw new JsonException("The DefaultReferenceResolver can only be used internally.");
        //        }
        //    }

        //    return internalSerializer.DefaultReferenceMappings;
        //}

        //public object ResolveReference(object context, string reference)
        //{
        //    object value;
        //    GetMappings(context).TryGetByFirst(reference, out value);
        //    return value;
        //}
        BidirectionalDictionary<string, object> mappings = new BidirectionalDictionary<string, object>();
        public string GetReference(object context, object value)
        {


            string reference;
            if (!mappings.TryGetBySecond(value, out reference))
            {
                _referenceCount++;
                reference = _referenceCount.ToString(CultureInfo.InvariantCulture);
                mappings.Set(reference, value);
            }

            return reference;
        }

        public void AddReference(object context, string reference, object value)
        {
            mappings.Set(reference, value);
        }

        public bool IsReferenced(object context, object value)
        {
            string reference;
            return mappings.TryGetBySecond(value, out reference);
        }
    }

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
        /// enable refrence serializing when duplicate object detected
        /// </summary>
        public bool IsEnabledReferenceResolver { get; set; } = true;
        public bool IsEnabledReferenceResolverForArray { get; set; } = true;
        private DefaultReferenceResolver ReferenceResolver { get; set; } = new DefaultReferenceResolver();
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
        Dictionary<int, object> SerializedReferencedObjects = new Dictionary<int, object>();


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

        public static string refProperty = "$ref";
        public static string idProperty = "$id";
        public static string valuesProperty = "$values";

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

            //var jToken = JToken.Load(reader);
            //JsonSerializer sz = new JsonSerializer()
            //{
            //    MissingMemberHandling = MissingMemberHandling.Ignore,
            //    NullValueHandling = NullValueHandling.Ignore
            //};
            //sz.Converters.Add(new CustomICollectionCreationConverter() { });

            //object obj = null;

            //try
            //{
            //    obj = jToken.ToObject(objectType, sz);
            //}
            //catch (Exception ex)
            //{

            //}
            //try
            //{
            //    if (obj == null)
            //    {
            //        var str = jToken.ToString();
            //        obj = JsonConvert.DeserializeObject(str, objectType, new JsonSerializerSettings()
            //        {
            //            Error = (o, x) =>
            //            {

            //            },
            //            NullValueHandling = NullValueHandling.Ignore
            //        });
            //    }

            //}
            //catch (Exception ex)
            //{

            //}
            //GenerateProperties(obj);
            //return obj;

            //try
            //{
            if (reader.TokenType == JsonToken.StartObject)
            {
                var instance = ReadNewObject(reader, objectType, existingValue, serializer);
                while (reader.Read())
                {

                }
                //var instance = Activator.CreateInstance(objectType);
                //while (reader.Read())
                //{
                //    if (reader.TokenType == JsonToken.PropertyName)
                //    {
                //        ReadNewProperty(instance, reader, objectType, existingValue, serializer);
                //    }
                //}
                return instance;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var instance = ReadNewArray(null, reader, objectType, existingValue, serializer);
                return instance;
            }
            else
            {
                return SerializeHelper.ConvertType(objectType, reader.Value);
            }
            //            }
            //            catch (Exception ex)
            //            {
            //#if (!PORTABLE)
            //                Console.WriteLine(ex);
            //#endif
            //                AutoLogger.LogError(ex, "ReadJson");
            //            }

            ////return null;
            //if (reader.TokenType == JsonToken.Null)
            //    return null;

            //else if (reader.TokenType == JsonToken.StartArray)
            //{
            //    // No $ref.  Deserialize as a List<T> to avoid infinite recursion and return as an array.
            //    var elementType = objectType.GetElementType();
            //    var listType = typeof(List<>).MakeGenericType(elementType);
            //    var list = (IList)serializer.Deserialize(reader, listType);
            //    if (list == null)
            //        return null;
            //    var array = Array.CreateInstance(elementType, list.Count);
            //    list.CopyTo(array, 0);
            //    return array;
            //}
            //else if (reader.TokenType == JsonToken.StartObject)
            //{
            //    var instance = Activator.CreateInstance(objectType);
            //    serializer.Populate(reader, instance);
            //    return instance;
            //}
            //else
            //{
            //    //var instance = Activator.CreateInstance(objectType);


            //    return reader.Value;
            //    //var obj = JObject.Load(reader);
            //    //var refId = (string)obj[refProperty];
            //    //if (refId != null)
            //    //{
            //    //    var reference = serializer.ReferenceResolver.ResolveReference(serializer, refId);
            //    //    if (reference != null)
            //    //        return reference;
            //    //}
            //    //var values = obj[valuesProperty];
            //    //if (values == null || values.Type == JTokenType.Null)
            //    //    return null;
            //    //if (!(values is JArray))
            //    //{
            //    //    throw new JsonSerializationException(string.Format("{0} was not an array", values));
            //    //}
            //    //var count = ((JArray)values).Count;

            //    //var elementType = objectType.GetElementType();
            //    //var array = Array.CreateInstance(elementType, count);

            //    //var objId = (string)obj[idProperty];
            //    //if (objId != null)
            //    //{
            //    //    // Add the empty array into the reference table BEFORE poppulating it,
            //    //    // to handle recursive references.
            //    //    serializer.ReferenceResolver.AddReference(serializer, objId, array);
            //    //}

            //    //var listType = typeof(List<>).MakeGenericType(elementType);
            //    //using (var subReader = values.CreateReader())
            //    //{
            //    //    var list = (IList)serializer.Deserialize(subReader, listType);
            //    //    list.CopyTo(array, 0);
            //    //}

            //    //return array;
            //}
        }

        object CreateInstance(Type type)
        {
            if (type.IsArray)
                return Activator.CreateInstance(type, 0);
            else
            {
                if (type.GetIsGenericType() && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    var generic = type.GetListOfGenericArguments().FirstOrDefault();
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(generic));
                }
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        object ReadNewObject(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object instance = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    if (propertyName == refProperty)
                    {
                        reader.Read();
                        var value = reader.Value.ToString();
                        reader.Read();
                        return SerializedReferencedObjects[int.Parse(value)];
                    }
                    else if (propertyName == valuesProperty)
                    {
                        var value = ReadNewArray(instance, reader, objectType, reader.Value, serializer);
                        instance = value;
                        continue;
                    }
                    if (instance == null)
                        instance = CreateInstance(objectType);
                    ReadNewProperty(instance, reader, objectType, existingValue, serializer);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    var value = ReadNewArray(null, reader, objectType, reader.Value, serializer);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
                else
                {

                }

            }
            if (instance == null)
                instance = CreateInstance(objectType);
            return instance;
        }

        static void ResizeArray(ref System.Array array, int newSize)
        {
            Type elementType = array.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, newSize);
            Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
            array = newArray;

            //var method = typeof(System.Array).FindMethod("Resize");
            //System.Type elementType = oldArray.GetType().GetElementType();
            //var newMethod = method.MakeGenericMethod(elementType);
            //newMethod.Invoke(null, new object[] { oldArray, newSize });
            //int oldSize = oldArray.Length;
            //System.Array newArray = System.Array.CreateInstance(elementType, newSize);

            //int preserveLength = System.Math.Min(oldSize, newSize);

            //if (preserveLength > 0)
            //    System.Array.Copy(oldArray, newArray, preserveLength);

            //return newArray;
        }

        object ReadNewArray(object instance, JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (instance == null)
                instance = CreateInstance(objectType);
            var addMethod = instance.GetType().FindMethod("Add");
            Array array = null;
            Type elementType = null;
            if (objectType.IsArray)
            {
                array = (Array)instance;
                elementType = array.GetType().GetElementType();
            }
            else
            {
                if (addMethod != null)
                {
                    elementType = addMethod.GetParameters().FirstOrDefault().ParameterType;
                }
                else
                {

                }
            }
            //read value of property
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    ReadNewProperty(instance, reader, elementType, existingValue, serializer);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    if (array != null)
                    {
                        var obj = ReadNewObject(reader, elementType, existingValue, serializer);
                        ResizeArray(ref array, array.Length + 1);
                        array.SetValue(obj, array.Length - 1);
                    }
                    else
                    {
                        if (addMethod == null)
                        {

                        }
                        else
                        {
                            var obj = ReadNewObject(reader, elementType, existingValue, serializer);
                            addMethod.Invoke(instance, new object[] { obj });
                        }

                    }
                }
                else if (reader.TokenType == JsonToken.EndArray)
                    break;
                else
                {
                    if (reader.Value == null)
                        continue;
                    if (array != null)
                    {
                        var value = SerializeHelper.ConvertType(elementType, reader.Value);
                        ResizeArray(ref array, array.Length + 1);
                        array.SetValue(value, array.Length - 1);
                    }
                    else
                    {
                        if (addMethod == null)
                        {

                        }
                        else
                        {
                            var value = SerializeHelper.ConvertType(elementType, reader.Value);
                            addMethod.Invoke(instance, new object[] { value });
                        }
                    }
                    //
                    //if (array != null)


                }

            }
            if (array != null)
                return array;
            return instance;
        }

        void ReadNewProperty(object instance, JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //bool isDictionary= typeof(IDictionary).GetIsAssignableFrom(type);
            //value of property
            string propertyName = (string)reader.Value;

            if (reader.Read())
            {
                if (propertyName == idProperty)
                {
                    if (reader.Value == null)
                        return;
                    SerializedReferencedObjects.Add(int.Parse(reader.Value.ToString()), instance);
                }
                else if (propertyName == valuesProperty)
                {

                }
                else if (propertyName == refProperty)
                {

                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    var property = objectType.GetPropertyInfo(propertyName);
                    if (property != null)
                    {
                        var array = ReadNewArray(null, reader, property.PropertyType, existingValue, serializer);
                        property.SetValue(instance, array, null);

                    }
                    else
                    {
                        var field = objectType.GetFieldInfo(propertyName);
                        if (field != null)
                        {
                            var array = ReadNewArray(null, reader, field.FieldType, existingValue, serializer);
                            field.SetValue(instance, array);
                        }
                        else
                            AutoLogger.LogText($"property {propertyName} not found in {objectType.FullName}");
                    }

                }
                else
                {
                    var property = objectType.GetPropertyInfo(propertyName);
                    if (property != null)
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            var value = ReadNewObject(reader, property.PropertyType, reader.Value, serializer);
                            if (property.CanWrite)
                                property.SetValue(instance, value, null);
                            else
                            {
                                AutoLogger.LogText($"property {property.Name} cannot write");
                            }
                        }
                        else
                        {
                            try
                            {
                                var value = SerializeHelper.ConvertType(property.PropertyType, reader.Value);
                                if (property.CanWrite)
                                    property.SetValue(instance, value, null);
                                else
                                {
                                    AutoLogger.LogText($"property {property.Name} cannot write");
                                }
                            }
                            catch (Exception ex)
                            {
                                AutoLogger.LogError(ex, $"Deserialize Error {property.Name} :");
                            }
                        }
                    }
                    else
                    {
                        var field = objectType.GetFieldInfo(propertyName);
                        if (field != null)
                        {
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                var value = ReadNewObject(reader, field.FieldType, reader.Value, serializer);
                                field.SetValue(instance, value);
                            }
                            else
                            {
                                var value = SerializeHelper.ConvertType(field.FieldType, reader.Value);
                                field.SetValue(instance, value);
                            }
                        }
                    }
                }
            }
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
            //try
            //{
            if (!SerializedObjects.Contains(value))
                SerializedObjects.Add(value);
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
            {
                if (IsEnabledReferenceResolverForArray)
                {
                    writer.WriteStartObject();
                    WriteReferenceProperty(writer, type, value, idProperty);
                    WriteReferenceJustPropertyName(writer, type, value, valuesProperty);
                }
                writer.WriteStartArray();
            }
            else
            {
                writer.WriteStartObject();
                if (IsEnabledReferenceResolver)
                    WriteReferenceProperty(writer, type, value, idProperty);
            }


            if (isDictionary)
            {
                //writer.WriteEndObject();
                //if (!SerializedObjects.Contains(value))
                GenerateDictionary(value, writer, serializer);
            }
            else if (isArray)
            {
                //if (!SerializedObjects.Contains(value))
                GenerateArray(value, writer, serializer);
                //else

            }
            else
            {
                WriteData(type, value, writer, serializer);
            }

            //writer.WriteEnd();
            if (isArray && !isDictionary)
            {
                writer.WriteEndArray();
                if (IsEnabledReferenceResolverForArray)
                    writer.WriteEndObject();
            }
            else
                writer.WriteEndObject();
            //}
            //catch (Exception ex)
            //{
            //    AutoLogger.LogError(ex, "WriteJson");
            //}

        }

        private void WriteReferenceProperty(JsonWriter writer, Type type, object value, string flag)
        {
            string reference = GetReference(writer, value);

            writer.WritePropertyName(flag, false);
            writer.WriteValue(reference);
        }

        private void WriteReferenceJustPropertyName(JsonWriter writer, Type type, object value, string flag)
        {
            string reference = GetReference(writer, value);

            writer.WritePropertyName(flag, false);
        }

        private string GetReference(JsonWriter writer, object value)
        {
            string reference = ReferenceResolver.GetReference(this, value);

            return reference;
        }

        void GenerateDictionary(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<DictionaryEntry> items = new List<DictionaryEntry>();
            List<DictionaryEntry> existObjects = new List<DictionaryEntry>();
            foreach (DictionaryEntry item in (IDictionary)value)
            {
                if (item.Value == null)
                    continue;
                var itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.Value.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (existObjects.Contains(item))
                    {
                        if (IsEnabledReferenceResolverForArray)
                        {
                            writer.WriteStartObject();
                            WriteReferenceProperty(writer, item.Value.GetType(), item.Value, refProperty);
                            writer.WriteEndObject();
                        }

                    }
                    else
                        existObjects.Add(item);
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
                if (!SerializedObjects.Contains(item.Value))
                    SerializedObjects.Add(item.Value);
                else
                {
                    if (IsEnabledReferenceResolverForArray)
                    {
                        writer.WritePropertyName(item.Key.ToString());
                        writer.WriteStartObject();
                        WriteReferenceProperty(writer, item.Value.GetType(), item.Value, refProperty);
                        writer.WriteEndObject();
                    }
                    continue;
                }
                writer.WritePropertyName(item.Key.ToString());
                serializer.Serialize(writer, item.Value);
            }
        }

        void GenerateArray(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<object> objects = new List<object>();
            List<object> existObjects = new List<object>();
            foreach (var item in (IEnumerable)value)
            {
                if (item == null)
                    continue;
                var itemType = item.GetType();
                var itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (existObjects.Contains(item))
                    {
                        if (SerializedObjects.Contains(item) && IsEnabledReferenceResolverForArray)
                        {
                            writer.WriteStartObject();
                            WriteReferenceProperty(writer, item.GetType(), item, refProperty);
                            writer.WriteEndObject();
                        }
                    }
                    else
                        existObjects.Add(item);
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
                if (!SerializedObjects.Contains(item))
                    SerializedObjects.Add(item);
                else
                {
                    if (IsEnabledReferenceResolverForArray)
                    {
                        if (writer.WriteState != WriteState.Object)
                            writer.WriteStartObject();
                        WriteReferenceProperty(writer, item.GetType(), item, refProperty);
                        writer.WriteEndObject();
                    }
                    continue;
                }
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
                                        {
                                            if (IsEnabledReferenceResolverForArray)
                                            {
                                                writer.WritePropertyName(property.Name);
                                                writer.WriteStartObject();
                                                WriteReferenceProperty(writer, propValue.GetType(), propValue, refProperty);
                                                writer.WriteEndObject();
                                            }
                                            return;
                                        }
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
                                        {
                                            if (IsEnabledReferenceResolver)
                                            {
                                                writer.WritePropertyName(property.Name);
                                                writer.WriteStartObject();
                                                WriteReferenceProperty(writer, value.GetType(), value, refProperty);
                                                writer.WriteEndObject();
                                            }
                                            return;
                                        }
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
                                        {
                                            if (IsEnabledReferenceResolver)
                                            {
                                                writer.WritePropertyName(property.Name);
                                                writer.WriteStartObject();
                                                WriteReferenceProperty(writer, value.GetType(), value, refProperty);
                                                writer.WriteEndObject();
                                            }
                                            return;
                                        }
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
