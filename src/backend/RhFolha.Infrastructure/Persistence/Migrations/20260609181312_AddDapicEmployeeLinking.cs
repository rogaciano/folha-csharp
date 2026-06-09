using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhFolha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDapicEmployeeLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "employee_id",
                table: "dapic_employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ignored_at",
                table: "dapic_employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ignored_reason",
                table: "dapic_employees",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "link_status",
                table: "dapic_employees",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "pendente");

            migrationBuilder.AddColumn<DateTime>(
                name: "linked_at",
                table: "dapic_employees",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_dapic_employees_company_id_employee_id",
                table: "dapic_employees",
                columns: new[] { "company_id", "employee_id" });

            migrationBuilder.CreateIndex(
                name: "IX_dapic_employees_company_id_link_status",
                table: "dapic_employees",
                columns: new[] { "company_id", "link_status" });

            migrationBuilder.CreateIndex(
                name: "IX_dapic_employees_employee_id",
                table: "dapic_employees",
                column: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_dapic_employees_employees_employee_id",
                table: "dapic_employees",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dapic_employees_employees_employee_id",
                table: "dapic_employees");

            migrationBuilder.DropIndex(
                name: "IX_dapic_employees_company_id_employee_id",
                table: "dapic_employees");

            migrationBuilder.DropIndex(
                name: "IX_dapic_employees_company_id_link_status",
                table: "dapic_employees");

            migrationBuilder.DropIndex(
                name: "IX_dapic_employees_employee_id",
                table: "dapic_employees");

            migrationBuilder.DropColumn(
                name: "employee_id",
                table: "dapic_employees");

            migrationBuilder.DropColumn(
                name: "ignored_at",
                table: "dapic_employees");

            migrationBuilder.DropColumn(
                name: "ignored_reason",
                table: "dapic_employees");

            migrationBuilder.DropColumn(
                name: "link_status",
                table: "dapic_employees");

            migrationBuilder.DropColumn(
                name: "linked_at",
                table: "dapic_employees");
        }
    }
}
