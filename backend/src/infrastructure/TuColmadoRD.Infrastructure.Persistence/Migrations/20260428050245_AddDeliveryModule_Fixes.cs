using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryModule_Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrders_DeliveryPersons_DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrders_DeliveryPersons_DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders",
                column: "DeliveryPersonId",
                principalSchema: "Logistics",
                principalTable: "DeliveryPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrders_DeliveryPersons_DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrders_DeliveryPersons_DeliveryPersonId",
                schema: "Logistics",
                table: "DeliveryOrders",
                column: "DeliveryPersonId",
                principalSchema: "Logistics",
                principalTable: "DeliveryPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
