using SignalGo.Shared.Log;
using System;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
#if (!NET35 && !NET40)
    /// <summary>
    /// a TaskCompletionSource with concurrent support with skip deadlock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentTaskCompletionSource<T>
    {
        TaskCompletionSource<T> Value { get; set; } = new TaskCompletionSource<T>();
        public Task<T> Task
        {
            get
            {
                return GetTask();
            }
        }

        public async Task<T> GetTask()
        {
            return await Value.Task;
        }

        public bool IsCompleted()
        {
            return Value.Task.IsCompleted;
        }

        public void SetException(Exception exception)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Value.SetException(exception);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ConcurrentTaskCompletionSource SetException");
                }
            });
        }

        public bool TrySetException(Exception exception)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Value.TrySetException(exception);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ConcurrentTaskCompletionSource TrySetException");
                }
            });
            return true;
        }

        public void SetResult(T result)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Value.SetResult(result);
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ConcurrentTaskCompletionSource SetResult");
                }
            });
        }

        public bool TrySetResult(T result)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (!Value.TrySetResult(result))
                        AutoLogger.Default.LogText($"ConcurrentTaskCompletionSource TrySetResult is false {IsCompleted()} {Value.Task.Status} {Value.Task.Exception}");
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ConcurrentTaskCompletionSource TrySetResult");
                }
            });
            return true;
        }

        public bool TrySetCanceled()
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Value.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "ConcurrentTaskCompletionSource TrySetCanceled");
                }
            });
            return true;
        }
    }
#else
    /// <summary>
    /// a TaskCompletionSource with concurrent support with skip deadlock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentTaskCompletionSource<T>
    {
        TaskCompletionSource<T> Value { get; set; } = new TaskCompletionSource<T>();
        public Task<T> Task
        {
            get
            {
                return new Task<T>(GetTask);
            }
        }

        public T GetTask()
        {
            return Value.Task.Result;
        }

        public bool IsCompleted()
        {
            return Value.Task.IsCompleted;
        }

        public void SetException(Exception exception)
        {
            try
            {
                Value.SetException(exception);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "SetException");
            }
        }

        public bool TrySetException(Exception exception)
        {
            try
            {
                return Value.TrySetException(exception);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "TrySetException");
            }
            return false;
        }

        public void SetResult(T result)
        {
            try
            {
                Value.SetResult(result);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "SetResult");
            }
        }

        public bool TrySetResult(T result)
        {
            try
            {
                return Value.TrySetResult(result);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "TrySetResult");
            }
            return false;
        }

        public bool TrySetCanceled()
        {
            try
            {
                return Value.TrySetCanceled();
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "TrySetCanceled");
            }
            return false;
        }
    }
#endif
}
