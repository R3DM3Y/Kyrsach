using System;

namespace Kyrsach.Models
{
    public class ScheduleSettings
    {
        public int Id { get; set; }
        
        // Время начала рабочего дня (например, 09:00)
        public TimeSpan DayStart { get; set; } = new TimeSpan(9, 0, 0);
        
        // Время окончания рабочего дня (например, 21:00)
        public TimeSpan DayEnd { get; set; } = new TimeSpan(21, 0, 0);
        
        // Перерыв между записями в минутах (например, 20)
        public int BreakBetweenSlots { get; set; } = 20;
        
        // Рабочие дни недели (битовая маска)
        public int WorkingDays { get; set; } = 0b0111110; // Пн-Пт (1 << 1 | 1 << 2 | ... | 1 << 5)
    }
}