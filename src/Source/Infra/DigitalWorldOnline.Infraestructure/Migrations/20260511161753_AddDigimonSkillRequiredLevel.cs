using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigimonSkillRequiredLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "RequiredLevel",
                table: "Asset_DigimonSkill",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 11, 17, 53, 683, DateTimeKind.Local).AddTicks(8722));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 11, 11, 17, 53, 687, DateTimeKind.Local).AddTicks(5713), new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Local) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "Asset_DigimonSkill");

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 14, 35, 9, 166, DateTimeKind.Local).AddTicks(3455));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 10, 14, 35, 9, 170, DateTimeKind.Local).AddTicks(791), new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Local) });
        }
    }
}
