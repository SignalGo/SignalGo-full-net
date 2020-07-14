using MvvmGo.ViewModels;
using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SignalGo.Publisher.Engines.Commands
{
    public class TestsCommandInfo : CommandBaseInfo
    {
        public TestsCommandInfo()
        {
            Name = "run tests";
            ExecutableFile = "cmd.exe";
            //Command = $"{UserSettingInfo.Current.UserSettings.TestRunnerExecutableFile}";
            Command = "dotnet";
            Arguments = "test --nologo --no-build --logger console;verbosity=detailed -v q";
            IsEnabled = true;
        }
        public override async Task<RunStatusType> Run(CancellationToken cancellationToken,string caller)
        {
            var result = await base.Run(cancellationToken,caller);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
        public ObservableCollection<TestInfo> Tests { get; set; } = new ObservableCollection<TestInfo>();
        public override async Task Initialize(ProcessStartInfo processStartInfo)
        {
            var tests = await LoadTestsCount(WorkingPath);
            RunOnUIAction(() =>
            {
                Tests.Clear();
                foreach (var item in tests)
                {
                    Tests.Add(item);
                }
            });

            Size = tests.Count;

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.FileName = $"{ExecutableFile}";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Arguments = $"/c {Command} {Arguments}";
            processStartInfo.WorkingDirectory = WorkingPath;
        }

        public override bool CalculateStatus(string line)
        {
            if (line.TrimStart().StartsWith("Failed:"))
            {
                Status = RunStatusType.Error;
                return true;
            }
            bool isPass = line.TrimStart().StartsWith("û");
            bool isError = line.TrimStart().StartsWith("X");
            if (isPass || isError)
            {
                var name = line.Trim().Split(' ')[1];
                var find = Tests.FirstOrDefault(x => x.Name == name);
                if (find != null)
                {
                    find.Status = isPass ? TestStatus.Pass : TestStatus.Error;
                }
                Position++;
            }
            return false;
        }


        static async Task<List<TestInfo>> LoadTestsCount(string path)
        {
            var process = new Process();
            List<TestInfo> tests = new List<TestInfo>();
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                FileName = $"cmd.exe",
                CreateNoWindow = true,
                Arguments = $"/c dotnet test -t --no-build -v q --nologo",
                WorkingDirectory = path
            };
            try
            {
                process = Process.Start(processInfo);
                bool isTestsFound = false;
                var standardOutputResult = string.Empty;
                while (true)
                {
                    standardOutputResult = await process.StandardOutput.ReadLineAsync();
                    if (standardOutputResult == null)
                        break;
                    else
                    {
                        if (isTestsFound)
                            tests.Add(new TestInfo() { Name = standardOutputResult.Trim() });
                        if (!isTestsFound && standardOutputResult.Contains("Tests are available:"))
                            isTestsFound = true;
                    }
                }
            }
            catch (Exception e)
            {
                AutoLogger.Default.LogError(e, "load tests count");
            }
            finally
            {
                process.Dispose();
            }
            return tests;
        }

    }
}
