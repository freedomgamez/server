using System;
using DigitalWorldOnline.Infraestructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260512213000_AddNormalizedItemStorageFoundation")]
    public partial class AddNormalizedItemStorageFoundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shared_ItemInstanceNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
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
                name: "Shared_ItemListNormalized",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
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
                name: "Shared_ItemInstanceAccessoryStatusNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemInstanceAccessoryStatusNormalized", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemInstanceAccessoryStatusNormalized_Shared_ItemInstanceNormalized_ItemInstanceId",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemInstanceNormalized",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemInstanceSocketStatusNormalized",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    AttributeId = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemInstanceSocketStatusNormalized", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemInstanceSocketStatusNormalized_Shared_ItemInstanceNormalized_ItemInstanceId",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemInstanceNormalized",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemSlotNormalized", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemSlotNormalized_Shared_ItemInstanceNormalized_ItemInstanceId",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemInstanceNormalized",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shared_ItemSlotNormalized_Shared_ItemListNormalized_ItemListId",
                        column: x => x.ItemListId,
                        principalTable: "Shared_ItemListNormalized",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemInstanceNormalized_ItemId",
                table: "Shared_ItemInstanceNormalized",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemInstanceAccessoryStatusNormalized_ItemInstanceId",
                table: "Shared_ItemInstanceAccessoryStatusNormalized",
                column: "ItemInstanceId");

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemInstanceAccessoryStatusNormalized_Instance_Slot",
                table: "Shared_ItemInstanceAccessoryStatusNormalized",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemInstanceSocketStatusNormalized_ItemInstanceId",
                table: "Shared_ItemInstanceSocketStatusNormalized",
                column: "ItemInstanceId");

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemInstanceSocketStatusNormalized_Instance_Slot",
                table: "Shared_ItemInstanceSocketStatusNormalized",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Shared_ItemInstanceAccessoryStatusNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemInstanceSocketStatusNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemSlotNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemInstanceNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemListNormalized");
        }
    }
}
