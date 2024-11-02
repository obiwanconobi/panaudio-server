using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanAudioServer.Migrations
{
    /// <inheritdoc />
    public partial class albumfavourite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Favourite",
                table: "Artists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Favourite",
                table: "Album",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Favourite",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Favourite",
                table: "Album");
        }
    }
}
