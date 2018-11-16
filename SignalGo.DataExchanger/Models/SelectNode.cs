using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.DataExchanger.Models
{
    /// <summary>
    /// select node is a node of tree selection of select query that will start from '{' and end to '}' character
    /// </summary>
    public class SelectNode
    {
        /// <summary>
        /// list of properties of node
        /// </summary>
        public Dictionary<string, List<SelectNode>> Properties { get; set; } = new Dictionary<string, List<SelectNode>>();
        /// <summary>
        /// parent of node
        /// </summary>
        public SelectNode Parent { get; set; }
    }
}
