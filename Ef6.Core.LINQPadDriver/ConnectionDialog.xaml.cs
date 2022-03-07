using System;
using System.IO;
using System.Windows;
using LINQPad.Extensibility.DataContext;
using LINQPad.Extensibility.DataContext.UI;
using Microsoft.Win32;

namespace Ef6.Core.LINQPadDriver;

/// <summary>
///     Interaction logic for ConnectionDialog.xaml
/// </summary>
public partial class ConnectionDialog : Window
{
    readonly IConnectionInfo _cxInfo;

    public ConnectionDialog(IConnectionInfo cxInfo)
    {
        _cxInfo = cxInfo;
        DataContext = cxInfo;
        InitializeComponent();
    }

    void BrowseAppConfig(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose application config file",
            DefaultExt = ".config"
        };

        if (dialog.ShowDialog() == true)
            _cxInfo.AppConfigPath = dialog.FileName;
    }

    void BrowseAssembly(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose custom assembly",
            DefaultExt = ".dll"
        };

        if (dialog.ShowDialog() == true)
            _cxInfo.CustomTypeInfo.CustomAssemblyPath = dialog.FileName;
    }

    void btnOK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    void ChooseType(object sender, RoutedEventArgs e)
    {
        var assemPath = _cxInfo.CustomTypeInfo.CustomAssemblyPath;
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

        var result = (string) Dialogs.PickFromList("Choose Custom Type", customTypes);
        if (result != null) _cxInfo.CustomTypeInfo.CustomTypeName = result;
    }
}
