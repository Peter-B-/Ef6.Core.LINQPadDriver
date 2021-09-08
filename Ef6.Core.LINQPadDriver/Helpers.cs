using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using LINQPad;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver
{
    public static class Helpers
    {
        public static IEnumerable<T> Descend<T>(T item, Func<T, T> descendFunc) where T : class
        {
            while (item != null)
            {
                yield return item;
                item = descendFunc(item);
            }
        }

        [Conditional("DEBUG")]
        public static void Debug()
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();
        }
    }
}