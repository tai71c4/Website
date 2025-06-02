using Microsoft.EntityFrameworkCore.Migrations;

namespace WebsiteBanPhuKien.Migrations
{
    public partial class AddColorAndDeviceToCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MauSac",
                table: "GioHang",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DongMay",
                table: "GioHang",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MauSac",
                table: "GioHang");

            migrationBuilder.DropColumn(
                name: "DongMay",
                table: "GioHang");
        }
    }
}