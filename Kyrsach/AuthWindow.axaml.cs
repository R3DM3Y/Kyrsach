using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Kyrsach;

public partial class AuthWindow : Window
{
    public bool IsAuthenticated { get; private set; }
    public bool IsAdmin { get; private set; }

    public AuthWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        IsAdmin = (CodeTextBox.Text == "0000"); // Проверка кода
        IsAuthenticated = true; // Флаг успешной авторизации
        Close();
    }
    
    
}