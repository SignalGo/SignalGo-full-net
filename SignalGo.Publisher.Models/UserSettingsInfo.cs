using SignalGo.Publisher.Models.Shared.Types;
using System;

namespace SignalGo.Publisher.Models
{
    public class UserSettingsInfo
    {

        public UserSettingsInfo()
        {

        }

        public string Username { get; set; }
        public string MasterPassword { get; set; }
        public bool UseUiVirtualization = true;
        public bool RunAuthenticateAtFirst = true;
        public string MsbuildPath { get; set; }
        public string TestRunnerPath { get; set; }
        public string LoggerPath { get; set; }
        public string CommandRunnerLogsPath { get; set; }
        public string ServiceUpdaterLogFilePath { get; set; }
        public int MaxThreads { get; set; }
        public PriorityTypes StartPriority { get; set; } = PriorityTypes.NORMAL;

        public CompileConfigurationTypes CompileConfigurationType { get; set; } = CompileConfigurationTypes.REBUILD;
        public PackageConfigurationTypes PackageConfigurationType { get; set; } = PackageConfigurationTypes.RESTORE;
        public CompileOutputTypes CompileOutputType { get; set; } = CompileOutputTypes.DEBUG;
        public TestRunnerTypes DefaultTestRunner { get; set; } = TestRunnerTypes.NetCoreSDK;
        public LoggingVerbosityTypes LoggingVerbosity { get; set; } = LoggingVerbosityTypes.Minimuum;
    }
}
