using System;

namespace SignalGo.Publisher.Models.Shared.Types
{
    [Flags]
    public enum CompilePlatformTypes
    {
        PORTABLE,
        WIN_64,
        WIN_32,
        WIN_ARM,
        LINUX_64,
        LINUX_32,
        LINUX_ARM,
        OSX_64,
    }
}
