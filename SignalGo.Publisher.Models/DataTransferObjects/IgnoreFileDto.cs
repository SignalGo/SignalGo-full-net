using SignalGo.Publisher.Models.Shared.Types;

namespace SignalGo.Publisher.Models.DataTransferObjects
{
    public class IgnoreFileDto
    {

        public IgnoreFileDto()
        {

        }


        public int ID { get; set; }
        public string FileName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public IgnoreFileType IgnoreFileType { get; set; }

        public int ProjectId { get; set; }
        //public ProjectDto ProjectDto { get; set; }

        public static implicit operator IgnoreFileDto(IgnoreFileInfo ignoreFileInfo)
        {
            if (ignoreFileInfo == null)
                return null;
            return new IgnoreFileDto
            {
                ID = ignoreFileInfo.ID,
                FileName = ignoreFileInfo.FileName,
                IgnoreFileType = ignoreFileInfo.IgnoreFileType,
                IsEnabled = ignoreFileInfo.IsEnabled,
                ProjectId = ignoreFileInfo.ProjectId
                //ProjectDto = ignoreFileInfo.ProjectInfo 
            };
        }

        public static implicit operator IgnoreFileInfo(IgnoreFileDto ignoreFileDto)
        {
            if (ignoreFileDto == null)
                return null;
            return new IgnoreFileInfo
            {
                ID = ignoreFileDto.ID,
                FileName = ignoreFileDto.FileName,
                IgnoreFileType = ignoreFileDto.IgnoreFileType,
                IsEnabled = ignoreFileDto.IsEnabled,
                ProjectId = ignoreFileDto.ProjectId
                //ProjectInfo = ignoreFileDto.ProjectDto
            };
        }
    }
}
