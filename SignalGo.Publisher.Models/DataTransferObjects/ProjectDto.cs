using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Publisher.Models.DataTransferObjects
{
    public class ProjectDto
    {

        public ProjectDto()
        {
            IgnoreFiles = new HashSet<IgnoreFileDto>();

        }

        public int ID { get; set; }
        public Guid ProjectKey { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string ProjectPath { get; set; }
        public string ProjectAssembliesPath { get; set; }
        public string LastUpdateDateTime { get; set; }

        public int? CategoryId { get; set; }
        public CategoryDto Category { get; set; }
        public ICollection<IgnoreFileDto> IgnoreFiles { get; set; }

        public static implicit operator ProjectDto(ProjectInfo projectInfo)
        {
            if (projectInfo == null)
                return null;
            var dto = new ProjectDto
            {
                ID = projectInfo.ID,
                Category = projectInfo.Category ?? null,
                CategoryId = projectInfo.CategoryId,
                LastUpdateDateTime = projectInfo.LastUpdateDateTime,
                ProjectAssembliesPath = projectInfo.ProjectAssembliesPath,
                ProjectPath = projectInfo.ProjectPath,
                ProjectKey = projectInfo.ProjectKey,
                Name = projectInfo.Name,
                IgnoreFiles = projectInfo.IgnoreFiles?
                .Select(x => (IgnoreFileDto)x).ToList()
            };
            return dto;
        }

        public static implicit operator ProjectInfo(ProjectDto projectDto)
        {
            if (projectDto == null)
                return null;
            var info = new ProjectInfo
            {
                ID = projectDto.ID,
                Category = projectDto.Category ?? null,
                CategoryId = projectDto.CategoryId,
                LastUpdateDateTime = projectDto.LastUpdateDateTime,
                ProjectAssembliesPath = projectDto.ProjectAssembliesPath,
                ProjectPath = projectDto.ProjectPath,
                ProjectKey = projectDto.ProjectKey,
                Name = projectDto.Name,
                IgnoreFiles = projectDto.IgnoreFiles?
                .Select(x => (IgnoreFileInfo)x).ToList()
            };
            return info;
        }
    }
}
