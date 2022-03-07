using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ef6.Core.LINQPadDriver;

public static class Extensions
{
    public static CustomAttributeData TryGetAttribute<TAttribute>(this MemberInfo memberInfo)
    {
        return memberInfo.CustomAttributes
            .FirstOrDefault(cad => cad.AttributeType == typeof(CategoryAttribute));
    }
}
