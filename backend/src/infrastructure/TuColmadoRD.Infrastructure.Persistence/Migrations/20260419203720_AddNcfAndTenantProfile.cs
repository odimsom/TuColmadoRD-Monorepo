using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds NCF support to the Sales module and creates TenantProfiles for DGI compliance.
    /// - Sales.Sales: NcfNumber (B01/B02) column
    /// - System.TenantProfiles: business name, RNC, address for fiscal receipts (Norma 06-18)
    /// </summary>
    [Migration("20260419203720_AddNcfAndTenantProfile")]
    public partial class AddNcfAndTenantProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. NcfNumber column on Sales ─────────────────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "NcfNumber",
                schema: "Sales",
                table: "Sales",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // ── 2. TenantProfiles table ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TenantProfiles",
                schema: "System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rnc = table.Column<string>(name: "Rnc", type: "character varying(15)", maxLength: 15, nullable: true),
                    BusinessAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantProfiles_TenantId",
                schema: "System",
                table: "TenantProfiles",
                column: "TenantId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantProfiles",
                schema: "System");

            migrationBuilder.DropColumn(
                name: "NcfNumber",
                schema: "Sales",
                table: "Sales");
        }
    }
}
