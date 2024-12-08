using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanAudioServer.Migrations
{
    /// <inheritdoc />
    public partial class playbackSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Seconds",
                table: "PlaybackHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seconds",
                table: "PlaybackHistory");
        }
    }
}
