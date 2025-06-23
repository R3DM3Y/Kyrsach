using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Kyrsach.Data;
using Kyrsach.Models;
using Kyrsach.Services;

namespace Kyrsach.Windows
{
    public partial class EditServiceWindow : Window
    {
        private string _newImagePath;
        private readonly string _imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "service_images");
        private readonly ServicePresenter _editedService;
        private readonly string _originalImagePath;
        
        public EditServiceWindow()
        {
            InitializeComponent();
        }

        public EditServiceWindow(ServicePresenter service) : this() // Вызываем конструктор без параметров
        {
            _editedService = service;
            _originalImagePath = service.MainImagePath;
            DataContext = service;

            if (!string.IsNullOrEmpty(service.MainImagePath) && File.Exists(service.MainImagePath))
            {
                LoadImagePreview(service.MainImagePath);
            }
        }

        private async void SelectMainImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите основное изображение",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Изображения", Extensions = { "jpg", "jpeg", "png", "bmp" } }
                }
            };

            var result = await dialog.ShowAsync(this);
            if (result?.Length > 0)
            {
                try
                {
                    // Проверка валидности изображения
                    using (var testImage = new Bitmap(result[0]))
                    {
                        if (testImage.PixelSize.Width == 0 || testImage.PixelSize.Height == 0)
                        {
                            await MessageBoxService.ShowError(this, "Ошибка", "Выбранный файл не является допустимым изображением");
                            return;
                        }
                    }

                    _newImagePath = result[0];
                    LoadImagePreview(_newImagePath);
                }
                catch (Exception ex)
                {
                    await MessageBoxService.ShowError(this, "Ошибка", $"Не удалось загрузить изображение: {ex.Message}");
                }
            }
        }

        private void LoadImagePreview(string imagePath)
        {
            try
            {
                MainImagePreview.Source = new Bitmap(imagePath);
            }
            catch (Exception)
            {
                MainImagePreview.Source = null;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(_editedService.Title))
            {
                await MessageBoxService.ShowError(this, "Ошибка", "Название услуги не может быть пустым");
                return;
            }

            try
            {
                using var context = new PostgresContext();
                var dbService = await context.Services.FindAsync(_editedService.Id);
                if (dbService != null)
                {
                    // Обработка изображения
                    if (!string.IsNullOrEmpty(_newImagePath))
                    {
                        Directory.CreateDirectory(_imagesDirectory);
                        var ext = Path.GetExtension(_newImagePath);
                        var newFileName = $"{Guid.NewGuid()}{ext}";
                        var destPath = Path.Combine(_imagesDirectory, newFileName);

                        // Копируем новое изображение
                        File.Copy(_newImagePath, destPath);

                        // Удаляем старое изображение (если оно было и это не то же самое изображение)
                        if (!string.IsNullOrEmpty(_originalImagePath) && 
                            File.Exists(_originalImagePath) && 
                            !_originalImagePath.Equals(destPath, StringComparison.OrdinalIgnoreCase))
                        {
                            try { File.Delete(_originalImagePath); }
                            catch { /* Логируем ошибку удаления */ }
                        }

                        dbService.MainImagePath = destPath;
                        _editedService.MainImagePath = destPath;
                    }

                    // Обновляем остальные данные
                    dbService.Title = _editedService.Title;
                    dbService.Cost = _editedService.Cost;
                    dbService.DurationInMinutes = _editedService.DurationInMinutes;
                    dbService.Discount = _editedService.Discount;
                    dbService.Description = _editedService.Description;

                    await context.SaveChangesAsync();
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка сохранения", ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Очистка ресурсов
            (MainImagePreview.Source as Bitmap)?.Dispose();
            base.OnClosed(e);
        }
    }
}