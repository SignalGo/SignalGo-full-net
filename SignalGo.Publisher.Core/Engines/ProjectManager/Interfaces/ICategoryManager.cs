using SignalGo.Publisher.Models.DataTransferObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Core.Engines.Interfaces.ProjectManager
{
    public interface ICategoryManager
    {

        Task<bool> RemoveCategoryAsync(int id);
        Task<bool> RemoveCategoryAsync(string name);
        Task<bool> RemoveCategoryAsync(CategoryDto categoryDto);

        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryAsync(int categoryId);
        Task<CategoryDto> GetCategoryAsync(string name);
        Task<CategoryDto> GetCategoryAsync(CategoryDto categoryDto);

        Task<CategoryDto> AddOrUpdateCategoryAsync(CategoryDto categoryDto);

    }
}