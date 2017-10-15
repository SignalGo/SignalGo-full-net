using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class ModelDetailsInfo
    {
        /// <summary>
        /// id of class
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// name of model
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// name and namce space of class
        /// </summary>
        public string FullNameSpace { get; set; }
        /// <summary>
        /// comment of class
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// json template of model
        /// </summary>
        public string JsonTemplate { get; set; }
    }
}
