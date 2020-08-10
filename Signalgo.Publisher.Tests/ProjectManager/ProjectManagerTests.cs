using NUnit.Framework;
using SignalGo.Publisher.Core.Extensions;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models.Shared.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalgo.Publisher.Tests.ProjectManager
{
    public class ProjectManagerTests : PublisherProjectManagerBase
    {
        public ProjectManagerTests() : base()
        {

        }

        [Test]
        public void GetAllProjectsAsEnumerableAsync()
        {


        }
        [Test]
        public async Task GetAllProjectsInfo()
        {
            var allProjects = await _projectManager.GetAllProjectsAsync();
            foreach (var item in allProjects)
            {
                var projectServerIgnoreFiles = item.IgnoreFiles
                    .Where(type => type.IgnoreFileType == IgnoreFileType.SERVER)
                    .ToList();
                var projectClientIgnoreFiles = item.IgnoreFiles
                    .Where(type => type.IgnoreFileType == IgnoreFileType.CLIENT)
                    .ToList();
            }
        }

        [Test]
        public async Task GetProjectAsync()
        {
            // ProjectTest1_CategoryTest_SubCategory_Child1
            ProjectDto project = await _projectManager
                .GetProjectAsync(TestProjectsList.ElementAt(1).Name);

            //Assert.False(project == null || project.Category
            //    .ParentCategory.ParentCategory
            //    .Name != TestCategoriesList.ElementAt(0).Name);

            Assert.True(project != null ||
                project.Name == "ProjectTest1_CategoryTest_SubCategory_Child1");

        }

        [Test]
        public async Task RemoveAllProjectIgnoreFiles()
        {
            // get a test project from categories
            var project = await _projectManager.GetProjectAsync(TestProjectsList[0]);
            Assert.False(project == null, "Could'nt find test project");

            await _projectManager.RemoveProjectIgnoreFilesAsync(project);

            // check if project ignore files was deleted successfully and project ignore file's are empty
            var allProjects = await _projectManager.GetAllProjectsAsync();
            // ProjectTest1
            Assert.False(allProjects
                .FirstOrDefault(prj => prj.Name == TestProjectsList[0].Name)
                .IgnoreFiles
                .Any());

        }
        [Test]
        public async Task RemoveProjectIgnoreFilesAsync()
        {
            //var project = await _projectManager.GetProjectByNameAsync(TestProjectsList[0].Name);
            var ig = TestProjectsList[0]
                .IgnoreFiles.Select(x => (IgnoreFileDto)x)
                .ToList();
            var result = await _projectManager
                .RemoveProjectIgnoreFilesAsync(TestProjectsList[0],
                ig.Take(2).ToList());

            Assert.True(await _projectManager.GetIgnoreFileAsync(ig[0]) == null);
            Assert.True(await _projectManager.GetIgnoreFileAsync(ig[1]) == null);
        }

        [Test]
        public async Task RemoveProjectIgnoreFile()
        {

            await _projectManager
                .RemoveProjectIgnoreFilesAsync(TestProjectsList[0], TestProjectsList[0].IgnoreFiles.ElementAt(0));
            Assert.True(await _projectManager
                .GetIgnoreFileAsync(TestProjectsList[0], TestProjectsList[0].IgnoreFiles.ElementAt(0)) == null);
        }

        [Test]
        public async Task GetIgnoreFileByTypeAsync()
        {
            var find = await _projectManager
                .GetIgnoreFilesAsync(TestProjectsList[0], IgnoreFileType.SERVER);
            Assert.True(find[0].FileName == "ConfigGo.json");
            Assert.True(find[1].FileName == "ServerIgnoreFile.txt");
        }
        [Test]
        public async Task GetIgnoreFileAsync()
        {
            var find = await _projectManager
                .GetIgnoreFileAsync(TestProjectsList[0], TestProjectsList[0].IgnoreFiles.ElementAt(0));
            Assert.True(find.FileName == "ConfigGo.json");
        }
        [Test]
        public async Task GetProjectIgnoreFilesAsync()
        {

            var ignoreFiles = await _projectManager
                .GetIgnoreFilesAsync(TestProjectsList[0]);
            Assert.True(ignoreFiles != null, "can't find test project");

            var projectServerIgnoreFiles = ignoreFiles
                .Where(type => type.IgnoreFileType == IgnoreFileType.SERVER)
                .ToList();
            Assert.True(projectServerIgnoreFiles.Count == 2);

            var projectClientIgnoreFiles = ignoreFiles
                .Where(type => type.IgnoreFileType == IgnoreFileType.CLIENT)
                .ToList();
            Assert.True(projectClientIgnoreFiles.Count == 1);

            var allProjectIgnoreFiles = ignoreFiles.ToList();
            Assert.True(allProjectIgnoreFiles.Count == 3);

        }

        [Test]
        public async Task AddOrUpdateProject()
        {
            // add new project
            var newProject = new ProjectDto
            {
                Name = "New TestProject",
                ProjectPath = "NewProjectPath\\AbsoloutPath",
                ProjectKey = Guid.NewGuid(),
                IgnoreFiles = new List<IgnoreFileDto>
                {
                    new IgnoreFileDto
                    {
                        FileName = "IgnoreFileServer",
                        IgnoreFileType = IgnoreFileType.SERVER
                    },
                    new IgnoreFileDto
                    {
                        FileName = "IgnoreFileClient",
                        IgnoreFileType = IgnoreFileType.SERVER,
                        IsEnabled= false
                    }
                }
            };
            newProject = await _projectManager.AddOrUpdateProjectAsync(newProject);
            Assert.True(newProject.IsEntityValidAndExist());

            // update existing project
            ProjectDto projectToUpdate = await _projectManager.GetProjectAsync(newProject);
            Assert.False(projectToUpdate == null);

            CategoryDto newCategory = await _categoryManager
                .GetCategoryAsync("CategoryTest_SubCategory1");
            projectToUpdate.Name = "updated new project test 1";
            projectToUpdate.LastUpdateDateTime = DateTime.Now.ToString();
            projectToUpdate.IgnoreFiles.Remove(projectToUpdate.IgnoreFiles.ElementAt(0));
            projectToUpdate.Category = newCategory;

            ProjectDto updatedProject = await _projectManager
                .AddOrUpdateProjectAsync(projectToUpdate);
            Assert.True(updatedProject.IsEntityValidAndExist());
            Assert.True(updatedProject.IgnoreFiles.Count == 1);


        }
    }
}
