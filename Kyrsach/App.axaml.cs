using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Kyrsach;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var authWindow = new Windows.AuthWindow();
            authWindow.Show();
        
            authWindow.Closed += (_, _) => 
            {
                if (authWindow.IsAuthenticated)
                {
                    desktop.MainWindow = new Windows.MainWindow(authWindow.IsAdmin);
                    desktop.MainWindow.Show();
                }
                else
                {
                    desktop.Shutdown();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}