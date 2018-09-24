using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    public class ConcurrentBlockingCollection<T>
    {
        public ConcurrentBlockingCollection()
        {
            _taskCompletionSource = CreateNewTask();
        }

        private readonly SemaphoreSlim _addLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _takeLock = new SemaphoreSlim(1, 1);

        private readonly List<object> _items = new List<object>();
        private TaskCompletionSource<T> _taskCompletionSource;
        private bool _isTakeTaskResult = false;
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

#if (NET35 || NET40)
        public void AddAsync(T item)
#else
        public async Task AddAsync(T item)
#endif
        {
            try
            {
                if (_IsCanceled)
                    return;
#if (NET35 || NET40)
                _addLock.Wait();
#else
                await _addLock.WaitAsync();
#endif
                _items.Add(item);
                //Console.WriteLine("added" + item);
                if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
                {
                    CompleteTask();
                }
                else
                {
                    //Console.WriteLine("wrong status 2 : " + _taskCompletionSource.Task.Status);
                }
            }
            finally
            {
                _addLock.Release();
            }
        }


        private TaskCompletionSource<T> CreateNewTask()
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            CancellationTokenSource ct = new CancellationTokenSource();
            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs;
        }

        private void CompleteTask()
        {
            if (_IsCanceled && _items.Count == 0)
                return;
            if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                if (find != null)
                {
                    _items.RemoveAt(0);
                    _taskCompletionSource.SetResult((T)find);
                }
            }
            else if (_taskCompletionSource.Task.Status == TaskStatus.RanToCompletion)
            {
                if (_isTakeTaskResult)
                {
                    object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                    if (find != null)
                    {
                        _items.RemoveAt(0);
                        _taskCompletionSource = CreateNewTask();
                        _taskCompletionSource.SetResult((T)find);
                    }
                    else
                        _taskCompletionSource = CreateNewTask();
                    _isTakeTaskResult = false;
                }
            }
            else
            {
                Console.WriteLine("wrong status : " + _taskCompletionSource.Task.Status);
            }
        }

        private bool _IsCanceled = false;
#if (NET35 || NET40)
        public void CancelAsync()
#else
        public async Task CancelAsync()
#endif
        {
            try
            {
#if (NET35 || NET40)
                _addLock.Wait();
#else
                await _addLock.WaitAsync();
#endif
                _IsCanceled = true;
                object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                _taskCompletionSource.TrySetResult((T)find);
            }
            finally
            {
                _addLock.Release();
            }
        }

#if (NET35 || NET40)
        public T TakeAsync()
#else
        public async Task<T> TakeAsync()
#endif
        {
            try
            {
#if (NET35 || NET40)
                _addLock.Wait();
#else
                await _addLock.WaitAsync();
#endif
                CompleteTask();
            }
            finally
            {
                _addLock.Release();
            }

            try
            {
#if (NET35 || NET40)
                _takeLock.Wait();
#else
                await _takeLock.WaitAsync();
#endif
                Task<T> result = _taskCompletionSource.Task;
                _isTakeTaskResult = true;
#if (NET35 || NET40)
                return result.Result;
#else
                return await result;
#endif
            }
            finally
            {
                _takeLock.Release();
            }


            //CompleteTask();
        }



        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Cast<T>().GetEnumerator();
        }
    }

    //public class ConcurrentQueueCollection<T>
    //{
    // The underlying collection of items.
    //private readonly IProducerConsumerCollection<T> collection;

    //// The maximum number of items allowed.
    //private readonly int maxCount;

    //// Synchronization primitives.
    //private readonly AsyncLock mutex;
    //private readonly AsyncConditionVariable notFull;
    //private readonly AsyncConditionVariable notEmpty;

    //public ConcurrentQueueCollection(IProducerConsumerCollection<T> collection = null, int maxCount = int.MaxValue)
    //{
    //    if (maxCount <= 0)
    //        throw new ArgumentOutOfRangeException("maxCount", "The maximum count must be greater than zero.");
    //    this.collection = collection ?? new ConcurrentQueue<T>();
    //    this.maxCount = maxCount;

    //    mutex = new AsyncLock();
    //    notFull = new AsyncConditionVariable(mutex);
    //    notEmpty = new AsyncConditionVariable(mutex);
    //}

    //// Convenience properties to make the code a bit clearer.
    //private bool Empty { get { return collection.Count == 0; } }
    //private bool Full { get { return collection.Count == maxCount; } }

    //public async Task AddAsync(T item)
    //{
    //    using (await mutex.LockAsync())
    //    {
    //        while (Full)
    //            await notFull.WaitAsync();

    //        if (!collection.TryAdd(item))
    //            throw new InvalidOperationException("The underlying collection refused the item.");
    //        notEmpty.NotifyOne();
    //    }
    //}

    //public async Task<T> TakeAsync()
    //{
    //    using (await mutex.LockAsync())
    //    {
    //        while (Empty)
    //            await notEmpty.WaitAsync();

    //        T ret;
    //        if (!collection.TryTake(out ret))
    //            throw new InvalidOperationException("The underlying collection refused to provide an item.");
    //        notFull.NotifyOne();
    //        return ret;
    //    }
    //}
    //public ConcurrentQueueCollection()
    //{
    //    _taskCompletionSource = CreateNewTask();
    //}

    //private readonly List<object> _items = new List<object>();
    //bool _isTaskCompleted = false;
    //TaskCompletionSource<T> _taskCompletionSource;
    //public int Count
    //{
    //    get
    //    {
    //        return _items.Count;
    //    }
    //}

    //public void Add(T item)
    //{
    //    lock (_items)
    //    {
    //        _items.Add(item);
    //        if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
    //        {
    //            CompleteTask();
    //        }
    //        else
    //        {
    //            //Console.WriteLine("wrong status 2 : " + _taskCompletionSource.Task.Status);
    //        }
    //    }
    //}


    //public TaskCompletionSource<T> CreateNewTask()
    //{
    //    var tcs = new TaskCompletionSource<T>();

    //    var ct = new CancellationTokenSource();
    //    ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
    //    return tcs;
    //}

    //private void CompleteTask()
    //{
    //    if (_isTaskCompleted)
    //        return;
    //    if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
    //    {
    //        var find = _items.DefaultIfEmpty(null).FirstOrDefault();
    //        if (find != null)
    //        {
    //            _items.RemoveAt(0);
    //            _taskCompletionSource.SetResult((T)find);
    //            _isTaskCompleted = true;
    //        }
    //    }
    //    else if (_taskCompletionSource.Task.Status == TaskStatus.RanToCompletion)
    //    {
    //        object find = _items.DefaultIfEmpty(null).FirstOrDefault();
    //        if (find != null)
    //        {
    //            _items.RemoveAt(0);
    //            _taskCompletionSource = CreateNewTask();
    //            _taskCompletionSource.SetResult((T)find);
    //            _isTaskCompleted = true;
    //        }
    //        else
    //            _taskCompletionSource = CreateNewTask();
    //    }
    //    else
    //    {
    //        Console.WriteLine("wrong status : " + _taskCompletionSource.Task.Status);
    //    }
    //}

    //public void Cancel()
    //{
    //    lock (_items)
    //    {
    //        _taskCompletionSource.TrySetResult(
    //    }
    //}

    //public Task<T> TakeAsync()
    //{
    //    lock (_items)
    //    {
    //        CompleteTask();
    //        _isTaskCompleted = false;
    //        var result = _taskCompletionSource.Task;
    //        //result.ContinueWith((task) =>
    //        //{
    //        //    CompleteTask();
    //        //});
    //        return result;
    //    }
    //}



    //public void Dispose()
    //{
    //    GC.SuppressFinalize(this);
    //}

    //public IEnumerator<T> GetEnumerator()
    //{
    //    return _items.Cast<T>().GetEnumerator();
    //}

    //IEnumerator IEnumerable.GetEnumerator()
    //{
    //    return _items.GetEnumerator();
    //}
    //}
}
