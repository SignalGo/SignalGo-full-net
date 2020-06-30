using SignalGo.Shared.Models;

namespace SignalGo.ServiceManager.Core.Models
{
    public class ServerDetailsInfo : NotifyPropertyChangedBase
    {
        public bool IsEnabled { get; set; }

        public string _ServiceMemoryUsage;
        public string ServiceMemoryUsage
        {
            get
            {
                return _ServiceMemoryUsage;
            }
            set
            {
                _ServiceMemoryUsage = value;
                OnPropertyChanged(nameof(ServiceMemoryUsage));
            }
        }
    }
}
