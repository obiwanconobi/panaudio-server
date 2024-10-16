﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanAudioServer.Migrations
{
    /// <inheritdoc />
    public partial class pathSong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Songs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "Songs");
        }
    }
}
