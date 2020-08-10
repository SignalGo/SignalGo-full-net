using Microsoft.EntityFrameworkCore;
using SignalGo.Publisher.Core.Engines.Publisher.Interfaces;
using SignalGo.Publisher.Core.Extensions;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Models.DataTransferObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Core.Engines.Publisher
{
    public class SettingManagerModule : ISettingManager
    {
        public SettingManagerModule()
        {

        }

        public async Task<List<UserSettingsDto>> GetSettings()
        {
            using var dbContext = new PublisherDbContext();
            List<UserSettingsInfo> allSettings = await dbContext.UserSettingsInfos
                .ToListAsync();

            List<UserSettingsDto> settings = new List<UserSettingsDto>();
            settings.AddRange(allSettings.Select(x => (UserSettingsDto)x).ToList());

            return settings;
        }
        public async Task<UserSettingsDto> GetSettings(string userName)
        {
            if (!userName.IsValid())
                return null;
            using var dbContext = new PublisherDbContext();
            UserSettingsDto settings = await dbContext.UserSettingsInfos
                .FirstOrDefaultAsync(u=>u.Username == userName);

            return settings;
        }
        public async Task<UserSettingsDto> SetSettings(UserSettingsDto userSettingsDto)
        {
            using var dbContext = new PublisherDbContext();
            var settings = dbContext.UserSettingsInfos
                .Update(userSettingsDto);
            await dbContext.SaveChangesAsync();

            return settings.Entity;
        }
    }
}
