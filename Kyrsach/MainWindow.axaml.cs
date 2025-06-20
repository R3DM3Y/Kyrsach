using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
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
            DiscountFilterComboBox.SelectedIndex = 0; // Устанавливаем "Все скидки" по умолчанию
            DiscountFilterComboBox.SelectionChanged += (s, e) => ApplyFilters();
        }

        private void LoadServices()
        {
            using var context = new PostgresContext();
            var services = context.Services
                .Select(service => new ServicePresenter(service))
                .ToList();

            _allServices = new ObservableCollection<ServicePresenter>(services);
            ServiceBox.ItemsSource = _allServices; // Изначально показываем все услуги
        }
        private void ApplyFilters()
        {
            var filtered = _allServices.AsEnumerable();

            switch (DiscountFilterComboBox.SelectedIndex)
            {
                case 1: filtered = filtered.Where(s => s.Discount >= 0 && s.Discount < 6); break;
                case 2: filtered = filtered.Where(s => s.Discount >= 5 && s.Discount < 16); break;
                case 3: filtered = filtered.Where(s => s.Discount >= 15 && s.Discount < 31); break;
                case 4: filtered = filtered.Where(s => s.Discount >= 30 && s.Discount < 71); break;
                case 5: filtered = filtered.Where(s => s.Discount >= 70 && s.Discount <= 100); break;
                // case 0: "Все скидки" - оставляем как есть
            }

            ServiceBox.ItemsSource = new ObservableCollection<ServicePresenter>(filtered);
        }
        private ObservableCollection<ServicePresenter> _allServices = new();
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

    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
}
