using System;

namespace SignalGo.Shared.Models
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

        ///// <summary>
        ///// Service solutions files path
        ///// </summary>
        //public string ServicePath
        //{
        //    get
        //    {
        //        return _ServicePath;
        //    }
        //    set
        //    {
        //        _ServicePath = value;
        //    }
        //}
        ///// <summary>
        ///// Service assemblies(dll's and exe) path
        ///// </summary>
        //public string ServiceAssembliesPath
        //{
        //    get
        //    {
        //        return _ServiceAssembliesPath;
        //    }
        //    set
        //    {
        //        _ServiceAssembliesPath = value;
        //    }
        //}

        //public static explicit operator ServiceContract(Shared.Models.ProjectInfo projectInfo)
        //{
        //    return new ServiceContract()
        //    {
        //        Name = projectInfo.Name,
        //        ServiceAssembliesPath = projectInfo.ProjectAssembliesPath,
        //        ServiceKey = projectInfo.ProjectKey,
        //        ServicePath = projectInfo.ProjectPath
        //    };
        //}

    }
}
