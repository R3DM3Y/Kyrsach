using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Kyrsach.Models;

namespace Kyrsach
{
    public partial class MainWindow : Window
    {
        private bool _isAdmin = false;
        public MainWindow()
        {
            InitializeComponent();
            ServicePresenter.ServiceEdited += OnServiceEdited;
            ServicePresenter.ServiceDeleted += OnServiceDeleted;
            this.Opened += OnMainWindowOpened; // Подписываемся на событие открытия окна
            LoadServices();
    
            // Подписываемся на все события
            SearchTextBox.TextChanged += OnSearchTextChanged;
            DiscountFilterComboBox.SelectionChanged += (s, e) => ApplyAllFilters();
            SortComboBox.SelectionChanged += (s, e) => ApplyAllFilters();
        }
        private async void OnMainWindowOpened(object sender, EventArgs e)
        {
            this.Opened -= OnMainWindowOpened; // Отписываемся после первого вызова
            await CheckAuthorization();
        }

        private async Task CheckAuthorization()
        {
            var authWindow = new AuthWindow();
            await authWindow.ShowDialog(this); // Теперь окно владельца видимо
        
            _isAdmin = authWindow.IsAdmin;
            UpdateAdminVisibility();
            
            if (!authWindow.IsAuthenticated)
            {
                Close(); // Закрываем приложение, если авторизация отменена
            }
        }

        private void UpdateAdminVisibility()
        {
            AddServiceButton.IsVisible = _isAdmin;
            AppointmentsButton.IsVisible = _isAdmin;
            
            // Для кнопок редактирования/удаления в списке услуг:
            foreach (var item in ServiceBox.Items.OfType<ServicePresenter>())
            {
                item.IsAdmin = _isAdmin;    
            }
        }

        private void LoadServices()
        {
            using var context = new PostgresContext();
            var services = context.Services.ToList();
    
            // Сохраняем все услуги в _allServices
            _allServices = new ObservableCollection<ServicePresenter>(
                services.Select(s => new ServicePresenter(s))
            );
    
            // Обновляем ItemsSource
            ServiceBox.ItemsSource = _allServices;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyAllFilters();
        }
        private void ApplyAllFilters()
        {
            IEnumerable<ServicePresenter> filtered = _allServices;

            // 1. Поиск по тексту
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                var searchText = SearchTextBox.Text.ToLower();
                filtered = filtered.Where(s => 
                    s.Title.ToLower().Contains(searchText) ||
                    (s.Description?.ToLower()?.Contains(searchText) ?? false));
            }

            // 2. Фильтрация по скидке
            switch (DiscountFilterComboBox.SelectedIndex)
            {
                case 1: filtered = filtered.Where(s => s.Discount >= 0 && s.Discount < 5); break;
                case 2: filtered = filtered.Where(s => s.Discount >= 5 && s.Discount < 15); break;
                case 3: filtered = filtered.Where(s => s.Discount >= 15 && s.Discount < 30); break;
                case 4: filtered = filtered.Where(s => s.Discount >= 30 && s.Discount < 70); break;
                case 5: filtered = filtered.Where(s => s.Discount >= 70 && s.Discount <= 100); break;
            }

            // 3. Сортировка
            filtered = SortComboBox.SelectedIndex switch
            {
                1 => filtered.OrderBy(s => s.Cost),
                2 => filtered.OrderByDescending(s => s.Cost),
                _ => filtered // Без сортировки
            };

            // Обновляем список
            ServiceBox.ItemsSource = new ObservableCollection<ServicePresenter>(filtered);
        }
        
        private ObservableCollection<ServicePresenter> _allServices = new();
        
        private void OnServiceDeleted(object? sender, EventArgs e)
        {
            if (sender is ServicePresenter deletedService)
            {
                // Удаляем из основной коллекции
                _allServices.Remove(deletedService);
        
                // Переприменяем фильтры
                ApplyAllFilters();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ServicePresenter.ServiceDeleted -= OnServiceDeleted;
            base.OnClosed(e);
        }
        
        private void OnServiceEdited(object? sender, EventArgs e)
        {
            if (sender is ServicePresenter editedService)
            {
                // Обновляем данные в основной коллекции
                if (ServiceBox.ItemsSource is ObservableCollection<ServicePresenter> items)
                {
                    var existing = items.FirstOrDefault(s => s.Id == editedService.Id);
                    if (existing != null)
                    {
                        // Обновляем все свойства
                        existing.Title = editedService.Title;
                        existing.Cost = editedService.Cost;
                        existing.DurationInMinutes = editedService.DurationInMinutes;
                        existing.Discount = editedService.Discount;
                
                        // Принудительно обновляем привязки
                        items[items.IndexOf(existing)] = existing;
                    }
                }
            }
        }
    }
}