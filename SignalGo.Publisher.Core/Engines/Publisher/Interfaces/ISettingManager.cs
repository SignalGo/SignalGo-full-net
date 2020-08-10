using SignalGo.Publisher.Models.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Core.Engines.Publisher.Interfaces
{
    public interface ISettingManager
    {

        public Task<List<UserSettingsDto>> GetSettings();
        public Task<UserSettingsDto> GetSettings(string userName);
        public Task<UserSettingsDto> SetSettings(UserSettingsDto userSettingsDto);

    }
}
