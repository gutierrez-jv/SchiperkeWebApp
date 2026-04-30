using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class VaccinationRecord
{
    public int VaccinationId { get; set; }

    public int PetId { get; set; }

    public int? AppointmentId { get; set; }

    public string VaccineName { get; set; } = null!;

    public DateOnly DateGiven { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public string? Dose { get; set; }

    public string? Route { get; set; }

    public string? Manufacturer { get; set; }

    public string? LotNumber { get; set; }

    public string? Remarks { get; set; }

    public int RecordedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User RecordedByUser { get; set; } = null!;
}
