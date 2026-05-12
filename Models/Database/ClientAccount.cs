using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class ClientAccount
{
    public int ClientAccountId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public Guid SecurityStamp { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ClientPetLink> ClientPetLinks { get; set; } = new List<ClientPetLink>();

    public virtual ICollection<PetRecordAccessRequest> PetRecordAccessRequests { get; set; } = new List<PetRecordAccessRequest>();
}
