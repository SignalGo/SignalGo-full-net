using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// object types of enumerable
    /// </summary>
    public enum SerializeObjectType
    {
        /// <summary>
        /// uknown file type or type is null
        /// </summary>
        None = 0,
        Object = 1,
        Char = 2,
        CharNullable = 3,
        Boolean = 4,
        BooleanNullable = 5,
        Byte = 6,
        ByteNullable = 7,
        SByte = 8,
        SByteNullable = 9,
        Int16 = 10,
        Int16Nullable = 11,
        UInt16 = 12,
        UInt16Nullable = 13,
        Int32 = 14,
        Int32Nullable = 15,
        UInt32 = 16,
        UInt32Nullable = 17,
        Int64 = 18,
        Int64Nullable = 19,
        UInt64 = 20,
        UInt64Nullable = 21,
        Single = 22,
        FloatNullable = 23,
        Double = 24,
        DoubleNullable = 25,
        DateTime = 26,
        DateTimeNullable = 27,
        DateTimeOffset = 28,
        DateTimeOffsetNullable = 29,
        Decimal = 30,
        DecimalNullable = 31,
        Guid = 32,
        GuidNullable = 33,
        TimeSpan = 34,
        TimeSpanNullable = 35,
        BigInteger = 36,
        BigIntegerNullable = 37,
        Uri = 38,
        String = 39,
        Bytes = 40,
        DBNull = 41,
        Enum = 42,
        Void = 43,
        IntPtr = 44,
        EnumNullable = 45
    }

    /// <summary>
    /// helper of serializing
    /// </summary>
    public class SerializeHelper
    {
        public static AutoLogger AutoLogger { get; set; } = new AutoLogger() { FileName = "SerializeHelper Logs.log" };
        private static readonly Dictionary<Type, SerializeObjectType> TypeCodeMap = new Dictionary<Type, SerializeObjectType>
        {
                { typeof(char), SerializeObjectType.Char },
                { typeof(char?), SerializeObjectType.CharNullable },
                { typeof(bool), SerializeObjectType.Boolean },
                { typeof(bool?), SerializeObjectType.BooleanNullable },
                { typeof(byte), SerializeObjectType.Byte },
                { typeof(byte?), SerializeObjectType.ByteNullable },
                { typeof(sbyte), SerializeObjectType.SByte },
                { typeof(sbyte?), SerializeObjectType.SByteNullable },
                { typeof(short), SerializeObjectType.Int16 },
                { typeof(short?), SerializeObjectType.Int16Nullable },
                { typeof(ushort), SerializeObjectType.UInt16 },
                { typeof(ushort?), SerializeObjectType.UInt16Nullable },
                { typeof(int), SerializeObjectType.Int32 },
                { typeof(int?), SerializeObjectType.Int32Nullable },
                { typeof(uint), SerializeObjectType.UInt32 },
                { typeof(uint?), SerializeObjectType.UInt32Nullable },
                { typeof(long), SerializeObjectType.Int64 },
                { typeof(long?), SerializeObjectType.Int64Nullable },
                { typeof(ulong), SerializeObjectType.UInt64 },
                { typeof(ulong?), SerializeObjectType.UInt64Nullable },
                { typeof(float), SerializeObjectType.Single },
                { typeof(float?), SerializeObjectType.FloatNullable },
                { typeof(double), SerializeObjectType.Double },
                { typeof(double?), SerializeObjectType.DoubleNullable },
                { typeof(DateTime), SerializeObjectType.DateTime },
                { typeof(DateTime?), SerializeObjectType.DateTimeNullable },
                { typeof(DateTimeOffset), SerializeObjectType.DateTimeOffset },
                { typeof(DateTimeOffset?), SerializeObjectType.DateTimeOffsetNullable },
                { typeof(decimal), SerializeObjectType.Decimal },
                { typeof(decimal?), SerializeObjectType.DecimalNullable },
                { typeof(Guid), SerializeObjectType.Guid },
                { typeof(Guid?), SerializeObjectType.GuidNullable },
                { typeof(TimeSpan), SerializeObjectType.TimeSpan },
                { typeof(TimeSpan?), SerializeObjectType.TimeSpanNullable },
                { typeof(string), SerializeObjectType.String },
                { typeof(void), SerializeObjectType.Void },
                { typeof(IntPtr), SerializeObjectType.IntPtr },
#if (!NETSTANDARD && !NETCOREAPP && !PORTABLE)
                { typeof(DBNull), SerializeObjectType.DBNull }
#endif
        };

        /// <summary>
        /// get type code of type
        /// </summary>
        /// <param name="type">your type</param>
        /// <returns>type code</returns>
        public static SerializeObjectType GetTypeCodeOfObject(Type type)
        {
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (type == null)
                return SerializeObjectType.None;
            else if (TypeCodeMap.ContainsKey(type))
                return TypeCodeMap[type];
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            else if (type.GetTypeInfo().IsEnum)
                return SerializeObjectType.Enum;
            else if (nullableType != null && nullableType.GetTypeInfo().IsEnum)
                return SerializeObjectType.EnumNullable;
#else
            else if (type.GetIsEnum())
                return SerializeObjectType.Enum;
            else if (nullableType != null && nullableType.IsEnum)
                return SerializeObjectType.EnumNullable;
#endif
            return SerializeObjectType.Object;
        }

        internal static ConcurrentDictionary<Type, Delegate> HandleSerializingObjectList = new ConcurrentDictionary<Type, Delegate>();
        internal static ConcurrentDictionary<Type, SerializeDelegateHandler> HandleDeserializingObjectList = new ConcurrentDictionary<Type, SerializeDelegateHandler>();

        public static void HandleSerializingObject<TType, TResultType>(Func<TType, TResultType> func)
        {
            HandleSerializingObjectList.TryAdd(typeof(TType), func);
        }

        public static void HandleDeserializingObject<TType, TResultType>(Func<TType, TResultType> func)
        {
            HandleDeserializingObjectList.TryAdd(typeof(TResultType), new SerializeDelegateHandler() { Delegate = func, ParameterType = typeof(TType) });
        }

        public static object ConvertType(Type toType, object value)
        {
            if (value == null)
                return null;
            else if (value.GetType() == toType)
                return value;
            SerializeObjectType targetPropertyType = GetTypeCodeOfObject(toType);
            if (targetPropertyType == SerializeObjectType.Boolean)
            {
                if (bool.TryParse(value.ToString(), out bool result))
                    return result;
                return default(bool);
            }
            else if (targetPropertyType == SerializeObjectType.BooleanNullable)
            {
                if (bool.TryParse(value.ToString(), out bool result))
                    return (bool?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Byte)
            {
                if (byte.TryParse(value.ToString(), out byte result))
                    return result;
                return default(byte);
            }
            else if (targetPropertyType == SerializeObjectType.ByteNullable)
            {
                if (byte.TryParse(value.ToString(), out byte result))
                    return (byte?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Char)
            {
                if (char.TryParse(value.ToString(), out char result))
                    return result;
                return default(char);
            }
            else if (targetPropertyType == SerializeObjectType.CharNullable)
            {
                if (char.TryParse(value.ToString(), out char result))
                    return (char?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.DateTime)
            {
                if (DateTime.TryParse(value.ToString(), out DateTime result))
                    return result;
                return default(DateTime);
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeNullable)
            {
                if (DateTime.TryParse(value.ToString(), out DateTime result))
                    return (DateTime?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeOffset)
            {
                if (DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset result))
                    return result;
                return default(DateTimeOffset);
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeOffsetNullable)
            {
                if (DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset result))
                    return (DateTimeOffset?)result;
                return null;
                //return (DateTimeOffset?)new DateTimeOffset(Convert.ToDateTime(value));
            }
            else if (targetPropertyType == SerializeObjectType.Decimal)
            {
                if (decimal.TryParse(value.ToString(), out decimal result))
                    return result;
                return default(decimal);
            }
            else if (targetPropertyType == SerializeObjectType.DecimalNullable)
            {
                if (decimal.TryParse(value.ToString(), out decimal result))
                    return (decimal?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Double)
            {
                if (double.TryParse(value.ToString(), out double result))
                    return result;
                return default(double);
            }
            else if (targetPropertyType == SerializeObjectType.DoubleNullable)
            {
                if (double.TryParse(value.ToString(), out double result))
                    return (double?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Enum)
            {
                if (value == null)
                {
                    Array values = Enum.GetValues(toType);
                    if (values.Length > 0)
                        return Enum.GetValues(toType).GetValue(0);
                    else
                        return null;
                }
                return Enum.Parse(toType, value.ToString(), true);
            }
            else if (targetPropertyType == SerializeObjectType.EnumNullable)
            {
                try
                {
                    if (value.ToString() == "null")
                        return null;
                    Type nullableType = Nullable.GetUnderlyingType(toType);
                    return Enum.Parse(nullableType, value.ToString());
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, $"cannot convert value {value} type of {value.GetType()} to EnumNullable!");
                    return null;
                }
            }
            else if (targetPropertyType == SerializeObjectType.Guid || targetPropertyType == SerializeObjectType.GuidNullable)
            {
#if (NET35)
                return new Guid(value.ToString());
#else
                if (Guid.TryParse(value.ToString(), out Guid result))
                    return result;
                return Guid.Empty;
#endif
            }
            else if (targetPropertyType == SerializeObjectType.Int16)
            {
                if (short.TryParse(value.ToString(), out short result))
                    return result;
                return default(short);
            }
            else if (targetPropertyType == SerializeObjectType.Int16Nullable)
            {
                if (short.TryParse(value.ToString(), out short result))
                    return result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Int32)
            {
                if (int.TryParse(value.ToString(), out int result))
                    return result;
                return default(int);
            }
            else if (targetPropertyType == SerializeObjectType.Int32Nullable)
            {
                if (int.TryParse(value.ToString(), out int result))
                    return (int?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Int64)
            {
                if (long.TryParse(value.ToString(), out long result))
                    return result;
                return default(long);
            }
            else if (targetPropertyType == SerializeObjectType.Int64Nullable)
            {
                if (long.TryParse(value.ToString(), out long result))
                    return (long?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.UInt16)
            {
                if (ushort.TryParse(value.ToString(), out ushort result))
                    return result;
                return default(ushort);
            }
            else if (targetPropertyType == SerializeObjectType.UInt16Nullable)
            {
                if (ushort.TryParse(value.ToString(), out ushort result))
                    return (ushort?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.UInt32)
            {
                if (uint.TryParse(value.ToString(), out uint result))
                    return result;
                return default(uint);
            }
            else if (targetPropertyType == SerializeObjectType.UInt32Nullable)
            {
                if (uint.TryParse(value.ToString(), out uint result))
                    return (uint?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.UInt64)
            {
                if (ulong.TryParse(value.ToString(), out ulong result))
                    return result;
                return default(ulong);
            }
            else if (targetPropertyType == SerializeObjectType.UInt64Nullable)
            {
                if (ulong.TryParse(value.ToString(), out ulong result))
                    return (ulong?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.SByte)
            {
                if (sbyte.TryParse(value.ToString(), out sbyte result))
                    return result;
                return default(sbyte);
            }
            else if (targetPropertyType == SerializeObjectType.SByteNullable)
            {
                if (sbyte.TryParse(value.ToString(), out sbyte result))
                    return (sbyte?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Single)
            {
                if (float.TryParse(value.ToString(), out float result))
                    return result;
                return default(float);
            }
            else if (targetPropertyType == SerializeObjectType.FloatNullable)
            {
                if (float.TryParse(value.ToString(), out float result))
                    return (float?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.String)
            {
                if (value.ToString() == "null")
                    return null;
                return value.ToString();
            }
            else if (targetPropertyType == SerializeObjectType.TimeSpan)
            {
                if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
                    return result;
                return TimeSpan.MinValue;
            }
            else if (targetPropertyType == SerializeObjectType.TimeSpanNullable)
            {
                if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
                    return (TimeSpan?)result;
                return null;
            }
            else if (targetPropertyType == SerializeObjectType.Uri)
            {
                if (Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out Uri result))
                    return result;
                return null;
            }
            else
            {
                return null;
            }
        }
    }

    public class SerializeDelegateHandler
    {
        public Delegate Delegate { get; set; }
        public Type ParameterType { get; set; }
    }
}
