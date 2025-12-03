using System.Linq.Expressions;
namespace Shared.Application.Helpers;


public static class ExpressionBuilder
{
    public static Expression<Func<T, object>>? BuildOrderByExpression<T>(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return null;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyInfo = typeof(T).GetProperty(propertyName);
        if (propertyInfo == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'.");

        var property = Expression.Property(parameter, propertyInfo);
        var converted = Expression.Convert(property, typeof(object));

        return Expression.Lambda<Func<T, object>>(converted, parameter);
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}


