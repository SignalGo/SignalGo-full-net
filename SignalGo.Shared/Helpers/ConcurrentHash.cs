using System.Collections;
using System.Collections.Generic;

namespace SignalGo.Shared.Helpers
{
    public class ConcurrentHash<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private HashSet<T> _internalList;
        public ConcurrentHash()
        {
            _internalList = new HashSet<T>();
        }

        public int Count
        {
            get
            {
                lock (this)
                    return _internalList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Add(T item)
        {
            lock (this)
                return _internalList.Add(item);
        }

        public void Clear()
        {
            lock (this)
                _internalList.Clear();
        }

        public bool Contains(T item)
        {
            lock (this)
                return _internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
                _internalList.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (this)
                _internalList.ExceptWith(other);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this)
                return _internalList.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            lock (this)
                _internalList.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.Overlaps(other);
        }

        public bool Remove(T item)
        {
            lock (this)
                return _internalList.Remove(item);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (this)
                return _internalList.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (this)
                _internalList.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            lock (this)
                _internalList.UnionWith(other);
        }

        void ICollection<T>.Add(T item)
        {
            lock (this)
                _internalList.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this)
                return _internalList.GetEnumerator();
        }
    }
}
