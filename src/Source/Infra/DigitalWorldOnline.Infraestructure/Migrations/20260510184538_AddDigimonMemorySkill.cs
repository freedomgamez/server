using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigimonMemorySkill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Available",
                table: "Asset_TamerSkill",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "UseAreaCheck",
                table: "Asset_TamerSkill",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "UseState",
                table: "Asset_TamerSkill",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "AttType",
                table: "Asset_SkillInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "MemorySkill",
                table: "Asset_SkillInfo",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "SkillGroup",
                table: "Asset_SkillInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillRank",
                table: "Asset_SkillInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Digimon_EvolutionMemorySkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    MaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)10),
                    AcquiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    EvolutionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_EvolutionMemorySkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_EvolutionMemorySkill_Digimon_Evolution_EvolutionId",
                        column: x => x.EvolutionId,
                        principalTable: "Digimon_Evolution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 10, 13, 45, 38, 170, DateTimeKind.Local).AddTicks(9707), new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Local) });

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_EvolutionMemorySkill_EvolutionId_SkillId",
                table: "Digimon_EvolutionMemorySkill",
                columns: new[] { "EvolutionId", "SkillId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Digimon_EvolutionMemorySkill");

            migrationBuilder.DropColumn(
                name: "Available",
                table: "Asset_TamerSkill");

            migrationBuilder.DropColumn(
                name: "UseAreaCheck",
                table: "Asset_TamerSkill");

            migrationBuilder.DropColumn(
                name: "UseState",
                table: "Asset_TamerSkill");

            migrationBuilder.DropColumn(
                name: "AttType",
                table: "Asset_SkillInfo");

            migrationBuilder.DropColumn(
                name: "MemorySkill",
                table: "Asset_SkillInfo");

            migrationBuilder.DropColumn(
                name: "SkillGroup",
                table: "Asset_SkillInfo");

            migrationBuilder.DropColumn(
                name: "SkillRank",
                table: "Asset_SkillInfo");

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
    }
}
