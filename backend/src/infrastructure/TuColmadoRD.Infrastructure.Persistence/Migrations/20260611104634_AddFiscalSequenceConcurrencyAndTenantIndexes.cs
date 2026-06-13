using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFiscalSequenceConcurrencyAndTenantIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sales_TenantId",
                schema: "Sales",
                table: "Sales",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId",
                schema: "Inventory",
                table: "Products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalSequences_TenantId",
                schema: "Fiscal",
                table: "FiscalSequences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                schema: "Customers",
                table: "Customers",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_TenantId",
                schema: "Sales",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_FiscalSequences_TenantId",
                schema: "Fiscal",
                table: "FiscalSequences");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId",
                schema: "Customers",
                table: "Customers");
        }
    }
}
