using System.Diagnostics;

namespace Ef6.Core.LINQPadDriver;

public static class Helpers
{
    [Conditional("DEBUG")]
    public static void Debug()
    {
        if (!Debugger.IsAttached)
            Debugger.Launch();
    }
}
