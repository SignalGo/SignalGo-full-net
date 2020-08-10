using SignalGo.Publisher.Models.Shared.Types;
using System.ComponentModel.DataAnnotations;

namespace SignalGo.Publisher.Models
{
    public class IgnoreFileInfo
    {
        public IgnoreFileInfo()
        {

        }

        public int ID { get; set; }
        public string FileName { get; set; }
        public bool IsEnabled { get; set; }
        public IgnoreFileType IgnoreFileType { get; set; } = IgnoreFileType.SERVER;
        public int ProjectId { get; set; }
        public virtual ProjectInfo ProjectInfo { get; set; }
    }
}
