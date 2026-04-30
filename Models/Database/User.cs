using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<ConsultationRecord> ConsultationRecords { get; set; } = new List<ConsultationRecord>();

    public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();

    public virtual ICollection<WellnessRecord> WellnessRecords { get; set; } = new List<WellnessRecord>();
}
