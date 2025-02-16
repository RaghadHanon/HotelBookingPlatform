using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBookingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelAmenity_Amenities_AmenityId",
                table: "HotelAmenity");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomAmenity_Amenities_AmenityId",
                table: "RoomAmenity");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelAmenity_Amenities_AmenityId",
                table: "HotelAmenity",
                column: "AmenityId",
                principalTable: "Amenities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAmenity_Amenities_AmenityId",
                table: "RoomAmenity",
                column: "AmenityId",
                principalTable: "Amenities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelAmenity_Amenities_AmenityId",
                table: "HotelAmenity");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomAmenity_Amenities_AmenityId",
                table: "RoomAmenity");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelAmenity_Amenities_AmenityId",
                table: "HotelAmenity",
                column: "AmenityId",
                principalTable: "Amenities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAmenity_Amenities_AmenityId",
                table: "RoomAmenity",
                column: "AmenityId",
                principalTable: "Amenities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
