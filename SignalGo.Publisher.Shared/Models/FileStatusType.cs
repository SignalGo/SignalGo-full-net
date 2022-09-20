using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Shared.DataTypes
{
    public enum FileStatusType : byte
    {
        /// <summary>
        /// File is unchanged
        /// </summary>
        Unchanged = 1,

        /// <summary>
        /// File is modified
        /// </summary>
        Modified = 2,

        /// <summary>
        /// File is a new one
        /// </summary>
        Added = 3,

        /// <summary>
        /// File has to be deleted
        /// </summary>
        Deleted = 4,

        /// <summary>
        /// File that has to be ignored
        /// </summary>
        Ignored = 5,
    }
}
