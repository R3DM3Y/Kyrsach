using System;
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
            var services = context.Services
                .Select(service => new ServicePresenter(service))
                .ToList();

            _allServices = new ObservableCollection<ServicePresenter>(services);
            ServiceBox.ItemsSource = _allServices; // Изначально показываем все услуги
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyAllFilters();
        }
        private void ApplyAllFilters()
        {
            var filtered = _allServices.AsEnumerable();

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
            switch (SortComboBox.SelectedIndex)
            {
                case 1: filtered = filtered.OrderBy(s => s.Cost); break;
                case 2: filtered = filtered.OrderByDescending(s => s.Cost); break;
            }

            // Обновляем список
            ServiceBox.ItemsSource = new ObservableCollection<ServicePresenter>(filtered);
        }
        
        private ObservableCollection<ServicePresenter> _allServices = new();
        
        
    }
}