using System;
using System.IO;
using System.Linq;
using System.Windows;
using LINQPad.Extensibility.DataContext;

namespace Ef6.Core.LINQPadDriver
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        IConnectionInfo _cxInfo;

        public ConnectionDialog(IConnectionInfo cxInfo)
        {
            _cxInfo = cxInfo;
            DataContext = cxInfo;
            InitializeComponent();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void BrowseAssembly(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Choose custom assembly",
                DefaultExt = ".dll",
            };

            if (dialog.ShowDialog() == true)
                _cxInfo.CustomTypeInfo.CustomAssemblyPath = dialog.FileName;
        }

        void BrowseAppConfig(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Choose application config file",
                DefaultExt = ".config",
            };

            if (dialog.ShowDialog() == true)
                _cxInfo.AppConfigPath = dialog.FileName;
        }

        void ChooseType(object sender, RoutedEventArgs e)
        {
            string assemPath = _cxInfo.CustomTypeInfo.CustomAssemblyPath;
            if (assemPath.Length == 0)
            {
                MessageBox.Show("First enter a path to an assembly.");
                return;
            }

            if (!File.Exists(assemPath))
            {
                MessageBox.Show("File '" + assemPath + "' does not exist.");
                return;
            }

            string[] customTypes;
            try
            {
                customTypes = _cxInfo.CustomTypeInfo.GetCustomTypesInAssembly("System.Data.Entity.DbContext");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error obtaining custom types: " + ex.Message);
                return;
            }
            if (customTypes.Length == 0)
            {
                MessageBox.Show("There are no public types based on \"System.Data.Entity.DbContext\" in that assembly.");
                return;
            }

            string result = (string)LINQPad.Extensibility.DataContext.UI.Dialogs.PickFromList("Choose Custom Type", customTypes);
            if (result != null) _cxInfo.CustomTypeInfo.CustomTypeName = result;
        }
    }
}
