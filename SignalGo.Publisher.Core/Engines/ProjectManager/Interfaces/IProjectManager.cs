using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalGo.Publisher.Models.Shared.Types;

namespace SignalGo.Publisher.Core.Engines.Interfaces.ProjectManager
{
    public interface IProjectManager
    {
        public IAsyncEnumerable<ProjectInfo> GetAllProjectsAsAsyncEnumerable();
        public Task<List<ProjectDto>> GetAllProjectsAsync();
        public Task<ProjectDto> GetProjectAsync(ProjectDto projectDto);
        public Task<ProjectDto> GetProjectAsync(string name);
        public Task<ProjectDto> GetProjectAsync(int id);

        public Task<List<IgnoreFileDto>> GetIgnoreFilesAsync(ProjectDto project, IgnoreFileType ignoreFileType);
        public Task<List<IgnoreFileDto>> GetIgnoreFilesAsync(ProjectDto project);
        public Task<IgnoreFileDto> GetIgnoreFileAsync(ProjectDto project, IgnoreFileDto ignoreFile);
        public Task<IgnoreFileDto> GetIgnoreFileAsync(IgnoreFileDto ignoreFile);

        public Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project, IgnoreFileDto ignoreFile);
        public Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project);
        public Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project, List<IgnoreFileDto> ignoreFile);

        public Task<ProjectDto> AddOrUpdateProjectAsync(ProjectDto projectDto);



    }
}