using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace app.repositories.Extensions
{
    public static class IncludeExtension
    {
        public static IQueryable<TSource> IncludeIf<TSource>(this IQueryable<TSource> query, bool condition, params Expression<Func<TSource, object>>[] includes) where TSource : class
        {
            if (condition)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }
    }
}
