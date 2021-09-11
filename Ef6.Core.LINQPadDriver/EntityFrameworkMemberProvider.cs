using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad;

namespace Ef6.Core.LINQPadDriver
{
    /// <summary>
    ///     Decompiled from LINQPad.Extensibility.DataContext.EntityFrameworkMemberProvider class of LINQPad 5 by Joseph
    ///     Albahari
    /// </summary>
    internal class EntityFrameworkMemberProvider : ICustomMemberProvider
    {
        private static MethodInfo getItemMethod;
        private static MethodInfo getEntitySetMethod;

        private readonly string[] names;

        private readonly Type[] types;

        private readonly object[] values;

        public EntityFrameworkMemberProvider(object objectToWrite)
        {
            var entityFrameworkMemberProvider = this;
            var objectContext = GetObjectContext(objectToWrite);
            var dbContext = GetDbContext(objectContext);
            var navProps = new HashSet<string>(GetNavPropNames(objectContext, objectToWrite) ?? Array.Empty<string>());
            var source = (from m in objectToWrite.GetType().GetMembers()
                where m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property
                where (m.MemberType != MemberTypes.Field || m.Name != "_entityWrapper") && (m.MemberType != MemberTypes.Property || m.Name != "RelationshipManager")
                let isNav = navProps.Contains(m.Name)
                let type = GetFieldPropType(m)
                orderby isNav, m.MetadataToken
                select new
                {
                    m.Name,
                    type,
                    value = ((isNav && (IsUnloadedEntityAssociation(dbContext, objectToWrite, m) ?? true))
                        ? InternalUtil.OnDemand(m.Name, () => entityFrameworkMemberProvider.GetFieldPropValue(objectToWrite, m), runOnNewThread: false, typeof(IEnumerable).IsAssignableFrom(type))
                        : entityFrameworkMemberProvider.GetFieldPropValue(objectToWrite, m))
                }).ToList();
            names = source.Select(q => q.Name).ToArray();
            types = source.Select(q => q.type).ToArray();
            values = source.Select(q => q.value).ToArray();
        }

        public IEnumerable<string> GetNames() => names;
        public IEnumerable<Type> GetTypes() => types;
        public IEnumerable<object> GetValues() => values;

        public static bool IsEntity(Type t)
        {
            if (t.IsValueType || t.IsPrimitive || t.AssemblyQualifiedName?.EndsWith("=b77a5c561934e089") == true) return false;
            return t.GetField("_entityWrapper") != null;
        }

        private static object GetDbContext(object objectContext)
        {
            if (objectContext == null) return null;
            var type = objectContext.GetType().Assembly.GetType("System.Data.Entity.DbContext");
            if (type == null) return null;
            var constructor = type.GetConstructor(new[]
            {
                objectContext.GetType(),
                typeof(bool)
            });
            if (constructor == null) return null;
            try
            {
                return Activator.CreateInstance(type, objectContext, false);
            }
            catch
            {
                return null;
            }
        }

        private static Type GetFieldPropType(MemberInfo m) =>
            m switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new InvalidOperationException("Expected FieldInfo or PropertyInfo")
            };

        private object GetFieldPropValue(object value, MemberInfo m)
        {
            try
            {
                return m switch
                {
                    FieldInfo fieldInfo => fieldInfo.GetValue(value),
                    PropertyInfo propertyInfo => propertyInfo.GetValue(value, null),
                    _ => throw new InvalidOperationException("Expected FieldInfo or PropertyInfo")
                };
            }
            catch (Exception result)
            {
                return result;
            }
        }

        private IEnumerable<string> GetNavPropNames(object objectContext, object entity)
        {
            if (entity == null || objectContext == null) return null;
            var field = entity.GetType().GetField("_entityWrapper");
            if (field == null) return null;
            var value = field.GetValue(entity);
            var property = value?.GetType().GetProperty("EntityKey");
            if (property == null) return null;
            dynamic value2 = property.GetValue(value, null);
            if (value2 == null || value2.GetType().Name != "EntityKey") return null;
            var val = ((dynamic)objectContext).MetadataWorkspace;
            Type type = val.GetType();
            if (getItemMethod == null)
                getItemMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "TryGetItem" && m.IsGenericMethod && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType.Name == "DataSpace" && m.GetParameters()[2].IsOut);
            if (getItemMethod == null) return null;
            var type2 = type.Assembly.GetType("System.Data.Entity.Core.Metadata.Edm.EntityContainer") ?? 
                type.Assembly.GetType("System.Data.Metadata.Edm.EntityContainer");
            if (type2 == null) return null;
            var methodInfo = getItemMethod.MakeGenericMethod(type2);
            var array = new object[]
            {
                value2.EntityContainerName,
                1,
                null
            };
            if ((!true.Equals(methodInfo.Invoke(val, array)))) return null;
            var obj = array[2];
            if (obj == null) return null;
            if (getEntitySetMethod == null || getEntitySetMethod.ReflectedType != obj.GetType())
                getEntitySetMethod = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "TryGetEntitySetByName" && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType == typeof(bool) && m.GetParameters()[2].IsOut);
            if (getEntitySetMethod == null) return null;
            array = new object[]
            {
                value2.EntitySetName,
                false,
                null
            };
            if (!true.Equals(getEntitySetMethod.Invoke(obj, array))) return null;
            dynamic val2 = array[2];
            if (val2 == null) return null;
            return ((IEnumerable<object>)val2.ElementType.NavigationProperties).Select((dynamic np) => np.Name).OfType<string>();
        }

        private static object GetObjectContext(object entity)
        {
            if (entity == null) return null;
            var field = entity.GetType().GetField("_entityWrapper");
            if (field == null) return null;
            var value = field.GetValue(entity);
            var property = value?.GetType().GetProperty("Context");
            if (property == null) return null;
            var value2 = property.GetValue(value, null);
            if (value2 == null || value2.GetType().Name != "ObjectContext") return null;
            return value2;
        }

        private static bool? IsUnloadedEntityAssociation(dynamic dbContext, dynamic target, MemberInfo member)
        {
            try
            {
                var type = member is FieldInfo fieldInfo ? fieldInfo.FieldType : ((PropertyInfo)member).PropertyType;
                if (type.Name == "ICollection`1" || typeof(ICollection).IsAssignableFrom(type))
                {
                    try
                    {
                        return !dbContext.Entry(target).Collection(member.Name).IsLoaded;
                    }
                    catch
                    {
                        // ignored
                    }

                    return !dbContext.Entry(target).Reference(member.Name).IsLoaded;
                }

                try
                {
                    return !dbContext.Entry(target).Reference(member.Name).IsLoaded;
                }
                catch
                {
                    // ignored
                }

                return !dbContext.Entry(target).Collection(member.Name).IsLoaded;
            }
            catch
            {
                return null;
            }
        }
    }
}