using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class ConsultationRecord
{
    public int ConsultationId { get; set; }

    public int PetId { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime ConsultationDate { get; set; }

    public string? ChiefComplaint { get; set; }

    public string? History { get; set; }

    public string? Vitals { get; set; }

    public string? PhysicalExamination { get; set; }

    public string? LabExam { get; set; }

    public string? Assessment { get; set; }

    public string? Treatment { get; set; }

    public string? Notes { get; set; }

    public int RecordedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User RecordedByUser { get; set; } = null!;
}
