using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SignalGo.Publisher.Core.Engines.Interfaces.ProjectManager;
using SignalGo.Publisher.Core.Extensions;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalGo.Publisher.Models.Shared.Types;

namespace SignalGo.Publisher.Core.Engines.ProjectManager
{
    /// <summary>
    /// Manage Project's and their ignore files in database
    /// </summary>
    public class ProjectManagerModule : CrudBase, IProjectManager
    {

        public ProjectManagerModule()
        {

        }
        public async Task<List<ProjectDto>> GetAllProjectsAsync()
        {
            using var dbContext = new PublisherDbContext();
            List<ProjectInfo> allProjects = await dbContext.ProjectInfos
                .Include(x => x.Category)
                .Include(y => y.IgnoreFiles)
                .ToListAsync();
            List<ProjectDto> projects = new List<ProjectDto>();
            projects.AddRange(allProjects.Select(x => (ProjectDto)x).ToList());

            return projects;
        }

        // TODO: Complete Yieldable Get Query
        public async IAsyncEnumerable<ProjectInfo> GetAllProjectsAsAsyncEnumerable()
        {
            using var dbContext = new PublisherDbContext();
            yield return (ProjectInfo)dbContext.ProjectInfos
                .Include(x => x.Category)
                .Include(y => y.IgnoreFiles)
                .AsAsyncEnumerable();
            await Task.Delay(100);

        }

        public async Task<ProjectDto> GetProjectAsync(ProjectDto projectDto)
        {
            if (projectDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                ProjectDto result = await dbContext.ProjectInfos
                    .FirstOrDefaultAsync(x => x.ID == projectDto.ID);

                return result;
            }
            else
                return null;
        }
        public async Task<ProjectDto> GetProjectAsync(string projectName)
        {
            using var dbContext = new PublisherDbContext();
            ProjectDto result = await dbContext.ProjectInfos
                .FirstOrDefaultAsync(x => x.Name == projectName);

            return result;
        }
        public async Task<ProjectDto> GetProjectAsync(int projectId)
        {
            if (!projectId.IsValid())
                return null;
            using var dbContext = new PublisherDbContext();
            ProjectDto result = await dbContext.ProjectInfos
                .FirstOrDefaultAsync(x => x.ID == projectId);

            return result;
        }
        public async Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project)
        {
            if (project.IsEntityValidAndExist())
            {
                using var dbContext = new PublisherDbContext();
                List<IgnoreFileInfo> find = dbContext.IgnoreFileInfos
                    .Where(f => f.ProjectId == project.ID).ToList();

                dbContext.IgnoreFileInfos.RemoveRange(find);
                await dbContext.SaveChangesAsync();
                return true;
            }
            else
                return false;
        }
        public async Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project, IgnoreFileDto ignoreFile)
        {
            if (!project.IsEntityValidAndExist())
                return false;
            using var dbContext = new PublisherDbContext();

            IgnoreFileInfo find = await dbContext.IgnoreFileInfos
                .Where(p => p.ProjectId == project.ID)
                .FirstOrDefaultAsync(i => i.ID == ignoreFile.ID);

            dbContext.IgnoreFileInfos.Remove(find);
            await dbContext.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RemoveProjectIgnoreFilesAsync(ProjectDto project, List<IgnoreFileDto> ignoreFiles)
        {
            if (!project.IsEntityValidAndExist() || !ignoreFiles.Any())
                return false;
            using var dbContext = new PublisherDbContext();
            var find = await dbContext.IgnoreFileInfos
                .Include(p => p.ProjectInfo)
                .Where(f => f.ProjectId == project.ID)
                .ToListAsync();

            foreach (IgnoreFileDto item in ignoreFiles)
            {
                dbContext.IgnoreFileInfos
                    .RemoveRange(find.Where(i=> i.ID ==  item.ID));
            }
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IgnoreFileDto> GetIgnoreFileAsync(IgnoreFileDto ignoreFile)
        {
            using var dbContext = new PublisherDbContext();

            IgnoreFileDto ignoreFileDto = await dbContext.IgnoreFileInfos
                .Include(p => p.ProjectInfo)
                .FirstOrDefaultAsync(file => file.ID == ignoreFile.ID);

            return ignoreFileDto;
        }
        public async Task<IgnoreFileDto> GetIgnoreFileAsync(ProjectDto project, IgnoreFileDto ignoreFile)
        {
            using var dbContext = new PublisherDbContext();
            var find = await dbContext.IgnoreFileInfos
                .Include(p => p.ProjectInfo)
                .Where(x => x.ID == ignoreFile.ID)
                .FirstOrDefaultAsync(y => y.ProjectId == project.ID);

            return find;

        }
        public async Task<List<IgnoreFileDto>> GetIgnoreFilesAsync(ProjectDto project, IgnoreFileType ignoreFileType)
        {
            using var dbContext = new PublisherDbContext();
            List<IgnoreFileInfo> projectIgnoreFiles = await dbContext.IgnoreFileInfos
                .Include(p => p.ProjectInfo)
                .Where(prj => prj.ProjectId == project.ID)
                .Where(t => t.IgnoreFileType == ignoreFileType)
                .ToListAsync();

            List<IgnoreFileDto> ignoreFiles = new List<IgnoreFileDto>();
            ignoreFiles.AddRange(projectIgnoreFiles.Select(x => (IgnoreFileDto)x).ToList());


            return ignoreFiles;
        }
        public async Task<List<IgnoreFileDto>> GetIgnoreFilesAsync(ProjectDto project)
        {
            using var dbContext = new PublisherDbContext();
            List<IgnoreFileInfo> projectIgnoreFiles = await dbContext.IgnoreFileInfos
                .Include(p => p.ProjectInfo)
                .Where(prj => prj.ProjectId == project.ID)
                .ToListAsync();

            List<IgnoreFileDto> ignoreFiles = new List<IgnoreFileDto>();
            ignoreFiles.AddRange(projectIgnoreFiles
                .Select(x => (IgnoreFileDto)x).ToList());

            return ignoreFiles;
        }

        public async Task<ProjectDto> AddOrUpdateProjectAsync(ProjectDto projectDto)
        {
            // check null value's
            if (projectDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                // check exist or duplicate name
                if (projectDto.IsEntityExist())
                {
                    EntityEntry<ProjectInfo> updated = dbContext.ProjectInfos
                         .Update((ProjectInfo)projectDto);
                    await dbContext.SaveChangesAsync();
                    return updated.Entity;
                }
                else
                {
                    EntityEntry<ProjectInfo> added = await dbContext.ProjectInfos
                        .AddAsync((ProjectInfo)projectDto);
                    await dbContext.SaveChangesAsync();
                    return added.Entity;
                }
            }
            else
                return null;
        }

    }
}
