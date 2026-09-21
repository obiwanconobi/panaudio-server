using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanAudioServer.Migrations
{
    /// <inheritdoc />
    public partial class SongUniquePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // One-time dedup: the scanner previously re-inserted the same file on
            // every pass. Remove duplicate rows, keeping one row per Path.
            migrationBuilder.Sql(
                "DELETE FROM \"Songs\" WHERE \"Id\" NOT IN (SELECT MIN(\"Id\") FROM \"Songs\" GROUP BY \"Path\");");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Path",
                table: "Songs",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Songs_Path",
                table: "Songs");
        }
    }
}
