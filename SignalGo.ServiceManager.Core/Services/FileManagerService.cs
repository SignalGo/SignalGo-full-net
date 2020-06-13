using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignalGo.ServiceManager.Core.Services
{
    [ServiceContract("FileManager", ServiceType.ServerService, InstanceType.SingleInstance)]
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
    }
}
