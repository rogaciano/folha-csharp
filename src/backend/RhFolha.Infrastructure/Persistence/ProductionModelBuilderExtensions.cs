using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Departments;
using RhFolha.Domain.Employees;
using RhFolha.Domain.JobPositions;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Production;
using RhFolha.Domain.Security;

namespace RhFolha.Infrastructure.Persistence;

internal static class ProductionModelBuilderExtensions
{
    public static void ConfigureProduction(this ModelBuilder modelBuilder)
    {
        ConfigureSynchronizedCatalogs(modelBuilder);
        ConfigureRateTables(modelBuilder);
        ConfigureProductionEntries(modelBuilder);
    }

    private static void ConfigureSynchronizedCatalogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionProduct>(entity =>
        {
            entity.ToTable("production_products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Id).HasColumnName("id");
            entity.Property(product => product.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(product => product.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(product => product.Reference).HasColumnName("reference").HasMaxLength(80).IsRequired();
            entity.Property(product => product.FactoryDescription).HasColumnName("factory_description").HasMaxLength(250).IsRequired();
            entity.Property(product => product.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(product => product.ExternalCreatedAt).HasColumnName("external_created_at");
            entity.Property(product => product.ExternalUpdatedAt).HasColumnName("external_updated_at");
            entity.Property(product => product.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(product => product.DeletedAt).HasColumnName("deleted_at");
            entity.Property(product => product.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(product => product.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(product => product.Company).WithMany().HasForeignKey(product => product.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(product => new { product.CompanyId, product.ExternalId }).IsUnique();
            entity.HasIndex(product => new { product.CompanyId, product.Reference });
            entity.HasIndex(product => new { product.CompanyId, product.Status });
        });

        modelBuilder.Entity<ProductionOperation>(entity =>
        {
            entity.ToTable("production_operations");
            entity.HasKey(operation => operation.Id);
            entity.Property(operation => operation.Id).HasColumnName("id");
            entity.Property(operation => operation.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(operation => operation.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(operation => operation.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(operation => operation.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(operation => operation.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(operation => operation.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(operation => operation.DeletedAt).HasColumnName("deleted_at");
            entity.Property(operation => operation.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(operation => operation.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(operation => operation.Company).WithMany().HasForeignKey(operation => operation.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(operation => new { operation.CompanyId, operation.ExternalId }).IsUnique();
            entity.HasIndex(operation => new { operation.CompanyId, operation.Name });
        });

        modelBuilder.Entity<ProductionCell>(entity =>
        {
            entity.ToTable("production_cells");
            entity.HasKey(cell => cell.Id);
            entity.Property(cell => cell.Id).HasColumnName("id");
            entity.Property(cell => cell.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(cell => cell.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(cell => cell.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(cell => cell.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(cell => cell.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(cell => cell.DepartmentId).HasColumnName("department_id");
            entity.Property(cell => cell.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(cell => cell.DeletedAt).HasColumnName("deleted_at");
            entity.Property(cell => cell.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(cell => cell.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(cell => cell.Company).WithMany().HasForeignKey(cell => cell.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(cell => cell.Department).WithMany().HasForeignKey(cell => cell.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(cell => new { cell.CompanyId, cell.ExternalId }).IsUnique();
            entity.HasIndex(cell => new { cell.CompanyId, cell.DepartmentId });
        });

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.ToTable("production_orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Id).HasColumnName("id");
            entity.Property(order => order.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(order => order.ExternalId).HasColumnName("external_id").HasMaxLength(80).IsRequired();
            entity.Property(order => order.Number).HasColumnName("number").HasMaxLength(80);
            entity.Property(order => order.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(order => order.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(order => order.IssueDate).HasColumnName("issue_date");
            entity.Property(order => order.StartDate).HasColumnName("start_date");
            entity.Property(order => order.EndDate).HasColumnName("end_date");
            entity.Property(order => order.ExternalUpdatedAt).HasColumnName("external_updated_at");
            entity.Property(order => order.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(order => order.RawStatus).HasColumnName("raw_status").HasMaxLength(80);
            entity.Property(order => order.DeletedAt).HasColumnName("deleted_at");
            entity.Property(order => order.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(order => order.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(order => order.Company).WithMany().HasForeignKey(order => order.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(order => new { order.CompanyId, order.ExternalId }).IsUnique();
            entity.HasIndex(order => new { order.CompanyId, order.Number });
            entity.HasIndex(order => new { order.CompanyId, order.Status });
            entity.HasIndex(order => new { order.CompanyId, order.IssueDate });
        });

        modelBuilder.Entity<ProductionOrderProduct>(entity =>
        {
            entity.ToTable("production_order_products");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(item => item.ProductionOrderId).HasColumnName("production_order_id").IsRequired();
            entity.Property(item => item.ProductionProductId).HasColumnName("production_product_id").IsRequired();
            entity.Property(item => item.ExternalId).HasColumnName("external_id").HasMaxLength(80);
            entity.Property(item => item.ReferenceSnapshot).HasColumnName("reference_snapshot").HasMaxLength(80).IsRequired();
            entity.Property(item => item.DescriptionSnapshot).HasColumnName("description_snapshot").HasMaxLength(250).IsRequired();
            entity.Property(item => item.Color).HasColumnName("color").HasMaxLength(80);
            entity.Property(item => item.Size).HasColumnName("size").HasMaxLength(80);
            entity.Property(item => item.Grade).HasColumnName("grade").HasMaxLength(80);
            entity.Property(item => item.PlannedQuantity).HasColumnName("planned_quantity").HasPrecision(14, 4);
            entity.Property(item => item.ProducedQuantity).HasColumnName("produced_quantity").HasPrecision(14, 4).IsRequired();
            entity.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(item => item.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(item => item.DeletedAt).HasColumnName("deleted_at");
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(item => item.ProductionOrder).WithMany(order => order.Products).HasForeignKey(item => item.ProductionOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ProductionProduct).WithMany().HasForeignKey(item => item.ProductionProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(item => item.ProductionOrderId);
            entity.HasIndex(item => item.ProductionProductId);
            entity.HasIndex(item => new { item.CompanyId, item.ReferenceSnapshot });
        });

        ConfigureTechnicalSheets(modelBuilder);
    }

    private static void ConfigureTechnicalSheets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductTechnicalSheet>(entity =>
        {
            entity.ToTable("product_technical_sheets");
            entity.HasKey(sheet => sheet.Id);
            entity.Property(sheet => sheet.Id).HasColumnName("id");
            entity.Property(sheet => sheet.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(sheet => sheet.ProductionProductId).HasColumnName("production_product_id").IsRequired();
            entity.Property(sheet => sheet.ExternalProductId).HasColumnName("external_product_id").HasMaxLength(80).IsRequired();
            entity.Property(sheet => sheet.VersionLabel).HasColumnName("version_label").HasMaxLength(80);
            entity.Property(sheet => sheet.LastSyncedAt).HasColumnName("last_synced_at").IsRequired();
            entity.Property(sheet => sheet.RawHash).HasColumnName("raw_hash").HasMaxLength(120);
            entity.Property(sheet => sheet.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(sheet => sheet.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(sheet => sheet.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(sheet => sheet.ProductionProduct).WithMany().HasForeignKey(sheet => sheet.ProductionProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(sheet => sheet.Company).WithMany().HasForeignKey(sheet => sheet.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(sheet => new { sheet.CompanyId, sheet.ProductionProductId });
            entity.HasIndex(sheet => new { sheet.CompanyId, sheet.ExternalProductId });
        });

        modelBuilder.Entity<ProductTechnicalSheetOperation>(entity =>
        {
            entity.ToTable("product_technical_sheet_operations");
            entity.HasKey(operation => operation.Id);
            entity.Property(operation => operation.Id).HasColumnName("id");
            entity.Property(operation => operation.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(operation => operation.ProductTechnicalSheetId).HasColumnName("product_technical_sheet_id").IsRequired();
            entity.Property(operation => operation.ProductionOperationId).HasColumnName("production_operation_id").IsRequired();
            entity.Property(operation => operation.OperationNameSnapshot).HasColumnName("operation_name_snapshot").HasMaxLength(160).IsRequired();
            entity.Property(operation => operation.Sequence).HasColumnName("sequence").IsRequired();
            entity.Property(operation => operation.StandardQuantity).HasColumnName("standard_quantity").HasPrecision(14, 4);
            entity.Property(operation => operation.StandardTime).HasColumnName("standard_time").HasPrecision(14, 4);
            entity.Property(operation => operation.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(operation => operation.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(operation => operation.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(operation => operation.ProductTechnicalSheet).WithMany(sheet => sheet.Operations).HasForeignKey(operation => operation.ProductTechnicalSheetId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(operation => operation.ProductionOperation).WithMany().HasForeignKey(operation => operation.ProductionOperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(operation => operation.Company).WithMany().HasForeignKey(operation => operation.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(operation => new { operation.ProductTechnicalSheetId, operation.Sequence });
        });
    }

    private static void ConfigureRateTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionRateTable>(entity =>
        {
            entity.ToTable("production_rate_tables");
            entity.HasKey(table => table.Id);
            entity.Property(table => table.Id).HasColumnName("id");
            entity.Property(table => table.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(table => table.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(table => table.EffectiveFrom).HasColumnName("effective_from").IsRequired();
            entity.Property(table => table.EffectiveTo).HasColumnName("effective_to");
            entity.Property(table => table.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(table => table.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(table => table.DeletedAt).HasColumnName("deleted_at");
            entity.Property(table => table.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(table => table.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(table => table.Company).WithMany().HasForeignKey(table => table.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(table => new { table.CompanyId, table.EffectiveFrom, table.Status });
        });

        modelBuilder.Entity<ProductionRate>(entity =>
        {
            entity.ToTable("production_rates");
            entity.HasKey(rate => rate.Id);
            entity.Property(rate => rate.Id).HasColumnName("id");
            entity.Property(rate => rate.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(rate => rate.ProductionRateTableId).HasColumnName("production_rate_table_id").IsRequired();
            entity.Property(rate => rate.ProductionProductId).HasColumnName("production_product_id");
            entity.Property(rate => rate.ProductionOperationId).HasColumnName("production_operation_id");
            entity.Property(rate => rate.JobPositionId).HasColumnName("job_position_id");
            entity.Property(rate => rate.DepartmentId).HasColumnName("department_id");
            entity.Property(rate => rate.ProductionCellId).HasColumnName("production_cell_id");
            entity.Property(rate => rate.UnitValue).HasColumnName("unit_value").HasPrecision(14, 4).IsRequired();
            entity.Property(rate => rate.CalculationType).HasColumnName("calculation_type").HasMaxLength(40).IsRequired();
            entity.Property(rate => rate.MinimumQuantity).HasColumnName("minimum_quantity").HasPrecision(14, 4);
            entity.Property(rate => rate.MaximumQuantity).HasColumnName("maximum_quantity").HasPrecision(14, 4);
            entity.Property(rate => rate.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(rate => rate.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(rate => rate.DeletedAt).HasColumnName("deleted_at");
            entity.Property(rate => rate.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(rate => rate.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(rate => rate.ProductionRateTable).WithMany(table => table.Rates).HasForeignKey(rate => rate.ProductionRateTableId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rate => rate.ProductionProduct).WithMany().HasForeignKey(rate => rate.ProductionProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rate => rate.ProductionOperation).WithMany().HasForeignKey(rate => rate.ProductionOperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rate => rate.JobPosition).WithMany().HasForeignKey(rate => rate.JobPositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rate => rate.Department).WithMany().HasForeignKey(rate => rate.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rate => rate.ProductionCell).WithMany().HasForeignKey(rate => rate.ProductionCellId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rate => rate.Company).WithMany().HasForeignKey(rate => rate.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(rate => rate.ProductionRateTableId);
            entity.HasIndex(rate => new { rate.ProductionProductId, rate.ProductionOperationId });
            entity.HasIndex(rate => new { rate.DepartmentId, rate.JobPositionId, rate.ProductionCellId });
        });
    }

    private static void ConfigureProductionEntries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeProductionEntry>(entity =>
        {
            entity.ToTable("employee_production_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasColumnName("id");
            entity.Property(entry => entry.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(entry => entry.PayrollPeriodId).HasColumnName("payroll_period_id").IsRequired();
            entity.Property(entry => entry.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(entry => entry.ProductionDate).HasColumnName("production_date").IsRequired();
            entity.Property(entry => entry.ProductionOrderId).HasColumnName("production_order_id");
            entity.Property(entry => entry.ProductionOrderProductId).HasColumnName("production_order_product_id");
            entity.Property(entry => entry.ProductionProductId).HasColumnName("production_product_id").IsRequired();
            entity.Property(entry => entry.ProductionOperationId).HasColumnName("production_operation_id").IsRequired();
            entity.Property(entry => entry.ProductionCellId).HasColumnName("production_cell_id");
            entity.Property(entry => entry.Quantity).HasColumnName("quantity").HasPrecision(14, 4).IsRequired();
            entity.Property(entry => entry.UnitValue).HasColumnName("unit_value").HasPrecision(14, 4).IsRequired();
            entity.Property(entry => entry.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2).IsRequired();
            entity.Property(entry => entry.RateSource).HasColumnName("rate_source").HasMaxLength(40).IsRequired();
            entity.Property(entry => entry.ProductionRateId).HasColumnName("production_rate_id");
            entity.Property(entry => entry.Origin).HasColumnName("origin").HasMaxLength(40).IsRequired();
            entity.Property(entry => entry.Status).HasColumnName("status").HasMaxLength(40).IsRequired();
            entity.Property(entry => entry.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(entry => entry.ApprovedAt).HasColumnName("approved_at");
            entity.Property(entry => entry.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(entry => entry.IntegratedPayrollCalculationId).HasColumnName("integrated_payroll_calculation_id");
            entity.Property(entry => entry.IntegratedPayrollCalculationItemId).HasColumnName("integrated_payroll_calculation_item_id");
            entity.Property(entry => entry.EmployeeRegistrationSnapshot).HasColumnName("employee_registration_snapshot").HasMaxLength(30).IsRequired();
            entity.Property(entry => entry.EmployeeNameSnapshot).HasColumnName("employee_name_snapshot").HasMaxLength(160).IsRequired();
            entity.Property(entry => entry.OrderNumberSnapshot).HasColumnName("order_number_snapshot").HasMaxLength(80);
            entity.Property(entry => entry.ProductReferenceSnapshot).HasColumnName("product_reference_snapshot").HasMaxLength(80).IsRequired();
            entity.Property(entry => entry.ProductDescriptionSnapshot).HasColumnName("product_description_snapshot").HasMaxLength(250).IsRequired();
            entity.Property(entry => entry.OperationNameSnapshot).HasColumnName("operation_name_snapshot").HasMaxLength(160).IsRequired();
            entity.Property(entry => entry.CellNameSnapshot).HasColumnName("cell_name_snapshot").HasMaxLength(160);
            entity.Property(entry => entry.DeletedAt).HasColumnName("deleted_at");
            entity.Property(entry => entry.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(entry => entry.Company).WithMany().HasForeignKey(entry => entry.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.PayrollPeriod).WithMany().HasForeignKey(entry => entry.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.Employee).WithMany().HasForeignKey(entry => entry.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionOrder).WithMany().HasForeignKey(entry => entry.ProductionOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionOrderProduct).WithMany().HasForeignKey(entry => entry.ProductionOrderProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionProduct).WithMany().HasForeignKey(entry => entry.ProductionProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionOperation).WithMany().HasForeignKey(entry => entry.ProductionOperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionCell).WithMany().HasForeignKey(entry => entry.ProductionCellId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.ProductionRate).WithMany().HasForeignKey(entry => entry.ProductionRateId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(entry => entry.ApprovedByUser).WithMany().HasForeignKey(entry => entry.ApprovedByUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(entry => entry.IntegratedPayrollCalculation).WithMany().HasForeignKey(entry => entry.IntegratedPayrollCalculationId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(entry => entry.IntegratedPayrollCalculationItem).WithMany().HasForeignKey(entry => entry.IntegratedPayrollCalculationItemId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(entry => new { entry.PayrollPeriodId, entry.EmployeeId });
            entity.HasIndex(entry => entry.ProductionDate);
            entity.HasIndex(entry => entry.ProductionOrderId);
            entity.HasIndex(entry => entry.ProductionProductId);
            entity.HasIndex(entry => entry.ProductionOperationId);
            entity.HasIndex(entry => entry.Status);
        });

        ConfigureProductionBatches(modelBuilder);
    }

    private static void ConfigureProductionBatches(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeProductionBatch>(entity =>
        {
            entity.ToTable("employee_production_batches");
            entity.HasKey(batch => batch.Id);
            entity.Property(batch => batch.Id).HasColumnName("id");
            entity.Property(batch => batch.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(batch => batch.PayrollPeriodId).HasColumnName("payroll_period_id").IsRequired();
            entity.Property(batch => batch.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(batch => batch.ProductionDate).HasColumnName("production_date").IsRequired();
            entity.Property(batch => batch.ProductionOrderId).HasColumnName("production_order_id");
            entity.Property(batch => batch.ProductionProductId).HasColumnName("production_product_id");
            entity.Property(batch => batch.ProductionOperationId).HasColumnName("production_operation_id");
            entity.Property(batch => batch.ProductionCellId).HasColumnName("production_cell_id");
            entity.Property(batch => batch.DefaultQuantity).HasColumnName("default_quantity").HasPrecision(14, 4);
            entity.Property(batch => batch.DefaultUnitValue).HasColumnName("default_unit_value").HasPrecision(14, 4);
            entity.Property(batch => batch.DefaultNotes).HasColumnName("default_notes").HasMaxLength(1000);
            entity.Property(batch => batch.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(batch => batch.TotalEmployees).HasColumnName("total_employees").IsRequired();
            entity.Property(batch => batch.TotalQuantity).HasColumnName("total_quantity").HasPrecision(14, 4).IsRequired();
            entity.Property(batch => batch.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2).IsRequired();
            entity.Property(batch => batch.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(batch => batch.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(batch => batch.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(batch => batch.Company).WithMany().HasForeignKey(batch => batch.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.PayrollPeriod).WithMany().HasForeignKey(batch => batch.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.ProductionOrder).WithMany().HasForeignKey(batch => batch.ProductionOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.ProductionProduct).WithMany().HasForeignKey(batch => batch.ProductionProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.ProductionOperation).WithMany().HasForeignKey(batch => batch.ProductionOperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.ProductionCell).WithMany().HasForeignKey(batch => batch.ProductionCellId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(batch => batch.CreatedByUser).WithMany().HasForeignKey(batch => batch.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(batch => new { batch.CompanyId, batch.PayrollPeriodId, batch.Status });
        });

        modelBuilder.Entity<EmployeeProductionBatchItem>(entity =>
        {
            entity.ToTable("employee_production_batch_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(item => item.EmployeeProductionBatchId).HasColumnName("employee_production_batch_id").IsRequired();
            entity.Property(item => item.EmployeeProductionEntryId).HasColumnName("employee_production_entry_id");
            entity.Property(item => item.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(14, 4).IsRequired();
            entity.Property(item => item.UnitValue).HasColumnName("unit_value").HasPrecision(14, 4).IsRequired();
            entity.Property(item => item.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2).IsRequired();
            entity.Property(item => item.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(item => item.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(item => item.Company).WithMany().HasForeignKey(item => item.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.EmployeeProductionBatch).WithMany(batch => batch.Items).HasForeignKey(item => item.EmployeeProductionBatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.EmployeeProductionEntry).WithMany().HasForeignKey(item => item.EmployeeProductionEntryId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(item => item.Employee).WithMany().HasForeignKey(item => item.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(item => item.EmployeeProductionBatchId);
            entity.HasIndex(item => item.EmployeeProductionEntryId);
            entity.HasIndex(item => item.EmployeeId);
        });
    }
}
