using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssetMonsterSkillTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asset_MonsterSkill");

            migrationBuilder.DropTable(
                name: "Asset_MonsterSkillInfo");

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 15, 7, 198, DateTimeKind.Local).AddTicks(559));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 14, 15, 7, 201, DateTimeKind.Local).AddTicks(9048));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asset_MonsterSkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MonsterSkill", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MonsterSkillInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActiveType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AnimationDelay = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    CastingTime = table.Column<int>(type: "int", nullable: false),
                    Cooldown = table.Column<int>(type: "int", nullable: false),
                    MaxValue = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: false),
                    NoticeTime = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    RangeId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    SkillType = table.Column<int>(type: "int", nullable: false),
                    TargetCount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TargetMax = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TargetMin = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UseTerms = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MonsterSkillInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                column: "CreatedAt",
                value: new DateTime(2026, 5, 11, 11, 17, 53, 687, DateTimeKind.Local).AddTicks(5713));
        }
    }
}
