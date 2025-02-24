using System.Linq.Expressions;
using System.Reflection;

namespace app.repositories.Extensions
{
    public static class ReflectionExtension
    {
        public static string GetPropertyValue<TSource>(this TSource source, string propertyName)
        {
            if (source == null || string.IsNullOrEmpty(propertyName))
                return string.Empty;

            var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
                return string.Empty;

            var value = propertyInfo.GetValue(source);
            return value?.ToString() ?? string.Empty;
        }

        public static string GetPropertyName<TKey, TSource>(Expression<Func<TSource, TKey>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }

            throw new ArgumentException("Invalid expression type");
        }
    }
}
