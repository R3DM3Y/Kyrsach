using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Models;

namespace Kyrsach
{
    public partial class EditServiceWindow : Window
    {
        // Конструктор по умолчанию для XAML-парсера
        public EditServiceWindow()
        {
            InitializeComponent();
        }

        // Основной конструктор для использования в коде
        public EditServiceWindow(ServicePresenter service) : this()
        {
            EditedService = service;
            DataContext = this;
            
            // Сохраняем оригинальные значения для отслеживания изменений
            _originalService = new Service
            {
                Id = service.Id,
                Title = service.Title,
                Cost = service.Cost,
                DurationInMinutes = service.DurationInMinutes,
                Discount = service.Discount,
            };
        }

        public ServicePresenter EditedService { get; }
        private readonly Service _originalService;

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значения из формы
            decimal basePrice = (decimal)PriceUpDown.Value; // Цена БЕЗ скидки
            double? discount = DiscountUpDown.Value.HasValue 
                ? (double?)DiscountUpDown.Value.Value 
                : null;

            // Просто сохраняем как есть (цена без скидки)
            EditedService.Cost = basePrice;
            EditedService.Discount = discount;
            if (string.IsNullOrWhiteSpace(EditedService.Title))
            {
                return;
            }

            try
            {
                using var context = new PostgresContext();
                var dbService = await context.Services.FindAsync(EditedService.Id);
                if (dbService != null)
                {
                    dbService.Title = EditedService.Title;
                    dbService.Cost = EditedService.Cost;
                    dbService.DurationInMinutes = EditedService.DurationInMinutes;
                    dbService.Discount = EditedService.Discount;
                    
                    await context.SaveChangesAsync();
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                Close(false);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}