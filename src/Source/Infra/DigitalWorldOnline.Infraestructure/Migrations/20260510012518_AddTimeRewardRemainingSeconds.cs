using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeRewardRemainingSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asset_CharacterBaseStatus");

            migrationBuilder.AddColumn<int>(
                name: "RemainingSeconds",
                table: "Event_TimeReward",
                type: "int",
                nullable: false,
                defaultValue: 1800);

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 9, 20, 25, 18, 491, DateTimeKind.Local).AddTicks(5271));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 9, 20, 25, 18, 494, DateTimeKind.Local).AddTicks(9482), new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Local) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingSeconds",
                table: "Event_TimeReward");

            migrationBuilder.CreateTable(
                name: "Asset_CharacterBaseStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_CharacterBaseStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 7, 38, 36, 189, DateTimeKind.Local).AddTicks(299));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 8, 7, 38, 36, 192, DateTimeKind.Local).AddTicks(4304), new DateTime(2026, 5, 9, 0, 0, 0, 0, DateTimeKind.Local) });
        }
    }
}
