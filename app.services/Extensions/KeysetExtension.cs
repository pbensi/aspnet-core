using app.shared.Dto;
using System.Linq.Expressions;
using System.Reflection;
using app.repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace app.services.Extensions
{
    internal static class KeysetExtension
    {
        private static readonly Dictionary<string, MethodInfo> CachedCompareToMethods = new Dictionary<string, MethodInfo>();

        private static IQueryable<TSource> TieBreak<TSource>(this IQueryable<TSource> query,
           string column,
           string direction,
           string value,
           int IdtieBreakValue,
           bool isNext)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException(nameof(column), "Column name cannot be null or empty.");
            }

            var property = typeof(TSource).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, column, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                throw new ArgumentException($"Property '{column}' not found on type '{typeof(TSource).Name}'.", nameof(column));
            }

            if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
            {
                throw new InvalidOperationException($"The property '{column}' does not implement IComparable. It cannot be used for cursor-based pagination.");
            }

            var tieBreakProperty = typeof(TSource).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));

            if (tieBreakProperty == null || !typeof(IComparable).IsAssignableFrom(tieBreakProperty.PropertyType))
            {
                throw new InvalidOperationException($"The entity does not have a valid 'Id' property for tie-breaking.");
            }

            var parameter = Expression.Parameter(typeof(TSource), "p");

            var propertyExpression = Expression.Property(parameter, property);
            var cursorExpression = Expression.Constant(Convert.ChangeType(value, property.PropertyType));

            var tieBreakExpression = Expression.Property(parameter, tieBreakProperty);
            var tieBreakConstant = Expression.Constant(Convert.ChangeType(IdtieBreakValue, tieBreakProperty.PropertyType));

            var compareToKey = $"{property.PropertyType.FullName}_CompareTo";
            if (!CachedCompareToMethods.TryGetValue(compareToKey, out var compareToMethod))
            {
                compareToMethod = property.PropertyType.GetMethod("CompareTo", new[] { property.PropertyType });
                if (compareToMethod == null)
                {
                    throw new InvalidOperationException($"The property '{column}' of type '{property.PropertyType.Name}' does not have a 'CompareTo' method.");
                }

                CachedCompareToMethods[compareToKey] = compareToMethod;
            }

            var compareToCall = Expression.Call(propertyExpression, compareToMethod, cursorExpression);

            Expression comparisonExpression;

            if (direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                comparisonExpression = isNext
                    ? Expression.GreaterThan(compareToCall, Expression.Constant(0))
                    : Expression.LessThan(compareToCall, Expression.Constant(0));

                var tieBreakComparison = Expression.Equal(compareToCall, Expression.Constant(0));
                var tieBreakComparisonExpression = isNext
                    ? Expression.GreaterThan(tieBreakExpression, tieBreakConstant)
                    : Expression.LessThan(tieBreakExpression, tieBreakConstant);

                comparisonExpression = Expression.OrElse(comparisonExpression, Expression.AndAlso(tieBreakComparison, tieBreakComparisonExpression));
            }
            else
            {
                comparisonExpression = isNext
                    ? Expression.LessThan(compareToCall, Expression.Constant(0))
                    : Expression.GreaterThan(compareToCall, Expression.Constant(0));

                var tieBreakComparison = Expression.Equal(compareToCall, Expression.Constant(0));
                var tieBreakComparisonExpression = isNext
                    ? Expression.LessThan(tieBreakExpression, tieBreakConstant)
                    : Expression.GreaterThan(tieBreakExpression, tieBreakConstant);

                comparisonExpression = Expression.OrElse(comparisonExpression, Expression.AndAlso(tieBreakComparison, tieBreakComparisonExpression));
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(comparisonExpression, parameter);

            return query.Where(lambda);
        }


        public static IQueryable<TSource> Cursor<TSource>(this IQueryable<TSource> query, KeysetQueryDto keysetQuery)
        {
            if (keysetQuery.HasNextPage && !string.IsNullOrEmpty(keysetQuery.NextCursor) && keysetQuery.NextUniqueId.HasValue)
            {
                return query
                    .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection!)
                    .ThenBySorting("Id", keysetQuery.SortDirection!)
                    .TieBreak(keysetQuery.SortColumn!, keysetQuery.SortDirection!, keysetQuery.NextCursor, keysetQuery.NextUniqueId!.Value, isNext: true)
                    .Take(keysetQuery.PageSize);
            }
            else if (keysetQuery.HasPreviousPage && !string.IsNullOrEmpty(keysetQuery.PreviousCursor) && keysetQuery.PreviousUniqueId.HasValue)
            {
                return query
                    .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection == "asc" ? "desc" : "asc")
                    .ThenBySorting("Id", keysetQuery.SortDirection == "asc" ? "desc" : "asc")
                    .TieBreak(keysetQuery.SortColumn!, keysetQuery.SortDirection!, keysetQuery.PreviousCursor, keysetQuery.PreviousUniqueId!.Value, isNext: false)
                    .Take(keysetQuery.PageSize)
                    .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection!)
                    .ThenBySorting("Id", keysetQuery.SortDirection!);
            }
            else
            {
                return query
                    .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection!)
                    .ThenBySorting("Id", keysetQuery.SortDirection!)
                    .Take(keysetQuery.PageSize);
            }
        }

        public static async Task<bool> HasNextCursorPage<TSource>(this IQueryable<TSource> query, KeysetQueryDto keysetQuery, string nextCursor, int nextUniqueId)
        {
            return await query
                .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection!)
                .ThenBySorting("Id", keysetQuery.SortDirection!)
                .TieBreak(keysetQuery.SortColumn!, keysetQuery.SortDirection!, nextCursor, nextUniqueId, isNext: true)
                .Take(1)
                .AnyAsync();
        }

        public static async Task<bool> HasPreviousCursorPage<TSource>(this IQueryable<TSource> query, KeysetQueryDto keysetQuery, string previousCursor, int previousUniqueId)
        {
            return await query
                 .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection == "asc" ? "desc" : "asc")
                 .ThenBySorting("Id", keysetQuery.SortDirection == "asc" ? "desc" : "asc")
                 .TieBreak(keysetQuery.SortColumn!, keysetQuery.SortDirection!, previousCursor, previousUniqueId, isNext: false)
                 .Take(1)
                 .Sorting(keysetQuery.SortColumn!, keysetQuery.SortDirection!)
                 .ThenBySorting("Id", keysetQuery.SortDirection!)
                 .AnyAsync();
        }
    }
}
