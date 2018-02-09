using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Client
{
    public enum PriorityAction
    {
        TryAgain = 1,
        NoPlan = 2,
        BreakAll = 3,
        HoldAll = 4
    }

    /// <summary>
    /// status of signalgo connection
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// signal go successfuly connected to server
        /// </summary>
        Connected = 1,
        /// <summary>
        /// signalgo disconnected from server
        /// </summary>
        Disconnected = 2,
        /// <summary>
        /// signalgo trying to connected
        /// </summary>
        Reconnecting = 3,
        /// <summary>
        /// signalgo reconnected
        /// </summary>
        Reconnected = 4
    }
}
