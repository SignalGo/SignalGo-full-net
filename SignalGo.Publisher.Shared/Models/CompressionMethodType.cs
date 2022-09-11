namespace SignalGo.Publisher.Shared.Models
{
    /// <summary>
    /// specify the compression algoritm, using to manage compression and decompression data
    /// </summary>
    public enum CompressionMethodType
    {
        /// <summary>
        /// no compressed method. use it if you have't many files and the are small
        /// </summary>
        None = 0,
        /// <summary>
        /// Default And Cross Platform Compression Algoritm that provide low compression but Fasater IO Processing
        /// </summary>
        Zip = 1,
        /// <summary>
        /// useful in unix base systems like linux and MacOs
        /// </summary>
        Gzip = 2,
        /// <summary>
        /// Medium to Higher Compression But Limited to Windows Os and require it's library to manage
        /// </summary>
        Rar = 3,
        /// <summary>
        /// useful in unix base systems
        /// </summary>
        Tar = 4,
        /// <summary>
        /// Higher Compression but need it's dpendencies, useful in unix based os
        /// </summary>
        bzip = 5,
        /// <summary>
        /// Medium to Higher Compression But Require it's library and os compatibility/support
        /// </summary>
        Zip7 = 6,
    }
}
