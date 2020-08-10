using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models.Shared.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalgo.Publisher.Tests
{
    public class DataSeeder : TestBase
    {
        static DataSeeder()
        {

            TestIgnoreFilesList
                .AddRange(new List<IgnoreFileInfo>
                {
                    new IgnoreFileInfo
            {
                FileName="ConfigGo.json",
                IsEnabled=true,
                IgnoreFileType= IgnoreFileType.SERVER
            },
                    new IgnoreFileInfo
            {
                FileName="ServerIgnoreFile.txt",
                IsEnabled=false,
                IgnoreFileType= IgnoreFileType.SERVER
            },
                    new IgnoreFileInfo
            {
                FileName="TestClientFile",
                IsEnabled=true,
                IgnoreFileType= IgnoreFileType.CLIENT
            }
                });

        }

        [Test]
        public static async Task SeedDatabaseAsync()
        {
            using var dbContext = new PublisherDbContext(true);

            #region Seed Categories

            #region Categories Data

            TestCategoriesList.Clear();
            TestCategoriesList.AddRange(new List<CategoryInfo>
            {
                // category 1 with 2 level sub categories
                new CategoryInfo
                    {
                        Name = "CategoryTest",
                        Description = "Level 1 Parent Category",
                        SubCategories = new List<CategoryInfo>
                        {
                            new CategoryInfo
                            {
                                Name = "CategoryTest_SubCategory1",
                                Description = "Level 2 sub Category",
                                SubCategories = new List<CategoryInfo>
                                {
                                    new CategoryInfo
                                    {
                                        Name="CategoryTest_SubCategory1_SubCategory1",
                                        Description = "Level 3 sub Category child",
                                    },
                                    new CategoryInfo
                                    {
                                        Name="CategoryTest_SubCategory1_SubCategory2",
                                        Description = "Level 3 sub Category child",
                                    }
                                }
                            }
                        }
                    },
                new CategoryInfo
                {
                    Name = "Utravs",
                    Description = "Level 1 Parent Category",
                    SubCategories = new List<CategoryInfo>
                    {
                        new CategoryInfo
                        {
                            Name = "Flights",
                            Description = "Level 2 sub Category",
                            SubCategories = new List<CategoryInfo>
                            {
                                new CategoryInfo
                                {
                                    Name="SepehrFlight",
                                    Description = "Level 3 sub Category child",
                                },
                                new CategoryInfo
                                {
                                    Name="FaranegarFlight",
                                    Description = "Level 3 sub Category child",
                                }
                            }
                        },
                        new CategoryInfo
                        {
                            Name = "MicroServices",
                            SubCategories = new List<CategoryInfo>
                            {
                                new CategoryInfo
                                {
                                    Name="Logger"
                                },
                                new CategoryInfo
                                {
                                    Name="SMS"
                                }
                            }
                        }
                    }
                },
                new CategoryInfo
                {
                    Name = "Personal",
                    Description = "Level 1 Parent Category",
                    SubCategories = new List<CategoryInfo>
                    {
                        new CategoryInfo
                        {
                            Description = "Level 2 sub Category",
                            Name = "GitHub",
                            SubCategories = new List<CategoryInfo>
                            {
                                new CategoryInfo
                                {
                                    Name="SignalGo",
                                    Description = "Level 3 sub Category child",
                                },
                                new CategoryInfo
                                {
                                    Name="Publisher",
                                    Description = "Level 3 sub Category child",
                                }
                            }
                        }
                    }
                }
            });
            #endregion

            if (!await dbContext.CategoryInfos.AnyAsync())
            {
                await dbContext.CategoryInfos.AddRangeAsync(TestCategoriesList);
            }
            await dbContext.SaveChangesAsync();
            #endregion

            Assert.True(dbContext.CategoryInfos.ToListAsync().Result.Count == 15);

            #region Seed Projects

            #region Projects Data
            TestProjectsList.Clear();
            TestProjectsList.AddRange(new List<ProjectInfo>
                {
                    new ProjectInfo
                    {
                        //ID=10,
                        Name = "ProjectTest1_CategoryTest",
                        ProjectPath = "D\\DevOps\\ProjectTest1",
                        ProjectAssembliesPath = "D:\\DevOps\\ProjectTest1\\Utavs.Hub.ProjectTest1.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos.FirstOrDefault(x => x.Name == "CategoryTest"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="ServerIgnoreFile.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="TestClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        //ID=11,
                        Name = "ProjectTest1_CategoryTest_SubCategory_Child1",
                        ProjectPath = "D\\DevOps\\ProjectTest1",
                        ProjectAssembliesPath = "D:\\DevOps\\ProjectTest1\\Utavs.Hub.ProjectTest1.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos.FirstOrDefault(x => x.Name == "CategoryTest_SubCategory1_Child1"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="ServerIgnoreFile.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="TestClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        //ID=12,
                        Name = "ProjectTest2_CategoryTest_SubCategory",
                        ProjectPath = "D\\DevOps\\ProjectTest1",
                        ProjectAssembliesPath = "D:\\DevOps\\ProjectTest1\\Utavs.Hub.ProjectTest1.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos.FirstOrDefault(x => x.Name == "CategoryTest_SubCategory1"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="ServerIgnoreFile.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="TestClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        Name = "ProjectTest2_CategoryTest_SubCategory_Child1",
                        ProjectPath = "D\\DevOps\\ProjectTest2",
                        ProjectAssembliesPath = "D:\\DevOps\\ProjectTest2\\Utavs.Hub.ProjectTest2.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos.FirstOrDefault(x => x.Name == "CategoryTest_SubCategory1_Child1"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="ServerIgnoreFile.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="TestClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        Name = "Logger",
                        ProjectPath = "D\\DevOps\\Logger",
                        ProjectAssembliesPath = "D:\\DevOps\\Logger\\Utavs.Hub.Logger.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos.FirstOrDefault(x => x.Name == "MicroServices"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
    },
                            new IgnoreFileInfo
                            {
                                FileName="ServerIgnoreFile.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
},
                            new IgnoreFileInfo
                            {
                                FileName="LoggerClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        Name = "Storage",
                        ProjectPath = "D\\DevOps\\Storage",
                        ProjectAssembliesPath = "D:\\DevOps\\Logger\\Utavs.Hub.StorageManagement.ConsoleApp\\bin\\Debug\\netcoreapp3.1\\",
                        Category = dbContext.CategoryInfos
                        .FirstOrDefault(x => x.Name == "MicroServices"),
                        IgnoreFiles = new List<IgnoreFileInfo>
                        {
                            new IgnoreFileInfo
                            {
                                FileName="ConfigGo.json",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="IgnoreFile2.txt",
                                IsEnabled=false,
                                IgnoreFileType= IgnoreFileType.SERVER
                            },
                            new IgnoreFileInfo
                            {
                                FileName="StorageClientFile",
                                IsEnabled=true,
                                IgnoreFileType= IgnoreFileType.CLIENT
                            }
                        }
                    },
                    new ProjectInfo
                    {
                        Name = "Flights",
                        ProjectPath = "D\\DevOps\\Flights",
                        //ProjectAssembliesPath = "D:\\DevOps\\Flights\\Utavs.Hub.Flights.ConsoleApp\\bin\\Debug\\netcoreapp3.1",
                        Category = dbContext.CategoryInfos
                        .FirstOrDefault(x => x.Name == "Flights"),
                    }
                });
            #endregion
            if (!await dbContext.ProjectInfos.AnyAsync())
            {
                await dbContext.ProjectInfos.AddRangeAsync(TestProjectsList);
            }
            dbContext.SaveChanges();
            #endregion

            Assert.True(await dbContext.ProjectInfos.CountAsync() == 7);
            #region Seed Settings
            #region Seed Settings Data
            UserSettingsList.Clear();
            UserSettingsList.AddRange(new List<UserSettingsInfo>
            {
                new UserSettingsDto(),
                new UserSettingsDto
                {
                    Username = "Test",
                }
            });
            #endregion
            if (!await dbContext.UserSettingsInfos.AnyAsync())
            {
                await dbContext.UserSettingsInfos
                    .AddRangeAsync(UserSettingsList
                    .Select(x => (UserSettingsInfo)x).ToList());
            }
            await dbContext.SaveChangesAsync();
            Assert.True(await dbContext.UserSettingsInfos.CountAsync() == 2);
            #endregion
        }
    }
}