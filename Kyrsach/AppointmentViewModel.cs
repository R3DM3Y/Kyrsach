using System;
using System.ComponentModel;
using Avalonia.Threading;

namespace Kyrsach.Windows
{
    public class AppointmentViewModel : INotifyPropertyChanged
    {
        private DateTime _dateTime;
        private TimeSpan _timeLeft;
        private bool _isUrgent;
        
        public string ServiceName { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                _dateTime = value;
                UpdateTimeLeft();
            }
        }
        
        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            private set
            {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimeLeft));
                OnPropertyChanged(nameof(TimeLeftText));
            }
        }
        
        public bool IsUrgent
        {
            get => _isUrgent;
            private set
            {
                _isUrgent = value;
                OnPropertyChanged(nameof(IsUrgent));
            }
        }
        
        private readonly DispatcherTimer _timer;

        public AppointmentViewModel()
        {
            // Таймер обновления каждую секунду
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => UpdateTimeLeft();
            _timer.Start();
        }

        private void UpdateTimeLeft()
        {
            TimeLeft = DateTime - DateTime.Now;
            IsUrgent = TimeLeft.TotalHours < 1;
        }

        public string TimeLeftText => 
            TimeLeft.TotalSeconds <= 0 
                ? "Услуга началась" 
                : $"{(int)TimeLeft.TotalHours} ч {TimeLeft.Minutes} мин";
        
        public string FormattedDateTime => DateTime.ToString("dd.MM.yyyy HH:mm");
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}