using System;
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
        [Conditional("DEBUG")]
        public static void Debug()
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();
        }
    }
}