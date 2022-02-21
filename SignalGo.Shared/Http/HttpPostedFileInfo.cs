using SignalGo.Shared.IO;
using System.IO;

namespace SignalGo.Shared.Http
{
    /// <summary>
    /// model of post a file from client to server
    /// </summary>
    public class HttpPostedFileInfo
    {
        /// <summary>
        /// parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// size of file
        /// </summary>
        public long ContentLength { get; set; }
        /// <summary>
        /// type of file
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// name of file
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// stream of file to read
        /// </summary>
        public Stream InputStream { get; set; }
    }
}
