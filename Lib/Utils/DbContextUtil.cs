using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Lib.Utils;

public static class DbContextUtil
{
    public static IEnumerable<PropertyInfo> GetDbSets<TDbContext, TType>() where TType : class =>
        typeof(TDbContext)
            .GetProperties()
            .Where(x => x.PropertyType.IsGenericType)
            .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Where(x => x.PropertyType.GenericTypeArguments[0].IsAssignableTo(typeof(TType)));
}