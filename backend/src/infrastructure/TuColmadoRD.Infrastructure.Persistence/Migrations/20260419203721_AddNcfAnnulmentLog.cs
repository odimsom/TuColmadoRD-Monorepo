using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNcfAnnulmentLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackId",
                schema: "Fiscal",
                table: "FiscalReceipts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NcfAnnulmentLogs",
                schema: "Fiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    NCF = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    SaleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AnnulledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NcfAnnulmentLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NcfAnnulmentLogs",
                schema: "Fiscal");

            migrationBuilder.DropColumn(
                name: "TrackId",
                schema: "Fiscal",
                table: "FiscalReceipts");
        }
    }
}
