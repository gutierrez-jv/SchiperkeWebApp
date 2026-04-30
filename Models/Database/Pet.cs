using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class Pet
{
    public int PetId { get; set; }

    public string PatientNo { get; set; } = null!;

    public string PetName { get; set; } = null!;

    public string Species { get; set; } = null!;

    public string? Breed { get; set; }

    public string Sex { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public int? AgeYears { get; set; }

    public string? Color { get; set; }

    public decimal? Weight { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ConsultationRecord> ConsultationRecords { get; set; } = new List<ConsultationRecord>();

    public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();

    public virtual ICollection<WellnessRecord> WellnessRecords { get; set; } = new List<WellnessRecord>();
}
