using Newtonsoft.Json;
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
using System.Threading.Tasks;

namespace SignalGo.Shared.Converters
{
    public class DataExchanger
    {
        static DataExchanger()
        {
            CreateInstance = Activator.CreateInstance;
            CreateInstanceParams = Activator.CreateInstance;
        }
        public static Func<Type, object> CreateInstance { get; set; }
        public static Func<Type, object[], object> CreateInstanceParams { get; set; }
        /// <summary>
        /// ListOfContextsDataExchangers
        /// </summary>
        public static Dictionary<int, List<CustomDataExchangerAttribute>> ListOfContextsDataExchangers { get; set; } = new Dictionary<int, List<CustomDataExchangerAttribute>>();

        public static void TakeOnly(object instance)
        {
            string[] value = null;
            TakeOnly(instance, value);
        }

        public static void Ignore(object instance)
        {
            string[] value = null;
            Ignore(instance, value);
        }

        public static void TakeOnly(Type type)
        {
            string[] value = null;
            Take(type, value);
        }

        public static void Ignore(Type type)
        {
            string[] value = null;
            Ignore(type, value);
        }

        public static void TakeOnly(object instance, params string[] property)
        {
            if (Task.CurrentId == null)
                throw new Exception("current context is null please don't call this method inside of another thread!");
            if (!ListOfContextsDataExchangers.ContainsKey(Task.CurrentId.GetValueOrDefault()))
                ListOfContextsDataExchangers.Add(Task.CurrentId.GetValueOrDefault(), new List<CustomDataExchangerAttribute>());
            CustomDataExchangerAttribute result = new CustomDataExchangerAttribute
            {
                ExchangeType = CustomDataExchangerType.TakeOnly,
                Properties = property,
                Instance = instance,
                LimitationMode = LimitExchangeType.Both
            };
            ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].Add(result);
        }

        /// <summary>
        /// ignore custom properties of object from current context
        /// </summary>
        /// <param name="instance">instance of object you want ingore properties</param>
        /// <param name="property">properties to ignore</param>
        public static void Ignore(object instance, params string[] property)
        {
            Ignore(Task.CurrentId, instance, property);
        }
        public static void Ignore(int? taskId, object instance, params string[] property)
        {
            if (taskId == null)
                throw new Exception("current context is null please don't call this method inside of another thread!");
            if (!ListOfContextsDataExchangers.ContainsKey(taskId.GetValueOrDefault()))
                ListOfContextsDataExchangers.Add(taskId.GetValueOrDefault(), new List<CustomDataExchangerAttribute>());
            CustomDataExchangerAttribute result = new CustomDataExchangerAttribute
            {
                ExchangeType = CustomDataExchangerType.Ignore,
                Properties = property,
                Instance = instance,
                LimitationMode = LimitExchangeType.Both
            };
            ListOfContextsDataExchangers[taskId.GetValueOrDefault()].Add(result);
        }

        public static void Take(Type type, params string[] property)
        {
            if (Task.CurrentId == null)
                throw new Exception("current context is null please don't call this method inside of another thread!");
            if (!ListOfContextsDataExchangers.ContainsKey(Task.CurrentId.GetValueOrDefault()))
                ListOfContextsDataExchangers.Add(Task.CurrentId.GetValueOrDefault(), new List<CustomDataExchangerAttribute>());
            CustomDataExchangerAttribute result = new CustomDataExchangerAttribute
            {
                ExchangeType = CustomDataExchangerType.TakeOnly,
                Properties = property,
                Type = type,
                LimitationMode = LimitExchangeType.Both
            };
            ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].Add(result);

        }

        public static void Ignore(Type type, params string[] property)
        {
            if (Task.CurrentId == null)
                throw new Exception("current context is null please don't call this method inside of another thread!");
            if (!ListOfContextsDataExchangers.ContainsKey(Task.CurrentId.GetValueOrDefault()))
                ListOfContextsDataExchangers.Add(Task.CurrentId.GetValueOrDefault(), new List<CustomDataExchangerAttribute>());
            CustomDataExchangerAttribute result = new CustomDataExchangerAttribute
            {
                ExchangeType = CustomDataExchangerType.Ignore,
                Properties = property,
                Type = type,
                LimitationMode = LimitExchangeType.Both
            };
            ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].Add(result);
        }

        public static void Clear(int? id)
        {
            if (id != null && ListOfContextsDataExchangers.ContainsKey(id.GetValueOrDefault()))
            {
                ListOfContextsDataExchangers.Remove(id.GetValueOrDefault());
            }
        }

        public static void Clear()
        {
            Clear(Task.CurrentId);
        }

        public static bool ExistContext()
        {
            if (Task.CurrentId != null && ListOfContextsDataExchangers.ContainsKey(Task.CurrentId.GetValueOrDefault()) && ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].Count > 0)
            {
                return true;
            }
            return false;
        }

        public static List<CustomDataExchangerAttribute> GetContextAttributes()
        {
            if (Task.CurrentId != null && ListOfContextsDataExchangers.ContainsKey(Task.CurrentId.GetValueOrDefault()) && ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].Count > 0)
            {
                return ListOfContextsDataExchangers[Task.CurrentId.GetValueOrDefault()].ToList();
            }
            return null;
        }

    }

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
        private BidirectionalDictionary<string, object> mappings = new BidirectionalDictionary<string, object>();
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
        public static AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "DataExchanger Logs.log" };

        private Type BaseType { get; set; }

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
                        JToken json = JToken.Load(reader);
                        object instance = null;
                        if (serializehandling.ParameterType == typeof(JToken))
                            instance = json;
                        else
                            instance = json.ToObject(serializehandling.ParameterType);
                        value = serializehandling.Delegate.DynamicInvoke(instance);
                    }
                    catch (Exception ex)
                    {
                        AutoLogger.LogError(ex, "CustomICollectionCreationConverter ReadJson");
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
            Type generic = objectType.GetListOfGenericArguments().FirstOrDefault();
            return DataExchanger.CreateInstance(typeof(List<>).MakeGenericType(generic));
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
            //objectType.GetIsGenericType() && objectType.GetGenericTypeDefinition() == BaseType ||
            if (SerializeHelper.HandleDeserializingObjectList.ContainsKey(objectType))
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
        public int? CurrentTaskId { get; set; }
        /// <summary>
        /// log system for data exchanger
        /// </summary>
        public AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "DataExchanger Logs.log" };
        /// <summary>
        /// enable refrence serializing when duplicate object detected
        /// </summary>
        public bool IsEnabledReferenceResolver { get; set; } = true;
        /// <summary>
        /// enable refrence serializing when duplicate list of objects detected
        /// </summary>
        public bool IsEnabledReferenceResolverForArray { get; set; } = true;
        /// <summary>
        /// validation manager of data exchanger
        /// </summary>
        public ValidationRuleInfoManager ValidationRuleInfoManager { get; set; }
        /// <summary>
        /// reference resolver for $id and $ref and $values
        /// </summary>
        private DefaultReferenceResolver ReferenceResolver { get; set; } = new DefaultReferenceResolver();
        /// <summary>
        /// server of signalGo that called exchanger
        /// </summary>
        public object Server { get; set; }
        /// <summary>
        /// client of signalGo that called exchanger
        /// </summary>
        public object Client { get; set; }

        internal bool IsClient
        {
            get
            {
                return Server == null;
            }
        }
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

        private List<object> SerializedObjects = new List<object>();
        private Dictionary<int, object> SerializedReferencedObjects = new Dictionary<int, object>();

        private bool CanIgnoreCustomDataExchanger(Type type, object instance)
        {
            if (type == null)
                return false;
            if (DataExchanger.ExistContext())
            {
                List<CustomDataExchangerAttribute> items = DataExchanger.GetContextAttributes();
                if (instance != null)
                {
                    if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                        return true;
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                        return false;
                }

                if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                    return true;
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                    return false;
            }
            IEnumerable<CustomDataExchangerAttribute> implementICollection = null;
            implementICollection = ExchangerTypes?.Where(x => x.Type == type && (x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both));
            if (implementICollection == null || implementICollection.Count() == 0)
                implementICollection = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both);

            bool canIgnore = implementICollection == null ? false : implementICollection.Any(x => x.CanIgnore(instance, null, null, type, Server, Client) ?? false);
            if (canIgnore)
                return true;
            else if (implementICollection.Any(x => x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null && (x.GetLimitationMode(IsClient) == LimitExchangeType.Both || x.GetLimitationMode(IsClient) == Mode)))
                return true;
            return false;
        }

        private CustomDataExchangerAttribute GetCustomDataExchanger(Type type, object instance)
        {
            if (DataExchanger.ExistContext())
            {
                List<CustomDataExchangerAttribute> items = DataExchanger.GetContextAttributes();
                if (instance != null)
                {
                    if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore))
                        return items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore);
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly))
                        return items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly);
                }

                if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore))
                    return items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore);
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly))
                    return items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly);
            }
            CustomDataExchangerAttribute implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both)).FirstOrDefault();
            if (implementICollection == null)
                implementICollection = type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both).FirstOrDefault();

            return implementICollection;
        }

        private bool CanIgnoreCustomDataExchanger(Type type, PropertyInfo property, object instance)
        {
            if (DataExchanger.ExistContext())
            {
                List<CustomDataExchangerAttribute> items = DataExchanger.GetContextAttributes();
                if (instance != null)
                {
                    if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                        return true;
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                        return false;
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null))
                    {
                        CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null);
                        if (find.Properties.Contains(property.Name))
                            return true;
                        else
                            return false;
                    }
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null))
                    {
                        CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null);
                        if (find.Properties.Contains(property.Name))
                            return false;
                        else
                            return true;
                    }
                }

                if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                    return true;
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                    return false;
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null))
                {
                    CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null);
                    if (find.Properties.Contains(property.Name))
                        return true;
                    else
                        return false;
                }
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null))
                {
                    CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null);
                    if (find.Properties.Contains(property.Name))
                        return false;
                    else
                        return true;
                }
            }
            IEnumerable<CustomDataExchangerAttribute> implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both));
            if (implementICollection == null || implementICollection.Count() == 0)
                implementICollection = property.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both);
            //if (type != null)
            //{
            //    List<CustomDataExchangerAttribute> newItems = implementICollection.ToList();
            //    newItems.AddRange(type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both));
            //    implementICollection = newItems;
            //}

            bool canIgnore = implementICollection == null ? false : implementICollection.Any(x => x.CanIgnore(instance, property, null, type, Server, Client) ?? false);
            if (canIgnore)
                return true;
            else if (implementICollection != null)
            {
                if (implementICollection.Any(x => x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null && x.Properties.Contains(property.Name) && (x.GetLimitationMode(IsClient) == LimitExchangeType.Both || x.GetLimitationMode(IsClient) == Mode)))
                    return true;
                else if (implementICollection.Any(x => x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null && (x.GetLimitationMode(IsClient) == LimitExchangeType.Both || x.GetLimitationMode(IsClient) == Mode)))
                    return true;
                else if (implementICollection.Any(x => x.Properties != null && x.ExchangeType == CustomDataExchangerType.TakeOnly && !x.Properties.Contains(property.Name) && (x.GetLimitationMode(IsClient) == LimitExchangeType.Both || x.GetLimitationMode(IsClient) == Mode)))
                    return true;
            }

            return false;
        }

        private bool CanIgnoreCustomDataExchanger(Type type, FieldInfo fieldInfo, object instance)
        {
            if (DataExchanger.ExistContext())
            {
                List<CustomDataExchangerAttribute> items = DataExchanger.GetContextAttributes();
                if (instance != null)
                {
                    if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                        return true;
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                        return false;
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null))
                    {
                        CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null);
                        if (find.Properties.Contains(fieldInfo.Name))
                            return true;
                        else
                            return false;
                    }
                    else if (items.Any(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null))
                    {
                        CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Instance == instance && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null);
                        if (find.Properties.Contains(fieldInfo.Name))
                            return false;
                        else
                            return true;
                    }
                }

                if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties == null))
                    return true;
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties == null))
                    return false;
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null))
                {
                    CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.Ignore && x.Properties != null);
                    if (find.Properties.Contains(fieldInfo.Name))
                        return true;
                    else
                        return false;
                }
                else if (items.Any(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null))
                {
                    CustomDataExchangerAttribute find = items.FirstOrDefault(x => x.Type == type && x.ExchangeType == CustomDataExchangerType.TakeOnly && x.Properties != null);
                    if (find.Properties.Contains(fieldInfo.Name))
                        return false;
                    else
                        return true;
                }
            }

            IEnumerable<CustomDataExchangerAttribute> implementICollection = null;
            implementICollection = ExchangerTypes == null ? null : ExchangerTypes.Where(x => x.Type == type && (x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both));
            if (implementICollection == null)
                implementICollection = fieldInfo.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both);
            if (type != null)
            {
                List<CustomDataExchangerAttribute> newItems = implementICollection.ToList();
                newItems.AddRange(type.GetCustomAttributes<CustomDataExchangerAttribute>(true).Where(x => x.GetLimitationMode(IsClient) == Mode || x.GetLimitationMode(IsClient) == LimitExchangeType.Both));
                implementICollection = newItems;
            }

            bool canIgnore = implementICollection == null ? false : implementICollection.Any(x => x.CanIgnore(instance, null, fieldInfo, type, Server, Client) ?? false);
            if (canIgnore)
                return true;
            else if (implementICollection != null && implementICollection.Any(x => x.ExchangeType == CustomDataExchangerType.Ignore && (x.GetLimitationMode(IsClient) == LimitExchangeType.Both || x.GetLimitationMode(IsClient) == Mode)))
                return true;
            return false;
        }

        internal static string refProperty = "$ref";
        internal static string idProperty = "$id";
        internal static string valuesProperty = "$values";

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
            reader.FloatParseHandling = FloatParseHandling.Double;
            if (CanIgnoreCustomDataExchanger(objectType, existingValue))
            {
                while (reader.Read())
                {

                }
                return null;
            }
            if (reader.TokenType == JsonToken.StartObject)
            {
                object instance = ReadNewObject(reader, objectType, existingValue, serializer, false);
                while (reader.Read())
                {

                }
                return instance;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                object instance = ReadNewArray(null, reader, objectType, existingValue, serializer, false);
                while (reader.Read())
                {

                }
                return instance;
            }
            else
            {
                return SerializeHelper.ConvertType(objectType, reader.Value);
            }
        }

        public static object GetDefault(Type t)
        {
            try
            {
                Type defaultGeneratorType =
      typeof(DefaultGenerator<>).MakeGenericType(t);
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
                MethodInfo method = defaultGeneratorType.GetTypeInfo().GetDeclaredMethod("GetDefault");
                return method.Invoke(null, null);
#else
                return defaultGeneratorType.InvokeMember(
                     "GetDefault",
                     BindingFlags.Static |
                     BindingFlags.Public |
                     BindingFlags.InvokeMethod,
                     null, null, new object[0]);
#endif
            }
            catch
            {
                return null;
            }
        }

        private object CreateInstance(Type type, bool isIgnore)
        {
            bool canIgnore = isIgnore ? true : CanIgnoreCustomDataExchanger(type, null);
            if (canIgnore)
                return null;
            if (type.IsArray)
                return DataExchanger.CreateInstanceParams(type, new object[] { 0 });
            else
            {
                if (type.GetIsGenericType() && (type.GetGenericTypeDefinition() == typeof(ICollection<>) || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    Type generic = type.GetListOfGenericArguments().FirstOrDefault();
                    return DataExchanger.CreateInstance(typeof(List<>).MakeGenericType(generic));
                }
                try
                {
                    ConstructorInfo[] ctors = type.GetListOfConstructors();
                    if (ctors.Any(x => x.GetParameters().Length == 0) || ctors.Length == 0)
                        return DataExchanger.CreateInstance(type);
                    else
                    {
                        System.Reflection.ParameterInfo[] parameters = ctors[0].GetParameters();
                        object[] args = new object[parameters.Length];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            args[i] = GetDefault(parameters[i].ParameterType);
                        }
                        return DataExchanger.CreateInstanceParams(type, args);
                    }
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "DataExchangeConverter CreateInstance");
                    return null;
                }
            }
        }

        private object ReadNewObject(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, bool isIgnore)
        {
            bool canIgnore = isIgnore ? true : CanIgnoreCustomDataExchanger(objectType, existingValue);
            object instance = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    if (propertyName == refProperty)
                    {
                        reader.Read();
                        string value = reader.Value.ToString();
                        reader.Read();
                        int parseValue = int.Parse(value);
                        if (!SerializedReferencedObjects.ContainsKey(parseValue))
                            return null;
                        return SerializedReferencedObjects[parseValue];
                    }
                    else if (propertyName == valuesProperty)
                    {
                        object value = ReadNewArray(instance, reader, objectType, reader.Value, serializer, false);
                        instance = value;
                        continue;
                    }
                    if (instance == null)
                        instance = CreateInstance(objectType, false);
                    ValidationRuleInfoManager?.AddObjectPropertyAsChecked(CurrentTaskId, objectType, instance, null, null, null);
                    ReadNewProperty(instance, reader, objectType, existingValue, serializer, false);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    object value = ReadNewArray(null, reader, objectType, reader.Value, serializer, false);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
                else
                {

                }

            }
            if (canIgnore)
                return null;
            if (instance == null)
            {
                instance = CreateInstance(objectType, isIgnore);
                if (instance == null)
                    Console.WriteLine($"instance is null of type{objectType}");
                foreach (var item in instance.GetType().GetListOfProperties())
                {
                    AddPropertyValidationRuleInfoAttribute(item, instance, null);
                }
            }
            return instance;
        }

        private static void ResizeArray(ref System.Array array, int newSize)
        {
            Type elementType = array.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, newSize);
            Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
            array = newArray;
        }

        private object ReadNewArray(object instance, JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, bool isIgnore)
        {
            bool canIgnore = isIgnore ? true : CanIgnoreCustomDataExchanger(objectType, instance);

            if (instance == null)
                instance = CreateInstance(objectType, false);
            MethodInfo addMethod = instance.GetType().FindMethod("Add");
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
            canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(elementType, instance);

            //read value of property
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    ReadNewProperty(instance, reader, elementType, existingValue, serializer, canIgnore);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    if (array != null)
                    {
                        object obj = ReadNewObject(reader, elementType, existingValue, serializer, canIgnore);
                        if (obj != null && !canIgnore)
                        {
                            ResizeArray(ref array, array.Length + 1);
                            array.SetValue(obj, array.Length - 1);
                        }
                    }
                    else
                    {
                        if (addMethod == null)
                        {

                        }
                        else
                        {
                            object obj = ReadNewObject(reader, elementType, existingValue, serializer, canIgnore);
                            if (!canIgnore)
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
                    canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(elementType, instance);
                    if (!canIgnore)
                    {
                        if (array != null)
                        {
                            object value = SerializeHelper.ConvertType(elementType, reader.Value);
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
                                object value = SerializeHelper.ConvertType(elementType, reader.Value);
                                addMethod.Invoke(instance, new object[] { value });
                            }
                        }
                    }
                }

            }
            if (array != null)
                return array;
            return instance;
        }

        private void ReadFreeObject(JsonReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    ReadFreeProperty(reader);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    ReadFreeArray(reader);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    ReadFreeObject(reader);
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }
        }

        private void ReadFreeProperty(JsonReader reader)
        {
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    ReadFreeArray(reader);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    ReadFreeObject(reader);
                }
                else if (reader.TokenType == JsonToken.PropertyName)
                {
                    ReadFreeProperty(reader);
                }
            }
        }

        private void ReadFreeArray(JsonReader reader)
        {
            //read value of property
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    ReadFreeProperty(reader);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    ReadFreeObject(reader);
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    ReadFreeArray(reader);
                }
                else if (reader.TokenType == JsonToken.EndArray)
                    break;
            }
        }

        public void ReadFreeAny(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                ReadFreeProperty(reader);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                ReadFreeObject(reader);
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                ReadFreeArray(reader);
            }
        }

        public static bool IsTupleType(Type type, bool checkBaseTypes = false)
        {
#if (NET35)
            return false;
#else
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(Tuple))
                return true;

            while (type != null)
            {
                if (type.GetIsGenericType())
                {
                    Type genType = type.GetGenericTypeDefinition();
                    if (genType == typeof(Tuple<>)
                        || genType == typeof(Tuple<,>)
                        || genType == typeof(Tuple<,,>)
                        || genType == typeof(Tuple<,,,>)
                        || genType == typeof(Tuple<,,,,>)
                        || genType == typeof(Tuple<,,,,,>)
                        || genType == typeof(Tuple<,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>))
                        return true;
                }

                if (!checkBaseTypes)
                    break;

                type = type.GetBaseType();
            }

            return false;
#endif
        }
        /// <summary>
        /// add property validation rules to check system
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="instance"></param>
        /// <param name="currentValue"></param>
        private void AddPropertyValidationRuleInfoAttribute(PropertyInfo propertyInfo, object instance, object currentValue)
        {
            if (!CurrentTaskId.HasValue || ValidationRuleInfoManager == null || instance == null || propertyInfo == null)
                return;
            ValidationRuleInfoManager.AddObjectPropertyAsChecked(CurrentTaskId, instance.GetType(), instance, propertyInfo.Name, propertyInfo, currentValue);
            foreach (BaseValidationRuleInfoAttribute item in propertyInfo.GetCustomAttributes(typeof(BaseValidationRuleInfoAttribute), true))
            {
                item.PropertyInfo = propertyInfo;
                item.Object = instance;
                item.CurrentValue = currentValue;
                ValidationRuleInfoManager.AddRule(CurrentTaskId, item);
            }
        }


        private void ReadNewProperty(object instance, JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, bool isIgnore)
        {
            //bool isDictionary= typeof(IDictionary).GetIsAssignableFrom(type);
            //value of property
            string propertyName = (string)reader.Value;
            bool canIgnore = isIgnore ? true : CanIgnoreCustomDataExchanger(objectType, instance);
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
                    PropertyInfo property = FindProperty(objectType, propertyName);
                    if (property != null)
                    {
                        canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(objectType, property, instance);
                        object array = ReadNewArray(null, reader, property.PropertyType, existingValue, serializer, canIgnore);

                        if (!canIgnore)
                        {
                            if (property.CanWrite)
                                property.SetValue(instance, array, null);
                            else
                                Console.WriteLine($"property {property.Name} cannot write");
                        }
                            
                        AddPropertyValidationRuleInfoAttribute(property, instance, array);
                    }
                    else
                    {
                        FieldInfo field = FindField(objectType, propertyName);
                        if (field != null)
                        {
                            canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(objectType, field, instance);
                            object array = ReadNewArray(null, reader, field.FieldType, existingValue, serializer, canIgnore);
                            if (!canIgnore)
                                field.SetValue(instance, array);
                        }
                        else
                        {
                            ReadFreeArray(reader);
                            AutoLogger.LogText($"json property {propertyName} not found in {objectType.FullName}");
                        }
                    }
                }
                else
                {
                    if (IsTupleType(objectType))
                    {
                        FieldInfo field = objectType.GetFieldInfo(propertyName);
                        if (field != null)
                        {
                            canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(objectType, field, instance);
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                object value = ReadNewObject(reader, field.FieldType, reader.Value, serializer, canIgnore);
                                if (!canIgnore)
                                    field.SetValue(instance, value);
                            }
                            else if (!canIgnore)
                            {
                                object value = SerializeHelper.ConvertType(field.FieldType, reader.Value);
                                field.SetValue(instance, value);
                            }
                        }
                        else
                        {
                            ReadFreeAny(reader);
                        }
                    }
                    else
                    {
                        PropertyInfo property = FindProperty(objectType, propertyName);
                        if (property != null)
                        {
                            canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(objectType, property, instance);
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                object value = ReadNewObject(reader, property.PropertyType, reader.Value, serializer, canIgnore);
                                if (!canIgnore)
                                {
                                    if (property.CanWrite)
                                    {
                                        property.SetValue(instance, value, null);
                                    }
                                    else
                                        AutoLogger?.LogText($"property {property.Name} cannot write");
                                }
                                AddPropertyValidationRuleInfoAttribute(property, instance, value);
                            }
                            else
                            {
                                try
                                {
                                    if (!canIgnore)
                                    {
                                        object value = SerializeHelper.ConvertType(property.PropertyType, reader.Value);
                                        if (property.CanWrite)
                                        {
                                            property.SetValue(instance, value, null);
                                        }
                                        else
                                            AutoLogger?.LogText($"property {property.Name} cannot write");
                                        AddPropertyValidationRuleInfoAttribute(property, instance, value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AutoLogger?.LogError(ex, $"Deserialize Error {property.Name} :");
                                }
                            }
                        }
                        else
                        {
                            FieldInfo field = FindField(objectType, propertyName);
                            if (field != null)
                            {
                                canIgnore = canIgnore ? true : CanIgnoreCustomDataExchanger(objectType, field, instance);
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    object value = ReadNewObject(reader, field.FieldType, reader.Value, serializer, canIgnore);
                                    if (!canIgnore)
                                        field.SetValue(instance, value);
                                }
                                else if (!canIgnore)
                                {
                                    object value = SerializeHelper.ConvertType(field.FieldType, reader.Value);
                                    field.SetValue(instance, value);
                                }
                            }
                            else
                                ReadFreeAny(reader);
                        }
                    }
                }
            }
        }

        private static PropertyInfo FindProperty(Type objectType, string propertyName)
        {
            PropertyInfo property = objectType.GetPropertyInfo(propertyName);
            if (property == null)
            {
                var find = objectType.GetProperties().Select(x => new { Property = x, Attributes = x.GetCustomAttributes<JsonPropertyAttribute>() }).Where(x => x.Attributes.Length > 0)
                    .FirstOrDefault(x => x.Attributes.Any(y => y.PropertyName == propertyName));
                if (find != null)
                    property = find.Property;
            }
            return property;
        }

        private static FieldInfo FindField(Type objectType, string fieldName)
        {
            FieldInfo field = objectType.GetFieldInfo(fieldName);
            if (field == null)
            {
                var find = objectType.GetListOfFields().Select(x => new { Field = x, Attributes = x.GetCustomAttributes<JsonPropertyAttribute>() }).Where(x => x.Attributes.Length > 0)
                    .FirstOrDefault(x => x.Attributes.Any(y => y.PropertyName == fieldName));
                if (find != null)
                    field = find.Field;
            }
            return field;
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
        private void GenerateProperties(object instance)
        {
            if (instance == null)
                return;
            Type type = instance.GetType();
            //MergeExchangeTypes(type, Mode);
            if (SerializeHelper.GetTypeCodeOfObject(instance.GetType()) != SerializeObjectType.Object)
            {
                return;
            }
            foreach (PropertyInfo property in type.GetListOfProperties())
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
                    bool isIgnored = CanIgnoreCustomDataExchanger(type, property, instance);

                    //if (!isIgnored)
                    //{
                    //    if (ExchangerTypes != null)
                    //    {
                    //        var find = ExchangerTypes.FirstOrDefault(x => x.Type == type && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == Mode));
                    //        if (find != null && find.Properties != null)
                    //        {
                    //            var manualCanIngnore = find.CanIgnore(instance, property, null, type, Client, Server);
                    //            if (find.ExchangeType == CustomDataExchangerType.Take)
                    //            {
                    //                if (find.Properties != null && !find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                    //                    isIgnored = true;
                    //            }
                    //            else
                    //            {
                    //                if (find.Properties != null && find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                    //                    isIgnored = true;
                    //            }
                    //        }
                    //    }
                    //}
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
                            object value = property.GetValue(instance, null);
                            if (value != null)
                                foreach (DictionaryEntry item in (IDictionary)value)
                                {
                                    GenerateProperties(item.Key);
                                    GenerateProperties(item.Value);
                                }
                        }
                        else if (isPropertyArray)
                        {
                            object value = property.GetValue(instance, null);
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

        private bool HasJsonIgnore(Type type)
        {
            return type != null && type.GetCustomAttributes<JsonIgnoreAttribute>(true).Any();
        }

        private bool HasJsonIgnore(PropertyInfo property)
        {
            return property != null && AttributeHelper.GetCustomAttributes<JsonIgnoreAttribute>(property, true).Any();
        }

        private bool HasJsonIgnore(FieldInfo field)
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
            Type type = value.GetType();
            if (HasJsonIgnore(type) || type.FullName.StartsWith("System.Reflection.") || typeof(Delegate).IsAssignableFrom(type))
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

#if (NETSTANDARD || NETCOREAPP || PORTABLE)
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
            if (CanIgnoreCustomDataExchanger(type, value))
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

        private void GenerateDictionary(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<DictionaryEntry> items = new List<DictionaryEntry>();
            List<DictionaryEntry> existObjects = new List<DictionaryEntry>();
            foreach (DictionaryEntry item in (IDictionary)value)
            {
                if (item.Value == null)
                    continue;
                SerializeObjectType itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.Value.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (existObjects.Contains(item))
                    {
                        //if (IsEnabledReferenceResolverForArray)
                        //{
                        writer.WriteStartObject();
                        WriteReferenceProperty(writer, item.Value.GetType(), item.Value, refProperty);
                        writer.WriteEndObject();
                        //}

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
            foreach (DictionaryEntry item in items)
            {
                if (!SerializedObjects.Contains(item.Value))
                    SerializedObjects.Add(item.Value);
                else
                {
                    //if (IsEnabledReferenceResolverForArray)
                    //{
                    writer.WritePropertyName(item.Key.ToString());
                    writer.WriteStartObject();
                    WriteReferenceProperty(writer, item.Value.GetType(), item.Value, refProperty);
                    writer.WriteEndObject();
                    //}
                    continue;
                }
                writer.WritePropertyName(item.Key.ToString());
                serializer.Serialize(writer, item.Value);
            }
        }

        private void GenerateArray(object value, JsonWriter writer, JsonSerializer serializer)
        {
            List<object> objects = new List<object>();
            List<object> existObjects = new List<object>();
            foreach (object item in (IEnumerable)value)
            {
                if (item == null)
                    continue;
                Type itemType = item.GetType();
                SerializeObjectType itemJsonType = SerializeHelper.GetTypeCodeOfObject(item.GetType());
                if (itemJsonType == SerializeObjectType.Object)
                {
                    if (existObjects.Contains(item))
                    {
                        if (SerializedObjects.Contains(item)) //&& IsEnabledReferenceResolverForArray)
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
#if (NETSTANDARD || NETCOREAPP)
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
            foreach (object item in objects)
            {
                if (!SerializedObjects.Contains(item))
                    SerializedObjects.Add(item);
                else
                {
                    //if (IsEnabledReferenceResolverForArray)
                    //{
                    if (writer.WriteState != WriteState.Object)
                        writer.WriteStartObject();
                    WriteReferenceProperty(writer, item.GetType(), item, refProperty);
                    writer.WriteEndObject();
                    //}
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
        private void WriteData(Type baseType, object instance, JsonWriter writer, JsonSerializer serializer)
        {
            try
            {
                CustomDataExchangerAttribute implementICollection = GetCustomDataExchanger(baseType, instance);
                if (CanIgnoreCustomDataExchanger(baseType, instance) || typeof(Delegate).IsAssignableFrom(baseType))
                    return;

                foreach (PropertyInfo property in baseType.GetListOfProperties())
                {
                    if (implementICollection != null)
                    {
                        if (implementICollection.ExchangeType == CustomDataExchangerType.Ignore && implementICollection.ContainsProperty(property.Name))
                            continue;
                        else if (implementICollection.ExchangeType == CustomDataExchangerType.TakeOnly && !implementICollection.ContainsProperty(property.Name))
                            continue;
                    }

                    GenerateValue(property, null);
                }
                foreach (FieldInfo field in baseType.GetListOfFields())
                {
                    if (implementICollection != null)
                    {
                        if (implementICollection.ExchangeType == CustomDataExchangerType.Ignore && implementICollection.ContainsProperty(field.Name))
                            continue;
                        else if (implementICollection.ExchangeType == CustomDataExchangerType.TakeOnly && !implementICollection.ContainsProperty(field.Name))
                            continue;
                    }
                    GenerateValue(null, field);
                }
            }
            catch (Exception ex)
            {
                AutoLogger?.LogError(ex, "WriteData 4");
            }

            void GenerateValue(PropertyInfo property, FieldInfo field)
            {
                if ((property != null && property.CanRead) || field != null)
                {
                    if (HasJsonIgnore(property) || HasJsonIgnore(field))
                        return;
                    bool isIgnored = false;
                    if (property != null)
                        isIgnored = CanIgnoreCustomDataExchanger(baseType, property, instance);
                    if (field != null)
                        isIgnored = CanIgnoreCustomDataExchanger(baseType, field, instance);
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
                    //if (!isIgnored)
                    //{
                    //    if (ExchangerTypes != null)
                    //    {
                    //        var find = ExchangerTypes.FirstOrDefault(x => x.Type == baseType && (x.LimitationMode == LimitExchangeType.Both || x.LimitationMode == Mode));
                    //        if (find != null && find.Properties != null)
                    //        {
                    //            var manualCanIngnore = find.CanIgnore(instance, property, field, baseType, Client, Server);
                    //            if (find.ExchangeType == CustomDataExchangerType.Take)
                    //            {
                    //                if (property != null)
                    //                {
                    //                    if (find.Properties != null && !find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                    //                        isIgnored = true;
                    //                }

                    //                else if (field != null)
                    //                {
                    //                    if (find.Properties != null && !find.Properties.Contains(field.Name) && (manualCanIngnore ?? false))
                    //                        isIgnored = true;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (property != null)
                    //                {
                    //                    if (find.Properties != null && find.Properties.Contains(property.Name) && (manualCanIngnore ?? false))
                    //                        isIgnored = true;
                    //                }
                    //                else if (field != null)
                    //                {
                    //                    if (find.Properties != null && find.Properties.Contains(field.Name) && (manualCanIngnore ?? false))
                    //                        isIgnored = true;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
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
                                AutoLogger?.LogError(ex, "WriteData 1");
                            }
                            if (propValue != null)
                            {
                                try
                                {
                                    SerializeObjectType itemJsonType = SerializeHelper.GetTypeCodeOfObject(propValue.GetType());
                                    if (itemJsonType == SerializeObjectType.Object)
                                    {
                                        if (SerializedObjects.Contains(propValue))
                                        {
                                            //if (IsEnabledReferenceResolverForArray)
                                            //{
                                            writer.WritePropertyName(property.Name);
                                            writer.WriteStartObject();
                                            WriteReferenceProperty(writer, propValue.GetType(), propValue, refProperty);
                                            writer.WriteEndObject();
                                            //}
                                            return;
                                        }
                                        else
                                            SerializedObjects.Add(propValue);
                                    }
                                    if (property != null)
                                        writer.WritePropertyName(property.Name);
                                    else if (field != null)
                                        writer.WritePropertyName(field.Name);
                                    if (property != null)
                                    {
                                        SerializeHelper.HandleSerializingObjectList.TryGetValue(property.PropertyType, out Delegate serializeHandler);
                                        if (serializeHandler != null)
                                            propValue = serializeHandler.DynamicInvoke(propValue);
                                    }
                                    serializer.Serialize(writer, propValue);
                                }
                                catch (Exception ex)
                                {
                                    AutoLogger?.LogError(ex, "WriteData 2");
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
                                    SerializeObjectType itemJsonType = SerializeHelper.GetTypeCodeOfObject(value.GetType());
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
                                    SerializeObjectType itemJsonType = SerializeHelper.GetTypeCodeOfObject(value.GetType());
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
                                AutoLogger?.LogError(ex, "WriteData 3");
                            }
                        }
                    }
                }
            }
        }
    }
}
