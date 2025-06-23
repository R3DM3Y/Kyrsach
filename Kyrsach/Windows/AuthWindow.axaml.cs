using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Services;

namespace Kyrsach.Windows;

public partial class AuthWindow : Window
{
    private const string AdminPassword = "0000"; // Стандартный пароль
    
    public bool IsAdmin { get; private set; }
    public bool IsAuthenticated { get; private set; }

    public AuthWindow()
    {
        InitializeComponent();
    }

    private void UserModeButton_Click(object sender, RoutedEventArgs e)
    {
        IsAdmin = false;
        IsAuthenticated = true;
        Close();
    }

    private void AdminModeButton_Click(object sender, RoutedEventArgs e)
    {
        PasswordPanel.IsVisible = true;
    }

    private void AdminLoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordBox.Text == AdminPassword)
        {
            IsAdmin = true;
            IsAuthenticated = true;
            Close();
        }
        else
        {
            MessageBoxService.ShowError(this, "Ошибка", "Неверный пароль");
            return;
        }
    }
}