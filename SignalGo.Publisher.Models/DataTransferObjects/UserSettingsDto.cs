using SignalGo.Publisher.Models.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Publisher.Models.DataTransferObjects
{
    public class UserSettingsDto
    {
        public UserSettingsDto()
        {

        }
        public string Username { get; set; } = Environment.UserName;
        public string MasterPassword { get; set; }
        public bool UseUiVirtualization = true;
        public bool RunAuthenticateAtFirst = true;
        public string MsbuildPath { get; set; } = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe";
        public string TestRunnerPath { get; set; } = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe";
        public string LoggerPath { get; set; } = "AppLogs.log";
        public string CommandRunnerLogsPath { get; set; } = "CommandRunnerLogs.log";
        public string ServiceUpdaterLogFilePath { get; set; } = "ServiceUpdaterLog.log";
        public int MaxThreads { get; set; } = Environment.ProcessorCount;
        public PriorityTypes StartPriority { get; set; } = PriorityTypes.NORMAL;

        public CompileConfigurationTypes CompileConfigurationType { get; set; } = CompileConfigurationTypes.REBUILD;
        public PackageConfigurationTypes PackageConfigurationType { get; set; } = PackageConfigurationTypes.RESTORE;
        public CompileOutputTypes CompileOutputType { get; set; } = CompileOutputTypes.DEBUG;
        public TestRunnerTypes DefaultTestRunner { get; set; } = TestRunnerTypes.NetCoreSDK;
        public LoggingVerbosityTypes LoggingVerbosity { get; set; } = LoggingVerbosityTypes.Minimuum;

        public static implicit operator UserSettingsDto(UserSettingsInfo userSettingsInfo)
        {
            return new UserSettingsDto
            {
                Username = userSettingsInfo.Username,
                LoggerPath = userSettingsInfo.LoggerPath,
                CommandRunnerLogsPath = userSettingsInfo.CommandRunnerLogsPath,
                ServiceUpdaterLogFilePath = userSettingsInfo.ServiceUpdaterLogFilePath,
                LoggingVerbosity = userSettingsInfo.LoggingVerbosity,
                DefaultTestRunner = userSettingsInfo.DefaultTestRunner,
                MsbuildPath = userSettingsInfo.MsbuildPath,
                CompileOutputType = userSettingsInfo.CompileOutputType,
                CompileConfigurationType = userSettingsInfo.CompileConfigurationType,
                PackageConfigurationType = userSettingsInfo.PackageConfigurationType,
                MasterPassword = userSettingsInfo.MasterPassword,
                MaxThreads = userSettingsInfo.MaxThreads,
                RunAuthenticateAtFirst = userSettingsInfo.RunAuthenticateAtFirst,
                StartPriority = userSettingsInfo.StartPriority,
                TestRunnerPath = userSettingsInfo.TestRunnerPath,
                UseUiVirtualization = userSettingsInfo.UseUiVirtualization
            };
        }
        public static implicit operator UserSettingsInfo(UserSettingsDto userSettingsDto)
        {
            return new UserSettingsInfo
            {
                Username = userSettingsDto.Username,
                LoggerPath = userSettingsDto.LoggerPath,
                CommandRunnerLogsPath = userSettingsDto.CommandRunnerLogsPath,
                ServiceUpdaterLogFilePath = userSettingsDto.ServiceUpdaterLogFilePath,
                LoggingVerbosity = userSettingsDto.LoggingVerbosity,
                DefaultTestRunner = userSettingsDto.DefaultTestRunner,
                MsbuildPath = userSettingsDto.MsbuildPath,
                CompileOutputType = userSettingsDto.CompileOutputType,
                CompileConfigurationType = userSettingsDto.CompileConfigurationType,
                PackageConfigurationType = userSettingsDto.PackageConfigurationType,
                MasterPassword = userSettingsDto.MasterPassword,
                MaxThreads = userSettingsDto.MaxThreads,
                RunAuthenticateAtFirst = userSettingsDto.RunAuthenticateAtFirst,
                StartPriority = userSettingsDto.StartPriority,
                TestRunnerPath = userSettingsDto.TestRunnerPath,
                UseUiVirtualization = userSettingsDto.UseUiVirtualization
            };
        }
    }
}
