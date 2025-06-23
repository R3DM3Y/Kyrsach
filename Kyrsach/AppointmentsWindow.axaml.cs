using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Kyrsach.Models;
using System.Collections.ObjectModel;
using System.Timers;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;

namespace Kyrsach
{
    public partial class AppointmentsWindow : Window
    {
        public ObservableCollection<AppointmentViewModel> Appointments { get; }
            = new ObservableCollection<AppointmentViewModel>();
        private readonly Timer _timeSimulationTimer;
        private DateTime _currentVirtualTime;
        private const bool UseVirtualTime = true;
        private const double TimeSpeedMultiplier = 60.0; // Ускорение времени (1 мин = 1 сек)

        public AppointmentsWindow()
        {
            InitializeComponent();
            DataContext = this;
        
            // Инициализируем виртуальное время
            _currentVirtualTime = new DateTime(2019, 1, 1, 14, 30, 0);
        
            // Таймер для обновления каждую секунду (реального времени)
            _timeSimulationTimer = new Timer(1000) { AutoReset = true };
            _timeSimulationTimer.Elapsed += (s, e) => 
            {
                _currentVirtualTime = _currentVirtualTime.AddMinutes(1); // Увеличиваем на 1 минуту каждую секунду
                Dispatcher.UIThread.InvokeAsync(LoadAppointments);
            };
        
            LoadAppointments();
            _timeSimulationTimer.Start();
        
            Closing += (s, e) => _timeSimulationTimer.Dispose();
        }

        private void LoadAppointments()
        {
            try
            {
                using var context = new PostgresContext();
        
                var startDate = UseVirtualTime ? _currentVirtualTime : DateTime.Now;
                var endDate = startDate.AddDays(2); // Показываем записи на 2 дня вперед
        
                var appointments = context.ClientServices
                    .Include(cs => cs.Service)
                    .Include(cs => cs.Client)
                    .Where(cs => cs.StartTime >= startDate && cs.StartTime <= endDate)
                    .OrderBy(cs => cs.StartTime)
                    .ToList();

                Appointments.Clear();
        
                foreach (var app in appointments)
                {
                    var timeLeft = app.StartTime - startDate;
            
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Appointments.Add(new AppointmentViewModel
                        {
                            ServiceName = app.Service.Title,
                            ClientName = $"{app.Client.LastName} {app.Client.FirstName} {app.Client.Patronymic}",
                            Email = app.Client.Email,
                            Phone = app.Client.Phone,
                            DateTime = app.StartTime,
                            TimeLeft = timeLeft,
                            IsUrgent = timeLeft.TotalHours < 1,
                            CurrentVirtualTime = _currentVirtualTime // Добавляем отображение текущего времени
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки записей: {ex.Message}");
            }
        }
    }

    public class AppointmentViewModel
    {
        public string ServiceName { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan TimeLeft { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime CurrentVirtualTime { get; set; }
    
        public string TimeLeftText => 
            TimeLeft.TotalSeconds <= 0 
                ? "Услуга началась" 
                : $"{(int)TimeLeft.TotalHours} ч {TimeLeft.Minutes} мин";
    
        public string FormattedDateTime => 
            DateTime.ToString("dd.MM.yyyy HH:mm");
    
        public string VirtualTimeDisplay =>
            CurrentVirtualTime.ToString("dd.MM.yyyy HH:mm:ss");
    }
}