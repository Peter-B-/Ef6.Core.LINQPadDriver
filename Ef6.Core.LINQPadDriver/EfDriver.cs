using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LINQPad;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver;

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

    public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
    {
        if (objectToWrite == null)
            return null;
        if (!EntityFrameworkMemberProvider.IsEntity(objectToWrite.GetType()))
            return null;

        var res = new EntityFrameworkMemberProvider(objectToWrite);
        return res;
    }

    public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
    {
        if (customType == null) throw new ArgumentException("No custom type selected. Please check the properties of this connection.");

        // DbSets can be grouped using the System.ComponentModel.CategoryAttribute.
        // DbSets wuthout this Attribute will be returned as top level items.
        var topLevelNodes =
            customType
                .GetDbSetProperties()
                .GroupBy(p => (p.TryGetAttribute<CategoryAttribute>() ??
                                 p.PropertyType.GetDbSetType()?.TryGetAttribute<CategoryAttribute>())
                             ?.ConstructorArguments.First().Value?.ToString()
                )
                .OrderBy(gr => gr.Key == null)
                .ThenBy(gr => gr.Key)
                .SelectMany(gr =>
                {
                    if (gr.Key == null)
                        // No category set. Return individual DbSets as nodes.
                        return gr.Select(CreateExplorerItem);
                    else
                        return new List<ExplorerItem>
                        {
                            // Return single node with DbSets as children
                            new ExplorerItem(gr.Key, ExplorerItemKind.Category, ExplorerIcon.Box)
                            {
                                Children = gr.Select(CreateExplorerItem).ToList(),
                            }
                        };
                })
                .ToList();

        var queryableObjects =
            GetItemsOfKindRecursive(topLevelNodes, ExplorerItemKind.QueryableObject)
                .ToList();

        // Create a lookup keying each element type to the properties of that type. This will allow
        // us to build hyperlink targets allowing the user to click between associations:
        var elementTypeLookup = queryableObjects.ToLookup(tp => (Type) tp.Tag);

        // Populate the columns (properties) of each entity:
        foreach (var table in queryableObjects)
        {
            var parentType = (Type) table.Tag;
            var props = parentType.GetProperties().Select(p => GetChildItem(elementTypeLookup, p.Name, p.PropertyType));
            var fields = parentType.GetFields().Select(f => GetChildItem(elementTypeLookup, f.Name, f.FieldType));
            table.Children = props.Union(fields).OrderBy(childItem => childItem.Kind).ToList();
        }

        return topLevelNodes;
    }

    public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
    {
        base.InitializeContext(cxInfo, context, executionManager);
        if (context is DbContext dbContext) dbContext.Database.Log = executionManager.SqlTranslationWriter.WriteLine;
    }

    public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
        => new ConnectionDialog(cxInfo).ShowDialog() == true;

    private static ExplorerItem CreateExplorerItem(PropertyInfo p)
    {
        return new ExplorerItem(p.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
        {
            IsEnumerable = true,
            ToolTipText = FormatTypeName(p.PropertyType, false),

            // Store the entity type to the Tag property. We'll use it later.
            Tag = p.PropertyType.GetDbSetType()
        };
    }

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

    private static IEnumerable<ExplorerItem> GetItemsOfKindRecursive(IEnumerable<ExplorerItem> items, ExplorerItemKind kind)
    {
        foreach (var item in items)
        {
            if (item.Kind == kind)
                yield return item;
            if (item.Children != null)
                foreach (var child in GetItemsOfKindRecursive(item.Children, kind))
                    yield return child;
        }
    }
}
