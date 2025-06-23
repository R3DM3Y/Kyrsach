using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Data;
using Kyrsach.Models;
using Kyrsach.Services;

namespace Kyrsach.Windows
{
    public partial class AddAppointmentWindow : Window
    {
        private readonly Service _selectedService;
        
        public ObservableCollection<Client> Clients { get; } = new();
        public Client SelectedClient { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string TimeText { get; set; }

        public AddAppointmentWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public AddAppointmentWindow(Service service) : this()
        {
            _selectedService = service;
            ServiceNameText.Text = service.Title;
            DurationText.Text = $"{service.DurationInMinutes} минут";
            TimeText = DateTime.Now.ToString("HH:mm");
            LoadClients();
        }

        private void LoadClients()
        {
            using var context = new PostgresContext();
            var clients = context.Clients.OrderBy(c => c.LastName).ToList();
            
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
            
            // Автовыбор первого клиента
            if (Clients.Count > 0)
            {
                SelectedClient = Clients[0];
                ClientComboBox.SelectedIndex = 0;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClient == null)
            {
                await MessageBoxService.ShowError(this, "Ошибка", "Выберите клиента из списка");
                return;
            }

            if (!TimeSpan.TryParse(TimeText, out var time))
            {
                await MessageBoxService.ShowError(this, "Ошибка", "Введите время в формате ЧЧ:ММ (например, 14:30)");
                return;
            }

            var startTime = SelectedDate.Add(time);

            try
            {
                using var context = new PostgresContext();
                context.ClientServices.Add(new ClientService
                {
                    ServiceId = _selectedService.Id,
                    ClientId = SelectedClient.Id,
                    StartTime = startTime,
                    Comment = NotesTextBox.Text
                });

                await context.SaveChangesAsync();
                Close(true);
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка", $"Не удалось сохранить запись: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}