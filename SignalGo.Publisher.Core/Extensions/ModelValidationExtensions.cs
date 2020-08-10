using Microsoft.EntityFrameworkCore.Internal;
using SignalGo.Publisher.DataAccessLayer.Context;
using SignalGo.Publisher.Models.DataTransferObjects;
using System;
using System.Linq;

namespace SignalGo.Publisher.Core.Extensions
{
    public static class ModelValidationExtensions
    {

        /// <summary>
        /// validate model value's
        /// </summary>
        /// <param name="categoryDto"></param>
        /// <returns></returns>
        public static bool IsModelValid(this CategoryDto categoryDto)
        {
            if (string.IsNullOrEmpty(categoryDto.Name))
            {
                return false;
            }


            return true;
        }
        public static bool IsModelValid(this IgnoreFileDto ignoreFileDto)
        {
            if (string.IsNullOrEmpty(ignoreFileDto.FileName))
            {
                return false;
            }
            return true;
        }
        public static bool IsValid(this int id)
        {
            if (id <= 0)
            {
                return false;
            }

            return true;
        }
        public static bool IsValid(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// validate model value's and check if exist in database
        /// </summary>
        /// <param name="projectDto"></param>
        /// <returns></returns>
        public static bool IsEntityValidAndExist(this ProjectDto projectDto)
        {
            if (projectDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                return dbContext.ProjectInfos
                .Any(x => x.Name == projectDto.Name);
            }
            else
                return false;
        }
        public static bool IsEntityValidAndExist(this IgnoreFileDto ignoreFileDto)
        {
            if (ignoreFileDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                return dbContext.IgnoreFileInfos
                .Any(x => x.FileName == ignoreFileDto.FileName);
            }
            else
                return false;
        }
        /// <summary>
        /// validate model value's and check if exist in database
        /// </summary>
        /// <param name="categoryDto"></param>
        /// <returns></returns>
        public static bool IsEntityValidAndExist(this CategoryDto categoryDto)
        {
            if (categoryDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                return dbContext.CategoryInfos
                    .Any(x => x.Name == categoryDto.Name);
            }
            else
                return false;
        }
        /// <summary>
        /// only check if exist in database
        /// </summary>
        /// <param name="projectDto"></param>
        /// <returns></returns>
        public static bool IsEntityExist(this ProjectDto projectDto)
        {
            if (projectDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                return dbContext.ProjectInfos
                .Any(x => x.Name == projectDto.Name || x.ID == projectDto.ID);
            }
            else
                return false;
        }
        /// <summary>
        /// only check if exist in database
        /// </summary>
        /// <param name="categoryDto"></param>
        /// <returns></returns>
        public static bool IsEntityExist(this CategoryDto categoryDto)
        {
            if (categoryDto.IsModelValid())
            {
                using var dbContext = new PublisherDbContext();
                return dbContext.CategoryInfos
                .Any(x => x.Name == categoryDto.Name);
            }
            else
                return false;
        }
        /// <summary>
        /// validate model value's
        /// </summary>
        /// <param name="projectDto"></param>
        /// <returns></returns>
        public static bool IsModelValid(this ProjectDto projectDto)
        {
            if (projectDto == null || string.IsNullOrEmpty(projectDto.Name))
            {
                return false;
            }
            else if (!Guid.TryParse(projectDto.ProjectKey.ToString(), out Guid tmp))
            {

                return false;
            }

            return true;
        }
    }
}
