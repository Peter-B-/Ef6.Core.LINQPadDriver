using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver
{
    // ReSharper disable once UnusedMember.Global
    public class EfDriver : StaticDataContextDriver
    {
        static EfDriver()
        {
            EnableDebugExceptions();
        }

        public override string Author => "Peter Butzhammer";

        public override string Name => "Enitiy Framework 6 on .Net Core";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
            => cxInfo.CustomTypeInfo.GetCustomTypeDescription();

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
            => new object[]
            {
                cxInfo.DatabaseInfo.GetCxString()
            };

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
            => new[]
            {
                new ParameterDescriptor("connectionString", "System.String")
            };

        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            // Return all DbSet<T> properties of the DbContext
            var topLevelProps =
                customType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .OrderBy(p => p.Name)
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
        private static void Debug()
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();
        }

        [Conditional("DEBUG")]
        private static void EnableDebugExceptions()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                if (args.Exception.StackTrace?.Contains(typeof(EfDriver).Namespace ?? string.Empty) == true)
                    Debug();
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
    }
}