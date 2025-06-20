using System;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Kyrsach.Models;

namespace Kyrsach;

public class ServicePresenter : Service
{
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public bool IsAdmin { get; set; }
    public ServicePresenter(Service service)
    {
        // Инициализация свойств
        Id = service.Id;
        MainImagePath = service.MainImagePath ?? string.Empty;
        Title = service.Title ?? string.Empty;
        Cost = service.Cost;
        DurationInMinutes = service.DurationInMinutes;
        Discount = service.Discount;
        Description = service.Description ?? string.Empty;

        EditCommand = new RelayCommand(_ => OnEditClicked(), _ => IsAdmin);
        DeleteCommand = new RelayCommand(_ => OnDeleteClicked(), _ => IsAdmin);
    }
    
    private void OnEditClicked()
    {
        Console.WriteLine($"Редактирование: {Title}");
        // Реальная логика будет добавлена позже
    }

    private void OnDeleteClicked()
    {
        Console.WriteLine($"Удаление: {Title}");
        // Реальная логика будет добавлена позже
    }
    
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