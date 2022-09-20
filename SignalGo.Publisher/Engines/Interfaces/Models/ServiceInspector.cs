using SignalGo.Publisher.Models;
using SignalGo.Publisher.Services;
using SignalGo.Publisher.Shared.Models;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Engines.Interfaces.Models
{
    public class ServiceInspector
    {
        private readonly Guid _projectKey;
        private readonly List<ServerInfo> _servers;
        private readonly List<Guid> _serviceKeys;
        private readonly string _serviceName;

        public ServiceInspector(Guid projectKey, List<Guid> serviceKeys, string serviceName)
        {
            _projectKey = projectKey;
            _servers = ServerSettingInfo.CurrentServer.ServerInfo.Where(x => x.IsChecked).ToList();
            _serviceKeys = serviceKeys;
            _serviceName = serviceName;
        }

        /// <summary>
        /// Returns a list of inspections for all services within the selected servers, containing the information about files on each server by comparing their files with origin ones.
        /// </summary>
        /// <param name="publishDir"></param>
        /// <param name="originHashes"></param>
        /// <returns></returns>
        public async Task<List<ServiceInspectionDto>> Inspect(string publishDir, List<HashedFileDto> originHashes)
        {
            //ServiceInspectionDto inspection;
            var result = new List<ServiceInspectionDto>();

            foreach (var server in _servers)
            {
                var provider = await PublisherServiceProvider.Initialize(server, _serviceName);
                //if (!provider.HasValue())
                //    return RunStatusType.Error;

                foreach (var serviceKey in _serviceKeys)
                {
                    var inspection = provider.FileManagerService.InspectServerMicroservice(serviceKey);
                    if (inspection.IsExist)// if microservice is exist on the server
                    {
                        inspection.ComparedHashes = CompareFileHashes(originHashes, inspection.FileHashes);
                        inspection.ArchivePath = Path.Combine(publishDir, $"{_serviceName}_{server.ServerKey}_{serviceKey}.zip");
                        //applying ignored files of the selected project to all target services
                        SettingInfo.Current.ProjectInfo?.FirstOrDefault(p => p.ProjectKey == _projectKey).ServerIgnoredFiles.ToList()
                            .ForEach(x =>
                            {
                                inspection.ComparedHashes.FirstOrDefault(y => y.FileName.Equals(x.FileName))?.MarkAsIgnored();
                            });
                        result.Add(inspection);
                        //inspection = null;
                    }
                }
            }

            return result;
        }

        #region Utilities
        /// <summary>
        /// Sets the status of each file in origin collection based on filename and filehash.
        /// </summary>
        /// <param name="originHashes"></param>
        /// <param name="destinationHashes"></param>
        /// <returns></returns>
        private List<HashedFileDto> CompareFileHashes(List<HashedFileDto> originHashes, List<HashedFileDto> destinationHashes)
        {
            var result = new List<HashedFileDto>();
            var destination = default(HashedFileDto);

            foreach (var origin in originHashes)
            {
                var x = origin.Clone();

                destination = destinationHashes.FirstOrDefault(x => x.FileName.Equals(origin.FileName));
                if (destination == null)
                    x.MarkAsAdded();
                else
                {
                    if (x.FileHash.Equals(destination.FileHash))
                        x.MarkAsUnchanged();
                    else
                        x.MarkAsModified();
                }

                result.Add(x);
            }

            //finding files that have to be deleted
            destinationHashes.Where(x => !originHashes.Any(y => x.FileName.Equals(y.FileName))).ToList()
                .ForEach(z =>
                {
                    z.MarkAsDeleted();
                    result.Add(z);
                });

            return result;
        }
        #endregion
    }
}
