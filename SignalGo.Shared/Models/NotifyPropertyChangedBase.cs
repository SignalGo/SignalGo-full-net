using System.ComponentModel;

namespace SignalGo.Shared.Models
{
    /// <summary>
    /// base of property changing helper
    /// </summary>
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// notify property changed to ui
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
