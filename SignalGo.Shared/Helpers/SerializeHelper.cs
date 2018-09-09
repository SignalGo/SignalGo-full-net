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
            SerializeObjectType targetPropertyType = GetTypeCodeOfObject(toType);
            if (targetPropertyType == SerializeObjectType.Boolean)
            {
                return Convert.ToBoolean(value);
            }
            else if (targetPropertyType == SerializeObjectType.BooleanNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (bool?)Convert.ToBoolean(value);
            }
            else if (targetPropertyType == SerializeObjectType.Byte)
            {
                return Convert.ToByte(value);
            }
            else if (targetPropertyType == SerializeObjectType.ByteNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (byte?)Convert.ToByte(value);
            }
            else if (targetPropertyType == SerializeObjectType.Char)
            {
                return Convert.ToChar(value);
            }
            else if (targetPropertyType == SerializeObjectType.CharNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (char?)Convert.ToChar(value);
            }
            else if (targetPropertyType == SerializeObjectType.DateTime)
            {
                try
                {
                    return Convert.ToDateTime(value);
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, $"cannot convert value {value} type of {value.GetType()} to DateTime!");
                    return DateTime.MinValue;
                }
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (DateTime?)Convert.ToDateTime(value);
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeOffset)
            {
                return new DateTimeOffset(Convert.ToDateTime(value));
            }
            else if (targetPropertyType == SerializeObjectType.DateTimeOffsetNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (DateTimeOffset?)new DateTimeOffset(Convert.ToDateTime(value));
            }
            else if (targetPropertyType == SerializeObjectType.Decimal)
            {
                return Convert.ToDecimal(value);
            }
            else if (targetPropertyType == SerializeObjectType.DecimalNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (decimal?)Convert.ToDecimal(value);
            }
            else if (targetPropertyType == SerializeObjectType.Double)
            {
                return Convert.ToDouble(value);
            }
            else if (targetPropertyType == SerializeObjectType.DoubleNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (double?)Convert.ToDouble(value);
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
            else if (targetPropertyType == SerializeObjectType.Guid)
            {
                return new Guid(value.ToString());
            }
            else if (targetPropertyType == SerializeObjectType.Int16)
            {
                return Convert.ToInt16(value);
            }
            else if (targetPropertyType == SerializeObjectType.Int16Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (short?)Convert.ToInt16(value);
            }
            else if (targetPropertyType == SerializeObjectType.Int32)
            {
                return Convert.ToInt32(value);
            }
            else if (targetPropertyType == SerializeObjectType.Int32Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (int?)Convert.ToInt32(value);
            }
            else if (targetPropertyType == SerializeObjectType.Int64)
            {
                return Convert.ToInt64(value);
            }
            else if (targetPropertyType == SerializeObjectType.Int64Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (long?)Convert.ToInt64(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt16)
            {
                return Convert.ToUInt16(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt16Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (ushort?)Convert.ToUInt16(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt32)
            {
                return Convert.ToUInt32(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt32Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (uint?)Convert.ToUInt32(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt64)
            {
                return Convert.ToUInt64(value);
            }
            else if (targetPropertyType == SerializeObjectType.UInt64Nullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (ulong?)Convert.ToUInt64(value);
            }
            else if (targetPropertyType == SerializeObjectType.SByte)
            {
                return Convert.ToSByte(value);
            }
            else if (targetPropertyType == SerializeObjectType.SByteNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (sbyte?)Convert.ToSByte(value);
            }
            else if (targetPropertyType == SerializeObjectType.Single)
            {
                return Convert.ToSingle(value);
            }
            else if (targetPropertyType == SerializeObjectType.FloatNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (float?)Convert.ToSingle(value);
            }
            else if (targetPropertyType == SerializeObjectType.String)
            {
                if (value.ToString() == "null")
                    return null;
                return value.ToString();
            }
            else if (targetPropertyType == SerializeObjectType.TimeSpan)
            {
                return TimeSpan.Parse(value.ToString());
            }
            else if (targetPropertyType == SerializeObjectType.TimeSpanNullable)
            {
                if (value.ToString() == "null")
                    return null;
                return (TimeSpan?)TimeSpan.Parse(value.ToString());
            }
            else if (targetPropertyType == SerializeObjectType.Uri)
            {
                return new Uri(value.ToString());
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
