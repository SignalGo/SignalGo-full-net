using Microsoft.EntityFrameworkCore;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models;
using System.Linq;

namespace SignalGo.Publisher.Core.Engines
{
    public class CrudBase
    {
        //private static PublisherDbContext _PublisherDbContext;
        /// <summary>
        /// Singleton Publisher DbContext
        /// </summary>
        //public static PublisherDbContext PublisherDbContext
        //{
        //    get
        //    {
        //        return _PublisherDbContext;
        //    }
        //    set
        //    {
        //        _PublisherDbContext = value;
        //    }
        //}

        static CrudBase()
        {
            //PublisherDbContext = new PublisherDbContext();

        }
        public CrudBase()
        {

        }
        /// <summary>
        /// Eager Load Categories As QueryAble
        /// </summary>
        /// <param name="publisherDbContext"></param>
        /// <returns></returns>
        public IQueryable<CategoryInfo> GetCategoriesAsQueryable(PublisherDbContext publisherDbContext)
        {

            using var dbContext = new PublisherDbContext();
            IQueryable<CategoryInfo> find = dbContext.CategoryInfos
                .AsQueryable()
                .Include(prj => prj.Projects)
                .Include(prj => prj.ParentCategory)
                .Include(sub => sub.SubCategories);

            return find;
        }

        public IQueryable<CategoryInfo> FindCategoryAsQueryable(CategoryInfo category)
        {
            using var dbContext = new PublisherDbContext();
            IQueryable<CategoryInfo> find = dbContext.CategoryInfos
                .AsQueryable()
                .Include(prj => prj.Projects)
                .Include(prj => prj.ParentCategory)
                .Include(sub => sub.SubCategories)
                .Where(i => i.ID == category.ID);

            return find;
        }

    }
}
