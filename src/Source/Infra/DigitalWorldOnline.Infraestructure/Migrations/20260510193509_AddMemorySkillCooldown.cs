using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMemorySkillCooldown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CooldownEndsAt",
                table: "Digimon_EvolutionMemorySkill",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "'1970-01-01 00:00:00.000000'");

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
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 14, 35, 9, 170, DateTimeKind.Local).AddTicks(791));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CooldownEndsAt",
                table: "Digimon_EvolutionMemorySkill");

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 13, 45, 38, 167, DateTimeKind.Local).AddTicks(3903));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 13, 45, 38, 170, DateTimeKind.Local).AddTicks(9707));
        }
    }
}
