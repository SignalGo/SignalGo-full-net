using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        public override async Task<RunStatusType> Run(CancellationToken cancellationToken)
        {
            var result = await base.Run(cancellationToken);
            //var output = result.StartInfo;
            //Status = Models.RunStatusType.Done;
            //Status = Models.RunStatusType.Error;
            return result;
        }
    }
}
