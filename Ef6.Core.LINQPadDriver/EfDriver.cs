using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using LINQPad;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver
{
    // ReSharper disable once UnusedMember.Global
    public class EfDriver : StaticDataContextDriver
    {
        private ILookup<Type, Type> entityTypes;

        static EfDriver()
        {
            EnableDebugExceptions();
        }

        public override string Author => "Peter Butzhammer";

        public override string Name => "Enitiy Framework 6 on .Net Core";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
            => cxInfo.CustomTypeInfo.GetCustomTypeDescription();

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            if (string.IsNullOrEmpty(cxInfo.DatabaseInfo.CustomCxString))
                return Array.Empty<object>();

            return new object[]
            {
                cxInfo.DatabaseInfo.CustomCxString
            };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            if (string.IsNullOrEmpty(cxInfo.DatabaseInfo.CustomCxString))
                return Array.Empty<ParameterDescriptor>();

            return new ParameterDescriptor[1]
            {
                new ParameterDescriptor("param", "System.String")
            };
        }

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            // Return all DbSet<T> properties of the DbContext
            var topLevelProps =
                customType
                    .GetDbSetProperties()
                    .Select(p => new ExplorerItem(p.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                    {
                        IsEnumerable = true,
                        ToolTipText = FormatTypeName(p.PropertyType, false),

                        // Store the entity type to the Tag property. We'll use it later.
                        Tag = p.PropertyType.GetGenericArguments().First()
                    })
                    .ToList();

            // Create a lookup keying each element type to the properties of that type. This will allow
            // us to build hyperlink targets allowing the user to click between associations:
            var elementTypeLookup = topLevelProps.ToLookup(tp => (Type)tp.Tag);

            // Populate the columns (properties) of each entity:
            foreach (var table in topLevelProps)
            {
                var parentType = (Type)table.Tag;
                var props = parentType.GetProperties().Select(p => GetChildItem(elementTypeLookup, p.Name, p.PropertyType));
                var fields = parentType.GetFields().Select(f => GetChildItem(elementTypeLookup, f.Name, f.FieldType));
                table.Children = props.Union(fields).OrderBy(childItem => childItem.Kind).ToList();
            }

            return topLevelProps;
        }



        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
            => new ConnectionDialog(cxInfo).ShowDialog() == true;

        [Conditional("DEBUG")]
        private static void EnableDebugExceptions()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                if (args.Exception.StackTrace?.Contains(typeof(EfDriver).Namespace ?? string.Empty) == true)
                    Helpers.Debug();
            };
        }

        private static ExplorerItem GetChildItem(ILookup<Type, ExplorerItem> elementTypeLookup, string childPropName, Type childPropType)
        {
            // If the property's type is in our list of entities, then it's a Many:1 (or 1:1) reference.
            // We'll assume it's a Many:1 (we can't reliably identify 1:1s purely from reflection).
            if (elementTypeLookup.Contains(childPropType))
                return new ExplorerItem(childPropName, ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne)
                {
                    HyperlinkTarget = elementTypeLookup[childPropType].First(),
                    // FormatTypeName is a helper method that returns a nicely formatted type name.
                    ToolTipText = FormatTypeName(childPropType, true)
                };

            // Is the property's type a collection of entities?
            var ienumerableOfT = childPropType.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (ienumerableOfT != null)
            {
                var elementType = ienumerableOfT.GetGenericArguments()[0];
                if (elementTypeLookup.Contains(elementType))
                    return new ExplorerItem(childPropName, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany)
                    {
                        HyperlinkTarget = elementTypeLookup[elementType].First(),
                        ToolTipText = FormatTypeName(elementType, true)
                    };
            }

            // Ordinary property:
            return new ExplorerItem(childPropName + " (" + FormatTypeName(childPropType, false) + ")",
                                    ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        public override void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            if (objectToWrite == null) return;
            if (entityTypes == null) return;

            var entity = objectToWrite;
            var type = entity.GetType();
            var baseTypes = Helpers.Descend(type, (Type x) => x.BaseType);

            if (!entityTypes.Contains(type) && !baseTypes.Any(t => entityTypes.Contains(t))) return;
            
            //entity is an entity

            var publicProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            string.Join(", ", publicProperties.Select(p => p.Name)).Dump();
            IDictionary<string,object> result = new ExpandoObject();
            foreach (var prop in publicProperties)
            {
                
                if (prop.PropertyType.IsSimpleType())
                    result[prop.Name] = prop.GetValue(entity);
                else
                    result[prop.Name] = "<complex>";
            }

            objectToWrite = result;
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            base.InitializeContext(cxInfo, context, executionManager);

            entityTypes = 
            context.GetType()
                .GetDbSetTypes()
                .ToLookup(t => t);
        }
    }
}