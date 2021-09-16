namespace SignalGo.Server.ServiceManager.Firewall
{
    /// <summary>
    /// danger of max data size
    /// </summary>
    public enum DangerDataType : byte
    {
        None = 0,
        /// <summary>
        /// when the first line size is max
        /// </summary>
        FirstLineSize = 1,
        /// <summary>
        /// when the header size is max
        /// </summary>
        HeaderSize = 2,
        /// <summary>
        /// when the request body size is max
        /// </summary>
        RequestBodySize = 3,
        /// <summary>
        /// when header key and value is not valid
        /// </summary>
        InvalidHeader = 4
    }
}
