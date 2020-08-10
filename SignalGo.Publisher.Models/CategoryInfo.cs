using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SignalGo.Publisher.Models
{
    public class CategoryInfo
    {
        /// <summary>
        /// Category data model
        /// </summary>
        public CategoryInfo()
        {
            SubCategories = new HashSet<CategoryInfo>();
            Projects = new HashSet<ProjectInfo>();

        }

        public int ID { get; set; }
        /// <summary>
        /// Category Nickname
        /// </summary>
        public string Name { get; set; }
        [MaxLength(300)]
        public string Description { get; set; }
        /// <summary>
        /// parent category id that contain this category as child.
        /// </summary>
        public int? ParentCategoryId { get; set; }
        /// <summary>
        /// navigation to parent category
        /// </summary>
        public virtual CategoryInfo ParentCategory { get; set; }


        /// <summary>
        /// list of child categories in the category
        /// </summary>
        public virtual ICollection<CategoryInfo> SubCategories { get; set; }
        /// <summary>
        /// contain a list of project's that assigned to to this category
        /// </summary>
        public virtual ICollection<ProjectInfo> Projects { get; set; }
    }
}
