using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class AppointmentsArchive
{
    public int ArchiveAppointmentId { get; set; }

    public int OriginalAppointmentId { get; set; }

    public int PetId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string ServiceType { get; set; } = null!;

    public string? ReasonForVisit { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime OriginalCreatedAt { get; set; }

    public DateTime ArchivedAt { get; set; }
}
