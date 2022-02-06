using SignalGo.Shared.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// Represents a thread-safe list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentList<T> : IList<T>
    {
        #region Fields

        private readonly IList<T> _internalList;

        private readonly object lockObject = new object();

        #endregion

        #region ctor

        public ConcurrentList()
        {
            _internalList = new List<T>();
        }

        public ConcurrentList(int capacity)
        {
            _internalList = new List<T>(capacity);
        }

        public ConcurrentList(IEnumerable<T> list)
        {
            _internalList = list.ToList();
        }

        #endregion

        public T this[int index]
        {
            get
            {
                return LockInternalListAndGet(l => l[index], index);
            }
            set
            {
                LockInternalListAndCommand(l => l[index] = value);
            }
        }

        public int Count
        {
            get
            {
                return LockInternalListAndQuery(l => l.Count());
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            LockInternalListAndCommand(l => l.Add(item));
        }

        public void AddWithNoDuplication(T item)
        {
            LockInternalListAndCommand(l =>
            {
                if (!l.Contains(item))
                    l.Add(item);
            });
        }

        public void Clear()
        {
            LockInternalListAndCommand(l => l.Clear());
        }

        public bool Contains(T item)
        {
            return LockInternalListAndQuery(l => l.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            LockInternalListAndCommand(l =>
            {
                Array.Resize(ref array, l.Count);
                l.CopyTo(array, arrayIndex);
            });
        }

        public T[] ToArray()
        {
            return LockInternalListAndQuery(l => l.ToArray());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return LockInternalListAndQuery(l => l.ToList().GetEnumerator());
        }

        public int IndexOf(T item)
        {
            return LockInternalListAndQuery(l => l.IndexOf(item));
        }

        public void Insert(int index, T item)
        {
            LockInternalListAndCommand(l => l.Insert(index, item));
        }

        public bool Remove(T item)
        {
            return LockInternalListAndQuery(l => l.Remove(item));
        }

        public void RemoveAt(int index)
        {
            LockInternalListAndCommand(l =>
            {
                if (_internalList.Count <= index)
                    return;
                l.RemoveAt(index);
            });
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LockInternalListAndQuery(l => l.ToArray().GetEnumerator());
        }

        #region Utilities

        protected virtual void LockInternalListAndCommand(Action<IList<T>> action)
        {
            lock (lockObject)
            {
                action(_internalList);
            }
        }

        protected virtual T LockInternalListAndGet(Func<IList<T>, T> func, int index = int.MinValue)
        {
            lock (lockObject)
            {
                try
                {
                    if (_internalList.Count == 0)
                        return default;
                    return func(_internalList);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, $"LockInternalListAndGet {index}");
                    AutoLogger.Default.LogText($"LockInternalListAndGet {_internalList.Count} {index} trace {Environment.StackTrace}");
                    return default;
                }
            }
        }

        protected virtual TObject LockInternalListAndQuery<TObject>(Func<IList<T>, TObject> query)
        {
            lock (lockObject)
            {
                return query(_internalList);
            }
        }

        #endregion
    }
}
