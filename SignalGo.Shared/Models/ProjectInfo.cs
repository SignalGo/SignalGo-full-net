using System;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// project info shared model
    /// </summary>
    public class ProjectInfo
    {

        private string _Name;
        private Guid _ProjectKey;
        private string _ProjectPath;
        private string _ProjectAssembliesPath;

        /// <summary>
        /// projet name
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
        /// unique key of project
        /// </summary>
        public Guid ProjectKey
        {
            get
            {
                if (_ProjectKey != Guid.Empty)
                {
                    return _ProjectKey;
                }
                else
                {
                    _ProjectKey = Guid.NewGuid();
                    return _ProjectKey;
                }
            }
            set
            {
                _ProjectKey = value;
            }
        }

        /// <summary>
        /// project solutions files path
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return _ProjectPath;
            }
            set
            {
                _ProjectPath = value;
            }
        }

        /// <summary>
        /// project assemblies(dll's and exe) path
        /// </summary>
        public string ProjectAssembliesPath
        {
            get
            {
                return _ProjectAssembliesPath;
            }
            set
            {
                _ProjectAssembliesPath = value;
            }
        }

    }
}
