using NUnit.Framework;
using SignalGo.Publisher.Core.Engines.Publisher;
using SignalGo.Publisher.Core.Engines.Publisher.Interfaces;
using SignalGo.Publisher.Models.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signalgo.Publisher.Tests.Publisher.SettingManager
{
    class SettingManagerTests : TestBase
    {
        private ISettingManager _settingManager;
        public SettingManagerTests() : base()
        {
            _settingManager = new SettingManagerModule();
        }

        [Test]
        public async Task GetAllSettings()
        {
            List<UserSettingsDto> settings = await _settingManager
                .GetSettings();

            Assert.True(settings.Count == 2);
            Assert.True(settings.Any(u => u.Username == "Test"));
            Assert.True(settings.Any(u => u.Username == Environment.UserName));
        }
        [Test]
        public async Task GetUserSettings()
        {
            string userName = Environment.UserName;
            UserSettingsDto settings = await _settingManager
                .GetSettings(userName);

            Assert.True(settings.Username == Environment.UserName);
        }

    }
}
