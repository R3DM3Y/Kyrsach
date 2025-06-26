using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Models;
using Kyrsach.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Kyrsach.Data;

namespace Kyrsach.Windows;

public partial class AddClientWindow : Window
{
    public AddClientWindow()
    {
        InitializeComponent();
        BirthDatePicker.SelectedDate = DateTimeOffset.Now.AddYears(-18); // Установка по умолчанию
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private async void Save_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // Валидация полей
        var (isValid, error) = ValidationService.ValidateName(LastNameTextBox.Text, "Фамилия");
        if (!isValid) { ErrorText.Text = error; return; }

        (isValid, error) = ValidationService.ValidateName(FirstNameTextBox.Text, "Имя");
        if (!isValid) { ErrorText.Text = error; return; }

        (isValid, error) = ValidationService.ValidatePhone(PhoneTextBox.Text);
        if (!isValid) { ErrorText.Text = error; return; }

        (isValid, error) = ValidationService.ValidateEmail(EmailTextBox.Text);
        if (!isValid) { ErrorText.Text = error; return; }

        DateOnly? birthDate = BirthDatePicker.SelectedDate.HasValue 
            ? DateOnly.FromDateTime(BirthDatePicker.SelectedDate.Value.DateTime)
            : null;
        
        (isValid, error) = ValidationService.ValidateBirthDate(birthDate);
        if (!isValid) { ErrorText.Text = error; return; }

        // Подготовка данных
        var email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();
        var phone = new string(PhoneTextBox.Text.Where(char.IsDigit).ToArray());

        // Проверка уникальности перед созданием клиента
        await using var db = new PostgresContext();
        
        // Проверка уникальности телефона
        if (await db.Clients.AnyAsync(c => c.Phone == phone))
        {
            ErrorText.Text = "Клиент с таким телефоном уже существует";
            return;
        }

        // Проверка уникальности email (если email указан)
        if (!string.IsNullOrEmpty(email) && await db.Clients.AnyAsync(c => c.Email == email))
        {
            ErrorText.Text = "Клиент с таким email уже существует";
            return;
        }

        // Создание клиента
        var client = new Client
        {
            LastName = LastNameTextBox.Text.Trim(),
            FirstName = FirstNameTextBox.Text.Trim(),
            Patronymic = string.IsNullOrWhiteSpace(PatronymicTextBox.Text) ? null : PatronymicTextBox.Text.Trim(),
            Birthday = birthDate,
            Email = email,
            Phone = phone,
            GenderCode = MaleRadioButton.IsChecked == true ? 'м' : 'ж',
            RegistrationDate = DateTime.Now
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync();
        Close();
    }
    catch (Exception ex)
    {
        ErrorText.Text = $"Ошибка сохранения: {ex.Message}";
    }
}
}