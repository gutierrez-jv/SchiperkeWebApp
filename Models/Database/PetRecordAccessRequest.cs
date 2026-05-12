using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class PetRecordAccessRequest
{
    public int PetRecordAccessRequestId { get; set; }

    public int ClientAccountId { get; set; }

    public int PetId { get; set; }

    public string RequestType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string? StaffNotes { get; set; }

    public virtual ClientAccount ClientAccount { get; set; } = null!;

    public virtual Pet Pet { get; set; } = null!;

    public virtual User? ReviewedByUser { get; set; }
}
