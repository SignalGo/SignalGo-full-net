using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Publisher.Engine.Models
{
    public enum UpdateMode
    {
        None = 0,
        //کلاینت باید حذف کند
        Remove = 1,
        //کلاینت باید آپلود کند
        Upload = 2,
        //کلاینت باید دانلود کند
        Download = 3
    }

    public class FileCheckSum
    {
        public Guid Guid { get; set; }
        public string Path { get; set; }
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                    return "";
                return System.IO.Path.GetFileName(Path);
            }
        }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }

        public UpdateMode UpdateMode { get; set; }

        public string OldPath { get; set; }
        public FileCheckSum Clone()
        {
            return (FileCheckSum)MemberwiseClone();
        }
    }
}
