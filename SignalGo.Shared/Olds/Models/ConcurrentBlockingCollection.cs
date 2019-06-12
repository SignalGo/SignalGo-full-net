using System;
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

#if (!NET35 && !NET40)
        public async Task AddAsync(T item)
        {
            try
            {
                if (_IsCanceled)
                    return;
                await _addLock.WaitAsync();
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
#endif
        public void Add(T item)
        {
            try
            {
                if (_IsCanceled)
                    return;
                _addLock.Wait();
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
#if (!NET35 && !NET40)
        public async Task CancelAsync()
        {
            try
            {
                await _addLock.WaitAsync();
                _IsCanceled = true;
                object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                _taskCompletionSource.TrySetResult((T)find);
            }
            finally
            {
                _addLock.Release();
            }
        }
#endif
        public void Cancel()
        {
            try
            {
                _addLock.Wait();
                _IsCanceled = true;
                object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                _taskCompletionSource.TrySetResult((T)find);
            }
            finally
            {
                _addLock.Release();
            }
        }

#if (!NET35 && !NET40)
        public async Task<T> TakeAsync()
        {
            try
            {
                await _addLock.WaitAsync();

                CompleteTask();
            }
            finally
            {
                _addLock.Release();
            }

            try
            {
                await _takeLock.WaitAsync();
                Task<T> result = _taskCompletionSource.Task;
                _isTakeTaskResult = true;
                return await result;
            }
            finally
            {
                _takeLock.Release();
            }


            //CompleteTask();
        }
#endif

        public T Take()
        {
            try
            {
                _addLock.Wait();
                CompleteTask();
            }
            finally
            {
                _addLock.Release();
            }

            try
            {
                _takeLock.Wait();
                Task<T> result = _taskCompletionSource.Task;
                _isTakeTaskResult = true;
                return result.Result;
            }
            finally
            {
                _takeLock.Release();
            }
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
}
