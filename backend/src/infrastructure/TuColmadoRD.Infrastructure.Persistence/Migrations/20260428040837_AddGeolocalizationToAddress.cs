using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeolocalizationToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Destination_Latitude",
                schema: "Logistics",
                table: "DeliveryOrders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Destination_Longitude",
                schema: "Logistics",
                table: "DeliveryOrders",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Latitude",
                schema: "Customers",
                table: "Customers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Address_Longitude",
                schema: "Customers",
                table: "Customers",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Destination_Latitude",
                schema: "Logistics",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "Destination_Longitude",
                schema: "Logistics",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "Address_Latitude",
                schema: "Customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Address_Longitude",
                schema: "Customers",
                table: "Customers");
        }
    }
}
