using Avalonia.Controls;
using Kyrsach.Data;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kyrsach.Windows
{
    public partial class AppointmentsWindow : Window
    {
        public ObservableCollection<AppointmentViewModel> Appointments { get; } 
            = new ObservableCollection<AppointmentViewModel>();

        public AppointmentsWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadAppointments();
        }

        private async void LoadAppointments()
        {
            try
            {
                await using var context = new PostgresContext();
                var appointments = await context.ClientServices
                    .Include(cs => cs.Service)
                    .Include(cs => cs.Client)
                    .Where(cs => cs.StartTime >= DateTime.Now)
                    .OrderBy(cs => cs.StartTime)
                    .ToListAsync();

                foreach (var app in appointments)
                {
                    Appointments.Add(new AppointmentViewModel
                    {
                        ServiceName = app.Service.Title,
                        ClientName = $"{app.Client.LastName} {app.Client.FirstName}",
                        Email = app.Client.Email,
                        Phone = app.Client.Phone,
                        DateTime = app.StartTime
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
        }
    }
}