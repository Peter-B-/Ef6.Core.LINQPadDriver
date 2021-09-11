using System;
using System.Linq;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver
{
    /// <summary>
    ///     Wrapper to read/write connection properties. This acts as our ViewModel - we will bind to it in
    ///     ConnectionDialog.xaml.
    /// </summary>
    class ConnectionProperties
    {
        public ConnectionProperties(IConnectionInfo cxInfo)
        {
            ConnectionInfo = cxInfo;
        }

        public IConnectionInfo ConnectionInfo { get; }

        XElement DriverData => ConnectionInfo.DriverData;

        // This is how to create custom connection properties.

        //public string SomeStringProperty
        //{
        //	get => (string)DriverData.Element ("SomeStringProperty") ?? "";
        //	set => DriverData.SetElementValue ("SomeStringProperty", value);
        //}

        //public int SomeIntProperty
        //{
        //	get => (int?)DriverData.Element ("SomeIntProperty") ?? 0;
        //	set => DriverData.SetElementValue ("SomeIntProperty", value);
        //}
    }
}
