namespace SignalGo.Client
{
    /// <summary>
    /// handle action of priority systems
    /// </summary>
    public enum PriorityAction
    {
        /// <summary>
        /// try agin to run priority method,this can run for ever if you return this always
        /// </summary>
        TryAgain = 1,
        /// <summary>
        /// no plan for this periority and it will go next priority or finished if was last
        /// </summary>
        NoPlan = 2,
        /// <summary>
        /// skip this priority and next and break all of priorities
        /// </summary>
        BreakAll = 3,
        /// <summary>
        /// hold all priority and wait until set UnHoldPriority method
        /// </summary>
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
