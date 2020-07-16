using System.Collections.Generic;

namespace SignalGo.Publisher.Models
{
    /// <summary>
    /// Project's Category Model
    /// </summary>
    public class CategoryInfo
    {
        public CategoryInfo()
        {
            SubCategories = new HashSet<CategoryInfo>();

        }
        /// <summary>
        /// category id
        /// </summary>
        public int ID { get; set; }
        //public Guid ID { get; set; } = Guid.NewGuid();
        /// <summary>
        /// category name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// parent/top level category id
        /// </summary>
        public int? ParentID { get; set; }
        /// <summary>
        /// list of sub categories related to this category id
        /// </summary>
        public virtual ICollection<CategoryInfo> SubCategories { get; set; }
        //public virtual CategoryInfo ParentCategory { get; set; }
    }
}
