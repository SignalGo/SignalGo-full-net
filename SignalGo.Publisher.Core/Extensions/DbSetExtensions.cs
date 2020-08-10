using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Publisher.Core.Extensions
{
    public static class DbSetExtensions
    {

        //public static async Task<TEntity[]> FindRecursiveAsync<TEntity, TKey>(
        //    this DbSet<TEntity> source,
        //    Expression<Func<TEntity, bool>> rootSelector,
        //    Func<TEntity, TKey> getEntityKey,
        //    Func<TEntity, TKey> getChildKeyToParent)
        //    where TEntity : class
        //{
        //    // Keeps a track of already processed, so as not to invoke
        //    // an infinte recursion
        //    var alreadyProcessed = new HashSet<TKey>();

        //    TEntity[] result = await source.Where(rootSelector).ToArrayAsync();

        //    TEntity[] currentRoots = result;
        //    while (currentRoots.Length > 0)
        //    {
        //        TKey[] currentParentKeys = currentRoots.Select(getEntityKey).Except(alreadyProcessed).ToArray();
        //        foreach (var item in currentParentKeys)
        //        {
        //            alreadyProcessed.Add(item);
        //        }
        //        Expression<Func<TEntity, bool>> childPredicate = x => currentParentKeys.Contains(getChildKeyToParent(x));
        //        currentRoots = await source.Where(childPredicate).ToArrayAsync();
        //    }

        //    return result;
        //}

    }
}
