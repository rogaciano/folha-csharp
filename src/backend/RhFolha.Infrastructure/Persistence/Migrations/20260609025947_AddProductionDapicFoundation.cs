using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionDapicFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dapic_employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    fantasy_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    display_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    raw_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_ignored = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dapic_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_dapic_employees_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_entity_maps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    external_entity_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    local_entity_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    local_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_display_name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    linked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_entity_maps", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_entity_maps_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_entity_maps_system_users_linked_by_user_id",
                        column: x => x.linked_by_user_id,
                        principalTable: "system_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "external_integrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    base_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    external_company_identifier = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    integration_token_secret = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: false),
                    access_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    access_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_integrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_integrations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_cells",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_cells", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_cells_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_cells_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "production_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_operations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    external_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_orders_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    factory_description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    external_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_products_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_rate_tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_rate_tables", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_rate_tables_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "external_sync_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_integration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    resource = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    requested_from = table.Column<DateOnly>(type: "date", nullable: true),
                    requested_to = table.Column<DateOnly>(type: "date", nullable: true),
                    page_count = table.Column<int>(type: "integer", nullable: false),
                    records_read = table.Column<int>(type: "integer", nullable: false),
                    records_created = table.Column<int>(type: "integer", nullable: false),
                    records_updated = table.Column<int>(type: "integer", nullable: false),
                    records_ignored = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_sync_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_sync_logs_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_sync_logs_external_integrations_external_integrati~",
                        column: x => x.external_integration_id,
                        principalTable: "external_integrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_sync_logs_system_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "system_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "employee_production_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    production_date = table.Column<DateOnly>(type: "date", nullable: false),
                    production_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_cell_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    default_unit_value = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    default_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_employees = table.Column<int>(type: "integer", nullable: false),
                    total_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_production_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_payroll_periods_payroll_period_~",
                        column: x => x.payroll_period_id,
                        principalTable: "payroll_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_production_cells_production_cel~",
                        column: x => x.production_cell_id,
                        principalTable: "production_cells",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_production_operations_productio~",
                        column: x => x.production_operation_id,
                        principalTable: "production_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_production_orders_production_or~",
                        column: x => x.production_order_id,
                        principalTable: "production_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_production_products_production_~",
                        column: x => x.production_product_id,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batches_system_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "system_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_technical_sheets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_product_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    version_label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_hash = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_technical_sheets", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_technical_sheets_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_technical_sheets_production_products_production_pro~",
                        column: x => x.production_product_id,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_order_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    reference_snapshot = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description_snapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    color = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    size = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    grade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    planned_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    produced_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_order_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_order_products_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_order_products_production_orders_production_orde~",
                        column: x => x.production_order_id,
                        principalTable: "production_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_order_products_production_products_production_pr~",
                        column: x => x.production_product_id,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_rate_table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_position_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_cell_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unit_value = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    calculation_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    minimum_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    maximum_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_rates", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_rates_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_job_positions_job_position_id",
                        column: x => x.job_position_id,
                        principalTable: "job_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_production_cells_production_cell_id",
                        column: x => x.production_cell_id,
                        principalTable: "production_cells",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_production_operations_production_operation~",
                        column: x => x.production_operation_id,
                        principalTable: "production_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_production_products_production_product_id",
                        column: x => x.production_product_id,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_rates_production_rate_tables_production_rate_tab~",
                        column: x => x.production_rate_table_id,
                        principalTable: "production_rate_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_technical_sheet_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_technical_sheet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_name_snapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    standard_quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    standard_time = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_technical_sheet_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_technical_sheet_operations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_technical_sheet_operations_product_technical_sheets~",
                        column: x => x.product_technical_sheet_id,
                        principalTable: "product_technical_sheets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_technical_sheet_operations_production_operations_pr~",
                        column: x => x.production_operation_id,
                        principalTable: "production_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee_production_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payroll_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_date = table.Column<DateOnly>(type: "date", nullable: false),
                    production_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_order_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    production_product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    production_cell_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    unit_value = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    rate_source = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    production_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origin = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    integrated_payroll_calculation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    integrated_payroll_calculation_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_registration_snapshot = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    employee_name_snapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    order_number_snapshot = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    product_reference_snapshot = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    product_description_snapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    operation_name_snapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    cell_name_snapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_production_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_payroll_calculation_items_integ~",
                        column: x => x.integrated_payroll_calculation_item_id,
                        principalTable: "payroll_calculation_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_payroll_calculations_integrated~",
                        column: x => x.integrated_payroll_calculation_id,
                        principalTable: "payroll_calculations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_payroll_periods_payroll_period_~",
                        column: x => x.payroll_period_id,
                        principalTable: "payroll_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_cells_production_cel~",
                        column: x => x.production_cell_id,
                        principalTable: "production_cells",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_operations_productio~",
                        column: x => x.production_operation_id,
                        principalTable: "production_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_order_products_produ~",
                        column: x => x.production_order_product_id,
                        principalTable: "production_order_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_orders_production_or~",
                        column: x => x.production_order_id,
                        principalTable: "production_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_products_production_~",
                        column: x => x.production_product_id,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_production_rates_production_rat~",
                        column: x => x.production_rate_id,
                        principalTable: "production_rates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_production_entries_system_users_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalTable: "system_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "employee_production_batch_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_production_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_production_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    unit_value = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_production_batch_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_production_batch_items_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_production_batch_items_employee_production_batches~",
                        column: x => x.employee_production_batch_id,
                        principalTable: "employee_production_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_production_batch_items_employee_production_entries~",
                        column: x => x.employee_production_entry_id,
                        principalTable: "employee_production_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employee_production_batch_items_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dapic_employees_company_id_external_id",
                table: "dapic_employees",
                columns: new[] { "company_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dapic_employees_company_id_name",
                table: "dapic_employees",
                columns: new[] { "company_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batch_items_company_id",
                table: "employee_production_batch_items",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batch_items_employee_id",
                table: "employee_production_batch_items",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batch_items_employee_production_batch_id",
                table: "employee_production_batch_items",
                column: "employee_production_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batch_items_employee_production_entry_id",
                table: "employee_production_batch_items",
                column: "employee_production_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_company_id_payroll_period_id_st~",
                table: "employee_production_batches",
                columns: new[] { "company_id", "payroll_period_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_created_by_user_id",
                table: "employee_production_batches",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_payroll_period_id",
                table: "employee_production_batches",
                column: "payroll_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_production_cell_id",
                table: "employee_production_batches",
                column: "production_cell_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_production_operation_id",
                table: "employee_production_batches",
                column: "production_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_production_order_id",
                table: "employee_production_batches",
                column: "production_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_batches_production_product_id",
                table: "employee_production_batches",
                column: "production_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_approved_by_user_id",
                table: "employee_production_entries",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_company_id",
                table: "employee_production_entries",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_employee_id",
                table: "employee_production_entries",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_integrated_payroll_calculation_~",
                table: "employee_production_entries",
                column: "integrated_payroll_calculation_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_integrated_payroll_calculation~1",
                table: "employee_production_entries",
                column: "integrated_payroll_calculation_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_payroll_period_id_employee_id",
                table: "employee_production_entries",
                columns: new[] { "payroll_period_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_cell_id",
                table: "employee_production_entries",
                column: "production_cell_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_date",
                table: "employee_production_entries",
                column: "production_date");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_operation_id",
                table: "employee_production_entries",
                column: "production_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_order_id",
                table: "employee_production_entries",
                column: "production_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_order_product_id",
                table: "employee_production_entries",
                column: "production_order_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_product_id",
                table: "employee_production_entries",
                column: "production_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_production_rate_id",
                table: "employee_production_entries",
                column: "production_rate_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_production_entries_status",
                table: "employee_production_entries",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_external_entity_maps_company_id_local_entity_type_local_ent~",
                table: "external_entity_maps",
                columns: new[] { "company_id", "local_entity_type", "local_entity_id", "provider" });

            migrationBuilder.CreateIndex(
                name: "IX_external_entity_maps_company_id_provider_external_entity_ty~",
                table: "external_entity_maps",
                columns: new[] { "company_id", "provider", "external_entity_type", "external_id" });

            migrationBuilder.CreateIndex(
                name: "IX_external_entity_maps_linked_by_user_id",
                table: "external_entity_maps",
                column: "linked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_integrations_company_id_provider",
                table: "external_integrations",
                columns: new[] { "company_id", "provider" },
                unique: true,
                filter: "status = 'Active' AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_external_integrations_company_id_provider_status",
                table: "external_integrations",
                columns: new[] { "company_id", "provider", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_external_sync_logs_company_id_resource_started_at",
                table: "external_sync_logs",
                columns: new[] { "company_id", "resource", "started_at" });

            migrationBuilder.CreateIndex(
                name: "IX_external_sync_logs_created_by_user_id",
                table: "external_sync_logs",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_sync_logs_external_integration_id_status",
                table: "external_sync_logs",
                columns: new[] { "external_integration_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheet_operations_company_id",
                table: "product_technical_sheet_operations",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheet_operations_product_technical_sheet_~",
                table: "product_technical_sheet_operations",
                columns: new[] { "product_technical_sheet_id", "sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheet_operations_production_operation_id",
                table: "product_technical_sheet_operations",
                column: "production_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheets_company_id_external_product_id",
                table: "product_technical_sheets",
                columns: new[] { "company_id", "external_product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheets_company_id_production_product_id",
                table: "product_technical_sheets",
                columns: new[] { "company_id", "production_product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_product_technical_sheets_production_product_id",
                table: "product_technical_sheets",
                column: "production_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_cells_company_id_department_id",
                table: "production_cells",
                columns: new[] { "company_id", "department_id" });

            migrationBuilder.CreateIndex(
                name: "IX_production_cells_company_id_external_id",
                table: "production_cells",
                columns: new[] { "company_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_cells_department_id",
                table: "production_cells",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_operations_company_id_external_id",
                table: "production_operations",
                columns: new[] { "company_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_operations_company_id_name",
                table: "production_operations",
                columns: new[] { "company_id", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_production_order_products_company_id_reference_snapshot",
                table: "production_order_products",
                columns: new[] { "company_id", "reference_snapshot" });

            migrationBuilder.CreateIndex(
                name: "IX_production_order_products_production_order_id",
                table: "production_order_products",
                column: "production_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_order_products_production_product_id",
                table: "production_order_products",
                column: "production_product_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_company_id_external_id",
                table: "production_orders",
                columns: new[] { "company_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_company_id_issue_date",
                table: "production_orders",
                columns: new[] { "company_id", "issue_date" });

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_company_id_number",
                table: "production_orders",
                columns: new[] { "company_id", "number" });

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_company_id_status",
                table: "production_orders",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_production_products_company_id_external_id",
                table: "production_products",
                columns: new[] { "company_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_products_company_id_reference",
                table: "production_products",
                columns: new[] { "company_id", "reference" });

            migrationBuilder.CreateIndex(
                name: "IX_production_products_company_id_status",
                table: "production_products",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_production_rate_tables_company_id_effective_from_status",
                table: "production_rate_tables",
                columns: new[] { "company_id", "effective_from", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_company_id",
                table: "production_rates",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_department_id_job_position_id_production_c~",
                table: "production_rates",
                columns: new[] { "department_id", "job_position_id", "production_cell_id" });

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_job_position_id",
                table: "production_rates",
                column: "job_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_production_cell_id",
                table: "production_rates",
                column: "production_cell_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_production_operation_id",
                table: "production_rates",
                column: "production_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_production_product_id_production_operation~",
                table: "production_rates",
                columns: new[] { "production_product_id", "production_operation_id" });

            migrationBuilder.CreateIndex(
                name: "IX_production_rates_production_rate_table_id",
                table: "production_rates",
                column: "production_rate_table_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dapic_employees");

            migrationBuilder.DropTable(
                name: "employee_production_batch_items");

            migrationBuilder.DropTable(
                name: "external_entity_maps");

            migrationBuilder.DropTable(
                name: "external_sync_logs");

            migrationBuilder.DropTable(
                name: "product_technical_sheet_operations");

            migrationBuilder.DropTable(
                name: "employee_production_batches");

            migrationBuilder.DropTable(
                name: "employee_production_entries");

            migrationBuilder.DropTable(
                name: "external_integrations");

            migrationBuilder.DropTable(
                name: "product_technical_sheets");

            migrationBuilder.DropTable(
                name: "production_order_products");

            migrationBuilder.DropTable(
                name: "production_rates");

            migrationBuilder.DropTable(
                name: "production_orders");

            migrationBuilder.DropTable(
                name: "production_cells");

            migrationBuilder.DropTable(
                name: "production_operations");

            migrationBuilder.DropTable(
                name: "production_products");

            migrationBuilder.DropTable(
                name: "production_rate_tables");
        }
    }
}
