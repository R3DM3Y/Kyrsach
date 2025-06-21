using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using Avalonia;

namespace Kyrsach
{
    public static class MessageBoxService
    {
        public static async Task<bool> ShowConfirmation(Window parent, string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var yesButton = new Button { Content = "Да", Margin = new Thickness(5) };
            var noButton = new Button { Content = "Нет", Margin = new Thickness(5) };

            yesButton.Click += (_, __) => dialog.Close(true);
            noButton.Click += (_, __) => dialog.Close(false);

            dialog.Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Margin = new Thickness(20) },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children = { yesButton, noButton }
                    }
                }
            };

            return await dialog.ShowDialog<bool>(parent);
        }
        
        public static async Task ShowError(Window parent, string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var okButton = new Button { Content = "OK", Margin = new Thickness(5) };
            okButton.Click += (_, __) => dialog.Close();

            dialog.Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Margin = new Thickness(20) },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children = { okButton }
                    }
                }
            };

            await dialog.ShowDialog(parent);
        }
    }
}