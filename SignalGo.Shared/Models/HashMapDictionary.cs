using SignalGo.Shared.Helpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    public class HashMapDictionary<T1, T2> : IEnumerable
    {
        private ConcurrentDictionary<T1, ConcurrentHash<T2>> _keyValue = new ConcurrentDictionary<T1, ConcurrentHash<T2>>();
        private ConcurrentDictionary<T2, ConcurrentHash<T1>> _valueKey = new ConcurrentDictionary<T2, ConcurrentHash<T1>>();

        public ICollection<T1> Keys
        {
            get
            {
                return _keyValue.Keys;
            }
        }

        public ICollection<T2> Values
        {
            get
            {
                return _valueKey.Keys;
            }
        }

        public int Count
        {
            get
            {
                return _keyValue.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ConcurrentHash<T2> this[T1 index]
        {
            get { return _keyValue[index]; }
            set { _keyValue[index] = value; }
        }

        public ConcurrentHash<T1> this[T2 index]
        {
            get { return _valueKey[index]; }
            set { _valueKey[index] = value; }
        }

        public void Add(T1 key, T2 value)
        {
            lock(this)
            {
                if (!_keyValue.TryGetValue(key, out ConcurrentHash<T2> result))
                    _keyValue.TryAdd(key, new ConcurrentHash<T2>() { value });
                else if (!result.Contains(value))
                    result.Add(value);

                if (!_valueKey.TryGetValue(value, out ConcurrentHash<T1> result2))
                    _valueKey.TryAdd(value, new ConcurrentHash<T1>() { key });
                else if (!result2.Contains(key))
                    result2.Add(key);
            }
        }

        public bool TryGetValues(T1 key, out ConcurrentHash<T2> value)
        {
            return _keyValue.TryGetValue(key, out value);
        }

        public ConcurrentHash<T2> GetValues(T1 key)
        {
            if (_keyValue.TryGetValue(key, out ConcurrentHash<T2> value))
                return value;
            return null;
        }

        public object[] GetObjectValues(T1 key)
        {
            if (_keyValue.TryGetValue(key, out ConcurrentHash<T2> value))
                return value.Cast<object>().ToArray();
            return new object[0];
        }

        public bool TryGetKeys(T2 value, out ConcurrentHash<T1> key)
        {
            return _valueKey.TryGetValue(value, out key);
        }

        public ConcurrentHash<T1> GetKeys(T2 key)
        {
            if (_valueKey.TryGetValue(key, out ConcurrentHash<T1> value))
                return value;
            return null;
        }

        public object[] GetObjectKeys(T2 key)
        {
            if (_valueKey.TryGetValue(key, out ConcurrentHash<T1> value))
                return value.Cast<object>().ToArray();
            return new object[0];
        }

        public bool ContainsKey(T1 key)
        {
            return _keyValue.ContainsKey(key);
        }

        public bool ContainsValue(T2 value)
        {
            return _valueKey.ContainsKey(value);
        }

        public void Remove(T1 key)
        {
            lock (this)
            {
                if (_keyValue.TryRemove(key, out ConcurrentHash<T2> values))
                {
                    foreach (var item in values)
                    {
                        var remove2 = _valueKey.TryRemove(item, out ConcurrentHash<T1> keys);
                    }
                }
            }
        }

        public void Remove(T2 value)
        {
            lock (this)
            {
                if (_valueKey.TryRemove(value, out ConcurrentHash<T1> keys))
                {
                    foreach (var item in keys)
                    {
                        var remove2 = _keyValue.TryRemove(item, out ConcurrentHash<T2> values);
                    }
                }
                
            }
        }

        public void Clear()
        {
            _keyValue.Clear();
            _valueKey.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyValue.GetEnumerator();
        }
    }
}
