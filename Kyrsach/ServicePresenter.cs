using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Kyrsach;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq; // для LINQ методов


public class ServicePresenter : Service
{
    public bool IsDeleted { get; private set; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public bool IsAdmin { get; set; }

    public ServicePresenter(Service service)
    {
        Id = service.Id;
        MainImagePath = service.MainImagePath ?? string.Empty;
        Title = service.Title ?? string.Empty;
        Cost = service.Cost;
        DurationInMinutes = service.DurationInMinutes;
        Discount = service.Discount ?? 0;
        Description = service.Description ?? string.Empty;

        EditCommand = new RelayCommand(_ => OnEditClicked(), _ => IsAdmin);
        DeleteCommand = new RelayCommand(async _ => await OnDeleteClicked(), _ => IsAdmin && !IsDeleted);
    }

    private async void OnEditClicked()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var editWindow = new EditServiceWindow(this);
            bool result = await editWindow.ShowDialog<bool>(desktop.MainWindow);
        
            if (result)
            {
                // Обновляем текущий объект
                using var context = new PostgresContext();
                var updatedService = await context.Services.FindAsync(this.Id);
                if (updatedService != null)
                {
                    this.Title = updatedService.Title;
                    this.Cost = updatedService.Cost;
                    this.DurationInMinutes = updatedService.DurationInMinutes;
                    this.Discount = updatedService.Discount;
                    this.Description = updatedService.Description;
                
                    ServiceEdited?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public static event EventHandler? ServiceEdited;


    private async Task OnDeleteClicked()
    {
        var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
            ? desktop.MainWindow 
            : null;
        if (topLevel == null) return;

        try
        {
            bool confirm = await MessageBoxService.ShowConfirmation(
                topLevel,
                "Подтверждение удаления",
                $"Удалить услугу '{Title}'?");

            if (!confirm) return;

            using var context = new PostgresContext();
        
            // Проверяем есть ли записи на эту услугу
            bool hasBookings = await context.ClientServices
                .AnyAsync(cs => cs.ServiceId == Id);
        
            if (hasBookings)
            {
                await MessageBoxService.ShowError(
                    topLevel,
                    "Ошибка",
                    "Нельзя удалить услугу с активными записями");
                return;
            }

            // Удаляем дополнительные фото если есть
            var photos = await context.ServicePhotos
                .Where(sp => sp.ServiceId == Id)
                .ToListAsync();
        
            context.ServicePhotos.RemoveRange(photos);
        
            // Удаляем саму услугу
            var service = await context.Services.FindAsync(Id);
            if (service != null)
            {
                context.Services.Remove(service);
                await context.SaveChangesAsync();
            
                // Уведомляем об удалении
                ServiceDeleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxService.ShowError(
                topLevel,
                "Ошибка удаления",
                $"Произошла ошибка: {ex.Message}");
        }
    }

    public static event EventHandler? ServiceDeleted;
    
    public IBrush Background 
    {
        get => HasDiscount ? Brushes.DarkGreen : Brushes.Transparent;
    }

    public Bitmap? Image => GetImage();

    private Bitmap? GetImage()
    {
        if (string.IsNullOrEmpty(MainImagePath))
            return null;

        try
        {
            return new Bitmap(MainImagePath);
        }
        catch
        {
            return null;
        }
    }

    public bool HasDiscount => Discount > 0;

    public string OldPriceFormatted => $"{OldPrice:0}";
    public string NewPriceFormatted => $"{Cost:0} рублей за {DurationInMinutes} минут";
    public string DiscountFormatted => $"* скидка {Discount}%";
    public decimal OldPrice => Cost / (decimal)(1 - Discount / (double?)100m);

    private void Edit()
    {
        /* Реализация редактирования */
    }

    private void Delete()
    {
        /* Реализация удаления */
    }
}