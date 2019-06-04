// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

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
        /// <summary>
        /// orginal items of list
        /// </summary>
        private readonly IList<T> _internalList;
        /// <summary>
        /// lock of thread safe get item
        /// </summary>
        private readonly object lockObject = new object();

        #endregion

        #region ctor
        /// <summary>
        /// generate list
        /// </summary>
        public ConcurrentList()
        {
            _internalList = new List<T>();
        }
        /// <summary>
        /// generate list
        /// </summary>
        public ConcurrentList(int capacity)
        {
            _internalList = new List<T>(capacity);
        }
        /// <summary>
        /// generate list
        /// </summary>
        public ConcurrentList(IEnumerable<T> list)
        {
            _internalList = list.ToList();
        }

        #endregion
        /// <summary>
        /// get item of list with index
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>return value from list</returns>
        public T this[int index]
        {
            get
            {
                return LockInternalListAndGet(l => l[index]);
            }
            set
            {
                LockInternalListAndCommand(l => l[index] = value);
            }
        }

        /// <summary>
        /// count of list
        /// </summary>
        public int Count
        {
            get
            {
                return LockInternalListAndQuery(l => l.Count());
            }
        }

        /// <summary>
        /// if list is read only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// add item to list
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            LockInternalListAndCommand(l => l.Add(item));
        }
        /// <summary>
        /// clear list items
        /// </summary>
        public void Clear()
        {
            LockInternalListAndCommand(l => l.Clear());
        }
        /// <summary>
        /// contains item in list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return LockInternalListAndQuery(l => l.Contains(item));
        }
        /// <summary>
        /// copy list to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            LockInternalListAndCommand(l => l.CopyTo(array, arrayIndex));
        }
        /// <summary>
        /// get array of list items
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            return LockInternalListAndQuery(l => l.ToArray());
        }
        /// <summary>
        /// get enumator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return LockInternalListAndQuery(l => l.GetEnumerator());
        }
        /// <summary>
        /// get index of item in list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return LockInternalListAndQuery(l => l.IndexOf(item));
        }
        /// <summary>
        /// insert item into list
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            LockInternalListAndCommand(l => l.Insert(index, item));
        }
        /// <summary>
        /// remove item from list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return LockInternalListAndQuery(l => l.Remove(item));
        }
        /// <summary>
        /// remove item from list with index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            LockInternalListAndCommand(l => l.RemoveAt(index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LockInternalListAndQuery(l => l.GetEnumerator());
        }

        #region Utilities
        /// <summary>
        /// do action with lock
        /// </summary>
        /// <param name="action"></param>
        protected virtual void LockInternalListAndCommand(Action<IList<T>> action)
        {
            lock (lockObject)
            {
                action(_internalList);
            }
        }
        /// <summary>
        /// do func with lock
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        protected virtual T LockInternalListAndGet(Func<IList<T>, T> func)
        {
            lock (lockObject)
            {
                return func(_internalList);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
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
