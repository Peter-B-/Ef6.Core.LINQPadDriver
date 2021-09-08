using LINQPad;
using LINQPad.Extensibility.DataContext;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ef6.Core.LINQPadDriver
{
	public class StaticDriver : StaticDataContextDriver
	{
		static StaticDriver()
		{
			// Uncomment the following code to attach to Visual Studio's debugger when an exception is thrown:
			//AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
			//{
			//	if (args.Exception.StackTrace.Contains ("Ef6.Core.LINQPadDriver"))
			//		Debugger.Launch ();
			//};
		}
	
		public override string Name => "(Name for your driver)";

		public override string Author => "(Your name)";

		public override string GetConnectionDescription (IConnectionInfo cxInfo)
			=> "(Description for this connection)";

		public override bool ShowConnectionDialog (IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
			=> new ConnectionDialog (cxInfo).ShowDialog () == true;

		public override List<ExplorerItem> GetSchema (IConnectionInfo cxInfo, Type customType)
		{
			// TODO - implement
			return new ExplorerItem[0].ToList();
		}
	}
}