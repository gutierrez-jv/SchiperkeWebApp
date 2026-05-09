using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? PetId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string ServiceType { get; set; } = null!;

    public string? ReasonForVisit { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? AppointmentCode { get; set; }

    public bool IsExistingPatient { get; set; }

    public string? PatientNoInput { get; set; }

    public string? PetName { get; set; }

    public string? Species { get; set; }

    public string? Breed { get; set; }

    public string? Sex { get; set; }

    public string? Color { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CancellationReason { get; set; }

    public string? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual ICollection<ConsultationRecord> ConsultationRecords { get; set; } = new List<ConsultationRecord>();

    public virtual User? CreatedByUser { get; set; }

    public virtual Pet? Pet { get; set; }

    public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();

    public virtual ICollection<WellnessRecord> WellnessRecords { get; set; } = new List<WellnessRecord>();
}
