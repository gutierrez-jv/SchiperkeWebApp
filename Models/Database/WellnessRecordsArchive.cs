using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class WellnessRecordsArchive
{
    public int ArchiveWellnessId { get; set; }

    public int OriginalWellnessId { get; set; }

    public int PetId { get; set; }

    public int? AppointmentId { get; set; }

    public string WellnessType { get; set; } = null!;

    public string ProductOrMedication { get; set; } = null!;

    public DateOnly DateGiven { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public string? Dose { get; set; }

    public string? Route { get; set; }

    public string? Remarks { get; set; }

    public int RecordedByUserId { get; set; }

    public DateTime OriginalCreatedAt { get; set; }

    public DateTime ArchivedAt { get; set; }
}
