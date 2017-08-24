using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.DataTypes
{
    public enum MainStructureEnum : byte
    {
        MainClass = 0,
        MainMethod = 1,
        DisposeMethod = 2,
        ServerObject = 3,
    }
    /// <summary>
    /// main class and main method that use for SignalGo ServerManager
    /// </summary>
    public class MainStructureAttribute : Attribute
    {
        public MainStructureAttribute(MainStructureEnum mode)
        {
            Mode = mode;
        }

        public MainStructureEnum Mode { get; set; }
    }
}
