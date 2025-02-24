using System.Linq.Expressions;
using System.Reflection;

namespace app.repositories.Extensions
{
    public static class QueryExtension
    {
        private static readonly Dictionary<string, MethodInfo> CachedSortingMethods = new Dictionary<string, MethodInfo>();

        public static IOrderedQueryable<TSource> Sorting<TSource>(this IQueryable<TSource> source, string column, string direction)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException(nameof(column), "Column name cannot be null or empty.");
            }

            var property = typeof(TSource).GetProperty(column);
            if (property == null)
            {
                throw new ArgumentException($"Property '{column}' does not exist on type '{typeof(TSource).Name}'.", nameof(column));
            }

            if (property.CanRead == false)
            {
                throw new ArgumentException($"Property '{column}' is not readable on type '{typeof(TSource).Name}'.", nameof(column));
            }

            if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
            {
                throw new InvalidOperationException($"The property '{column}' does not implement IComparable. Sorting is only supported for IComparable types.");
            }

            var parameter = Expression.Parameter(typeof(TSource), "p");
            var propertyExpression = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            var methodKey = $"{direction}_{column}";
            if (!CachedSortingMethods.TryGetValue(methodKey, out var method))
            {
                var methodName = direction.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "OrderByDescending" : "OrderBy";
                method = typeof(Queryable).GetMethods()
                    .FirstOrDefault(m => m.Name == methodName &&
                                         m.GetParameters().Length == 2 &&
                                         m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1);

                if (method == null)
                {
                    throw new InvalidOperationException($"Method '{methodName}' not found for sorting.");
                }

                CachedSortingMethods[methodKey] = method;
            }

            try
            {
                var result = method.MakeGenericMethod(typeof(TSource), property.PropertyType)
                                   .Invoke(null, new object[] { source, lambda });

                return (IOrderedQueryable<TSource>)result;
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidOperationException($"Error invoking sorting method: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
        public static IOrderedQueryable<TSource> ThenBySorting<TSource>(
    this IOrderedQueryable<TSource> source,
    string column,
    string direction)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new ArgumentNullException(nameof(column), "Column name cannot be null or empty.");
            }

            var property = typeof(TSource).GetProperty(column);
            if (property == null)
            {
                throw new ArgumentException($"Property '{column}' does not exist on type '{typeof(TSource).Name}'.", nameof(column));
            }

            if (property.CanRead == false)
            {
                throw new ArgumentException($"Property '{column}' is not readable on type '{typeof(TSource).Name}'.", nameof(column));
            }

            if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
            {
                throw new InvalidOperationException($"The property '{column}' does not implement IComparable. Sorting is only supported for IComparable types.");
            }

            var parameter = Expression.Parameter(typeof(TSource), "p");
            var propertyExpression = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            // Determine the method to call: Either "ThenBy" or "ThenByDescending"
            var methodName = direction.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "ThenByDescending" : "ThenBy";

            // Find the correct method in Queryable for the direction
            var method = typeof(Queryable).GetMethods()
                .FirstOrDefault(m => m.Name == methodName &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1);

            if (method == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found for sorting.");
            }

            try
            {
                // Apply the ThenBy/ThenByDescending method to the existing ordered query.
                var genericMethod = method.MakeGenericMethod(typeof(TSource), property.PropertyType);
                var result = genericMethod.Invoke(null, new object[] { source, lambda });

                // Return the ordered query
                return (IOrderedQueryable<TSource>)result;
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidOperationException($"Error invoking sorting method: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> query, bool condition, Expression<Func<TSource, bool>> filter) where TSource : class
        {
            return condition ? query.Where(filter) : query;
        }
    }
}
