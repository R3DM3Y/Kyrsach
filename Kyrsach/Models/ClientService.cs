﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kyrsach.Models;

public partial class ClientService
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int ServiceId { get; set; }

    public DateTime StartTime { get; set; }

    public string? Comment { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<DocumentByService> DocumentByServices { get; set; } = new List<DocumentByService>();

    public virtual ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();

    public virtual Service Service { get; set; } = null!;
    
    [NotMapped] // Вычисляемое свойство, не сохраняется в БД
    public DateTime EndTime => StartTime.AddMinutes(Service?.DurationInMinutes ?? 0);
}
