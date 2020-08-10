using NUnit.Framework;
using Signalgo.Publisher.Tests.ProjectManager;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalgo.Publisher.Tests
{
    public class TestBase
    {
        public TestBase() : base()
        {

        }
        /// <summary>
        /// for run all test's with single database initialize
        /// </summary>
        static TestBase()
        {
            //using var dbContext = new PublisherDbContext(true);
            //dbContext.Database.EnsureDeleted();
            //dbContext.Database.EnsureCreated();

            //DataSeeder.SeedDatabaseAsync().Wait();
        }

        [SetUp]
        public async Task Setup()
        {
            using var dbContext = new PublisherDbContext(true);
            // remove old database and data
            await dbContext.Database.EnsureDeletedAsync();
            // create new/empty database
            await dbContext.Database.EnsureCreatedAsync();
            await DataSeeder.SeedDatabaseAsync();
        }


        public async void RemoveDatabase()
        {
            using var dbContext = new PublisherDbContext(true);
            await dbContext.Database.EnsureDeletedAsync();
        }

        public async void HoldOn()
        {
            await Task.Delay(1000000);
        }

        #region Test Data Collections (Used In Data Seeder)

        protected static List<IgnoreFileInfo> TestIgnoreFilesList = new List<IgnoreFileInfo>();
        protected static List<CategoryInfo> TestCategoriesList = new List<CategoryInfo>();
        protected static List<ProjectInfo> TestProjectsList = new List<ProjectInfo>();
        protected static List<UserSettingsInfo> UserSettingsList = new List<UserSettingsInfo>();


        #endregion
    }
}
