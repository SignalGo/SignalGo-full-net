using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;

namespace SignalGo.Publisher.Shared.Models
{
    /// <summary>
    /// information about service
    /// </summary>
    public class ServiceContract
    {
        /// <summary>
        /// 
        /// </summary>
        public ServiceContract()
        {

        }

        private Guid _ServiceKey;
        private string _Name;
        private string _ServiceAssembliesPath;
        private List<IgnoreFileDto> _IgnoreFiles;

        /// <summary>
        /// name of service
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// unique key of Service
        /// </summary>
        public Guid ServiceKey
        {
            get
            {
                return _ServiceKey;
            }
            set
            {
                _ServiceKey = value;
            }
        }

        /// <summary>
        /// Service assemblies(dll's and executables) path
        /// </summary>
        public string ServiceAssembliesPath
        {
            get
            {
                return _ServiceAssembliesPath;
            }
            set
            {
                _ServiceAssembliesPath = value;
            }
        }
        /// <summary>
        /// files to ignore in service updates
        /// </summary>
        public List<IgnoreFileDto> IgnoreFiles
        {
            get
            {
                return _IgnoreFiles;
            }
            set
            {
                _IgnoreFiles = value;
            }
        }

        public CompressArchiveDto CompressArchive { get; set; }
    }
}
