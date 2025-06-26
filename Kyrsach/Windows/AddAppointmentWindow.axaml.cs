using Avalonia.Controls;
using Avalonia.Interactivity;
using Kyrsach.Data;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Kyrsach.Services;

namespace Kyrsach.Windows
{
    public partial class AddAppointmentWindow : Window
    {
        private readonly PostgresContext _context;
        private readonly Service _selectedService;
        
        public ObservableCollection<Client> Clients { get; } = new();
        public Client SelectedClient { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string TimeText { get; set; }

        public AddAppointmentWindow()
        {
            InitializeComponent();
        }

        // Основной конструктор с параметром
        public AddAppointmentWindow(Service service) : this()
        {
            _context = new PostgresContext();
            _selectedService = service;
            DataContext = this;
        
            InitializeUI();
            LoadClients();
        }

        private void InitializeUI()
        {
            ServiceNameText.Text = _selectedService.Title;
            DurationText.Text = $"{_selectedService.DurationInMinutes} минут";
            TimeText = GetDefaultStartTime().ToString("HH:mm");
        }

        private DateTime GetDefaultStartTime()
        {
            var now = DateTime.Now;
            var defaultTime = now.AddMinutes(30 - (now.Minute % 30));
            
            var dayStart = new TimeSpan(9, 0, 0);
            var dayEnd = new TimeSpan(21, 0, 0);
            
            if (defaultTime.TimeOfDay < dayStart)
                return defaultTime.Date.Add(dayStart);
            
            if (defaultTime.TimeOfDay > dayEnd.Add(TimeSpan.FromMinutes(-_selectedService.DurationInMinutes)))
                return defaultTime.Date.AddDays(1).Add(dayStart);
                
            return defaultTime;
        }

        private async void LoadClients()
        {
            try
            {
                var clients = await _context.Clients
                    .OrderBy(c => c.LastName)
                    .ToListAsync();
                
                Clients.Clear();
                foreach (var client in clients)
                    Clients.Add(client);
                
                if (Clients.Count > 0)
                    SelectedClient = Clients[0];
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка", $"Не удалось загрузить клиентов: {ex.Message}");
            }
        }

        private async Task<bool> IsTimeSlotAvailable(DateTime startTime)
        {
            var endTime = startTime.AddMinutes(_selectedService.DurationInMinutes);
            var breakTime = TimeSpan.FromMinutes(20);
            
            return !await _context.ClientServices
                .Include(cs => cs.Service)
                .Where(cs => cs.ServiceId == _selectedService.Id) // Фильтр по текущей услуге
                .AnyAsync(cs => 
                    startTime < cs.StartTime.AddMinutes(cs.Service.DurationInMinutes) && 
                    endTime > cs.StartTime);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedClient == null)
                {
                    await MessageBoxService.ShowError(this, "Ошибка", "Выберите клиента");
                    return;
                }

                if (!TimeSpan.TryParse(TimeText, out var time))
                {
                    await MessageBoxService.ShowError(this, "Ошибка", "Введите время в формате ЧЧ:ММ");
                    return;
                }

                var startTime = SelectedDate.Add(time);
                var endTime = startTime.AddMinutes(_selectedService.DurationInMinutes);
                var breakTime = TimeSpan.FromMinutes(20);

                // Проверка перерыва
                var previousAppointment = await _context.ClientServices
                    .Include(cs => cs.Service)
                    .Where(cs => cs.ServiceId == _selectedService.Id)
                    .OrderByDescending(cs => cs.StartTime)
                    .FirstOrDefaultAsync();

                if (previousAppointment != null && 
                    startTime < previousAppointment.EndTime.AddMinutes(breakTime.TotalMinutes))
                {
                    await MessageBoxService.ShowError(this, "Ошибка", 
                        $"Требуется перерыв {breakTime.TotalMinutes} мин. Ближайшее доступное время: {previousAppointment.EndTime.AddMinutes(breakTime.TotalMinutes):HH:mm}");
                    return;
                }

                // Проверка пересечений
                if (!await IsTimeSlotAvailable(startTime))
                {
                    await MessageBoxService.ShowError(this, "Ошибка", 
                        "Время занято (учитывается 20-минутный перерыв между записями)");
                    return;
                }

                // Проверки
                if (startTime < DateTime.Now)
                {
                    await MessageBoxService.ShowError(this, "Ошибка", "Нельзя записаться на прошедшее время");
                    return;
                }

                var dayStart = new TimeSpan(9, 0, 0);
                var dayEnd = new TimeSpan(21, 0, 0);
                if (startTime.TimeOfDay < dayStart || endTime.TimeOfDay > dayEnd)
                {
                    await MessageBoxService.ShowError(this, "Ошибка", $"Рабочее время: {dayStart:hh\\:mm}-{dayEnd:hh\\:mm}");
                    return;
                }

                if (!await IsTimeSlotAvailable(startTime))
                {
                    await MessageBoxService.ShowError(this, "Ошибка", "Время занято для этой услуги");
                    return;
                }

                // Создание записи
                _context.ClientServices.Add(new ClientService
                {
                    ClientId = SelectedClient.Id,
                    ServiceId = _selectedService.Id,
                    StartTime = startTime,
                    Comment = NotesTextBox.Text
                });

                await _context.SaveChangesAsync();
                Close(true);
            }
            catch (Exception ex)
            {
                await MessageBoxService.ShowError(this, "Ошибка", $"Ошибка сохранения: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close(false);
    }
}