using System.IO;

namespace SignalGo.Shared.Http
{
    public class FileActionResult : ActionResult
    {
        [Newtonsoft.Json.JsonIgnore()]
        public Stream FileStream { get; set; }

        public FileActionResult(Stream stream) : base(stream)
        {
            FileStream = stream;
        }

        public FileActionResult(string fileName) : base(fileName)
        {
#if (PORTABLE)
            throw new NotSupportedException();
#else
            FileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
#endif
        }
    }
}
