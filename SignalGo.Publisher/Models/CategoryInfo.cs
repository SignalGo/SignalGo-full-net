using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public class CategoryInfo
    {
        public string Name { get; set; }
        public CategoryInfo Parent { get; set; }
        public List<CategoryInfo> Categories { get; set; }
        public List<ProjectInfo> Projects { get; set; }
    }
}
