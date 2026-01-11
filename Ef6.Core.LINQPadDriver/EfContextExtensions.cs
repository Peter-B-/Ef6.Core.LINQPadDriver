using System.Data.Entity;
using System.Reflection;

namespace Ef6.Core.LINQPadDriver;

public static class EfContextExtensions
{
    public static IEnumerable<PropertyInfo> GetDbSetProperties(this Type dbContextType)
    {
        return dbContextType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .OrderBy(p => p.Name);
    }

    /// <summary>
    ///     Returns TEntity for a DbSet&lt;TEntity&gt; type
    /// </summary>
    /// <param name="dbSetType"></param>
    /// <returns></returns>
    public static Type? GetDbSetType(this Type dbSetType)
    {
        return dbSetType.GetGenericArguments().FirstOrDefault();
    }

    public static IEnumerable<Type> GetDbSetTypes(this Type dbContextType)
    {
        return dbContextType.GetDbSetProperties()
            .Select(p => p.PropertyType.GetGenericArguments().First());
    }
}
