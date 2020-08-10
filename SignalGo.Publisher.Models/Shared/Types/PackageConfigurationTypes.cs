using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Models.Shared.Types
{
    [Flags]
    public enum PackageConfigurationTypes
    {
        RESTORE,
        NORESTORE,
        UPDATE,

    }
}
