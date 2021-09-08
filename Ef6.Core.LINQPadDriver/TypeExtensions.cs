using System;
using System.Linq;

namespace Ef6.Core.LINQPadDriver
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines if a type is a simple type like <see cref="int"/>, <see cref="string"/>, <see cref="DateTime"/> etc. This is escpecially usefull for DataBases or .ToString() methods
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            var innerType = Nullable.GetUnderlyingType(type);
            if (innerType != null)
                type = innerType;

            var result = type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(Guid)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan);

            return result;
        }
    }
}