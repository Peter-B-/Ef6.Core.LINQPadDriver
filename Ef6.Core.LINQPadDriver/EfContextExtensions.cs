using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Ef6.Core.LINQPadDriver
{
    public static class EfContextExtensions
    {
        public static IEnumerable<PropertyInfo> GetDbSetProperties(this Type dbSetType)
        {
            return dbSetType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .OrderBy(p => p.Name);
        }

        public static IEnumerable<Type> GetDbSetTypes(this Type dbSetType)
        {
            return dbSetType.GetDbSetProperties()
                .Select(p => p.PropertyType.GetGenericArguments().First());
        }
    }
}
