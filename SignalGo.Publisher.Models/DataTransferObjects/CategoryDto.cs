using SignalGo.Publisher.Models;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Publisher.Models.DataTransferObjects
{
    public class CategoryDto
    {

        public CategoryDto()
        {
            SubCategories = new HashSet<CategoryDto>();
            //Projects = new HashSet<ProjectDto>();

        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        //public CategoryDto ParentCategory { get; set; }

        public ICollection<CategoryDto> SubCategories { get; set; }
        //public ICollection<ProjectDto> Projects { get; set; }

        public static implicit operator CategoryInfo(CategoryDto categoryDto)
        {
            if (categoryDto == null)
                return null;
            var info = new CategoryInfo
            {
                ID = categoryDto.ID,
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentCategoryId = categoryDto.ParentId,
                //ParentCategory = categoryDto.ParentCategory ?? null,
                //Projects = categoryDto.Projects?.Select(x => (ProjectInfo)x).ToList(),
                SubCategories = categoryDto.SubCategories?
                .Select(x => (CategoryInfo)x).ToList()
            };
            return info;
        }

        public static implicit operator CategoryDto(CategoryInfo categoryInfo)
        {
            if (categoryInfo == null)
                return null;
            var dto = new CategoryDto
            {
                ID = categoryInfo.ID,
                Name = categoryInfo.Name,
                Description = categoryInfo.Description,
                ParentId = categoryInfo.ParentCategoryId,
                //ParentCategory = categoryInfo.ParentCategory ?? null,
                //Projects = categoryInfo.Projects?.Select(x => (ProjectDto)x).ToList(),
                SubCategories = categoryInfo.SubCategories?
                .Select(x => (CategoryDto)x).ToList()
            };
            return dto;
        }
    }
}
