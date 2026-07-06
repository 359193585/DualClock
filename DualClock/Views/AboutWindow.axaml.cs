using Avalonia.Controls;
using System;
using System.Reflection;

namespace DualClock.Views;

public partial class AboutWindow : Window
{

    public AboutWindow()
    {
        InitializeComponent();
        this.DataContext = this;
        this.Icon = App.AppIcon;

        VersionTextBlock.Text = GetExeVersion();
    }

    private static string GetExeVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var versionString = version != null
            ? $"版本 {version.Major}.{version.Minor}.{version.Build}"
            : "版本 2.0.0";

        // 如果希望显示更详细的 InformationalVersion，可以这样：
        //var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        //if (!string.IsNullOrEmpty(infoVersion))
        //{
        //    versionString = infoVersion; // 例如 "2.0.23-beta+sha.abc123"
        //}
        return versionString;
    }

    private void OnOkClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}