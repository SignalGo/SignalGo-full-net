using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// a key value class like tupple
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class KeyValue<T1, T2>
    {
        public KeyValue()
        {

        }
        public KeyValue(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }
        public T1 Key { get; set; }
        public T2 Value { get; set; }
    }

    public class DoubleNullable<T>
    {
        public T Value { get; set; }
    }

}
