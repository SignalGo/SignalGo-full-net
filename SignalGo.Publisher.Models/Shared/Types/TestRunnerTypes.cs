using System;

namespace SignalGo.Publisher.Models.Shared.Types
{
    [Flags]
    public enum TestRunnerTypes
    {
        NetCoreSDK,
        VsTestConsole,
        UserDefined,
    }
}
