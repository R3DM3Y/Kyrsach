using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Kyrsach.Models;
using ReactiveUI;

namespace Kyrsach
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            using var context = new PostgresContext();
            var services = context.Services
                .Select(service => new ServicePresenter(service))
                .ToList();

            ServiceBox.ItemsSource = new ObservableCollection<ServicePresenter>(services);
        }
    }
}

public class ServicePresenter : Service
{
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

        EditCommand = ReactiveCommand.Create(Edit);
        DeleteCommand = ReactiveCommand.Create(Delete);
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

    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
}
