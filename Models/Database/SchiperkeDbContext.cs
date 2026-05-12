using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SchiperkeWebApp.Models.Database;

public partial class SchiperkeDbContext : DbContext
{
    public SchiperkeDbContext()
    {
    }

    public SchiperkeDbContext(DbContextOptions<SchiperkeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentsArchive> AppointmentsArchives { get; set; }

    public virtual DbSet<ClientAccount> ClientAccounts { get; set; }

    public virtual DbSet<ClientPetLink> ClientPetLinks { get; set; }

    public virtual DbSet<ConsultationRecord> ConsultationRecords { get; set; }

    public virtual DbSet<ConsultationRecordsArchive> ConsultationRecordsArchives { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PetRecordAccessRequest> PetRecordAccessRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VaccinationRecord> VaccinationRecords { get; set; }

    public virtual DbSet<VaccinationRecordsArchive> VaccinationRecordsArchives { get; set; }

    public virtual DbSet<WellnessRecord> WellnessRecords { get; set; }

    public virtual DbSet<WellnessRecordsArchive> WellnessRecordsArchives { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-TM6EI00;Database=SchiperkeDb;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC27A7A707F");

            entity.HasIndex(e => e.AppointmentDate, "IX_Appointments_AppointmentDate");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Appointments_CreatedByUserId");

            entity.HasIndex(e => e.PetId, "IX_Appointments_PetId");

            entity.HasIndex(e => e.AppointmentCode, "UX_Appointments_AppointmentCode")
                .IsUnique()
                .HasFilter("([AppointmentCode] IS NOT NULL)");

            entity.Property(e => e.AppointmentCode).HasMaxLength(30);
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.CancelledBy).HasMaxLength(30);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PatientNoInput).HasMaxLength(50);
            entity.Property(e => e.PetName).HasMaxLength(100);
            entity.Property(e => e.ReasonForVisit).HasMaxLength(255);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ServiceType).HasMaxLength(50);
            entity.Property(e => e.Sex).HasMaxLength(20);
            entity.Property(e => e.Species).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Appointments).HasForeignKey(d => d.CreatedByUserId);

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments).HasForeignKey(d => d.PetId);
        });

        modelBuilder.Entity<AppointmentsArchive>(entity =>
        {
            entity.HasKey(e => e.ArchiveAppointmentId).HasName("PK__Appointm__18DE631E48BB7D88");

            entity.ToTable("AppointmentsArchive");

            entity.HasIndex(e => e.OriginalAppointmentId, "IX_AppointmentsArchive_OriginalAppointmentId");

            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OriginalCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ReasonForVisit).HasMaxLength(255);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ServiceType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<ClientAccount>(entity =>
        {
            entity.HasIndex(e => e.Username, "UX_ClientAccounts_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.SecurityStamp).HasDefaultValueSql("(newid())");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<ClientPetLink>(entity =>
        {
            entity.HasIndex(e => e.PetId, "IX_ClientPetLinks_PetId");

            entity.HasIndex(e => new { e.ClientAccountId, e.PetId }, "UX_ClientPetLinks_Active_Client_Pet")
                .IsUnique()
                .HasFilter("([RevokedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LinkMethod)
                .HasMaxLength(30)
                .HasDefaultValue("PatientNoPetName");
            entity.Property(e => e.LinkStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.RevokedAt).HasPrecision(0);

            entity.HasOne(d => d.ClientAccount).WithMany(p => p.ClientPetLinks)
                .HasForeignKey(d => d.ClientAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPetLinks_ClientAccounts");

            entity.HasOne(d => d.LinkedByUser).WithMany(p => p.ClientPetLinkLinkedByUsers)
                .HasForeignKey(d => d.LinkedByUserId)
                .HasConstraintName("FK_ClientPetLinks_LinkedByUser");

            entity.HasOne(d => d.Pet).WithMany(p => p.ClientPetLinks)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientPetLinks_Pets");

            entity.HasOne(d => d.RevokedByUser).WithMany(p => p.ClientPetLinkRevokedByUsers)
                .HasForeignKey(d => d.RevokedByUserId)
                .HasConstraintName("FK_ClientPetLinks_RevokedByUser");
        });

        modelBuilder.Entity<ConsultationRecord>(entity =>
        {
            entity.HasKey(e => e.ConsultationId).HasName("PK__Consulta__5D014A984CAD7ECE");

            entity.HasIndex(e => e.AppointmentId, "IX_ConsultationRecords_AppointmentId");

            entity.HasIndex(e => e.ConsultationDate, "IX_ConsultationRecords_ConsultationDate");

            entity.HasIndex(e => e.PetId, "IX_ConsultationRecords_PetId");

            entity.Property(e => e.ChiefComplaint).HasMaxLength(255);
            entity.Property(e => e.ConsultationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Vitals).HasMaxLength(500);

            entity.HasOne(d => d.Appointment).WithMany(p => p.ConsultationRecords)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_ConsultationRecords_Appointments");

            entity.HasOne(d => d.Pet).WithMany(p => p.ConsultationRecords)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConsultationRecords_Pets");

            entity.HasOne(d => d.RecordedByUser).WithMany(p => p.ConsultationRecords)
                .HasForeignKey(d => d.RecordedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConsultationRecords_Users");
        });

        modelBuilder.Entity<ConsultationRecordsArchive>(entity =>
        {
            entity.HasKey(e => e.ArchiveConsultationId).HasName("PK__Consulta__3A95A90345D12EC8");

            entity.ToTable("ConsultationRecordsArchive");

            entity.HasIndex(e => e.OriginalConsultationId, "IX_ConsultationRecordsArchive_OriginalConsultationId");

            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChiefComplaint).HasMaxLength(255);
            entity.Property(e => e.ConsultationDate).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OriginalCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Vitals).HasMaxLength(500);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pets__48E53862832C0AB0");

            entity.HasIndex(e => e.PatientNo, "UQ__Pets__970ED8BCBCFEFDA4").IsUnique();

            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PatientNo).HasMaxLength(30);
            entity.Property(e => e.PetName).HasMaxLength(100);
            entity.Property(e => e.Sex).HasMaxLength(20);
            entity.Property(e => e.Species).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(6, 2)");
        });

        modelBuilder.Entity<PetRecordAccessRequest>(entity =>
        {
            entity.HasIndex(e => e.PetId, "IX_PetRecordAccessRequests_PetId");

            entity.HasIndex(e => new { e.ClientAccountId, e.PetId, e.RequestType }, "UX_PetRecordAccessRequests_Open_Request")
                .IsUnique()
                .HasFilter("([Status]=N'Pending')");

            entity.Property(e => e.RequestType)
                .HasMaxLength(30)
                .HasDefaultValue("MedicalRecords");
            entity.Property(e => e.RequestedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ReviewedAt).HasPrecision(0);
            entity.Property(e => e.StaffNotes).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.ClientAccount).WithMany(p => p.PetRecordAccessRequests)
                .HasForeignKey(d => d.ClientAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PetRecordAccessRequests_ClientAccounts");

            entity.HasOne(d => d.Pet).WithMany(p => p.PetRecordAccessRequests)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PetRecordAccessRequests_Pets");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.PetRecordAccessRequests)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK_PetRecordAccessRequests_ReviewedByUser");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CDC888CF5");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E42A686AA4").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(30);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<VaccinationRecord>(entity =>
        {
            entity.HasKey(e => e.VaccinationId).HasName("PK__Vaccinat__466430471EA7FBDB");

            entity.HasIndex(e => e.AppointmentId, "IX_VaccinationRecords_AppointmentId");

            entity.HasIndex(e => e.DateGiven, "IX_VaccinationRecords_DateGiven");

            entity.HasIndex(e => e.PetId, "IX_VaccinationRecords_PetId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dose).HasMaxLength(50);
            entity.Property(e => e.LotNumber).HasMaxLength(50);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.VaccineName).HasMaxLength(100);

            entity.HasOne(d => d.Appointment).WithMany(p => p.VaccinationRecords)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_VaccinationRecords_Appointments");

            entity.HasOne(d => d.Pet).WithMany(p => p.VaccinationRecords)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VaccinationRecords_Pets");

            entity.HasOne(d => d.RecordedByUser).WithMany(p => p.VaccinationRecords)
                .HasForeignKey(d => d.RecordedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VaccinationRecords_Users");
        });

        modelBuilder.Entity<VaccinationRecordsArchive>(entity =>
        {
            entity.HasKey(e => e.ArchiveVaccinationId).HasName("PK__Vaccinat__424D33A8D49FA31C");

            entity.ToTable("VaccinationRecordsArchive");

            entity.HasIndex(e => e.OriginalVaccinationId, "IX_VaccinationRecordsArchive_OriginalVaccinationId");

            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dose).HasMaxLength(50);
            entity.Property(e => e.LotNumber).HasMaxLength(50);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.OriginalCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.VaccineName).HasMaxLength(100);
        });

        modelBuilder.Entity<WellnessRecord>(entity =>
        {
            entity.HasKey(e => e.WellnessId).HasName("PK__Wellness__DD4F29746E7A6976");

            entity.HasIndex(e => e.AppointmentId, "IX_WellnessRecords_AppointmentId");

            entity.HasIndex(e => e.DateGiven, "IX_WellnessRecords_DateGiven");

            entity.HasIndex(e => e.PetId, "IX_WellnessRecords_PetId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dose).HasMaxLength(50);
            entity.Property(e => e.ProductOrMedication).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.WellnessType).HasMaxLength(50);

            entity.HasOne(d => d.Appointment).WithMany(p => p.WellnessRecords)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK_WellnessRecords_Appointments");

            entity.HasOne(d => d.Pet).WithMany(p => p.WellnessRecords)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WellnessRecords_Pets");

            entity.HasOne(d => d.RecordedByUser).WithMany(p => p.WellnessRecords)
                .HasForeignKey(d => d.RecordedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WellnessRecords_Users");
        });

        modelBuilder.Entity<WellnessRecordsArchive>(entity =>
        {
            entity.HasKey(e => e.ArchiveWellnessId).HasName("PK__Wellness__135A6B8AD21B6083");

            entity.ToTable("WellnessRecordsArchive");

            entity.HasIndex(e => e.OriginalWellnessId, "IX_WellnessRecordsArchive_OriginalWellnessId");

            entity.Property(e => e.ArchivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dose).HasMaxLength(50);
            entity.Property(e => e.OriginalCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ProductOrMedication).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.WellnessType).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
