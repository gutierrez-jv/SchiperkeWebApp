using System;
using System.Collections.Generic;

namespace SchiperkeWebApp.Models.Database;

public partial class ClientPetLink
{
    public int ClientPetLinkId { get; set; }

    public int ClientAccountId { get; set; }

    public int PetId { get; set; }

    public string LinkStatus { get; set; } = null!;

    public string LinkMethod { get; set; } = null!;

    public int? LinkedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public int? RevokedByUserId { get; set; }

    public virtual ClientAccount ClientAccount { get; set; } = null!;

    public virtual User? LinkedByUser { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User? RevokedByUser { get; set; }
}
