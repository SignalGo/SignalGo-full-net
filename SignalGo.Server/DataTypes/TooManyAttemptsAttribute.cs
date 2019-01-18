using SignalGo.Server.Models;
using System;
using System.Collections.Concurrent;

namespace SignalGo.Server.DataTypes
{
    internal class TooManyAttemptsInfo
    {
        /// <summary>
        /// last requested datetime
        /// </summary>
        public DateTime LastRequestedDateTime { get; set; }
        /// <summary>
        /// count of called each time
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// check too many attempts system
    /// limit client too call methods each times
    /// </summary>
    public class TooManyAttemptsAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration">duration of limit time each call methods</param>
        public TooManyAttemptsAttribute(TimeSpan duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// duration of limit time each call methods
        /// </summary>
        public TimeSpan Duration { get; set; } = new TimeSpan(0, 0, 1);

        /// <summary>
        /// last request date time each ip address
        /// </summary>
        internal ConcurrentDictionary<long, TooManyAttemptsInfo> LastRequestedEachIpAddress { get; set; } = new ConcurrentDictionary<long, TooManyAttemptsInfo>();
        /// <summary>
        /// check id client has permissions
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public virtual bool HasPermissions(ClientInfo clientInfo)
        {
            return true;
        }


    }
}
