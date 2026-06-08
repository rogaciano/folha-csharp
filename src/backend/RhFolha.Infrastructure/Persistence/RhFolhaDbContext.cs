using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Departments;
using RhFolha.Domain.Employees;
using RhFolha.Domain.JobPositions;
using RhFolha.Domain.Payroll;
using RhFolha.Domain.Security;

namespace RhFolha.Infrastructure.Persistence;

public sealed class RhFolhaDbContext(DbContextOptions<RhFolhaDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeEvent> EmployeeEvents => Set<EmployeeEvent>();
    public DbSet<Rubric> Rubrics => Set<Rubric>();
    public DbSet<RubricValidity> RubricValidities => Set<RubricValidity>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollEntry> PayrollEntries => Set<PayrollEntry>();
    public DbSet<FixedPayrollEntry> FixedPayrollEntries => Set<FixedPayrollEntry>();
    public DbSet<PayrollCalculation> PayrollCalculations => Set<PayrollCalculation>();
    public DbSet<PayrollCalculationItem> PayrollCalculationItems => Set<PayrollCalculationItem>();
    public DbSet<StatutoryTable> StatutoryTables => Set<StatutoryTable>();
    public DbSet<StatutoryTableRange> StatutoryTableRanges => Set<StatutoryTableRange>();
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(company => company.Id);
            entity.Property(company => company.Id).HasColumnName("id");
            entity.Property(company => company.LegalName).HasColumnName("legal_name").HasMaxLength(200).IsRequired();
            entity.Property(company => company.TradeName).HasColumnName("trade_name").HasMaxLength(200);
            entity.Property(company => company.DocumentNumber).HasColumnName("document_number").HasMaxLength(20).IsRequired();
            entity.Property(company => company.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(company => company.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(company => company.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(company => company.DocumentNumber).IsUnique();
        });

        modelBuilder.Entity<SystemUser>(entity =>
        {
            entity.ToTable("system_users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.CompanyId).HasColumnName("company_id");
            entity.Property(user => user.FullName).HasColumnName("full_name").HasMaxLength(160).IsRequired();
            entity.Property(user => user.Email).HasColumnName("email").HasMaxLength(180).IsRequired();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasColumnName("role").HasMaxLength(40).IsRequired();
            entity.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(user => user.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(user => user.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(user => user.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<Company>()
                .WithMany()
                .HasForeignKey(user => user.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => new { user.CompanyId, user.Role });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Id).HasColumnName("id");
            entity.Property(log => log.UserId).HasColumnName("user_id");
            entity.Property(log => log.UserName).HasColumnName("user_name").HasMaxLength(160).IsRequired();
            entity.Property(log => log.UserEmail).HasColumnName("user_email").HasMaxLength(180).IsRequired();
            entity.Property(log => log.UserRole).HasColumnName("user_role").HasMaxLength(40).IsRequired();
            entity.Property(log => log.Action).HasColumnName("action").HasMaxLength(80).IsRequired();
            entity.Property(log => log.EntityName).HasColumnName("entity_name").HasMaxLength(120).IsRequired();
            entity.Property(log => log.EntityId).HasColumnName("entity_id");
            entity.Property(log => log.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
            entity.Property(log => log.IpAddress).HasColumnName("ip_address").HasMaxLength(80);
            entity.Property(log => log.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(log => log.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => new { log.EntityName, log.EntityId });
            entity.HasIndex(log => log.UserId);
            entity.HasOne<SystemUser>()
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(department => department.Id);
            entity.Property(department => department.Id).HasColumnName("id");
            entity.Property(department => department.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(department => department.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(department => department.InternalCode).HasColumnName("internal_code").HasMaxLength(30).IsRequired();
            entity.Property(department => department.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(department => department.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(department => department.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(department => department.Company)
                .WithMany(company => company.Departments)
                .HasForeignKey(department => department.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(department => new { department.CompanyId, department.InternalCode }).IsUnique();
        });

        modelBuilder.Entity<JobPosition>(entity =>
        {
            entity.ToTable("job_positions");
            entity.HasKey(jobPosition => jobPosition.Id);
            entity.Property(jobPosition => jobPosition.Id).HasColumnName("id");
            entity.Property(jobPosition => jobPosition.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(jobPosition => jobPosition.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(jobPosition => jobPosition.InternalCode).HasColumnName("internal_code").HasMaxLength(30).IsRequired();
            entity.Property(jobPosition => jobPosition.Cbo).HasColumnName("cbo").HasMaxLength(10);
            entity.Property(jobPosition => jobPosition.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(jobPosition => jobPosition.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(jobPosition => jobPosition.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(jobPosition => jobPosition.Company)
                .WithMany(company => company.JobPositions)
                .HasForeignKey(jobPosition => jobPosition.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(jobPosition => new { jobPosition.CompanyId, jobPosition.InternalCode }).IsUnique();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(employee => employee.Id);
            entity.Property(employee => employee.Id).HasColumnName("id");
            entity.Property(employee => employee.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(employee => employee.DepartmentId).HasColumnName("department_id").IsRequired();
            entity.Property(employee => employee.JobPositionId).HasColumnName("job_position_id").IsRequired();
            entity.Property(employee => employee.ResponsibleEmployeeId).HasColumnName("responsible_employee_id");
            entity.Property(employee => employee.Registration).HasColumnName("registration").HasMaxLength(30).IsRequired();
            entity.Property(employee => employee.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(employee => employee.DocumentNumber).HasColumnName("document_number").HasMaxLength(20).IsRequired();
            entity.Property(employee => employee.AdmissionDate).HasColumnName("admission_date").IsRequired();
            entity.Property(employee => employee.CompensationModel).HasColumnName("compensation_model").HasMaxLength(30).IsRequired();
            entity.Property(employee => employee.BaseSalary).HasColumnName("base_salary").HasPrecision(14, 2).IsRequired();
            entity.Property(employee => employee.ProductionUnitValue).HasColumnName("production_unit_value").HasPrecision(14, 2).IsRequired();
            entity.Property(employee => employee.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
            entity.Property(employee => employee.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(employee => employee.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(employee => employee.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(employee => employee.Company)
                .WithMany(company => company.Employees)
                .HasForeignKey(employee => employee.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(employee => employee.Department)
                .WithMany()
                .HasForeignKey(employee => employee.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(employee => employee.JobPosition)
                .WithMany()
                .HasForeignKey(employee => employee.JobPositionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(employee => employee.ResponsibleEmployee)
                .WithMany()
                .HasForeignKey(employee => employee.ResponsibleEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(employee => new { employee.CompanyId, employee.Registration }).IsUnique();
            entity.HasIndex(employee => new { employee.CompanyId, employee.DocumentNumber }).IsUnique();
            entity.HasIndex(employee => new { employee.CompanyId, employee.ResponsibleEmployeeId });
        });

        modelBuilder.Entity<EmployeeEvent>(entity =>
        {
            entity.ToTable("employee_events");
            entity.HasKey(employeeEvent => employeeEvent.Id);
            entity.Property(employeeEvent => employeeEvent.Id).HasColumnName("id");
            entity.Property(employeeEvent => employeeEvent.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(employeeEvent => employeeEvent.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(employeeEvent => employeeEvent.EventDate).HasColumnName("event_date").IsRequired();
            entity.Property(employeeEvent => employeeEvent.Type).HasColumnName("type").HasMaxLength(40).IsRequired();
            entity.Property(employeeEvent => employeeEvent.Title).HasColumnName("title").HasMaxLength(140).IsRequired();
            entity.Property(employeeEvent => employeeEvent.Description).HasColumnName("description").HasMaxLength(1000).IsRequired();
            entity.Property(employeeEvent => employeeEvent.Responsible).HasColumnName("responsible").HasMaxLength(120).IsRequired();
            entity.Property(employeeEvent => employeeEvent.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(employeeEvent => employeeEvent.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(employeeEvent => employeeEvent.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(employeeEvent => employeeEvent.Company)
                .WithMany(company => company.EmployeeEvents)
                .HasForeignKey(employeeEvent => employeeEvent.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(employeeEvent => employeeEvent.Employee)
                .WithMany()
                .HasForeignKey(employeeEvent => employeeEvent.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(employeeEvent => new { employeeEvent.CompanyId, employeeEvent.EmployeeId, employeeEvent.EventDate });
            entity.HasIndex(employeeEvent => new { employeeEvent.CompanyId, employeeEvent.Type });
            entity.HasIndex(employeeEvent => new { employeeEvent.CompanyId, employeeEvent.Status });
        });

        modelBuilder.Entity<Rubric>(entity =>
        {
            entity.ToTable("rubrics");
            entity.HasKey(rubric => rubric.Id);
            entity.Property(rubric => rubric.Id).HasColumnName("id");
            entity.Property(rubric => rubric.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(rubric => rubric.Code).HasColumnName("code").HasMaxLength(30).IsRequired();
            entity.Property(rubric => rubric.Name).HasColumnName("name").HasMaxLength(140).IsRequired();
            entity.Property(rubric => rubric.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
            entity.Property(rubric => rubric.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(rubric => rubric.ESocialNature).HasColumnName("esocial_nature").HasMaxLength(20);
            entity.Property(rubric => rubric.AllowsManualEntry).HasColumnName("allows_manual_entry").IsRequired();
            entity.Property(rubric => rubric.AllowsMassEntry).HasColumnName("allows_mass_entry").IsRequired();
            entity.Property(rubric => rubric.AllowsFixedEntry).HasColumnName("allows_fixed_entry").IsRequired();
            entity.Property(rubric => rubric.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(rubric => rubric.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(rubric => rubric.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(rubric => rubric.Company)
                .WithMany(company => company.Rubrics)
                .HasForeignKey(rubric => rubric.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(rubric => new { rubric.CompanyId, rubric.Code }).IsUnique();
        });

        modelBuilder.Entity<RubricValidity>(entity =>
        {
            entity.ToTable("rubric_validities");
            entity.HasKey(validity => validity.Id);
            entity.Property(validity => validity.Id).HasColumnName("id");
            entity.Property(validity => validity.RubricId).HasColumnName("rubric_id").IsRequired();
            entity.Property(validity => validity.StartsOn).HasColumnName("starts_on").IsRequired();
            entity.Property(validity => validity.EndsOn).HasColumnName("ends_on");
            entity.Property(validity => validity.IncidenceInss).HasColumnName("incidence_inss").IsRequired();
            entity.Property(validity => validity.IncidenceFgts).HasColumnName("incidence_fgts").IsRequired();
            entity.Property(validity => validity.IncidenceIrrf).HasColumnName("incidence_irrf").IsRequired();
            entity.Property(validity => validity.IncidenceDsr).HasColumnName("incidence_dsr").IsRequired();
            entity.Property(validity => validity.CalculationMethod).HasColumnName("calculation_method").HasMaxLength(40).IsRequired();
            entity.Property(validity => validity.CalculationBase).HasColumnName("calculation_base").HasMaxLength(40).IsRequired();
            entity.Property(validity => validity.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(validity => validity.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(validity => validity.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(validity => validity.Rubric)
                .WithMany(rubric => rubric.Validities)
                .HasForeignKey(validity => validity.RubricId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(validity => new { validity.RubricId, validity.StartsOn }).IsUnique();
        });

        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("payroll_periods");
            entity.HasKey(period => period.Id);
            entity.Property(period => period.Id).HasColumnName("id");
            entity.Property(period => period.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(period => period.Year).HasColumnName("year").IsRequired();
            entity.Property(period => period.Month).HasColumnName("month").IsRequired();
            entity.Property(period => period.Code).HasColumnName("code").HasMaxLength(7).IsRequired();
            entity.Property(period => period.StartsOn).HasColumnName("starts_on").IsRequired();
            entity.Property(period => period.EndsOn).HasColumnName("ends_on").IsRequired();
            entity.Property(period => period.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(period => period.OpenedAt).HasColumnName("opened_at").IsRequired();
            entity.Property(period => period.ClosedAt).HasColumnName("closed_at");
            entity.Property(period => period.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(period => period.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(period => period.Company)
                .WithMany(company => company.PayrollPeriods)
                .HasForeignKey(period => period.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(period => new { period.CompanyId, period.Year, period.Month }).IsUnique();
            entity.HasIndex(period => new { period.CompanyId, period.Code }).IsUnique();
        });

        modelBuilder.Entity<PayrollEntry>(entity =>
        {
            entity.ToTable("payroll_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasColumnName("id");
            entity.Property(entry => entry.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(entry => entry.PayrollPeriodId).HasColumnName("payroll_period_id").IsRequired();
            entity.Property(entry => entry.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(entry => entry.RubricId).HasColumnName("rubric_id").IsRequired();
            entity.Property(entry => entry.EntryDate).HasColumnName("entry_date").IsRequired();
            entity.Property(entry => entry.Amount).HasColumnName("amount").HasPrecision(14, 2).IsRequired();
            entity.Property(entry => entry.Quantity).HasColumnName("quantity").HasPrecision(14, 4);
            entity.Property(entry => entry.Reference).HasColumnName("reference").HasMaxLength(80);
            entity.Property(entry => entry.Notes).HasColumnName("notes").HasMaxLength(500);
            entity.Property(entry => entry.Origin).HasColumnName("origin").HasMaxLength(30).IsRequired();
            entity.Property(entry => entry.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(entry => entry.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(entry => entry.Company)
                .WithMany(company => company.PayrollEntries)
                .HasForeignKey(entry => entry.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.PayrollPeriod)
                .WithMany()
                .HasForeignKey(entry => entry.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.Employee)
                .WithMany()
                .HasForeignKey(entry => entry.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.Rubric)
                .WithMany()
                .HasForeignKey(entry => entry.RubricId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(entry => new { entry.CompanyId, entry.PayrollPeriodId });
            entity.HasIndex(entry => new { entry.CompanyId, entry.EmployeeId, entry.PayrollPeriodId });
            entity.HasIndex(entry => new { entry.CompanyId, entry.RubricId, entry.PayrollPeriodId });
        });

        modelBuilder.Entity<FixedPayrollEntry>(entity =>
        {
            entity.ToTable("fixed_payroll_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).HasColumnName("id");
            entity.Property(entry => entry.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(entry => entry.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(entry => entry.RubricId).HasColumnName("rubric_id").IsRequired();
            entity.Property(entry => entry.StartsOn).HasColumnName("starts_on").IsRequired();
            entity.Property(entry => entry.EndsOn).HasColumnName("ends_on");
            entity.Property(entry => entry.Amount).HasColumnName("amount").HasPrecision(14, 2).IsRequired();
            entity.Property(entry => entry.Quantity).HasColumnName("quantity").HasPrecision(14, 4);
            entity.Property(entry => entry.Notes).HasColumnName("notes").HasMaxLength(500);
            entity.Property(entry => entry.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(entry => entry.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(entry => entry.Company)
                .WithMany(company => company.FixedPayrollEntries)
                .HasForeignKey(entry => entry.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.Employee)
                .WithMany()
                .HasForeignKey(entry => entry.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(entry => entry.Rubric)
                .WithMany()
                .HasForeignKey(entry => entry.RubricId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(entry => new { entry.CompanyId, entry.EmployeeId });
            entity.HasIndex(entry => new { entry.CompanyId, entry.RubricId });
            entity.HasIndex(entry => new { entry.CompanyId, entry.IsActive });
        });

        modelBuilder.Entity<PayrollCalculation>(entity =>
        {
            entity.ToTable("payroll_calculations");
            entity.HasKey(calculation => calculation.Id);
            entity.Property(calculation => calculation.Id).HasColumnName("id");
            entity.Property(calculation => calculation.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(calculation => calculation.PayrollPeriodId).HasColumnName("payroll_period_id").IsRequired();
            entity.Property(calculation => calculation.PeriodCode).HasColumnName("period_code").HasMaxLength(7).IsRequired();
            entity.Property(calculation => calculation.CalculatedAt).HasColumnName("calculated_at").IsRequired();
            entity.Property(calculation => calculation.TotalProventos).HasColumnName("total_proventos").HasPrecision(14, 2).IsRequired();
            entity.Property(calculation => calculation.TotalDescontos).HasColumnName("total_descontos").HasPrecision(14, 2).IsRequired();
            entity.Property(calculation => calculation.TotalLiquido).HasColumnName("total_liquido").HasPrecision(14, 2).IsRequired();
            entity.Property(calculation => calculation.EmployeeCount).HasColumnName("employee_count").IsRequired();
            entity.Property(calculation => calculation.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entity.Property(calculation => calculation.IsCurrent).HasColumnName("is_current").IsRequired();
            entity.Property(calculation => calculation.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(calculation => calculation.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(calculation => calculation.Company)
                .WithMany(company => company.PayrollCalculations)
                .HasForeignKey(calculation => calculation.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(calculation => calculation.PayrollPeriod)
                .WithMany()
                .HasForeignKey(calculation => calculation.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(calculation => new { calculation.CompanyId, calculation.PayrollPeriodId, calculation.IsCurrent });
        });

        modelBuilder.Entity<PayrollCalculationItem>(entity =>
        {
            entity.ToTable("payroll_calculation_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.PayrollCalculationId).HasColumnName("payroll_calculation_id").IsRequired();
            entity.Property(item => item.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(item => item.EmployeeRegistration).HasColumnName("employee_registration").HasMaxLength(30).IsRequired();
            entity.Property(item => item.EmployeeName).HasColumnName("employee_name").HasMaxLength(160).IsRequired();
            entity.Property(item => item.RubricId).HasColumnName("rubric_id").IsRequired();
            entity.Property(item => item.RubricCode).HasColumnName("rubric_code").HasMaxLength(30).IsRequired();
            entity.Property(item => item.RubricName).HasColumnName("rubric_name").HasMaxLength(140).IsRequired();
            entity.Property(item => item.RubricType).HasColumnName("rubric_type").HasMaxLength(30).IsRequired();
            entity.Property(item => item.Origin).HasColumnName("origin").HasMaxLength(30).IsRequired();
            entity.Property(item => item.Amount).HasColumnName("amount").HasPrecision(14, 2).IsRequired();
            entity.Property(item => item.Quantity).HasColumnName("quantity").HasPrecision(14, 4);
            entity.Property(item => item.BaseAmount).HasColumnName("base_amount").HasPrecision(14, 2);
            entity.Property(item => item.CalculationRate).HasColumnName("calculation_rate").HasPrecision(7, 4);
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(item => item.PayrollCalculation)
                .WithMany(calculation => calculation.Items)
                .HasForeignKey(item => item.PayrollCalculationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Employee)
                .WithMany()
                .HasForeignKey(item => item.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Rubric)
                .WithMany()
                .HasForeignKey(item => item.RubricId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(item => new { item.PayrollCalculationId, item.EmployeeId });
            entity.HasIndex(item => new { item.PayrollCalculationId, item.RubricId });
        });

        modelBuilder.Entity<StatutoryTable>(entity =>
        {
            entity.ToTable("statutory_tables");
            entity.HasKey(table => table.Id);
            entity.Property(table => table.Id).HasColumnName("id");
            entity.Property(table => table.CompanyId).HasColumnName("company_id").IsRequired();
            entity.Property(table => table.Type).HasColumnName("type").HasMaxLength(30).IsRequired();
            entity.Property(table => table.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(table => table.StartsOn).HasColumnName("starts_on").IsRequired();
            entity.Property(table => table.EndsOn).HasColumnName("ends_on");
            entity.Property(table => table.Notes).HasColumnName("notes").HasMaxLength(500);
            entity.Property(table => table.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(table => table.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(table => table.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(table => table.Company)
                .WithMany(company => company.StatutoryTables)
                .HasForeignKey(table => table.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(table => new { table.CompanyId, table.Type, table.StartsOn }).IsUnique();
            entity.HasIndex(table => new { table.CompanyId, table.IsActive });
        });

        modelBuilder.Entity<StatutoryTableRange>(entity =>
        {
            entity.ToTable("statutory_table_ranges");
            entity.HasKey(range => range.Id);
            entity.Property(range => range.Id).HasColumnName("id");
            entity.Property(range => range.StatutoryTableId).HasColumnName("statutory_table_id").IsRequired();
            entity.Property(range => range.LowerLimit).HasColumnName("lower_limit").HasPrecision(14, 2).IsRequired();
            entity.Property(range => range.UpperLimit).HasColumnName("upper_limit").HasPrecision(14, 2);
            entity.Property(range => range.RatePercent).HasColumnName("rate_percent").HasPrecision(7, 4).IsRequired();
            entity.Property(range => range.DeductionAmount).HasColumnName("deduction_amount").HasPrecision(14, 2).IsRequired();
            entity.Property(range => range.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(range => range.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(range => range.StatutoryTable)
                .WithMany(table => table.Ranges)
                .HasForeignKey(range => range.StatutoryTableId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(range => new { range.StatutoryTableId, range.LowerLimit }).IsUnique();
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc);
        var companyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var operationsDepartmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
        var adminDepartmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
        var sewingDepartmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3");
        var managerJobId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
        var assistantJobId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
        var seamstressJobId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");
        var salaryRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01");
        var productionRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02");
        var advanceRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03");
        var advanceDiscountRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04");
        var transportRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05");
        var healthPlanRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06");
        var inssRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07");
        var irrfRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee08");
        var fgtsRubricId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeee09");
        var currentPeriodId = Guid.Parse("99999999-9999-9999-9999-999999999901");
        var inssTableId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var irrfTableId = Guid.Parse("33333333-3333-3333-3333-333333333302");
        var irrfReductionTableId = Guid.Parse("33333333-3333-3333-3333-333333333303");
        var fgtsMonthlyTableId = Guid.Parse("33333333-3333-3333-3333-333333333304");
        var fgtsBirthdayWithdrawalTableId = Guid.Parse("33333333-3333-3333-3333-333333333305");

        modelBuilder.Entity<Company>().HasData(new
        {
            Id = companyId,
            LegalName = "ConsulCLT Demonstracao Ltda",
            TradeName = "ConsulCLT",
            DocumentNumber = "12345678000190",
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        });

        modelBuilder.Entity<Department>().HasData(
            new { Id = operationsDepartmentId, CompanyId = companyId, Name = "Operacoes", InternalCode = "OPER", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null },
            new { Id = adminDepartmentId, CompanyId = companyId, Name = "Administrativo", InternalCode = "ADM", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null },
            new { Id = sewingDepartmentId, CompanyId = companyId, Name = "Producao", InternalCode = "PROD", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null });

        modelBuilder.Entity<JobPosition>().HasData(
            new { Id = managerJobId, CompanyId = companyId, Name = "Gerente de Operacoes", InternalCode = "GER-OPER", Cbo = "142105", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null },
            new { Id = assistantJobId, CompanyId = companyId, Name = "Assistente Administrativo", InternalCode = "ASS-ADM", Cbo = "411010", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null },
            new { Id = seamstressJobId, CompanyId = companyId, Name = "Costureira Industrial", InternalCode = "COST-IND", Cbo = "763210", IsActive = true, CreatedAt = createdAt, UpdatedAt = (DateTime?)null });

        modelBuilder.Entity<Employee>().HasData(
            new
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                CompanyId = companyId,
                DepartmentId = operationsDepartmentId,
                JobPositionId = managerJobId,
                Registration = "0001",
                Name = "Roberto de Souza Carvalho",
                DocumentNumber = "78912345608",
                AdmissionDate = new DateOnly(2022, 11, 1),
                CompensationModel = "mensalista",
                BaseSalary = 9500.00m,
                ProductionUnitValue = 0m,
                Status = EmployeeStatus.Active,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                CompanyId = companyId,
                DepartmentId = adminDepartmentId,
                JobPositionId = assistantJobId,
                Registration = "0002",
                Name = "Mariana Costa Santos",
                DocumentNumber = "45678912304",
                AdmissionDate = new DateOnly(2025, 1, 20),
                CompensationModel = "mensalista",
                BaseSalary = 3200.00m,
                ProductionUnitValue = 0m,
                Status = EmployeeStatus.Active,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd03"),
                CompanyId = companyId,
                DepartmentId = sewingDepartmentId,
                JobPositionId = seamstressJobId,
                Registration = "0003",
                Name = "Ana Beatris Oliveira",
                DocumentNumber = "98765432100",
                AdmissionDate = new DateOnly(2023, 8, 10),
                CompensationModel = "producao",
                BaseSalary = 0m,
                ProductionUnitValue = 12.50m,
                Status = EmployeeStatus.Active,
                CreatedAt = createdAt,
                UpdatedAt = (DateTime?)null
            });

        modelBuilder.Entity<Rubric>().HasData(
            RubricSeed(salaryRubricId, companyId, "001", "Salario mensal", "provento", "1000", false, false, false, createdAt),
            RubricSeed(productionRubricId, companyId, "002", "Producao", "provento", "1000", true, true, false, createdAt),
            RubricSeed(advanceRubricId, companyId, "101", "Adiantamento salarial", "provento", null, true, true, false, createdAt),
            RubricSeed(advanceDiscountRubricId, companyId, "201", "Desconto de adiantamento", "desconto", null, true, true, false, createdAt),
            RubricSeed(transportRubricId, companyId, "202", "Vale transporte", "desconto", null, true, true, true, createdAt),
            RubricSeed(healthPlanRubricId, companyId, "203", "Plano de saude", "desconto", null, true, true, true, createdAt),
            RubricSeed(inssRubricId, companyId, "901", "INSS", "desconto", null, false, false, false, createdAt),
            RubricSeed(irrfRubricId, companyId, "902", "IRRF", "desconto", null, false, false, false, createdAt),
            RubricSeed(fgtsRubricId, companyId, "903", "FGTS informativo", "informativa", null, false, false, false, createdAt));

        modelBuilder.Entity<RubricValidity>().HasData(
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff01"), salaryRubricId, createdAt, true, true, true, true, "sistema", "salario_base"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff02"), productionRubricId, createdAt, true, true, true, false, "quantidade_valor", "producao"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff03"), advanceRubricId, createdAt, false, false, false, false, "valor_fixo", "nenhuma"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff04"), advanceDiscountRubricId, createdAt, false, false, false, false, "valor_fixo", "nenhuma"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff05"), transportRubricId, createdAt, false, false, false, false, "valor_fixo", "nenhuma"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff06"), healthPlanRubricId, createdAt, false, false, false, false, "valor_fixo", "nenhuma"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff07"), inssRubricId, createdAt, false, false, false, false, "sistema", "base_inss"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff08"), irrfRubricId, createdAt, false, false, false, false, "sistema", "base_irrf"),
            ValiditySeed(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffff09"), fgtsRubricId, createdAt, false, false, false, false, "sistema", "base_fgts"));

        modelBuilder.Entity<PayrollPeriod>().HasData(new
        {
            Id = currentPeriodId,
            CompanyId = companyId,
            Year = 2026,
            Month = 6,
            Code = "2026-06",
            StartsOn = new DateOnly(2026, 6, 1),
            EndsOn = new DateOnly(2026, 6, 30),
            Status = "aberta",
            OpenedAt = createdAt,
            ClosedAt = (DateTime?)null,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        });

        modelBuilder.Entity<PayrollEntry>().HasData(
            PayrollEntrySeed(
                Guid.Parse("11111111-1111-1111-1111-111111111101"),
                companyId,
                currentPeriodId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                advanceRubricId,
                new DateOnly(2026, 6, 15),
                800m,
                null,
                "ADV-06/2026",
                "Adiantamento quinzenal",
                createdAt),
            PayrollEntrySeed(
                Guid.Parse("11111111-1111-1111-1111-111111111102"),
                companyId,
                currentPeriodId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd03"),
                productionRubricId,
                new DateOnly(2026, 6, 20),
                625m,
                50m,
                "PROD-06/2026",
                "Producao parcial do mes",
                createdAt),
            PayrollEntrySeed(
                Guid.Parse("11111111-1111-1111-1111-111111111103"),
                companyId,
                currentPeriodId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                healthPlanRubricId,
                new DateOnly(2026, 6, 30),
                180m,
                null,
                "SAUDE-06/2026",
                "Desconto plano de saude",
                createdAt));

        modelBuilder.Entity<FixedPayrollEntry>().HasData(
            FixedPayrollEntrySeed(
                Guid.Parse("22222222-2222-2222-2222-222222222201"),
                companyId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd01"),
                healthPlanRubricId,
                new DateOnly(2026, 6, 1),
                null,
                180m,
                null,
                "Plano de saude mensal",
                createdAt),
            FixedPayrollEntrySeed(
                Guid.Parse("22222222-2222-2222-2222-222222222202"),
                companyId,
                Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddd02"),
                transportRubricId,
                new DateOnly(2026, 6, 1),
                null,
                192m,
                null,
                "Vale transporte mensal",
                createdAt));

        modelBuilder.Entity<StatutoryTable>().HasData(
            StatutoryTableSeed(
                inssTableId,
                companyId,
                "inss",
                "INSS 2026",
                new DateOnly(2026, 1, 1),
                null,
                "Tabela informada para parametrizacao de 2026. Teto: R$ 8.475,55. Revisar a fonte oficial antes de uso em producao.",
                createdAt),
            StatutoryTableSeed(
                irrfTableId,
                companyId,
                "irrf",
                "IRRF 2026",
                new DateOnly(2026, 1, 1),
                null,
                "Tabela progressiva mensal publicada pela Receita Federal para fatos geradores a partir de janeiro de 2026.",
                createdAt),
            StatutoryTableSeed(
                irrfReductionTableId,
                companyId,
                "irrf_reducao",
                "IRRF reducao 2026",
                new DateOnly(2026, 1, 1),
                null,
                "Regra redutora da Lei 15.270/2025: ate R$ 5.000,00 reduz ate R$ 312,89; de R$ 5.000,01 a R$ 7.350,00 aplica reducao = R$ 978,62 - 0,133145 x rendimentos tributaveis mensais.",
                createdAt),
            StatutoryTableSeed(
                fgtsMonthlyTableId,
                companyId,
                "fgts",
                "FGTS mensal 2026",
                new DateOnly(2026, 1, 1),
                null,
                "Deposito mensal de FGTS a cargo do empregador: 8% da remuneracao paga ou devida ao trabalhador CLT.",
                createdAt),
            StatutoryTableSeed(
                fgtsBirthdayWithdrawalTableId,
                companyId,
                "fgts_saque_aniversario",
                "FGTS saque-aniversario 2026",
                new DateOnly(2026, 1, 1),
                null,
                "Tabela oficial de saque-aniversario para consulta. Nao compoe o calculo mensal da folha do empregador.",
                createdAt));

        modelBuilder.Entity<StatutoryTableRange>().HasData(
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444401"), inssTableId, 0m, 1621.00m, 7.5m, 0m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444402"), inssTableId, 1621.01m, 2902.84m, 9.0m, 24.32m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444403"), inssTableId, 2902.85m, 4354.27m, 12.0m, 111.40m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444404"), inssTableId, 4354.28m, 8475.55m, 14.0m, 198.49m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444405"), irrfTableId, 0m, 2428.80m, 0m, 0m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444406"), irrfTableId, 2428.81m, 2826.65m, 7.5m, 182.16m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444407"), irrfTableId, 2826.66m, 3751.05m, 15.0m, 394.16m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444408"), irrfTableId, 3751.06m, 4664.68m, 22.5m, 675.49m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444409"), irrfTableId, 4664.69m, null, 27.5m, 908.73m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444410"), irrfReductionTableId, 0m, 5000.00m, 0m, 312.89m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444411"), irrfReductionTableId, 5000.01m, 7350.00m, 13.3145m, 978.62m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444412"), fgtsMonthlyTableId, 0m, null, 8.0m, 0m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444413"), fgtsBirthdayWithdrawalTableId, 0.01m, 500.00m, 50.0m, 0m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444414"), fgtsBirthdayWithdrawalTableId, 500.01m, 1000.00m, 40.0m, 50.00m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444415"), fgtsBirthdayWithdrawalTableId, 1000.01m, 5000.00m, 30.0m, 150.00m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444416"), fgtsBirthdayWithdrawalTableId, 5000.01m, 10000.00m, 20.0m, 650.00m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444417"), fgtsBirthdayWithdrawalTableId, 10000.01m, 15000.00m, 15.0m, 1150.00m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444418"), fgtsBirthdayWithdrawalTableId, 15000.01m, 20000.00m, 10.0m, 1900.00m, createdAt),
            StatutoryTableRangeSeed(Guid.Parse("44444444-4444-4444-4444-444444444419"), fgtsBirthdayWithdrawalTableId, 20000.01m, null, 5.0m, 2900.00m, createdAt));
    }

    private static object RubricSeed(
        Guid id,
        Guid companyId,
        string code,
        string name,
        string type,
        string? esocialNature,
        bool allowsManualEntry,
        bool allowsMassEntry,
        bool allowsFixedEntry,
        DateTime createdAt)
    {
        return new
        {
            Id = id,
            CompanyId = companyId,
            Code = code,
            Name = name,
            Type = type,
            Description = (string?)null,
            ESocialNature = esocialNature,
            AllowsManualEntry = allowsManualEntry,
            AllowsMassEntry = allowsMassEntry,
            AllowsFixedEntry = allowsFixedEntry,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }

    private static object ValiditySeed(
        Guid id,
        Guid rubricId,
        DateTime createdAt,
        bool incidenceInss,
        bool incidenceFgts,
        bool incidenceIrrf,
        bool incidenceDsr,
        string calculationMethod,
        string calculationBase)
    {
        return new
        {
            Id = id,
            RubricId = rubricId,
            StartsOn = new DateOnly(2026, 1, 1),
            EndsOn = (DateOnly?)null,
            IncidenceInss = incidenceInss,
            IncidenceFgts = incidenceFgts,
            IncidenceIrrf = incidenceIrrf,
            IncidenceDsr = incidenceDsr,
            CalculationMethod = calculationMethod,
            CalculationBase = calculationBase,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }

    private static object PayrollEntrySeed(
        Guid id,
        Guid companyId,
        Guid payrollPeriodId,
        Guid employeeId,
        Guid rubricId,
        DateOnly entryDate,
        decimal amount,
        decimal? quantity,
        string reference,
        string notes,
        DateTime createdAt)
    {
        return new
        {
            Id = id,
            CompanyId = companyId,
            PayrollPeriodId = payrollPeriodId,
            EmployeeId = employeeId,
            RubricId = rubricId,
            EntryDate = entryDate,
            Amount = amount,
            Quantity = quantity,
            Reference = reference,
            Notes = notes,
            Origin = "manual",
            Status = "aprovado",
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }

    private static object FixedPayrollEntrySeed(
        Guid id,
        Guid companyId,
        Guid employeeId,
        Guid rubricId,
        DateOnly startsOn,
        DateOnly? endsOn,
        decimal amount,
        decimal? quantity,
        string notes,
        DateTime createdAt)
    {
        return new
        {
            Id = id,
            CompanyId = companyId,
            EmployeeId = employeeId,
            RubricId = rubricId,
            StartsOn = startsOn,
            EndsOn = endsOn,
            Amount = amount,
            Quantity = quantity,
            Notes = notes,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }

    private static object StatutoryTableSeed(
        Guid id,
        Guid companyId,
        string type,
        string name,
        DateOnly startsOn,
        DateOnly? endsOn,
        string notes,
        DateTime createdAt)
    {
        return new
        {
            Id = id,
            CompanyId = companyId,
            Type = type,
            Name = name,
            StartsOn = startsOn,
            EndsOn = endsOn,
            Notes = notes,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }

    private static object StatutoryTableRangeSeed(
        Guid id,
        Guid statutoryTableId,
        decimal lowerLimit,
        decimal? upperLimit,
        decimal ratePercent,
        decimal deductionAmount,
        DateTime createdAt)
    {
        return new
        {
            Id = id,
            StatutoryTableId = statutoryTableId,
            LowerLimit = lowerLimit,
            UpperLimit = upperLimit,
            RatePercent = ratePercent,
            DeductionAmount = deductionAmount,
            CreatedAt = createdAt,
            UpdatedAt = (DateTime?)null
        };
    }
}
