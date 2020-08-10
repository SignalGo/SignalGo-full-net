namespace SignalGo.Publisher.Models.Shared.Types
{
    /// <summary>
    /// Define ignore Type for file.
    /// </summary>
    public enum IgnoreFileType
    {
        /// <summary>
        /// Dont Include for Upload in Client
        /// </summary>
        CLIENT,
        /// <summary>
        /// Dont Update On Server
        /// </summary>
        SERVER,
        /// <summary>
        /// Dont Include File In Both Client & Server Operation's
        /// </summary>
        BOTH,

    }
}
