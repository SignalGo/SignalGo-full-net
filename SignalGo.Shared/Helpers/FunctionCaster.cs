using System;
using System.Threading.Tasks;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// cast object function to T function
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class FunctionCaster<TResult>
    {
        public Task<TResult> Do(Func<object> func)
        {
            return Task<TResult>.Factory.StartNew(() =>
            {
                return (TResult)func();
            });
        }
    }
}
