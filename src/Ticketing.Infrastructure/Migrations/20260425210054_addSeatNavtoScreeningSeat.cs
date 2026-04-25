using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSeatNavtoScreeningSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSeats_SeatId",
                table: "ScreeningSeats",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScreeningSeats_Seats_SeatId",
                table: "ScreeningSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScreeningSeats_Seats_SeatId",
                table: "ScreeningSeats");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningSeats_SeatId",
                table: "ScreeningSeats");
        }
    }
}
