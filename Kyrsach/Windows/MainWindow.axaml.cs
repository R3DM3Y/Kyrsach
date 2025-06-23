using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Kyrsach.Data;
using Kyrsach.Models;
using Kyrsach.Services;

namespace Kyrsach.Windows;

public partial class MainWindow : Window
{
    public bool IsAdmin { get; }
    private ObservableCollection<ServicePresenter> _allServices = new();

    public MainWindow() : this(false) {} // Конструктор по умолчанию для XAML

    public MainWindow(bool isAdmin) // Основной конструктор
    {
        IsAdmin = isAdmin;
        InitializeComponent();
        LoadServices();
        InitializeEventHandlers();
        UpdateAdminVisibility();
    }
    
    

    private void InitializeEventHandlers()
    {
        ServicePresenter.ServiceEdited += OnServiceEdited;
        ServicePresenter.ServiceDeleted += OnServiceDeleted;
        
        SearchTextBox.TextChanged += OnSearchTextChanged;
        DiscountFilterComboBox.SelectionChanged += (s, e) => ApplyAllFilters();
        SortComboBox.SelectionChanged += (s, e) => ApplyAllFilters();
    }

    private void UpdateAdminVisibility()
    {
        AddServiceButton.IsVisible = IsAdmin;
        AppointmentsButton.IsVisible = IsAdmin;
        
        if (ServiceBox.ItemsSource is IEnumerable<ServicePresenter> services)
        {
            foreach (var service in services)
            {
                service.IsAdmin = IsAdmin;
            }
        }
    }

    private void LoadServices()
    {
        using var context = new PostgresContext();
        var services = context.Services.ToList();
    
        // Создаем коллекцию ServicePresenter один раз
        _allServices = new ObservableCollection<ServicePresenter>(
            services.Select(s => new ServicePresenter(s, this) { IsAdmin = IsAdmin }));
    
        ServiceBox.ItemsSource = _allServices;
        UpdateRecordsCounter(_allServices.Count, services.Count);
    }


    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyAllFilters();

    private void ApplyAllFilters()
    {
        if (_allServices == null || !_allServices.Any()) 
            return;

        IEnumerable<ServicePresenter> filtered = _allServices;

        // Фильтрация по поиску
        if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            var searchText = SearchTextBox.Text.ToLower();
            filtered = filtered.Where(s => 
                s.Title.ToLower().Contains((string)searchText) ||
                (s.Description?.ToLower()?.Contains((string)searchText) ?? false));
        }

        // Фильтрация по скидке
        switch (DiscountFilterComboBox.SelectedIndex)
        {
            case 1: filtered = filtered.Where(s => s.Discount >= 0 && s.Discount < 5); break;
            case 2: filtered = filtered.Where(s => s.Discount >= 5 && s.Discount < 15); break;
            case 3: filtered = filtered.Where(s => s.Discount >= 15 && s.Discount < 30); break;
            case 4: filtered = filtered.Where(s => s.Discount >= 30 && s.Discount < 70); break;
            case 5: filtered = filtered.Where(s => s.Discount >= 70 && s.Discount <= 100); break;
        }

        // Сортировка
        filtered = SortComboBox.SelectedIndex switch
        {
            1 => filtered.OrderBy(s => s.Cost),
            2 => filtered.OrderByDescending(s => s.Cost),
            _ => filtered
        };

        // Обновляем отображение
        ServiceBox.ItemsSource = new ObservableCollection<ServicePresenter>(filtered);
        UpdateRecordsCounter(filtered.Count(), _allServices.Count);
    }

    private void OnServiceDeleted(object? sender, EventArgs e)
    {
        if (sender is ServicePresenter deletedService)
        {
            _allServices.Remove(deletedService);
            ApplyAllFilters();
        }
    }

    private void OnServiceEdited(object? sender, EventArgs e)
    {
        if (sender is ServicePresenter editedService && 
            ServiceBox.ItemsSource is ObservableCollection<ServicePresenter> items)
        {
            var existing = items.FirstOrDefault(s => s.Id == editedService.Id);
            if (existing != null)
            {
                existing.Title = editedService.Title;
                existing.Cost = editedService.Cost;
                existing.DurationInMinutes = editedService.DurationInMinutes;
                existing.Discount = editedService.Discount;
                items[items.IndexOf(existing)] = existing;
            }
        }
    }

    private async void AddServiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var addWindow = new Windows.AddServiceWindow();
            if (await addWindow.ShowDialog<bool>(desktop.MainWindow))
            {
                LoadServices();
            }
        }
    }

    private void AppointmentsButton_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            new Windows.AppointmentsWindow().ShowDialog(desktop.MainWindow);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        ServicePresenter.ServiceEdited -= OnServiceEdited;
        ServicePresenter.ServiceDeleted -= OnServiceDeleted;
        base.OnClosed(e);
    }
    
    private void UpdateRecordsCounter(int displayed, int total)
    {
        RecordsCounter.Text = $"Показано: {displayed} из {total}";
    }
}