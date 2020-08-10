using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SignalGo.Publisher.Core.Engines.Interfaces.ProjectManager;
using SignalGo.Publisher.Core.Extensions;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Core.Engines.ProjectManager
{
    /// <summary>
    /// Manage Category And their sub categories in database
    /// </summary>
    public class CategoryManagerModule : CrudBase, ICategoryManager
    {
        public CategoryManagerModule() : base()
        {

        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            using var dbContext = new PublisherDbContext();

            var allCategories = await dbContext.CategoryInfos
           .Include(prj => prj.Projects)
           .Include(p => p.ParentCategory)
           .Include(s => s.SubCategories)
           .ToListAsync();

            List<CategoryDto> categories = new List<CategoryDto>();

            categories.AddRange(allCategories.Select(x => (CategoryDto)x).ToList());
            return categories;
        }


        public async Task<CategoryDto> GetCategoryAsync(CategoryDto categoryDto)
        {
            // lazy loading auto proxy
            if (categoryDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                var result = await dbContext.CategoryInfos
                    .Include(p => p.ParentCategory)
                    .Include(s => s.SubCategories)
                    .Include(prj => prj.Projects)
                    .FirstOrDefaultAsync(c => c.ID == categoryDto.ID);

                #region eager include's
                //CategoryDto result = await dbContext.CategoryInfos
                //    .Include(p=>p.ParentCategory).ThenInclude(p => p.ParentCategory).ThenInclude(p => p.ParentCategory)
                //    .Include(s=>s.SubCategories).ThenInclude(s=>s.SubCategories).ThenInclude(s=>s.SubCategories)
                //    .Include(pr=>pr.Projects)
                //    .FirstOrDefaultAsync(x => x.ID == categoryDto.ID);
                #endregion

                return result;
            }
            else
                return null;
        }

        public async Task<CategoryDto> GetCategoryAsync(int categoryId)
        {
            if (categoryId.IsValid())
            {
                using var dbContext = new PublisherDbContext();
                CategoryDto result = await dbContext.CategoryInfos
                    .Include(p => p.ParentCategory)
                    .Include(s => s.SubCategories)
                    .FirstOrDefaultAsync(c => c.ID == categoryId);

                return result;
            }
            else
                return null;
        }
        public async Task<CategoryDto> GetCategoryAsync(string categoryName)
        {
            if (categoryName.IsValid())
            {
                using var dbContext = new PublisherDbContext();
                CategoryDto result = await dbContext.CategoryInfos
                    .Include(p => p.ParentCategory)
                    .Include(s => s.SubCategories)
                    .FirstOrDefaultAsync(c => c.Name == categoryName);

                return result;
            }
            else
                return null;
        }

        public async Task<bool> RemoveCategoryAsync(int categoryId)
        {
            if (categoryId.IsValid())
            {
                try
                {
                    using var dbContext = new PublisherDbContext();

                    CategoryInfo find = await dbContext.CategoryInfos
                        .Include(p => p.ParentCategory)
                        .Include(prj => prj.Projects)
                        .FirstOrDefaultAsync(x => x.ID == categoryId);
                    // set project's category that refrenced to this category, to parent category or null
                    foreach (var item in find.Projects)
                    {
                        item.CategoryId = item.Category?.ParentCategoryId;
                        item.Category = item.Category?.ParentCategory;
                    }
                    dbContext.CategoryInfos.Remove(find);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return true;
            }
            else
                return false;
        }

        public async Task<bool> RemoveCategoryAsync(CategoryDto categoryDto)
        {
            if (categoryDto.IsEntityValidAndExist())
            {
                try
                {
                    using var dbContext = new PublisherDbContext();
                    var find = await dbContext.CategoryInfos
                        .Include(p => p.ParentCategory)
                        .Include(prj => prj.Projects)
                        .FirstOrDefaultAsync(x => x.ID == categoryDto.ID);
                    // set project's category that refrenced to this category, to parent category or null
                    foreach (var item in find.Projects)
                    {
                        item.CategoryId = item.Category?.ParentCategoryId;
                        item.Category = item.Category?.ParentCategory;
                    }
                    dbContext.CategoryInfos.Remove(find);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return true;
            }
            else
                return false;
        }
        public async Task<bool> RemoveCategoryAsync(string categoryName)
        {
            if (categoryName.IsValid())
            {
                try
                {
                    using var dbContext = new PublisherDbContext();
                    var category = await dbContext.CategoryInfos
                        .Include(p => p.ParentCategory)
                        .Include(prj => prj.Projects)
                        .FirstOrDefaultAsync(cat => cat.Name == categoryName);
                    // set project's category that refrenced to this category, to parent category or null
                    foreach (var item in category.Projects)
                    {
                        item.CategoryId = item.Category?.ParentCategoryId;
                        item.Category = item.Category?.ParentCategory;
                    }
                    dbContext.CategoryInfos.Remove(category);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                }
                return true;
            }
            else
                return false;
        }

        public async Task<CategoryDto> AddOrUpdateCategoryAsync(CategoryDto categoryDto)
        {
            // lazy loading auto proxy
            if (categoryDto.IsModelValid())
            {
                try
                {
                    using var dbContext = new PublisherDbContext();

                    if (categoryDto.IsEntityExist())
                    {
                        EntityEntry<CategoryInfo> updated = dbContext
                            .CategoryInfos
                            .Update((CategoryInfo)categoryDto);
                        await dbContext.SaveChangesAsync();
                        return updated.Entity;
                    }
                    else
                    {
                        EntityEntry<CategoryInfo> result = await dbContext
                            .CategoryInfos
                            .AddAsync((CategoryInfo)categoryDto);
                        await dbContext.SaveChangesAsync();
                        return result.Entity;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return null;
        }

    }
}
