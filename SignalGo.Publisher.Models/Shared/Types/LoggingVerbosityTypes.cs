using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Models.Shared.Types
{

    [Flags]
    public enum LoggingVerbosityTypes
    {
        Full = 1,
        Minimuum = 2,
        Quiet = 3
    }
}
