using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Integrations;
using RhFolha.Domain.Security;

namespace RhFolha.Infrastructure.Persistence;

internal static class IntegrationModelBuilderExtensions
{
    public static void ConfigureIntegrations(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExternalIntegration>(entity =>
        {
            entity.ToTable("external_integrations");
            entity.HasKey(integration => integration.Id);
            entity.Property(integration => integration.Id).HasColumnName("id");
            entity.Property(integration => integration.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(integration => integration.Provider).HasColumnName("provider").HasMaxLength(40).IsRequired();
            entity.Property(integration => integration.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(integration => integration.BaseUrl).HasColumnName("base_url").HasMaxLength(300).IsRequired();
            entity.Property(integration => integration.ExternalCompanyIdentifier).HasColumnName("external_company_identifier").HasMaxLength(120).IsRequired();
            entity.Property(integration => integration.IntegrationTokenSecret).HasColumnName("integration_token_secret").HasMaxLength(800).IsRequired();
            entity.Property(integration => integration.AccessToken).HasColumnName("access_token").HasMaxLength(2000);
            entity.Property(integration => integration.AccessTokenExpiresAt).HasColumnName("access_token_expires_at");
            entity.Property(integration => integration.LastSyncAt).HasColumnName("last_sync_at");
            entity.Property(integration => integration.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(integration => integration.LastError).HasColumnName("last_error").HasMaxLength(1000);
            entity.Property(integration => integration.DeletedAt).HasColumnName("deleted_at");
            entity.Property(integration => integration.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(integration => integration.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(integration => integration.Company)
                .WithMany()
                .HasForeignKey(integration => integration.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(integration => new { integration.CompanyId, integration.Provider, integration.Status });
            entity.HasIndex(integration => new { integration.CompanyId, integration.Provider })
                .IsUnique()
                .HasFilter("status = 'Active' AND deleted_at IS NULL");
        });

        modelBuilder.Entity<ExternalSyncLog>(entity =>
        {
            entity.ToTable("external_sync_logs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Id).HasColumnName("id");
            entity.Property(log => log.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(log => log.ExternalIntegrationId).HasColumnName("external_integration_id").IsRequired();
            entity.Property(log => log.Provider).HasColumnName("provider").HasMaxLength(40).IsRequired();
            entity.Property(log => log.Resource).HasColumnName("resource").HasMaxLength(80).IsRequired();
            entity.Property(log => log.StartedAt).HasColumnName("started_at").IsRequired();
            entity.Property(log => log.FinishedAt).HasColumnName("finished_at");
            entity.Property(log => log.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(log => log.RequestedFrom).HasColumnName("requested_from");
            entity.Property(log => log.RequestedTo).HasColumnName("requested_to");
            entity.Property(log => log.PageCount).HasColumnName("page_count").IsRequired();
            entity.Property(log => log.RecordsRead).HasColumnName("records_read").IsRequired();
            entity.Property(log => log.RecordsCreated).HasColumnName("records_created").IsRequired();
            entity.Property(log => log.RecordsUpdated).HasColumnName("records_updated").IsRequired();
            entity.Property(log => log.RecordsIgnored).HasColumnName("records_ignored").IsRequired();
            entity.Property(log => log.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(log => log.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(log => log.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(log => log.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(log => log.ExternalIntegration)
                .WithMany()
                .HasForeignKey(log => log.ExternalIntegrationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(log => log.CreatedByUser)
                .WithMany()
                .HasForeignKey(log => log.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(log => log.Company)
                .WithMany()
                .HasForeignKey(log => log.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(log => new { log.CompanyId, log.Resource, log.StartedAt });
            entity.HasIndex(log => new { log.ExternalIntegrationId, log.Status });
        });

        modelBuilder.Entity<ExternalEntityMap>(entity =>
        {
            entity.ToTable("external_entity_maps");
            entity.HasKey(map => map.Id);
            entity.Property(map => map.Id).HasColumnName("id");
            entity.Property(map => map.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(map => map.Provider).HasColumnName("provider").HasMaxLength(40).IsRequired();
            entity.Property(map => map.ExternalEntityType).HasColumnName("external_entity_type").HasMaxLength(80).IsRequired();
            entity.Property(map => map.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(map => map.LocalEntityType).HasColumnName("local_entity_type").HasMaxLength(80).IsRequired();
            entity.Property(map => map.LocalEntityId).HasColumnName("local_entity_id").IsRequired();
            entity.Property(map => map.ExternalDisplayName).HasColumnName("external_display_name").HasMaxLength(180).IsRequired();
            entity.Property(map => map.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(map => map.LinkedAt).HasColumnName("linked_at");
            entity.Property(map => map.LinkedByUserId).HasColumnName("linked_by_user_id");
            entity.Property(map => map.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(map => map.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(map => map.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(map => map.Company)
                .WithMany()
                .HasForeignKey(map => map.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(map => map.LinkedByUser)
                .WithMany()
                .HasForeignKey(map => map.LinkedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(map => new { map.CompanyId, map.Provider, map.ExternalEntityType, map.ExternalId });
            entity.HasIndex(map => new { map.CompanyId, map.LocalEntityType, map.LocalEntityId, map.Provider });
        });

        modelBuilder.Entity<DapicEmployee>(entity =>
        {
            entity.ToTable("dapic_employees");
            entity.HasKey(employee => employee.Id);
            entity.Property(employee => employee.Id).HasColumnName("id");
            entity.Property(employee => employee.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(employee => employee.EmployeeId).HasColumnName("employee_id");
            entity.Property(employee => employee.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(employee => employee.Name).HasColumnName("name").HasMaxLength(180).IsRequired();
            entity.Property(employee => employee.FantasyName).HasColumnName("fantasy_name").HasMaxLength(180);
            entity.Property(employee => employee.DisplayName).HasColumnName("display_name").HasMaxLength(180);
            entity.Property(employee => employee.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(employee => employee.RawUpdatedAt).HasColumnName("raw_updated_at");
            entity.Property(employee => employee.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(employee => employee.IsIgnored).HasColumnName("is_ignored").IsRequired();
            entity.Property(employee => employee.LinkStatus).HasColumnName("link_status").HasMaxLength(30).IsRequired();
            entity.Property(employee => employee.LinkedAt).HasColumnName("linked_at");
            entity.Property(employee => employee.IgnoredAt).HasColumnName("ignored_at");
            entity.Property(employee => employee.IgnoredReason).HasColumnName("ignored_reason").HasMaxLength(500);
            entity.Property(employee => employee.DeletedAt).HasColumnName("deleted_at");
            entity.Property(employee => employee.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(employee => employee.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(employee => employee.Company)
                .WithMany()
                .HasForeignKey(employee => employee.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(employee => employee.Employee)
                .WithMany()
                .HasForeignKey(employee => employee.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(employee => new { employee.CompanyId, employee.ExternalId }).IsUnique();
            entity.HasIndex(employee => new { employee.CompanyId, employee.Name });
            entity.HasIndex(employee => new { employee.CompanyId, employee.EmployeeId });
            entity.HasIndex(employee => new { employee.CompanyId, employee.LinkStatus });
        });
    }
}
