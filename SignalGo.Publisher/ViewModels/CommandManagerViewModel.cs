using System;
using System.IO;
using System.Linq;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System.Windows.Forms;
using SignalGo.Publisher.Models;

namespace SignalGo.Publisher.ViewModels
{
    public class CommandManagerViewModel : BaseViewModel
    {

        public CommandManagerViewModel()
        {
            SaveCommandsCommand = new Command(SaveChanges);
            AddCommandCommand = new Command(AddCommand);

        }
        public Command SaveCommandsCommand { get; set; }
        public Command AddCommandCommand { get; set; }

        /// <summary>
        /// Save Changes In Command Manager
        /// </summary>
        private void AddCommand()
        {
            CurrentCommandSettingInfo.CommandSettings.Add(CommandSetting);
            CommandSetting = new CommandSetting();
            SaveChanges();
        }
        private void SaveChanges()
        {
            CommandSettingInfo.SaveCommandSettingInfo();
        }

        CommandSetting _CommandSetting = new CommandSetting();

        public CommandSetting CommandSetting
        {
            get
            {
                return _CommandSetting;
            }
            set
            {
                _CommandSetting = value;
                OnPropertyChanged(nameof(CommandSetting));
            }
        }
        public CommandSettingInfo CurrentCommandSettingInfo
        {
            get
            {
                return CommandSettingInfo.Current;
            }
        }

    }
}
