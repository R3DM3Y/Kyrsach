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

    private void OnEditClicked()
    {
        // Получаем главное окно через визуальное дерево
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var editWindow = new EditServiceWindow(this);
            editWindow.ShowDialog(desktop.MainWindow).ContinueWith(t =>
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public static event EventHandler? ServiceEdited;


    private async Task OnDeleteClicked()
    {
        // Получаем главное окно через визуальное дерево
        var control = new Control(); // Временный элемент для доступа к TopLevel
        var topLevel = control.GetVisualRoot() as Window;
        if (topLevel == null) return;

        bool confirm = await MessageBoxService.ShowConfirmation(
            topLevel,
            "Подтверждение удаления",
            $"Удалить услугу '{Title}'?");

        if (!confirm) return;

        try
        {
            using var context = new PostgresContext();
            bool hasBookings = await context.ClientServices
                .AnyAsync(cs => cs.ServiceId == Id);
            
            if (hasBookings)
            {
                await MessageBoxService.ShowConfirmation(
                    topLevel,
                    "Ошибка",
                    "Нельзя удалить услугу с активными записями");
                return;
            }

            var service = await context.Services.FindAsync(Id);
            if (service != null)
            {
                context.Services.Remove(service);
                await context.SaveChangesAsync();
                IsDeleted = true;
                ServiceDeleted?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxService.ShowConfirmation(
                topLevel,
                "Ошибка",
                $"Ошибка удаления: {ex.Message}");
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