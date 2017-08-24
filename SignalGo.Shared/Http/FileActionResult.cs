using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            FileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
    }
}
