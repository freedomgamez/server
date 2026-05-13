using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    public partial class AddNormalizedItemIntegrityConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE `Shared_ItemListNormalized` n
LEFT JOIN `Shared_ItemList` l ON l.`Id` = n.`Id`
SET
    n.`CharacterId` = COALESCE(n.`CharacterId`, l.`CharacterId`),
    n.`AccountId` = COALESCE(n.`AccountId`, l.`AccountId`)
WHERE n.`CharacterId` IS NULL OR n.`AccountId` IS NULL;");

            migrationBuilder.Sql(@"
UPDATE `Shared_ItemListNormalized`
SET `CharacterId` = NULL
WHERE `CharacterId` IS NOT NULL
  AND `AccountId` IS NOT NULL
  AND `Type` IN (30, 31, 32, 33);");

            migrationBuilder.Sql(@"
UPDATE `Shared_ItemListNormalized`
SET `AccountId` = NULL
WHERE `CharacterId` IS NOT NULL
  AND `AccountId` IS NOT NULL
  AND `Type` NOT IN (30, 31, 32, 33);");

            migrationBuilder.Sql(@"
DELETE FROM `Shared_ItemListNormalized`
WHERE `CharacterId` IS NULL AND `AccountId` IS NULL;");

            migrationBuilder.DropIndex(
                name: "IX_Shared_ItemListNormalized_Character_Type",
                table: "Shared_ItemListNormalized");

            migrationBuilder.DropIndex(
                name: "IX_Shared_ItemListNormalized_Account_Type",
                table: "Shared_ItemListNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Character_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "CharacterId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Account_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "AccountId", "Type" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Shared_ItemListNormalized_ExactlyOneOwner",
                table: "Shared_ItemListNormalized",
                sql: "((`CharacterId` IS NOT NULL AND `AccountId` IS NULL) OR (`CharacterId` IS NULL AND `AccountId` IS NOT NULL))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Shared_ItemListNormalized_ExactlyOneOwner",
                table: "Shared_ItemListNormalized");

            migrationBuilder.DropIndex(
                name: "IX_Shared_ItemListNormalized_Character_Type",
                table: "Shared_ItemListNormalized");

            migrationBuilder.DropIndex(
                name: "IX_Shared_ItemListNormalized_Account_Type",
                table: "Shared_ItemListNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Character_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "CharacterId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemListNormalized_Account_Type",
                table: "Shared_ItemListNormalized",
                columns: new[] { "AccountId", "Type" });
        }
    }
}
