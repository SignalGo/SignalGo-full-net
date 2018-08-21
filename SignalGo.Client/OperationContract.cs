using System.Collections.Concurrent;

namespace SignalGo.Client
{
    /// <summary>
    /// an opration contract for get server connector
    /// </summary>
    public static class OperationContract
    {
        /// <summary>
        /// dictionary of connectors
        /// </summary>
        internal static ConcurrentDictionary<object, object> OpartionContractKeyValues = new ConcurrentDictionary<object, object>();

        /// <summary>
        /// get connector of object key that add from SetConnector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T GetConnector<T>(object data)
        {
            return (T)OpartionContractKeyValues[data];
        }

        /// <summary>
        /// add connector for a object key
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connector"></param>
        internal static void SetConnector(object data, object connector)
        {
            OpartionContractKeyValues.TryAdd(data, connector);
        }
    }
}
