using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace Kyrsach
{
    public partial class AddServiceWindow : Window
    {
        public AddServiceWindow()
        {
            InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                await MessageBoxService.ShowError(this, "Ошибка", "Название услуги не может быть пустым!");
                return;
            }

            if (DurationUpDown.Value <= 0 || DurationUpDown.Value > 240)
            {
                await MessageBoxService.ShowError(this, "Ошибка", "Длительность должна быть от 1 до 240 минут!");
                return;
            }

            try
            {
                using var context = new PostgresContext();
                
                // Проверка уникальности названия
                if (await context.Services.AnyAsync(s => s.Title == TitleTextBox.Text))
                {
                    await MessageBoxService.ShowError(this, "Ошибка", "Услуга с таким названием уже существует!");
                    return;
                }

                // Создание новой услуги
                var newService = new Service
                {
                    Title = TitleTextBox.Text,
                    Cost = (decimal)PriceUpDown.Value,
                    DurationInMinutes = (int)DurationUpDown.Value,
                    Discount = (double?)DiscountUpDown.Value,
                    MainImagePath = _selectedImagePath
                };

                context.Services.Add(newService);
                await context.SaveChangesAsync();
                
                Close(true); // Успешное сохранение
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка", $"Не удалось добавить услугу: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false); // Отмена
        }
        
        private string? _selectedImagePath;

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Изображения", Extensions = { "jpg", "jpeg", "png", "bmp" } }
                },
                AllowMultiple = false
            };

            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                _selectedImagePath = result[0];
                try
                {
                    PreviewImage.Source = new Bitmap(_selectedImagePath);
                }
                catch
                {
                    // Используем MessageBoxManager или TopLevel.GetTopLevel(this).ShowMessageBox()
                    var topLevel = TopLevel.GetTopLevel(this) as Window;
                    await MessageBoxService.ShowError(topLevel, "Ошибка", "Неверный формат изображения");
                    _selectedImagePath = null;
                    PreviewImage.Source = null;
                }
            }
        }
    }
}