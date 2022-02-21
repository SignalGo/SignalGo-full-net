using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
    public interface ITaskCompletionManager<T>
    {
        void SetException(Exception exception);
        void SetResult(T result);
        bool TrySetCanceled();
        bool TrySetException(Exception exception);
    }

    public class TaskCompletionManager<T> : ITaskCompletionManager<T>
    {
        bool IsCompleted { get; set; }
        T Value { get; set; }
        Exception Exception { get; set; }
        ManualResetEvent ManualResetEvent { get; set; } = new ManualResetEvent(false);
        public void SetException(Exception exception)
        {
            Exception = exception;
            IsCompleted = true;
            ManualResetEvent.Set();
        }

        public void SetResult(T result)
        {
            Value = result;
            IsCompleted = true;
            ManualResetEvent.Set();
        }

        public T GetValue()
        {
            ManualResetEvent.WaitOne();
            if (Exception != null)
                throw Exception;
            return Value;
        }

        public bool TrySetCanceled()
        {
            Exception = new OperationCanceledException();
            IsCompleted = true;
            ManualResetEvent.Set();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            SetException(exception);
            return true;
        }
    }

    public class TaskCompletionManagerAsync<T> : ITaskCompletionManager<T>
    {
        ConcurrentTaskCompletionSource<T> Value { get; set; } = new ConcurrentTaskCompletionSource<T>();
#if (NET40 || NET35)
        public T GetValue()
        {
            return Value.Task.Result;
        }
#else
        public async Task<T> GetValue()
        {
            return await Value.Task;
        }
#endif
        public void SetException(Exception exception)
        {
            Value.SetException(exception);
        }

        public bool TrySetException(Exception exception)
        {
            return Value.TrySetException(exception);
        }

        public void SetResult(T result)
        {
            Value.SetResult(result);
        }
        public bool TrySetCanceled()
        {
            return Value.TrySetCanceled();
        }
    }
}
