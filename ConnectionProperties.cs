using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Ef6.Core.LINQPadDriver
{
	/// <summary>
	/// Wrapper to read/write connection properties. This acts as our ViewModel - we will bind to it in ConnectionDialog.xaml.
	/// </summary>
	class ConnectionProperties
	{
		public IConnectionInfo ConnectionInfo { get; private set; }

		XElement DriverData => ConnectionInfo.DriverData;

		public ConnectionProperties (IConnectionInfo cxInfo)
		{
			ConnectionInfo = cxInfo;
		}

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