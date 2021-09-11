using System;
using System.Collections;
using System.Linq;
using LINQPad;

namespace Ef6.Core.LINQPadDriver
{
    /// <summary>
    ///     Decompiled from LINQPad.Util class of LINQPad 5 by Joseph Albahari
    /// </summary>
    public static class InternalUtil
    {
        internal static DumpContainer OnDemand<T>(string description, Func<T> funcToEvalAndDump, bool runOnNewThread, bool isCollection)
        {
            var dc = new DumpContainer();
            var hyperlinq = new Hyperlinq(delegate
            {
                dc.Content = "Executing...";
                dc.Content = funcToEvalAndDump();
            }, description, runOnNewThread);
            if (isCollection || typeof(IEnumerable).IsAssignableFrom(typeof(T))) hyperlinq.CssClass = "collection";
            dc.Content = hyperlinq;
            return dc;
        }
    }
}
