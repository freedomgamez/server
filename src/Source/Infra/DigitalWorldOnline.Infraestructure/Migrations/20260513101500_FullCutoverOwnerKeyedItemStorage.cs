using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    public partial class FullCutoverOwnerKeyedItemStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageAccount",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Bits = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemStorageAccount", x => new { x.AccountId, x.Type });
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageAccount_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageCharacter",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Bits = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemStorageCharacter", x => new { x.CharacterId, x.Type });
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageCharacter_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageInstance",
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
                    table.PrimaryKey("PK_Shared_ItemStorageInstance", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageAccountSlot",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemStorageAccountSlot", x => new { x.AccountId, x.Type, x.Slot });
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageAccountSlot_Shared_ItemStorageAccount",
                        columns: x => new { x.AccountId, x.Type },
                        principalTable: "Shared_ItemStorageAccount",
                        principalColumns: new[] { "AccountId", "Type" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageAccountSlot_Shared_ItemStorageInstance",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemStorageInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageCharacterSlot",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ItemInstanceId = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemStorageCharacterSlot", x => new { x.CharacterId, x.Type, x.Slot });
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageCharacterSlot_Shared_ItemStorageCharacter",
                        columns: x => new { x.CharacterId, x.Type },
                        principalTable: "Shared_ItemStorageCharacter",
                        principalColumns: new[] { "CharacterId", "Type" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shared_ItemStorageCharacterSlot_Shared_ItemStorageInstance",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemStorageInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageInstanceAccessoryStatus",
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
                    table.PrimaryKey("PK_Shared_ItemStorageInstanceAccessoryStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShItemStorInstAcc_Inst",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemStorageInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemStorageInstanceSocketStatus",
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
                    table.PrimaryKey("PK_Shared_ItemStorageInstanceSocketStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShItemStorInstSock_Inst",
                        column: x => x.ItemInstanceId,
                        principalTable: "Shared_ItemStorageInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemStorageInstance_ItemId",
                table: "Shared_ItemStorageInstance",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemStorageAccountSlot_ItemInstanceId",
                table: "Shared_ItemStorageAccountSlot",
                column: "ItemInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemStorageCharacterSlot_ItemInstanceId",
                table: "Shared_ItemStorageCharacterSlot",
                column: "ItemInstanceId");

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemStorageInstAcc_Instance_Slot",
                table: "Shared_ItemStorageInstanceAccessoryStatus",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Shared_ItemStorageInstSock_Instance_Slot",
                table: "Shared_ItemStorageInstanceSocketStatus",
                columns: new[] { "ItemInstanceId", "Slot" },
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstance`
(`Id`,`ItemId`,`Amount`,`Power`,`RerollLeft`,`FamilyType`,`Duration`,`EndDate`,`FirstExpired`,`TamerShopSellPrice`,`CreatedAt`)
SELECT n.`Id`, n.`ItemId`, n.`Amount`, n.`Power`, n.`RerollLeft`, n.`FamilyType`, n.`Duration`, n.`EndDate`, n.`FirstExpired`, n.`TamerShopSellPrice`, n.`CreatedAt`
FROM `Shared_ItemInstanceNormalized` n
LEFT JOIN `Shared_ItemStorageInstance` s ON s.`Id` = n.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstance`
(`Id`,`ItemId`,`Amount`,`Power`,`RerollLeft`,`FamilyType`,`Duration`,`EndDate`,`FirstExpired`,`TamerShopSellPrice`,`CreatedAt`)
SELECT i.`Id`, i.`ItemId`, i.`Amount`, i.`Power`, i.`RerollLeft`, i.`FamilyType`, i.`Duration`, i.`EndDate`, i.`FirstExpired`, i.`TamerShopSellPrice`, CURRENT_TIMESTAMP(6)
FROM `Shared_Item` i
LEFT JOIN `Shared_ItemStorageInstance` s ON s.`Id` = i.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstanceAccessoryStatus`
(`Id`,`ItemInstanceId`,`Slot`,`Type`,`Value`)
SELECT a.`Id`, a.`ItemInstanceId`, a.`Slot`, a.`Type`, a.`Value`
FROM `Shared_ItemInstanceAccessoryStatusNormalized` a
LEFT JOIN `Shared_ItemStorageInstanceAccessoryStatus` s ON s.`Id` = a.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstanceAccessoryStatus`
(`Id`,`ItemInstanceId`,`Slot`,`Type`,`Value`)
SELECT a.`Id`, a.`ItemId`, a.`Slot`, a.`Type`, a.`Value`
FROM `Shared_ItemAccessoryStatus` a
LEFT JOIN `Shared_ItemStorageInstanceAccessoryStatus` s ON s.`Id` = a.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstanceSocketStatus`
(`Id`,`ItemInstanceId`,`Slot`,`Type`,`AttributeId`,`Value`)
SELECT a.`Id`, a.`ItemInstanceId`, a.`Slot`, a.`Type`, a.`AttributeId`, a.`Value`
FROM `Shared_ItemInstanceSocketStatusNormalized` a
LEFT JOIN `Shared_ItemStorageInstanceSocketStatus` s ON s.`Id` = a.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageInstanceSocketStatus`
(`Id`,`ItemInstanceId`,`Slot`,`Type`,`AttributeId`,`Value`)
SELECT a.`Id`, a.`ItemId`, a.`Slot`, a.`Type`, a.`AttributeId`, a.`Value`
FROM `Shared_ItemSocketStatus` a
LEFT JOIN `Shared_ItemStorageInstanceSocketStatus` s ON s.`Id` = a.`Id`
WHERE s.`Id` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageCharacter`
(`CharacterId`,`Type`,`Size`,`Bits`,`CreatedAt`,`UpdatedAt`)
SELECT n.`CharacterId`, n.`Type`, MAX(n.`Size`), MAX(n.`Bits`), MIN(n.`CreatedAt`), MAX(n.`UpdatedAt`)
FROM `Shared_ItemListNormalized` n
WHERE n.`CharacterId` IS NOT NULL
GROUP BY n.`CharacterId`, n.`Type`;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageAccount`
(`AccountId`,`Type`,`Size`,`Bits`,`CreatedAt`,`UpdatedAt`)
SELECT n.`AccountId`, n.`Type`, MAX(n.`Size`), MAX(n.`Bits`), MIN(n.`CreatedAt`), MAX(n.`UpdatedAt`)
FROM `Shared_ItemListNormalized` n
WHERE n.`AccountId` IS NOT NULL
GROUP BY n.`AccountId`, n.`Type`;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageCharacter`
(`CharacterId`,`Type`,`Size`,`Bits`,`CreatedAt`,`UpdatedAt`)
SELECT l.`CharacterId`, l.`Type`, CAST(l.`Size` AS UNSIGNED), l.`Bits`, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
FROM `Shared_ItemList` l
LEFT JOIN `Shared_ItemStorageCharacter` s
  ON s.`CharacterId` = l.`CharacterId` AND s.`Type` = l.`Type`
WHERE l.`CharacterId` IS NOT NULL
  AND s.`CharacterId` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageAccount`
(`AccountId`,`Type`,`Size`,`Bits`,`CreatedAt`,`UpdatedAt`)
SELECT l.`AccountId`, l.`Type`, CAST(l.`Size` AS UNSIGNED), l.`Bits`, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
FROM `Shared_ItemList` l
LEFT JOIN `Shared_ItemStorageAccount` s
  ON s.`AccountId` = l.`AccountId` AND s.`Type` = l.`Type`
WHERE l.`AccountId` IS NOT NULL
  AND s.`AccountId` IS NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageCharacterSlot`
(`CharacterId`,`Type`,`Slot`,`ItemInstanceId`,`UpdatedAt`)
SELECT l.`CharacterId`, l.`Type`, s.`Slot`, s.`ItemInstanceId`, s.`UpdatedAt`
FROM `Shared_ItemSlotNormalized` s
INNER JOIN `Shared_ItemListNormalized` l ON l.`Id` = s.`ItemListId`
WHERE l.`CharacterId` IS NOT NULL;");

            migrationBuilder.Sql(@"
INSERT INTO `Shared_ItemStorageAccountSlot`
(`AccountId`,`Type`,`Slot`,`ItemInstanceId`,`UpdatedAt`)
SELECT l.`AccountId`, l.`Type`, s.`Slot`, s.`ItemInstanceId`, s.`UpdatedAt`
FROM `Shared_ItemSlotNormalized` s
INNER JOIN `Shared_ItemListNormalized` l ON l.`Id` = s.`ItemListId`
WHERE l.`AccountId` IS NOT NULL;");

            migrationBuilder.Sql(@"
INSERT IGNORE INTO `Shared_ItemStorageCharacterSlot`
(`CharacterId`,`Type`,`Slot`,`ItemInstanceId`,`UpdatedAt`)
SELECT l.`CharacterId`, l.`Type`, CAST(i.`Slot` AS UNSIGNED), i.`Id`, CURRENT_TIMESTAMP(6)
FROM `Shared_Item` i
INNER JOIN `Shared_ItemList` l ON l.`Id` = i.`ItemListId`
WHERE l.`CharacterId` IS NOT NULL;");

            migrationBuilder.Sql(@"
INSERT IGNORE INTO `Shared_ItemStorageAccountSlot`
(`AccountId`,`Type`,`Slot`,`ItemInstanceId`,`UpdatedAt`)
SELECT l.`AccountId`, l.`Type`, CAST(i.`Slot` AS UNSIGNED), i.`Id`, CURRENT_TIMESTAMP(6)
FROM `Shared_Item` i
INNER JOIN `Shared_ItemList` l ON l.`Id` = i.`ItemListId`
WHERE l.`AccountId` IS NOT NULL;");

            migrationBuilder.DropTable(name: "Shared_ItemInstanceAccessoryStatusNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemInstanceSocketStatusNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemSlotNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemListNormalized");
            migrationBuilder.DropTable(name: "Shared_ItemAccessoryStatus");
            migrationBuilder.DropTable(name: "Shared_ItemSocketStatus");
            migrationBuilder.DropTable(name: "Shared_Item");
            migrationBuilder.DropTable(name: "Shared_ItemList");
            migrationBuilder.DropTable(name: "Shared_ItemInstanceNormalized");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Shared_ItemStorageInstanceAccessoryStatus");
            migrationBuilder.DropTable(name: "Shared_ItemStorageInstanceSocketStatus");
            migrationBuilder.DropTable(name: "Shared_ItemStorageAccountSlot");
            migrationBuilder.DropTable(name: "Shared_ItemStorageCharacterSlot");
            migrationBuilder.DropTable(name: "Shared_ItemStorageAccount");
            migrationBuilder.DropTable(name: "Shared_ItemStorageCharacter");
            migrationBuilder.DropTable(name: "Shared_ItemStorageInstance");
        }
    }
}
