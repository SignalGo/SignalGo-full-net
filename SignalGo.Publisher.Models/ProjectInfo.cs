using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SignalGo.Publisher.Models
{
    public class ProjectInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public ProjectInfo()
        {
            IgnoreFiles = new HashSet<IgnoreFileInfo>();

        }

        public int ID { get; set; }
        public Guid ProjectKey { get; set; } = Guid.NewGuid();
        [Required, MaxLength(15, ErrorMessage = "Name Can't be greeter that 15 Characters")]
        public string Name { get; set; }
        [MaxLength(length: 512)]
        public string ProjectPath { get; set; }
        [MaxLength(length: 512)]
        public string ProjectAssembliesPath { get; set; }
        [MaxLength(length: 10)]
        public string LastUpdateDateTime { get; set; }
        public int? CategoryId { get; set; }

        public virtual CategoryInfo Category { get; set; }
        public virtual ICollection<IgnoreFileInfo> IgnoreFiles { get; set; }

    }
}
