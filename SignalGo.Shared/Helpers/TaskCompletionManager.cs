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
        public void SetException(Exception exception)
        {
            Exception = exception;
            IsCompleted = true;
        }

        public void SetResult(T result)
        {
            Value = result;
            IsCompleted = true;
        }

        public T GetValue()
        {
            while (!IsCompleted)
            {
                Thread.Sleep(10);
            }
            if (Exception != null)
                throw Exception;
            return Value;
        }

        public bool TrySetCanceled()
        {
            Exception = new OperationCanceledException();
            IsCompleted = true;
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
        TaskCompletionSource<T> Value { get; set; } = new TaskCompletionSource<T>();
#if (NET40 || NET35)
        public Task<T> GetValue()
        {
            return Value.Task;
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
