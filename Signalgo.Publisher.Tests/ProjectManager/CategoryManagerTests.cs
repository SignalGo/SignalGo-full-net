using NUnit.Framework;
using SignalGo.Publisher.Models.DataTransferObjects;
using SignalGo.Publisher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signalgo.Publisher.Tests.ProjectManager
{
    public class CategoryManagerTests : PublisherProjectManagerBase
    {

        public CategoryManagerTests() : base()
        {

        }

        [Test]
        public async Task AddOrUpdateCategory()
        {
            CategoryDto existCategory = TestCategoriesList[0];
            existCategory.Description = "Category Updated";

            CategoryDto newCategory = new CategoryDto
            {
                Name = "New Test Category",
                Description = "Category Added",
                SubCategories = new List<CategoryDto>
                {
                    new CategoryDto {
                        Name = "new category sub",
                        SubCategories = new List<CategoryDto>
                        {
                            new CategoryDto
                            {
                                Name = "new category sub - child",
                            }
                        }
                    }
                }
            };

            CategoryDto result = await _categoryManager
                .AddOrUpdateCategoryAsync(existCategory);
            Assert.True(result.Description == "Category Updated");
            var updatedEntity = await _categoryManager
                .GetCategoryAsync(result);
            Assert.True(updatedEntity.Name == result.Name);

            // check if not exist, so it's new and must add
            Assert.True(await _categoryManager.GetCategoryAsync(newCategory) == null);
            var addResult = await _categoryManager
                .AddOrUpdateCategoryAsync(newCategory);
            Assert.True(addResult != null && addResult.Description == "Category Added");

        }
        [Test]
        public async Task AddBadCategory()
        {
            var result = await _categoryManager.AddOrUpdateCategoryAsync(new CategoryDto
            {
                Name = "",
            });

            Assert.True(result == null);
        }

        [Test]
        public async Task GetCategoryByName()
        {
            //using CategoryManagerModule categoryManagerModule = new CategoryManagerModule();
            try
            {
                CategoryDto find = await _categoryManager
                    .GetCategoryAsync(TestCategoriesList[0].Name);

                Assert.True(find != null);
                Assert.True(find.SubCategories
                    .ElementAt(0)
                    .SubCategories
                    .Count == 2);
            }
            catch (Exception ex)
            {

            }
        }
        [Test]
        public async Task GetCategoryById()
        {
            try
            {
                CategoryDto find = await _categoryManager
                    .GetCategoryAsync(TestCategoriesList[0].ID);

                Assert.True(find != null);
                Assert.True(find.SubCategories
                    .ElementAt(0)
                    .SubCategories
                    .Count == 2);
            }
            catch (Exception ex)
            {

            }
        }
        [Test]
        public async Task GetCategoryAsync()
        {
            try
            {
                CategoryDto find = await _categoryManager
                    .GetCategoryAsync(TestCategoriesList[0]);

                Assert.True(find != null);
                Assert.True(find.SubCategories
                    .ElementAt(0)
                    .SubCategories
                    .Count == 2);
            }
            catch (Exception ex)
            {

            }
        }
        [Test]
        public async Task GetSubCategoryChilds()
        {
            // query lvl 2 child category (sub category of sub category)
            var find = await _categoryManager
                .GetCategoryAsync(TestCategoriesList[0].SubCategories.ElementAt(0).SubCategories.ElementAt(0));
            Assert.False(find == null);
            // check this child parent name, up to 2 lvl and verify with that top parent
            //Assert.True(find.ParentCategory
            //.ParentCategory.Name == TestCategoriesList[0].Name);
        }
        [Test]
        public async Task GetSubCategory()
        {
            CategoryInfo category = await _categoryManager
                .GetCategoryAsync(TestCategoriesList[0].SubCategories.ElementAt(0).Name);

            Assert.False(category == null, "Test Sub Category Not Found!");

            Assert.True(category
                .SubCategories
                .Count == 2,
                "No LVL 2 Childs Found For Test Sub Category!");
        }
        [Test]
        public async Task RemoveChildCategory()
        {
            // get a test category from categories
            var find = await _categoryManager
                .GetCategoryAsync(TestCategoriesList[0].SubCategories
                .ElementAt(0).SubCategories.ElementAt(0));
            Assert.False(find == null, "Could'nt find test category child");

            await _categoryManager
                .RemoveCategoryAsync(find);

            // check if category was deleted successfully and not exist
            var allCategories = await _categoryManager.GetAllCategoriesAsync();
            // CategoryTest_SubCategory1_Child1
            Assert.False(allCategories.Any(category => category.Name == TestCategoriesList[0].SubCategories.ElementAt(0).SubCategories.ElementAt(0).Name));

            var projects = await _projectManager
                .GetAllProjectsAsync();
            // check project's category field that was refrened to this category, must be null. otherwise cascade relation not worked correctly.
            // CategoryTest_SubCategory1_Child1
            Assert.False(projects.Any(i => i.Category?.Name == TestCategoriesList[0].SubCategories
            .ElementAt(0).SubCategories.ElementAt(0).Name));

        }

        [Test]
        public async Task GetAllCategoriesInfo()
        {
            var categories = await _categoryManager
                .GetAllCategoriesAsync();
            Assert.True(categories.Any());
            Assert.True(categories.Count == 15);
        }


        [Test]
        public async Task RemoveCategoryByName()
        {
            var category = await _categoryManager
                .RemoveCategoryAsync(TestCategoriesList[0].Name);
            // check if category was deleted successfully and not exist
            var allCategories = await _categoryManager
                .GetAllCategoriesAsync();
            Assert.False(allCategories.Any(category => category.Name == TestCategoriesList[0].Name));

            var projects = await _projectManager.GetAllProjectsAsync();
            // check project's category field that was refrened to this category, must be null. otherwise cascade relation not worked correctly.
            Assert.False(projects.Any(i => i.Category?.Name == TestCategoriesList[0].Name));
        }
        [Test]
        public async Task RemoveCategoryById()
        {
            var find = await _categoryManager
                .GetCategoryAsync(TestCategoriesList[0]);
            Assert.False(find == null, "Could'nt find test category");

            await _categoryManager
                .RemoveCategoryAsync(find.ID);

            // check if category was deleted successfully and not exist
            var allCategories = await _categoryManager.GetAllCategoriesAsync();
            // CategoryTest_SubCategory1_Child1
            Assert.False(allCategories
                .Any(category => category.Name == TestCategoriesList[0].Name));

            var projects = await _projectManager.GetAllProjectsAsync();
            // check project's category field that was refrened to this category, must be null. otherwise cascade relation not worked correctly.
            Assert.False(projects.Any(i => i.Category?.Name == TestCategoriesList[0].Name));
        }


    }
}
