using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Kyrsach.Models;

namespace Kyrsach;

public partial class MainWindow : Window
{
    private ObservableCollection<ServicePresenter> services;
    
    public MainWindow()
    {
        InitializeComponent();
        using var context = new PostgresContext();
        var dataSource = context.Services.Select(Service => new ServicePresenter
        {
            Id = Service.Id,
            MainImagePath = Service.MainImagePath,
            Title = Service.Title,
            Cost = Service.Cost,
            DurationInMinutes = Service.DurationInMinutes,
            Discount = Service.Discount,
            Description = Service.Description,
        });
        services = new ObservableCollection<ServicePresenter>(dataSource);
        ServiceBox.ItemsSource = services;
    }
}

public class ServicePresenter() : Service
{
    Bitmap? Image
    {
        get
        {
            try
            {
                return new Bitmap(MainImagePath);
            }
            catch
            {
                return null;
            }
        }
    }
}