using SignalGo.Publisher.Engine.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Models;

namespace SignalGo.Publisher.Engines
{
    [ServiceContract("PublisherServerService", ServiceType.ClientService)]
    public interface IPublisherServer
    {
        //MessageContract<ClientPermissionMode> Login(string password);
        //MessageContract<List<FileCheckSum>> GetListOfChangesForDownload(List<FileCheckSum> lists);
        //MessageContract<List<FileCheckSum>> GetListOfChangesForUpload(List<FileCheckSum> lists);
        //MessageContract CommitChangesForDownload(List<FileCheckSum> lists);
        //MessageContract StartProcessBeforeUpload();
        //MessageContract StartProcessAfterUpload();

        StreamInfo DownloadFile(FileCheckSum checkSum, string password);
        void UploadFile(StreamInfo fileUpload);
    }
}
