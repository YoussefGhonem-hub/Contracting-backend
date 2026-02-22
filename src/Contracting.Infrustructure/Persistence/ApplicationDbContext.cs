using Contracting.Domain.Common;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.helper;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Shared.CurrentUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Contracting.Infrustructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departmentes => Set<Department>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<EngineerProject> EngineerProjects => Set<EngineerProject>();
    public DbSet<SpecialField> SpecialFields => Set<SpecialField>();
    public DbSet<DepartmentSpecialField> DepartmentSpecialFields => Set<DepartmentSpecialField>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<EngineerRequest> EngineerRequests => Set<EngineerRequest>();
    public DbSet<EngineerRequestNotes> EngineerRequestNotes => Set<EngineerRequestNotes>();
    public DbSet<EngineerRequestActivite> EngineerRequestActivites => Set<EngineerRequestActivite>();
    public DbSet<EngineerRequestAttachment> EngineerRequestAttachments => Set<EngineerRequestAttachment>();
    public DbSet<EngineerRequestSpecialFieldValue> EngineerRequestSpecialFieldValues => Set<EngineerRequestSpecialFieldValue>();
    public DbSet<EngineerSiteReport> EngineerSiteReports => Set<EngineerSiteReport>();
    public DbSet<EngineerSiteWorkLog> EngineerSiteWorkLogs => Set<EngineerSiteWorkLog>();
    public DbSet<EngineerSiteWorkLogAttachment> EngineerSiteWorkLogAttachments => Set<EngineerSiteWorkLogAttachment>();
    public DbSet<EngineerSiteMaterial> EngineerSiteMaterials => Set<EngineerSiteMaterial>();
    public DbSet<EngineerSiteEquipment> EngineerSiteEquipments => Set<EngineerSiteEquipment>();
    public DbSet<EngineerSiteSurveyQuestion> EngineerSiteSurveyQuestions => Set<EngineerSiteSurveyQuestion>();
    public DbSet<EngineerSiteSurveyQuestionTemplate> EngineerSiteSurveyQuestionTemplates => Set<EngineerSiteSurveyQuestionTemplate>();
    public DbSet<ConstructionItem> ConstructionItems => Set<ConstructionItem>();
    public DbSet<ReportConstructionItemWorker> ReportConstructionItemWorkers => Set<ReportConstructionItemWorker>();
    public DbSet<EngineerSiteReportAttachment> EngineerSiteReportAttachments => Set<EngineerSiteReportAttachment>();
    public DbSet<UserDeviceToken> userDeviceTokens => Set<UserDeviceToken>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<ExceptionLog> ExceptionLogs => Set<ExceptionLog>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<UserSignature> UserSignatures => Set<UserSignature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables to use security schema without AspNet prefix
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "security");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "security");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "security");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "security");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "security");

        modelBuilder.Entity<Engineer>()
        .HasOne(e => e.ApplicationUser) 
        .WithMany()
        .HasForeignKey(e => e.ApplicationUserId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.GetOnlyNotDeletedEntities();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = CurrentUser.Id;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseAuditableEntity auditable) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (auditable.CreatedDate == default) auditable.CreatedDate = now;
                    if (auditable.CreatedBy == Guid.Empty && userId.HasValue) auditable.CreatedBy = userId.Value;
                    auditable.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    auditable.ModifiedDate = now;
                    if (userId.HasValue) auditable.ModifiedBy = userId.Value;
                    entry.Property(nameof(BaseAuditableEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseAuditableEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    auditable.IsDeleted = true;
                    auditable.DeletedDate = now;
                    if (userId.HasValue) auditable.DeletedBy = userId.Value;
                    break;
            }
        }
    }


}
