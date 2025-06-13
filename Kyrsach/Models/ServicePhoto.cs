using System;
using System.Collections.Generic;

namespace Kyrsach.Models;

public partial class ServicePhoto
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public string PhotoPath { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
