using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanMayTinh.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrl2AndImageUrl3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl2",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl3",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrl3",
                table: "Products");
        }
    }
}
