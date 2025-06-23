using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Kyrsach;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;

public class ServicePresenter : Service
{
    public bool IsDeleted { get; private set; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand BookCommand { get; }
    public bool IsAdmin { get; set; }
    public ICommand OpenPhotosCommand { get; }
    
    private readonly Window _parentWindow;

    public ServicePresenter(Service service, Window parentWindow)
    {
        _parentWindow = parentWindow;
        
        Id = service.Id;
        MainImagePath = service.MainImagePath ?? string.Empty;
        Title = service.Title ?? string.Empty;
        Cost = service.Cost;
        DurationInMinutes = service.DurationInMinutes;
        Discount = service.Discount ?? 0;
        Description = service.Description ?? string.Empty;

        EditCommand = new RelayCommand(_ => OnEditClicked(), _ => IsAdmin);
        DeleteCommand = new RelayCommand(async _ => await OnDeleteClicked(), _ => IsAdmin && !IsDeleted);
        BookCommand = new RelayCommand(_ => BookAppointment(), _ => IsAdmin);
        OpenPhotosCommand = new RelayCommand(_ => OpenPhotosWindow(parentWindow));
    }

    private void BookAppointment()
    {
        if (_parentWindow != null)
        {
            var service = new Service
            {
                Id = this.Id,
                Title = this.Title,
                DurationInMinutes = this.DurationInMinutes
            };
            
            var window = new AddAppointmentWindow(service);
            window.ShowDialog(_parentWindow);
        }
    }
    
    private void OpenPhotosWindow(Window parentWindow)
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            new ServicePhotosWindow(this.Id).ShowDialog(parentWindow);
        }
    }

    public bool HasDiscount => Discount > 0;
    public decimal FinalPrice => HasDiscount ? Cost * (decimal)(1 - Discount / 100.0) : Cost;
    public string PriceDisplayText => HasDiscount ? $"{Cost:0} руб. → {FinalPrice:0} руб. (-{Discount}%)" : $"{Cost:0} руб.";
    public string DurationText => $"{DurationInMinutes} мин";
    public IBrush Background => HasDiscount ? new SolidColorBrush(Color.FromRgb(41, 176, 0)) : Brushes.Transparent;

    private async void OnEditClicked()
    {
        if (_parentWindow != null)
        {
            var editWindow = new EditServiceWindow(this);
            if (await editWindow.ShowDialog<bool>(_parentWindow))
            {
                ServiceEdited?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private async Task OnDeleteClicked()
    {
        if (_parentWindow == null) return;
        
        bool confirm = await MessageBoxService.ShowConfirmation(_parentWindow, 
            "Подтверждение", 
            $"Удалить услугу '{Title}'?");
        
        if (!confirm) return;

        try
        {
            using var context = new PostgresContext();
            if (await context.ClientServices.AnyAsync(cs => cs.ServiceId == Id))
            {
                await MessageBoxService.ShowError(_parentWindow, 
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
            await MessageBoxService.ShowError(_parentWindow, 
                "Ошибка", 
                $"Ошибка удаления: {ex.Message}");
        }
    }

    public Bitmap? Image => string.IsNullOrEmpty(MainImagePath) ? null : GetImage();
    
    private Bitmap? GetImage()
    {
        try { return new Bitmap(MainImagePath); }
        catch { return null; }
    }

    public static event EventHandler? ServiceEdited;
    public static event EventHandler? ServiceDeleted;
}