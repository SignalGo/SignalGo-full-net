using System;

namespace SignalGo.Publisher.Models.Extra
{
    [Flags]
    public enum LogTypeEnum
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Compiler = 3,
        System = 4,
        Unknown = 5,

    }
}
