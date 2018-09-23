using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.Models
{
    public class ConcurrentQueueCollection<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        public int Timeout { get; set; } = -1;
        public ConcurrentQueueCollection(int timeout)
        {
            Timeout = timeout;
            _taskCompletionSource = CreateNewTask();
        }

        private readonly List<object> _items = new List<object>();
        bool _isTaskCompleted = false;
        TaskCompletionSource<T> _taskCompletionSource;
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public void Add(T item)
        {
            lock (_items)
            {
                _items.Add(item);
                if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
                {
                    CompleteTask();
                }
                else
                {
                    //Console.WriteLine("wrong status 2 : " + _taskCompletionSource.Task.Status);
                }
            }
        }


        public TaskCompletionSource<T> CreateNewTask()
        {
            var tcs = new TaskCompletionSource<T>();

            var ct = new CancellationTokenSource(Timeout);
            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs;
        }

        private void CompleteTask()
        {
            if (_taskCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                if (_isTaskCompleted)
                    return;
                var find = _items.DefaultIfEmpty(null).FirstOrDefault();
                if (find != null)
                {
                    _items.RemoveAt(0);
                    _taskCompletionSource.SetResult((T)find);
                    _isTaskCompleted = true;
                }
            }
            else if (_taskCompletionSource.Task.Status == TaskStatus.RanToCompletion)
            {
                if (_isTaskCompleted)
                    return;
                object find = _items.DefaultIfEmpty(null).FirstOrDefault();
                if (find != null)
                {
                    _items.RemoveAt(0);
                    _taskCompletionSource = CreateNewTask();
                    _taskCompletionSource.SetResult((T)find);
                    _isTaskCompleted = true;
                }
                else
                    _taskCompletionSource = CreateNewTask();
            }
            else
            {
                Console.WriteLine("wrong status : " + _taskCompletionSource.Task.Status);
            }

        }

        public Task<T> TakeAsync()
        {
            lock (_items)
            {
                CompleteTask();
                _isTaskCompleted = false;
                var result = _taskCompletionSource.Task;
                //result.ContinueWith((task) =>
                //{
                //    CompleteTask();
                //});
                return result;
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
