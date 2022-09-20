using SignalGo.Publisher.Shared.Helpers;
using SignalGo.Publisher.Shared.Models;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignalGo.ServiceManager.Core.Services
{
    [ServiceContract("FileManager", ServiceType.ServerService, InstanceType.SingleInstance)]
    //[ServiceContract("FileManager", ServiceType.HttpService, InstanceType.SingleInstance)]
    public class FileManagerService
    {
        /// <summary>
        /// get list of files
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public List<string> GetTextFiles(Guid serviceKey)
        {
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (find == null)
                throw new Exception($"Service {serviceKey} not found!");

            var directory = Path.GetDirectoryName(find.AssemblyPath);

            return Directory.GetFiles(directory).Where(x =>
            {
                var extension = Path.GetExtension(x).ToLower();
                if (extension == ".txt" || extension == ".json")
                    return true;
                return false;
            }).ToList();
        }
        /// <summary>
        /// Returns file hashes of the specified microservice's assembly path.
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<HashedFileDto> CalculateFileHashes(Guid serviceKey)
        {
            var server = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (server == null)
                throw new Exception($"Service {serviceKey} not found!");

            return FileHelper.CalculateFileHashesInDirectory(server.AssemblyPath);
        }

        public ServiceInspectionDto InspectServerMicroservice(Guid serviceKey)
        {
            var result = new ServiceInspectionDto();

            var microservice = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            if (microservice == null)
                return result;

            //
            result.MarkAsExist();
            result.ServiceKey = serviceKey;
            result.RemoteServerKey = Guid.Empty;//we don't know the server on which we are trying to update services.
            result.FileHashes = FileHelper.CalculateFileHashesInDirectory(microservice.AssemblyPath);

            return result;
        }
    }
}
