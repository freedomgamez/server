using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountIdToNormalizedItemList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shared_ItemInstanceAccessoryStatusNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemInstanceAccessoryStatusNormalized", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemInstanceNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Power = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RerollLeft = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FamilyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    FirstExpired = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    TamerShopSellPrice = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemInstanceNormalized", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemInstanceSocketStatusNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    AttributeId = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemInstanceSocketStatusNormalized", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemListNormalized",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    CharacterId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Bits = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemListNormalized", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemSlotNormalized",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemListId = table.Column<long>(type: "bigint", nullable: false),
                    Slot = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemSlotNormalized", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Config_Hash",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 13, 1, 35, 39, 358, DateTimeKind.Local).AddTicks(3947));

            migrationBuilder.UpdateData(
                table: "Routine_Routine",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 13, 1, 35, 39, 362, DateTimeKind.Local).AddTicks(2993), new DateTime(2026, 5, 14, 0, 0, 0, 0, DateTimeKind.Local) });

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemInstanceAccessoryStatusNormalized_Instance_Slot",
                table: "Shared_ItemInstanceAccessoryStatusNormalized",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemInstanceNormalized_ItemId",
                table: "Shared_ItemInstanceNormalized",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemInstanceSocketStatusNormalized_Instance_Slot",
                table: "Shared_ItemInstanceSocketStatusNormalized",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Account_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "AccountId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Character_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "CharacterId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemSlotNormalized_ItemInstanceId",
                table: "Shared_ItemSlotNormalized",
                column: "ItemInstanceId");

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemSlotNormalized_ItemList_Slot",
                table: "Shared_ItemSlotNormalized",
                columns: new[] { "ItemListId", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shared_ItemInstanceAccessoryStatusNormalized");

            migrationBuilder.DropTable(
                name: "Shared_ItemInstanceNormalized");

            migrationBuilder.DropTable(
                name: "Shared_ItemInstanceSocketStatusNormalized");

            migrationBuilder.DropTable(
                name: "Shared_ItemListNormalized");

            migrationBuilder.DropTable(
                name: "Shared_ItemSlotNormalized");

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
                columns: new[] { "CreatedAt", "NextRunTime" },
                values: new object[] { new DateTime(2026, 5, 11, 14, 15, 7, 201, DateTimeKind.Local).AddTicks(9048), new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Local) });
        }
    }
}
