using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiPresentationStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPrice",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SalePrice",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitType",
                schema: "Inventory",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "MonetaryFunds",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonetaryFunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackagedStock",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagedStock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPresentations",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PresentationType = table.Column<int>(type: "integer", nullable: false),
                    SellMode = table.Column<int>(type: "integer", nullable: false),
                    MeasureUnit = table.Column<int>(type: "integer", nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Brand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    NominalCapacity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPresentations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockContainers",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NominalCapacity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ActualCapacity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    CurrentRemaining = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActiveSource = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmptiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockContainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockEntries",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FundTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundTransactions",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JustificationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundTransactions_MonetaryFunds_FundId",
                        column: x => x.FundId,
                        principalSchema: "Inventory",
                        principalTable: "MonetaryFunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockEntryLines",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerCount = table.Column<int>(type: "integer", nullable: false),
                    UnitsPerContainer = table.Column<int>(type: "integer", nullable: false),
                    NominalSizePerUnit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CostPerUnit = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockEntryLines_StockEntries_StockEntryId",
                        column: x => x.StockEntryId,
                        principalSchema: "Inventory",
                        principalTable: "StockEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_FundId",
                schema: "Inventory",
                table: "FundTransactions",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagedStock_PresentationId",
                schema: "Inventory",
                table: "PackagedStock",
                column: "PresentationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPresentations_ProductId",
                schema: "Inventory",
                table: "ProductPresentations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockContainers_PresentationId",
                schema: "Inventory",
                table: "StockContainers",
                column: "PresentationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockContainers_PresentationId_IsActive",
                schema: "Inventory",
                table: "StockContainers",
                columns: new[] { "PresentationId", "IsActiveSource" });

            migrationBuilder.CreateIndex(
                name: "IX_StockEntryLines_StockEntryId",
                schema: "Inventory",
                table: "StockEntryLines",
                column: "StockEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundTransactions",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PackagedStock",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProductPresentations",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockContainers",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockEntryLines",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "MonetaryFunds",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "StockEntries",
                schema: "Inventory");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                schema: "Inventory",
                table: "Products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePrice",
                schema: "Inventory",
                table: "Products",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockQuantity",
                schema: "Inventory",
                table: "Products",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                schema: "Inventory",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
