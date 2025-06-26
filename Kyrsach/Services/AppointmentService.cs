using Kyrsach.Data;
using Kyrsach.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kyrsach.Services
{
    public class AppointmentService
    {
        private readonly PostgresContext _context;

        public AppointmentService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTimeAvailableForService(int serviceId, DateTime startTime)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return false;
            

            var endTime = startTime.AddMinutes(service.DurationInMinutes);
            
            return !await _context.ClientServices
                .Include(cs => cs.Service)
                .Where(cs => cs.ServiceId == serviceId)
                .AnyAsync(cs => 
                    startTime < cs.StartTime.AddMinutes(cs.Service.DurationInMinutes) && 
                    endTime > cs.StartTime);
        }
    }
}