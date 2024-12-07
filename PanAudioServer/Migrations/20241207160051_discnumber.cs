using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanAudioServer.Migrations
{
    /// <inheritdoc />
    public partial class discnumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscNumber",
                table: "Songs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscNumber",
                table: "Songs");
        }
    }
}
