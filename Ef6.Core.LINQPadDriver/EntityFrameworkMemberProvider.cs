using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LINQPad;

namespace Ef6.Core.LINQPadDriver
{
    /// <summary>
    /// Decompiled from LINQPad.Extensibility.DataContext.EntityFrameworkMemberProvider class of LINQPad 5 by Joseph Albahari
    /// </summary>
    internal class EntityFrameworkMemberProvider : ICustomMemberProvider
    {
        private string[] _names;

        private Type[] _types;

        private object[] _values;

        private static MethodInfo _getItemMethod;

        private static MethodInfo _getEntitySetMethod;

        public static bool IsEntity(Type t)
        {
            if (t.IsValueType || t.IsPrimitive || t.AssemblyQualifiedName.EndsWith("=b77a5c561934e089"))
            {
                return false;
            }
            return t.GetField("_entityWrapper") != null;
        }

        public EntityFrameworkMemberProvider(object objectToWrite)
        {
            EntityFrameworkMemberProvider entityFrameworkMemberProvider = this;
            object objectContext = GetObjectContext(objectToWrite);
            object dbContext = GetDbContext(objectContext);
            HashSet<string> navProps = new HashSet<string>(GetNavPropNames(objectContext, objectToWrite) ?? new string[0]);
            var source = (from m in objectToWrite.GetType().GetMembers()
                          where m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property
                          where (m.MemberType != MemberTypes.Field || !(m.Name == "_entityWrapper")) && (m.MemberType != MemberTypes.Property || !(m.Name == "RelationshipManager"))
                          let isNav = navProps.Contains(m.Name)
                          let type = entityFrameworkMemberProvider.GetFieldPropType(m)
                          orderby isNav, m.MetadataToken
                          select new
                          {
                              Name = m.Name,
                              type = type,
                              value = ((isNav && (IsUnloadedEntityAssociation(dbContext, objectToWrite, m) ?? true)) ? InternalUtil.OnDemand(m.Name, () => entityFrameworkMemberProvider.GetFieldPropValue(objectToWrite, m), runOnNewThread: false, typeof(IEnumerable).IsAssignableFrom(type)) : entityFrameworkMemberProvider.GetFieldPropValue(objectToWrite, m))
                          }).ToList();
            _names = source.Select(q => q.Name).ToArray();
            _types = source.Select(q => q.type).ToArray();
            _values = source.Select(q => q.value).ToArray();
        }

        private static bool? IsUnloadedEntityAssociation(dynamic dbContext, dynamic target, MemberInfo member)
        {
            try
            {
                Type type = ((member is FieldInfo) ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType);
                if (type.Name == "ICollection`1" || typeof(ICollection).IsAssignableFrom(type))
                {
                    try
                    {
                        return !dbContext.Entry(target).Collection(member.Name).IsLoaded;
                    }
                    catch
                    {
                    }
                    return !dbContext.Entry(target).Reference(member.Name).IsLoaded;
                }
                try
                {
                    return !dbContext.Entry(target).Reference(member.Name).IsLoaded;
                }
                catch
                {
                }
                return !dbContext.Entry(target).Collection(member.Name).IsLoaded;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<string> GetNames()
        {
            return _names;
        }

        public IEnumerable<Type> GetTypes()
        {
            return _types;
        }

        public IEnumerable<object> GetValues()
        {
            return _values;
        }

        private static object GetObjectContext(object entity)
        {
            if (entity == null)
            {
                return null;
            }
            FieldInfo field = entity.GetType().GetField("_entityWrapper");
            if (field == null)
            {
                return null;
            }
            object value = field.GetValue(entity);
            PropertyInfo property = value.GetType().GetProperty("Context");
            if (property == null)
            {
                return null;
            }
            object value2 = property.GetValue(value, null);
            if (value2 == null || value2.GetType().Name != "ObjectContext")
            {
                return null;
            }
            return value2;
        }

        private static object GetDbContext(object objectContext)
        {
            if (objectContext == null)
            {
                return null;
            }
            Type type = objectContext.GetType().Assembly.GetType("System.Data.Entity.DbContext");
            if (type == null)
            {
                return null;
            }
            ConstructorInfo constructor = type.GetConstructor(new Type[2]
            {
            objectContext.GetType(),
            typeof(bool)
            });
            if (constructor == null)
            {
                return null;
            }
            try
            {
                return Activator.CreateInstance(type, objectContext, false);
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<string> GetNavPropNames(object objectContext, object entity)
        {
            if (entity == null || objectContext == null)
            {
                return null;
            }
            FieldInfo field = entity.GetType().GetField("_entityWrapper");
            if (field == null)
            {
                return null;
            }
            object value = field.GetValue(entity);
            PropertyInfo property = value.GetType().GetProperty("EntityKey");
            if (property == null)
            {
                return null;
            }
            dynamic value2 = property.GetValue(value, null);
            if (value2 == null || value2.GetType().Name != "EntityKey")
            {
                return null;
            }
            dynamic val = ((dynamic)objectContext).MetadataWorkspace;
            Type type = val.GetType();
            if (_getItemMethod == null)
            {
                _getItemMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "TryGetItem" && m.IsGenericMethod && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType.Name == "DataSpace" && m.GetParameters()[2].IsOut);
            }
            if (_getItemMethod == null)
            {
                return null;
            }
            Type type2 = type.Assembly.GetType("System.Data.Entity.Core.Metadata.Edm.EntityContainer");
            if (type2 == null)
            {
                type2 = type.Assembly.GetType("System.Data.Metadata.Edm.EntityContainer");
            }
            if (type2 == null)
            {
                return null;
            }
            MethodInfo methodInfo = _getItemMethod.MakeGenericMethod(type2);
            object[] array = new object[3]
            {
            value2.EntityContainerName,
            1,
            null
            };
            if ((!true.Equals(methodInfo.Invoke(val, array))))
            {
                return null;
            }
            object obj = array[2];
            if (obj == null)
            {
                return null;
            }
            if (_getEntitySetMethod == null || _getEntitySetMethod.ReflectedType != obj.GetType())
            {
                _getEntitySetMethod = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "TryGetEntitySetByName" && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].ParameterType == typeof(bool) && m.GetParameters()[2].IsOut);
            }
            if (_getEntitySetMethod == null)
            {
                return null;
            }
            array = new object[3]
            {
            value2.EntitySetName,
            false,
            null
            };
            if (!true.Equals(_getEntitySetMethod.Invoke(obj, array)))
            {
                return null;
            }
            dynamic val2 = array[2];
            if (val2 == null)
            {
                return null;
            }
            return ((IEnumerable<object>)val2.ElementType.NavigationProperties).Select((dynamic np) => np.Name).OfType<string>();
        }

        private Type GetFieldPropType(MemberInfo m)
        {
            if (m is FieldInfo)
            {
                return ((FieldInfo)m).FieldType;
            }
            if (m is PropertyInfo)
            {
                return ((PropertyInfo)m).PropertyType;
            }
            throw new InvalidOperationException("Expected FieldInfo or PropertyInfo");
        }

        private object GetFieldPropValue(object value, MemberInfo m)
        {
            try
            {
                if (m is FieldInfo)
                {
                    return ((FieldInfo)m).GetValue(value);
                }
                if (m is PropertyInfo)
                {
                    return ((PropertyInfo)m).GetValue(value, null);
                }
                throw new InvalidOperationException("Expected FieldInfo or PropertyInfo");
            }
            catch (Exception result)
            {
                return result;
            }
        }
    }
}