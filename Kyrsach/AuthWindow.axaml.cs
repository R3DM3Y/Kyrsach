using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Kyrsach;

public partial class AuthWindow : Window
{
    public bool IsAdmin { get; private set; }
    public bool IsAuthenticated { get; private set; }

    public AuthWindow()
    {
        InitializeComponent();
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        IsAdmin = (CodeTextBox.Text == "0000");
        IsAuthenticated = true;
        Close();
    }
}