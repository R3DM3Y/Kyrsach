using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using Kyrsach.Models;
using MessageBox.Avalonia;

namespace Kyrsach;

public partial class ServicePhotosWindow : Window
{
    private readonly int _serviceId;
    private readonly string _photosDirectory = Path.Combine("wwwroot", "service_photos");

    public ServicePhotosWindow()
    {
        InitializeComponent();
    }

    public ServicePhotosWindow(int serviceId) : this()
    {
        _serviceId = serviceId;
        Directory.CreateDirectory(_photosDirectory);
        LoadPhotos();
    }

    private void LoadPhotos()
    {
        PhotosPanel.Children.Clear();
        
        using var context = new PostgresContext();
        var photos = context.ServicePhotos
            .Where(sp => sp.ServiceId == _serviceId)
            .ToList();

        foreach (var photo in photos)
        {
            var photoControl = CreatePhotoControl(photo);
            PhotosPanel.Children.Add(photoControl);
        }
    }

    private StackPanel CreatePhotoControl(ServicePhoto photo)
    {
        var panel = new StackPanel 
        { 
            Margin = new Thickness(10),
            Width = 200
        };
        
        try
        {
            var image = new Image
            {
                Source = new Bitmap(photo.PhotoPath),
                Height = 150,
                Stretch = Stretch.UniformToFill
            };
            
            var deleteBtn = new Button
            {
                Content = "Удалить",
                Tag = photo.Id,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            deleteBtn.Click += DeletePhoto_Click;
            
            panel.Children.Add(image);
            panel.Children.Add(deleteBtn);
        }
        catch (Exception)
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = "Не удалось загрузить фото",
                TextWrapping = TextWrapping.Wrap
            });
        }
        
        return panel;
    }

    private async void AddPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото услуги",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Изображения", Extensions = { "jpg", "jpeg", "png" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        if (result?.Length > 0)
        {
            var sourcePath = result[0];
            var ext = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var destPath = Path.Combine(_photosDirectory, fileName);

            try
            {
                File.Copy(sourcePath, destPath);
                
                using var context = new PostgresContext();
                context.ServicePhotos.Add(new ServicePhoto
                {
                    ServiceId = _serviceId,
                    PhotoPath = destPath
                });
                await context.SaveChangesAsync();
                
                LoadPhotos();
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка", ex.Message);
            }
        }
    }

    private async void DeletePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: int photoId })
        {
            using var context = new PostgresContext();
            var photo = await context.ServicePhotos.FindAsync(photoId);
            if (photo != null)
            {
                try
                {
                    if (File.Exists(photo.PhotoPath))
                        File.Delete(photo.PhotoPath);
                    
                    context.ServicePhotos.Remove(photo);
                    await context.SaveChangesAsync();
                    LoadPhotos();
                }
                catch (Exception ex)
                {
                    await MessageBoxService.ShowError(this, "Ошибка", ex.Message);
                }
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}