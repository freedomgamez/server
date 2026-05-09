using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalWorldOnline.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Account_Account",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecondaryPassword = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DiscordId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccessLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    LastConnection = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MembershipExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Premium = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Silk = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastPlayedServer = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    LastPlayedCharacter = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ReceiveWelcome = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_Account", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Account_Server",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Experience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Maintenance = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    New = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Overload = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Port = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_Server", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Arena_Ranking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arena_Ranking", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_AccessoryRoll",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StatusAmount = table.Column<int>(type: "int", nullable: false),
                    RerollAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_AccessoryRoll", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Achievement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    BuffId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Achievement", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ArenaDailyItemRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WeekDay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ArenaDailyItemRewards", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Buff",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BuffId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DigimonSkillCode = table.Column<int>(type: "int", nullable: false),
                    SkillCode = table.Column<int>(type: "int", nullable: false),
                    MinLevel = table.Column<int>(type: "int", nullable: false),
                    ConditionLevel = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<short>(type: "smallint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LifeType = table.Column<int>(type: "int", nullable: false),
                    TimeType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Buff", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_CharacterBaseStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_CharacterBaseStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_CharacterLevelStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 80001),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    ExpValue = table.Column<long>(type: "bigint", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_CharacterLevelStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Clone",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemSection = table.Column<int>(type: "int", nullable: false),
                    MinLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bits = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Clone", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_CloneValue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MinLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: false),
                    MaxValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_CloneValue", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Container",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RewardAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Container", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_DigimonBaseInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    ScaleType = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    EvolutionType = table.Column<int>(type: "int", nullable: false),
                    Attribute = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Element = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family1 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family2 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family3 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_DigimonBaseInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_DigimonLevelStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    ExpValue = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ScaleType = table.Column<int>(type: "int", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_DigimonLevelStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_DigimonSkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_DigimonSkill", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Evolution",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EvolutionRank = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Evolution", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_EvolutionArmor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_EvolutionArmor", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ExtraEvolutionNpc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NpcId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ExtraEvolutionNpc", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Hatch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HatchType = table.Column<int>(type: "int", nullable: false),
                    LowClassDataSection = table.Column<int>(type: "int", nullable: false),
                    MidClassDataSection = table.Column<int>(type: "int", nullable: false),
                    LowClassDataAmount = table.Column<int>(type: "int", nullable: false),
                    MidClassDataAmount = table.Column<int>(type: "int", nullable: false),
                    LowClassBreakPoint = table.Column<int>(type: "int", nullable: false),
                    MidClassBreakPoint = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Hatch", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ItemCraft",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SequencialId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    NpcId = table.Column<int>(type: "int", nullable: false),
                    SuccessRate = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ItemCraft", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ItemInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Class = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TypeN = table.Column<int>(type: "int", nullable: false),
                    ApplyValueMin = table.Column<short>(type: "smallint", nullable: false),
                    ApplyValueMax = table.Column<short>(type: "smallint", nullable: false),
                    ApplyElement = table.Column<short>(type: "smallint", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: false),
                    SellType = table.Column<int>(type: "int", nullable: false),
                    BoundType = table.Column<int>(type: "int", nullable: false),
                    UseTimeType = table.Column<int>(type: "int", nullable: false),
                    SkillCode = table.Column<long>(type: "bigint", nullable: false),
                    TamerMinLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TamerMaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DigimonMinLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DigimonMaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SellPrice = table.Column<long>(type: "bigint", nullable: false),
                    ScanPrice = table.Column<int>(type: "int", nullable: false),
                    DigicorePrice = table.Column<int>(type: "int", nullable: false),
                    EventPriceId = table.Column<int>(type: "int", nullable: false),
                    EventPriceAmount = table.Column<int>(type: "int", nullable: false),
                    UsageTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Overlap = table.Column<short>(type: "smallint", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ItemInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Map",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionIndex = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Map", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MapRegionList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MapRegionList", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MonsterBaseInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    ViewRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HuntRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Class = table.Column<int>(type: "int", nullable: false),
                    ReactionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Attribute = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Element = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family1 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family2 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family3 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MonsterBaseInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MonsterSkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
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
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: false),
                    MaxValue = table.Column<int>(type: "int", nullable: false),
                    CastingTime = table.Column<int>(type: "int", nullable: false),
                    Cooldown = table.Column<int>(type: "int", nullable: false),
                    TargetCount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TargetMin = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TargetMax = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UseTerms = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RangeId = table.Column<int>(type: "int", nullable: false),
                    AnimationDelay = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    ActiveType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillType = table.Column<int>(type: "int", nullable: false),
                    NoticeTime = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MonsterSkillInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MonthlyEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrentDay = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MonthlyEvent", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Npc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NpcId = table.Column<int>(type: "int", nullable: false),
                    MapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Npc", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcColiseum",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NpcId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcColiseum", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Portal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    NpcId = table.Column<int>(type: "int", nullable: false),
                    DestinationMapId = table.Column<int>(type: "int", nullable: false),
                    DestinationX = table.Column<int>(type: "int", nullable: false),
                    DestinationY = table.Column<int>(type: "int", nullable: false),
                    PortalIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Portal", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Quest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    QuestType = table.Column<int>(type: "int", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Quest", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ScanDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MinAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    MaxAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ScanDetail", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_SealDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SealId = table.Column<int>(type: "int", nullable: false),
                    RequiredAmount = table.Column<short>(type: "smallint", nullable: false),
                    SequentialId = table.Column<short>(type: "smallint", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_SealDetail", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_SkillCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillCode = table.Column<long>(type: "bigint", nullable: false),
                    Comment = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_SkillCode", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_SkillInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FamilyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DSUsage = table.Column<int>(type: "int", nullable: false),
                    HPUsage = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CastingTime = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Cooldown = table.Column<int>(type: "int", nullable: false),
                    MaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RequiredPoints = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Target = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AreaOfEffect = table.Column<int>(type: "int", nullable: false),
                    AoEMinDamage = table.Column<int>(type: "int", nullable: false),
                    AoEMaxDamage = table.Column<int>(type: "int", nullable: false),
                    Range = table.Column<int>(type: "int", nullable: false),
                    UnlockLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MemoryChips = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FirstConditionCode = table.Column<int>(type: "int", nullable: false),
                    SecondConditionCode = table.Column<int>(type: "int", nullable: false),
                    ThirdConditionCode = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_SkillInfo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_StatusApply",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StageValue = table.Column<int>(type: "int", nullable: false),
                    ApplyValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_StatusApply", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_TamerSkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    SkillCode = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_TamerSkill", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_TitleStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    SCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    LASCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    FISCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    ICSCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    LISCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    STSCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    NESCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    DASCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    THSCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    WASCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    WISCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    WOSCD = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_TitleStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_Xai",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    XGauge = table.Column<int>(type: "int", nullable: false),
                    XCrystals = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_Xai", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Clone",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SuccessChance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    BreakChance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    MinAmount = table.Column<int>(type: "int", nullable: false),
                    MaxAmount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Clone", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Fruit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemSection = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Fruit", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Hash",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Hash", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Hatch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SuccessChance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    BreakChance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Hatch", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Map",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Map", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Summon",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Maps = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Summon", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccessLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_User", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_WelcomeMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_WelcomeMessage", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guild_Guild",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    CurrentExperience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notice = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExtraSlots = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Guild", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Routine_Routine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routine_Routine", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Security_LoginTry",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    Ip = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Result = table.Column<int>(type: "int", nullable: false, defaultValue: 4)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_LoginTry", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Account_AccountBlock",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Reason = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_AccountBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_AccountBlock_Account_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Account_SystemInformation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cpu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gpu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ip = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account_SystemInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Account_SystemInformation_Account_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Arena_Competitor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InsertDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    TamerId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    Position = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValueSql: "0"),
                    New = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "0"),
                    RankingId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arena_Competitor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Arena_Competitor_Arena_Ranking_RankingId",
                        column: x => x.RankingId,
                        principalTable: "Arena_Ranking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_AccessoryRollStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: false),
                    MaxValue = table.Column<int>(type: "int", nullable: false),
                    AccessoryRollAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_AccessoryRollStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_AccessoryRollStatus_Asset_AccessoryRoll_AccessoryRollA~",
                        column: x => x.AccessoryRollAssetId,
                        principalTable: "Asset_AccessoryRoll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ArenaDailyItemReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    RequiredCoins = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ArenaDailyItemReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ArenaDailyItemReward_Asset_ArenaDailyItemRewards_Rewar~",
                        column: x => x.RewardId,
                        principalTable: "Asset_ArenaDailyItemRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ContainerReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Rare = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContainerAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ContainerReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ContainerReward_Asset_Container_ContainerAssetId",
                        column: x => x.ContainerAssetId,
                        principalTable: "Asset_Container",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_EvolutionLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SlotLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UnlockLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UnlockQuestId = table.Column<short>(type: "smallint", nullable: false),
                    UnlockItemSection = table.Column<int>(type: "int", nullable: false),
                    UnlockItemSectionAmount = table.Column<int>(type: "int", nullable: false),
                    RequiredItem = table.Column<int>(type: "int", nullable: false),
                    RequiredAmount = table.Column<int>(type: "int", nullable: false),
                    EvolutionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_EvolutionLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_EvolutionLine_Asset_Evolution_EvolutionId",
                        column: x => x.EvolutionId,
                        principalTable: "Asset_Evolution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ExtraEvolutionInformation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IndexId = table.Column<int>(type: "int", nullable: false),
                    ExtraNpcId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ExtraEvolutionInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ExtraEvolutionInformation_Asset_ExtraEvolutionNpc_Extr~",
                        column: x => x.ExtraNpcId,
                        principalTable: "Asset_ExtraEvolutionNpc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ItemCraftMaterial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ItemCraftId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ItemCraftMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ItemCraftMaterial_Asset_ItemCraft_ItemCraftId",
                        column: x => x.ItemCraftId,
                        principalTable: "Asset_ItemCraft",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_MapRegion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Index = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 6500),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 6500),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MapRegionListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_MapRegion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_MapRegion_Asset_MapRegionList_MapRegionListId",
                        column: x => x.MapRegionListId,
                        principalTable: "Asset_MapRegionList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    NpcAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_NpcItem_Asset_Npc_NpcAssetId",
                        column: x => x.NpcAssetId,
                        principalTable: "Asset_Npc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcPortal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PortalType = table.Column<int>(type: "int", nullable: false),
                    PortalCount = table.Column<int>(type: "int", nullable: false),
                    NpcAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcPortal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_NpcPortal_Asset_Npc_NpcAssetId",
                        column: x => x.NpcAssetId,
                        principalTable: "Asset_Npc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcMobInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Round = table.Column<int>(type: "int", nullable: false),
                    SummonType = table.Column<int>(type: "int", nullable: false),
                    WinPoints = table.Column<int>(type: "int", nullable: false),
                    LosePoints = table.Column<int>(type: "int", nullable: false),
                    NpcAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcMobInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_NpcMobInfo_Asset_NpcColiseum_NpcAssetId",
                        column: x => x.NpcAssetId,
                        principalTable: "Asset_NpcColiseum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestCondition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConditionType = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    ConditionCount = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestCondition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestCondition_Asset_Quest_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Asset_Quest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestEvent",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestEvent_Asset_Quest_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Asset_Quest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestGoal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GoalType = table.Column<int>(type: "int", nullable: false),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    GoalAmount = table.Column<int>(type: "int", nullable: false),
                    CurTypeCount = table.Column<int>(type: "int", nullable: false),
                    SubValue = table.Column<int>(type: "int", nullable: false),
                    SubValueTwo = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestGoal_Asset_Quest_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Asset_Quest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reward = table.Column<int>(type: "int", nullable: false),
                    RewardType = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestReward_Asset_Quest_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Asset_Quest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestSupply",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestSupply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestSupply_Asset_Quest_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Asset_Quest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ScanRewardDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Rare = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ScanDetailAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ScanRewardDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ScanRewardDetail_Asset_ScanDetail_ScanDetailAssetId",
                        column: x => x.ScanDetailAssetId,
                        principalTable: "Asset_ScanDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_SkillCodeApply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Attribute = table.Column<int>(type: "int", nullable: false, defaultValue: 20),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Chance = table.Column<int>(type: "int", nullable: false),
                    AdditionalValue = table.Column<int>(type: "int", nullable: false),
                    IncreaseValue = table.Column<int>(type: "int", nullable: false),
                    SkillCodeAssetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_SkillCodeApply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_SkillCodeApply_Asset_SkillCode_SkillCodeAssetId",
                        column: x => x.SkillCodeAssetId,
                        principalTable: "Asset_SkillCode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_FruitSize",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HatchGrade = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    FruitConfigId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_FruitSize", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_FruitSize_Config_Fruit_FruitConfigId",
                        column: x => x.FruitConfigId,
                        principalTable: "Config_Fruit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_KillSpawn",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShowOnMinimap = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GameMapConfigId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_KillSpawn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_KillSpawn_Config_Map_GameMapConfigId",
                        column: x => x.GameMapConfigId,
                        principalTable: "Config_Map",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_Mob",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    ViewRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HuntRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Class = table.Column<int>(type: "int", nullable: false),
                    Coliseum = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Round = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    WeekDay = table.Column<int>(type: "int", nullable: false),
                    ColiseumMobType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReactionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Attribute = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Element = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family1 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family2 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family3 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RespawnInterval = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    GameMapConfigId = table.Column<long>(type: "bigint", nullable: false),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_Mob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_Mob_Config_Map_GameMapConfigId",
                        column: x => x.GameMapConfigId,
                        principalTable: "Config_Map",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonMob",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    ViewRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HuntRange = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Class = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReactionType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Attribute = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Element = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family1 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family2 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Family3 = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ASValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ARValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ATValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BLValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DEValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EVValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HPValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HTValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WSValue = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SummonDTOId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonMob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonMob_Config_Summon_SummonDTOId",
                        column: x => x.SummonDTOId,
                        principalTable: "Config_Summon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Tamer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Model = table.Column<int>(type: "int", nullable: false, defaultValue: 80001),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)10000),
                    State = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EventState = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ServerId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Channel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)255),
                    DigimonSlots = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)3),
                    CurrentHp = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    CurrentDs = table.Column<int>(type: "int", nullable: false, defaultValue: 40),
                    XGauge = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    XCrystals = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CurrentTitle = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    GuildId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Tamer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Tamer_Guild_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild_Guild",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guild_Authority",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Class = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    Title = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duty = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Authority", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guild_Authority_Guild_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild_Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guild_Historic",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    MasterClass = table.Column<int>(type: "int", nullable: false),
                    MasterName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MemberClass = table.Column<int>(type: "int", nullable: false),
                    MemberName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuildId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Historic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guild_Historic_Guild_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild_Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guild_Skill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    GuildId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Skill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guild_Skill_Guild_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild_Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_EvolutionStage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    EvolutionLineId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_EvolutionStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_EvolutionStage_Asset_EvolutionLine_EvolutionLineId",
                        column: x => x.EvolutionLineId,
                        principalTable: "Asset_EvolutionLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ExtraEvolution",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DigimonId = table.Column<int>(type: "int", nullable: false),
                    RequiredLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    ExtraInfoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ExtraEvolution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ExtraEvolution_Asset_ExtraEvolutionInformation_ExtraIn~",
                        column: x => x.ExtraInfoId,
                        principalTable: "Asset_ExtraEvolutionInformation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcPortalsAmount",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NpcAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcPortalsAmount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_NpcPortalsAmount_Asset_NpcPortal_NpcAssetId",
                        column: x => x.NpcAssetId,
                        principalTable: "Asset_NpcPortal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_QuestRewardObject",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reward = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    QuestRewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_QuestRewardObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_QuestRewardObject_Asset_QuestReward_QuestRewardId",
                        column: x => x.QuestRewardId,
                        principalTable: "Asset_QuestReward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_KillSpawnSourceMob",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceMobType = table.Column<int>(type: "int", nullable: false),
                    SourceMobRequiredAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    KillSpawnId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_KillSpawnSourceMob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_KillSpawnSourceMob_Config_KillSpawn_KillSpawnId",
                        column: x => x.KillSpawnId,
                        principalTable: "Config_KillSpawn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_KillSpawnTargetMob",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TargetMobType = table.Column<int>(type: "int", nullable: false),
                    TargetMobAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    KillSpawnId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_KillSpawnTargetMob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_KillSpawnTargetMob_Config_KillSpawn_KillSpawnId",
                        column: x => x.KillSpawnId,
                        principalTable: "Config_KillSpawn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_MobDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MinAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    MaxAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    MobId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_MobDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_MobDropReward_Config_Mob_MobId",
                        column: x => x.MobId,
                        principalTable: "Config_Mob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_MobExpReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TamerExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    DigimonExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    NatureExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ElementExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    SkillExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    MobId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_MobExpReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_MobExpReward_Config_Mob_MobId",
                        column: x => x.MobId,
                        principalTable: "Config_Mob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_MobLocation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MobConfigId = table.Column<long>(type: "bigint", nullable: false),
                    MapId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 4500),
                    Z = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_MobLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_MobLocation_Config_Mob_MobConfigId",
                        column: x => x.MobConfigId,
                        principalTable: "Config_Mob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonMobDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MinAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    MaxAmount = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    MobId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonMobDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonMobDropReward_Config_SummonMob_MobId",
                        column: x => x.MobId,
                        principalTable: "Config_SummonMob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonMobExpReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TamerExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    DigimonExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    NatureExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ElementExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    SkillExperience = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    MobId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonMobExpReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonMobExpReward_Config_SummonMob_MobId",
                        column: x => x.MobId,
                        principalTable: "Config_SummonMob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonMobLocation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 4500),
                    Z = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    MobConfigId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonMobLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonMobLocation_Config_SummonMob_MobConfigId",
                        column: x => x.MobConfigId,
                        principalTable: "Config_SummonMob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_ActiveEvolution",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DsPerSecond = table.Column<int>(type: "int", nullable: false),
                    XgPerSecond = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_ActiveEvolution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_ActiveEvolution_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_ActiveSkill",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Cooldown = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    Duration = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    EndCooldown = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_ActiveSkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_ActiveSkill_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_ArenaPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_ArenaPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_ArenaPoints_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_BuffList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_BuffList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_BuffList_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_DailyPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InsertDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_DailyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_DailyPoints_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_DigimonArchive",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slots = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_DigimonArchive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_DigimonArchive_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Foe",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Annotation = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoeId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Foe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Foe_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Friend",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Annotation = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Connected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FriendId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Friend", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Friend_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Incubator",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EggId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    HatchLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    BackupDiskId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Incubator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Incubator_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Location",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    MapId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 4500),
                    Z = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Location_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_MapRegion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Unlocked = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_MapRegion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_MapRegion_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Progress",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompletedData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedDataValue = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Progress_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_SealList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SealLeaderId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_SealList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_SealList_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_XaiInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    XGauge = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    XCrystals = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_XaiInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_XaiInfo_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_Digimon",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BaseType = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    Name = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Size = table.Column<short>(type: "smallint", nullable: false),
                    CurrentExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CurrentSkillExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TranscendenceExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    HatchGrade = table.Column<int>(type: "int", nullable: false),
                    CurrentType = table.Column<int>(type: "int", nullable: false),
                    Friendship = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    CurrentHp = table.Column<int>(type: "int", nullable: false, defaultValue: 150),
                    CurrentDs = table.Column<int>(type: "int", nullable: false, defaultValue: 140),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_Digimon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_Digimon_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Event_AttendanceReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TotalDays = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    LastRewardDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_AttendanceReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_AttendanceReward_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Event_TimeReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RewardIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event_TimeReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_TimeReward_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Guild_Member",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Contribution = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Authority = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guild_Member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guild_Member_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Guild_Member_Guild_Guild_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guild_Guild",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Security_ChatMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    Message = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Security_ChatMessage_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bits = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    AccountId = table.Column<long>(type: "bigint", nullable: true),
                    CharacterId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemList_Account_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shared_ItemList_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shop_ConsignedShop",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShopName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GeneralHandler = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shop_ConsignedShop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shop_ConsignedShop_Character_Tamer_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character_Tamer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ExtraEvolutionMaterial",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ExtraEvolutionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ExtraEvolutionMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ExtraEvolutionMaterial_Asset_ExtraEvolution_ExtraEvolu~",
                        column: x => x.ExtraEvolutionId,
                        principalTable: "Asset_ExtraEvolution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_ExtraEvolutionRequired",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    ExtraEvolutionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_ExtraEvolutionRequired", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_ExtraEvolutionRequired_Asset_ExtraEvolution_ExtraEvolu~",
                        column: x => x.ExtraEvolutionId,
                        principalTable: "Asset_ExtraEvolution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Asset_NpcPortals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ResourceAmount = table.Column<int>(type: "int", nullable: false),
                    NpcAssetId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset_NpcPortals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_NpcPortals_Asset_NpcPortalsAmount_NpcAssetId",
                        column: x => x.NpcAssetId,
                        principalTable: "Asset_NpcPortalsAmount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_BitsDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    DropRewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_BitsDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_BitsDropReward_Config_MobDropReward_DropRewardId",
                        column: x => x.DropRewardId,
                        principalTable: "Config_MobDropReward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_ItemDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    Rank = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DropRewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_ItemDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_ItemDropReward_Config_MobDropReward_DropRewardId",
                        column: x => x.DropRewardId,
                        principalTable: "Config_MobDropReward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonBitsDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    DropRewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonBitsDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonBitsDropReward_Config_SummonMobDropReward_DropR~",
                        column: x => x.DropRewardId,
                        principalTable: "Config_SummonMobDropReward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Config_SummonMobItemDropReward",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxAmount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Chance = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m),
                    DropRewardId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config_SummonMobItemDropReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_SummonMobItemDropReward_Config_SummonMobDropReward_Dr~",
                        column: x => x.DropRewardId,
                        principalTable: "Config_SummonMobDropReward",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Buff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuffListId = table.Column<long>(type: "bigint", nullable: false),
                    BuffId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Duration = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TypeN = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Cooldown = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CoolEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Buff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Buff_Character_BuffList_BuffListId",
                        column: x => x.BuffListId,
                        principalTable: "Character_BuffList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_DigimonArchiveItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false),
                    DigimonArchiveId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_DigimonArchiveItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_DigimonArchiveItem_Character_DigimonArchive_Digimo~",
                        column: x => x.DigimonArchiveId,
                        principalTable: "Character_DigimonArchive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_InProgressQuest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestId = table.Column<short>(type: "smallint", nullable: false),
                    FirstCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SecondCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ThirdCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FourthCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FifthCondition = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CharacterProgressId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_InProgressQuest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_InProgressQuest_Character_Progress_CharacterProgre~",
                        column: x => x.CharacterProgressId,
                        principalTable: "Character_Progress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Character_Seal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SealId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    Favorite = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SequentialId = table.Column<short>(type: "smallint", nullable: false),
                    SealListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character_Seal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Character_Seal_Character_SealList_SealListId",
                        column: x => x.SealListId,
                        principalTable: "Character_SealList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_AttributeExperience",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Data = table.Column<short>(type: "smallint", nullable: false),
                    Vaccine = table.Column<short>(type: "smallint", nullable: false),
                    Virus = table.Column<short>(type: "smallint", nullable: false),
                    Ice = table.Column<short>(type: "smallint", nullable: false),
                    Water = table.Column<short>(type: "smallint", nullable: false),
                    Fire = table.Column<short>(type: "smallint", nullable: false),
                    Land = table.Column<short>(type: "smallint", nullable: false),
                    Wind = table.Column<short>(type: "smallint", nullable: false),
                    Wood = table.Column<short>(type: "smallint", nullable: false),
                    Light = table.Column<short>(type: "smallint", nullable: false),
                    Dark = table.Column<short>(type: "smallint", nullable: false),
                    Thunder = table.Column<short>(type: "smallint", nullable: false),
                    Steel = table.Column<short>(type: "smallint", nullable: false),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_AttributeExperience", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_AttributeExperience_Digimon_Digimon_DigimonId",
                        column: x => x.DigimonId,
                        principalTable: "Digimon_Digimon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_BuffList",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_BuffList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_BuffList_Digimon_Digimon_DigimonId",
                        column: x => x.DigimonId,
                        principalTable: "Digimon_Digimon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_Digiclone",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ATLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    ATValue = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    BLLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    BLValue = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CTLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    CTValue = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    EVLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    EVValue = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HPLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    HPValue = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_Digiclone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_Digiclone_Digimon_Digimon_DigimonId",
                        column: x => x.DigimonId,
                        principalTable: "Digimon_Digimon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_Evolution",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Unlocked = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SkillExperience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SkillPoints = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    SkillMastery = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_Evolution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_Evolution_Digimon_Digimon_DigimonId",
                        column: x => x.DigimonId,
                        principalTable: "Digimon_Digimon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_Location",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DigimonId = table.Column<long>(type: "bigint", nullable: false),
                    MapId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 4500),
                    Z = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_Location_Digimon_Digimon_DigimonId",
                        column: x => x.DigimonId,
                        principalTable: "Digimon_Digimon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_Item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    FirstExpired = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    TamerShopSellPrice = table.Column<int>(type: "int", nullable: false),
                    Power = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RerollLeft = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FamilyType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    ItemListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_Item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_Item_Shared_ItemList_ItemListId",
                        column: x => x.ItemListId,
                        principalTable: "Shared_ItemList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shop_Location",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConsignedShopId = table.Column<long>(type: "bigint", nullable: false),
                    MapId = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    X = table.Column<int>(type: "int", nullable: false, defaultValue: 5000),
                    Y = table.Column<int>(type: "int", nullable: false, defaultValue: 4500),
                    Z = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shop_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shop_Location_Shop_ConsignedShop_ConsignedShopId",
                        column: x => x.ConsignedShopId,
                        principalTable: "Shop_ConsignedShop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_Buff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BuffListId = table.Column<long>(type: "bigint", nullable: false),
                    BuffId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Duration = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TypeN = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Cooldown = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SkillId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CoolEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_Buff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_Buff_Digimon_BuffList_BuffListId",
                        column: x => x.BuffListId,
                        principalTable: "Digimon_BuffList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_DigicloneHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ATValues = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BLValues = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CTValues = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EVValues = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HPValues = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DigimonDigicloneId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_DigicloneHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_DigicloneHistory_Digimon_Digiclone_DigimonDigicloneId",
                        column: x => x.DigimonDigicloneId,
                        principalTable: "Digimon_Digiclone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Digimon_EvolutionSkill",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrentLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    Duration = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    MaxLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)10),
                    EvolutionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digimon_EvolutionSkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Digimon_EvolutionSkill_Digimon_Evolution_EvolutionId",
                        column: x => x.EvolutionId,
                        principalTable: "Digimon_Evolution",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemAccessoryStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemAccessoryStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemAccessoryStatus_Shared_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Shared_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shared_ItemSocketStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AttributeId = table.Column<short>(type: "smallint", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Value = table.Column<short>(type: "smallint", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shared_ItemSocketStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shared_ItemSocketStatus_Shared_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Shared_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Asset_Xai",
                columns: new[] { "Id", "ItemId", "XCrystals", "XGauge" },
                values: new object[] { 1L, 40017, (short)3, 2000 });

            migrationBuilder.InsertData(
                table: "Config_Hash",
                columns: new[] { "Id", "CreatedAt", "Hash" },
                values: new object[] { 1L, new DateTime(2026, 5, 8, 7, 38, 36, 189, DateTimeKind.Local).AddTicks(299), "pMgM+NOH0Z+RwR9F1iFVOOwKrW1iDaifx4jWDnH1Dbo=" });

            migrationBuilder.InsertData(
                table: "Config_User",
                columns: new[] { "Id", "AccessLevel", "Password", "Username" },
                values: new object[] { 1L, 15, "pMgM+NOH0Z+RwR9F1iFVOOwKrW1iDaifx4jWDnH1Dbo=", "masteradmin" });

            migrationBuilder.InsertData(
                table: "Config_WelcomeMessage",
                columns: new[] { "Id", "Enabled", "Message" },
                values: new object[,]
                {
                    { 1L, false, "1 1 1 1 1 1 0 0 1 1 1" },
                    { 2L, true, "Please, drink some water! :)" },
                    { 3L, true, "Did you hear that?" },
                    { 4L, true, "Remember to feed your pet." },
                    { 5L, true, "Not a Pokémon game." },
                    { 6L, true, "Warning: Chat may be toxic." },
                    { 7L, true, "Be yourself!" },
                    { 8L, true, "Welcome to DSO!" },
                    { 9L, true, "Do you like chocolate?" },
                    { 10L, true, "Here we go again!" },
                    { 11L, true, "Join our Discord! discord.gg/dsooficial" },
                    { 12L, true, "Can you see that mountain over there?" },
                    { 13L, true, "\"Look into the source\"" },
                    { 14L, true, "The staff will NEVER ask your password!" },
                    { 15L, true, "Y0ur br4in 1s am4z1ng!" },
                    { 16L, true, "This is the rythm of the night! (8)" },
                    { 17L, false, "Happy new eyer !!!" }
                });

            migrationBuilder.InsertData(
                table: "Routine_Routine",
                columns: new[] { "Id", "Active", "CreatedAt", "Interval", "Name", "NextRunTime", "Type" },
                values: new object[] { 1L, true, new DateTime(2026, 5, 8, 7, 38, 36, 192, DateTimeKind.Local).AddTicks(4304), 1, "Daily Quests", new DateTime(2026, 5, 9, 0, 0, 0, 0, DateTimeKind.Local), 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountBlock_AccountId",
                table: "Account_AccountBlock",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_SystemInformation_AccountId",
                table: "Account_SystemInformation",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arena_Competitor_RankingId",
                table: "Arena_Competitor",
                column: "RankingId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AccessoryRollStatus_AccessoryRollAssetId",
                table: "Asset_AccessoryRollStatus",
                column: "AccessoryRollAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ArenaDailyItemReward_RewardId",
                table: "Asset_ArenaDailyItemReward",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ContainerReward_ContainerAssetId",
                table: "Asset_ContainerReward",
                column: "ContainerAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_EvolutionLine_EvolutionId",
                table: "Asset_EvolutionLine",
                column: "EvolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_EvolutionStage_EvolutionLineId",
                table: "Asset_EvolutionStage",
                column: "EvolutionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ExtraEvolution_ExtraInfoId",
                table: "Asset_ExtraEvolution",
                column: "ExtraInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ExtraEvolutionInformation_ExtraNpcId",
                table: "Asset_ExtraEvolutionInformation",
                column: "ExtraNpcId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ExtraEvolutionMaterial_ExtraEvolutionId",
                table: "Asset_ExtraEvolutionMaterial",
                column: "ExtraEvolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ExtraEvolutionRequired_ExtraEvolutionId",
                table: "Asset_ExtraEvolutionRequired",
                column: "ExtraEvolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ItemCraftMaterial_ItemCraftId",
                table: "Asset_ItemCraftMaterial",
                column: "ItemCraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_MapRegion_MapRegionListId",
                table: "Asset_MapRegion",
                column: "MapRegionListId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_NpcItem_NpcAssetId",
                table: "Asset_NpcItem",
                column: "NpcAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_NpcMobInfo_NpcAssetId",
                table: "Asset_NpcMobInfo",
                column: "NpcAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_NpcPortal_NpcAssetId",
                table: "Asset_NpcPortal",
                column: "NpcAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_NpcPortals_NpcAssetId",
                table: "Asset_NpcPortals",
                column: "NpcAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_NpcPortalsAmount_NpcAssetId",
                table: "Asset_NpcPortalsAmount",
                column: "NpcAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestCondition_QuestId",
                table: "Asset_QuestCondition",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestEvent_QuestId",
                table: "Asset_QuestEvent",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestGoal_QuestId",
                table: "Asset_QuestGoal",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestReward_QuestId",
                table: "Asset_QuestReward",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestRewardObject_QuestRewardId",
                table: "Asset_QuestRewardObject",
                column: "QuestRewardId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_QuestSupply_QuestId",
                table: "Asset_QuestSupply",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ScanRewardDetail_ScanDetailAssetId",
                table: "Asset_ScanRewardDetail",
                column: "ScanDetailAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_SkillCodeApply_SkillCodeAssetId",
                table: "Asset_SkillCodeApply",
                column: "SkillCodeAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_ActiveEvolution_CharacterId",
                table: "Character_ActiveEvolution",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_ActiveSkill_CharacterId",
                table: "Character_ActiveSkill",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_ArenaPoints_CharacterId",
                table: "Character_ArenaPoints",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_Buff_BuffListId",
                table: "Character_Buff",
                column: "BuffListId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_BuffList_CharacterId",
                table: "Character_BuffList",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_DailyPoints_CharacterId",
                table: "Character_DailyPoints",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_DigimonArchive_CharacterId",
                table: "Character_DigimonArchive",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_DigimonArchiveItem_DigimonArchiveId",
                table: "Character_DigimonArchiveItem",
                column: "DigimonArchiveId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Foe_CharacterId",
                table: "Character_Foe",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Friend_CharacterId",
                table: "Character_Friend",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Incubator_CharacterId",
                table: "Character_Incubator",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_InProgressQuest_CharacterProgressId",
                table: "Character_InProgressQuest",
                column: "CharacterProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Location_CharacterId",
                table: "Character_Location",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_MapRegion_CharacterId",
                table: "Character_MapRegion",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_Progress_CharacterId",
                table: "Character_Progress",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_Seal_SealListId",
                table: "Character_Seal",
                column: "SealListId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_SealList_CharacterId",
                table: "Character_SealList",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Character_Tamer_GuildId",
                table: "Character_Tamer",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Character_XaiInfo_CharacterId",
                table: "Character_XaiInfo",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_BitsDropReward_DropRewardId",
                table: "Config_BitsDropReward",
                column: "DropRewardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_FruitSize_FruitConfigId",
                table: "Config_FruitSize",
                column: "FruitConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_ItemDropReward_DropRewardId",
                table: "Config_ItemDropReward",
                column: "DropRewardId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_KillSpawn_GameMapConfigId",
                table: "Config_KillSpawn",
                column: "GameMapConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_KillSpawnSourceMob_KillSpawnId",
                table: "Config_KillSpawnSourceMob",
                column: "KillSpawnId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_KillSpawnTargetMob_KillSpawnId",
                table: "Config_KillSpawnTargetMob",
                column: "KillSpawnId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_Mob_GameMapConfigId",
                table: "Config_Mob",
                column: "GameMapConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_MobDropReward_MobId",
                table: "Config_MobDropReward",
                column: "MobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_MobExpReward_MobId",
                table: "Config_MobExpReward",
                column: "MobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_MobLocation_MobConfigId",
                table: "Config_MobLocation",
                column: "MobConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonBitsDropReward_DropRewardId",
                table: "Config_SummonBitsDropReward",
                column: "DropRewardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonMob_SummonDTOId",
                table: "Config_SummonMob",
                column: "SummonDTOId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonMobDropReward_MobId",
                table: "Config_SummonMobDropReward",
                column: "MobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonMobExpReward_MobId",
                table: "Config_SummonMobExpReward",
                column: "MobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonMobItemDropReward_DropRewardId",
                table: "Config_SummonMobItemDropReward",
                column: "DropRewardId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_SummonMobLocation_MobConfigId",
                table: "Config_SummonMobLocation",
                column: "MobConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_AttributeExperience_DigimonId",
                table: "Digimon_AttributeExperience",
                column: "DigimonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_Buff_BuffListId",
                table: "Digimon_Buff",
                column: "BuffListId");

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_BuffList_DigimonId",
                table: "Digimon_BuffList",
                column: "DigimonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_Digiclone_DigimonId",
                table: "Digimon_Digiclone",
                column: "DigimonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_DigicloneHistory_DigimonDigicloneId",
                table: "Digimon_DigicloneHistory",
                column: "DigimonDigicloneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_Digimon_CharacterId",
                table: "Digimon_Digimon",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_Evolution_DigimonId",
                table: "Digimon_Evolution",
                column: "DigimonId");

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_EvolutionSkill_EvolutionId",
                table: "Digimon_EvolutionSkill",
                column: "EvolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_Digimon_Location_DigimonId",
                table: "Digimon_Location",
                column: "DigimonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_AttendanceReward_CharacterId",
                table: "Event_AttendanceReward",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_TimeReward_CharacterId",
                table: "Event_TimeReward",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Authority_GuildId",
                table: "Guild_Authority",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Historic_GuildId",
                table: "Guild_Historic",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Member_CharacterId",
                table: "Guild_Member",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Member_GuildId",
                table: "Guild_Member",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guild_Skill_GuildId",
                table: "Guild_Skill",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Security_ChatMessage_CharacterId",
                table: "Security_ChatMessage",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_Item_ItemListId",
                table: "Shared_Item",
                column: "ItemListId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemAccessoryStatus_ItemId",
                table: "Shared_ItemAccessoryStatus",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemList_AccountId",
                table: "Shared_ItemList",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemList_CharacterId",
                table: "Shared_ItemList",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Shared_ItemSocketStatus_ItemId",
                table: "Shared_ItemSocketStatus",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Shop_ConsignedShop_CharacterId",
                table: "Shop_ConsignedShop",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shop_Location_ConsignedShopId",
                table: "Shop_Location",
                column: "ConsignedShopId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Account_AccountBlock");

            migrationBuilder.DropTable(
                name: "Account_Server");

            migrationBuilder.DropTable(
                name: "Account_SystemInformation");

            migrationBuilder.DropTable(
                name: "Arena_Competitor");

            migrationBuilder.DropTable(
                name: "Asset_AccessoryRollStatus");

            migrationBuilder.DropTable(
                name: "Asset_Achievement");

            migrationBuilder.DropTable(
                name: "Asset_ArenaDailyItemReward");

            migrationBuilder.DropTable(
                name: "Asset_Buff");

            migrationBuilder.DropTable(
                name: "Asset_CharacterBaseStatus");

            migrationBuilder.DropTable(
                name: "Asset_CharacterLevelStatus");

            migrationBuilder.DropTable(
                name: "Asset_Clone");

            migrationBuilder.DropTable(
                name: "Asset_CloneValue");

            migrationBuilder.DropTable(
                name: "Asset_ContainerReward");

            migrationBuilder.DropTable(
                name: "Asset_DigimonBaseInfo");

            migrationBuilder.DropTable(
                name: "Asset_DigimonLevelStatus");

            migrationBuilder.DropTable(
                name: "Asset_DigimonSkill");

            migrationBuilder.DropTable(
                name: "Asset_EvolutionArmor");

            migrationBuilder.DropTable(
                name: "Asset_EvolutionStage");

            migrationBuilder.DropTable(
                name: "Asset_ExtraEvolutionMaterial");

            migrationBuilder.DropTable(
                name: "Asset_ExtraEvolutionRequired");

            migrationBuilder.DropTable(
                name: "Asset_Hatch");

            migrationBuilder.DropTable(
                name: "Asset_ItemCraftMaterial");

            migrationBuilder.DropTable(
                name: "Asset_ItemInfo");

            migrationBuilder.DropTable(
                name: "Asset_Map");

            migrationBuilder.DropTable(
                name: "Asset_MapRegion");

            migrationBuilder.DropTable(
                name: "Asset_MonsterBaseInfo");

            migrationBuilder.DropTable(
                name: "Asset_MonsterSkill");

            migrationBuilder.DropTable(
                name: "Asset_MonsterSkillInfo");

            migrationBuilder.DropTable(
                name: "Asset_MonthlyEvent");

            migrationBuilder.DropTable(
                name: "Asset_NpcItem");

            migrationBuilder.DropTable(
                name: "Asset_NpcMobInfo");

            migrationBuilder.DropTable(
                name: "Asset_NpcPortals");

            migrationBuilder.DropTable(
                name: "Asset_Portal");

            migrationBuilder.DropTable(
                name: "Asset_QuestCondition");

            migrationBuilder.DropTable(
                name: "Asset_QuestEvent");

            migrationBuilder.DropTable(
                name: "Asset_QuestGoal");

            migrationBuilder.DropTable(
                name: "Asset_QuestRewardObject");

            migrationBuilder.DropTable(
                name: "Asset_QuestSupply");

            migrationBuilder.DropTable(
                name: "Asset_ScanRewardDetail");

            migrationBuilder.DropTable(
                name: "Asset_SealDetail");

            migrationBuilder.DropTable(
                name: "Asset_SkillCodeApply");

            migrationBuilder.DropTable(
                name: "Asset_SkillInfo");

            migrationBuilder.DropTable(
                name: "Asset_StatusApply");

            migrationBuilder.DropTable(
                name: "Asset_TamerSkill");

            migrationBuilder.DropTable(
                name: "Asset_TitleStatus");

            migrationBuilder.DropTable(
                name: "Asset_Xai");

            migrationBuilder.DropTable(
                name: "Character_ActiveEvolution");

            migrationBuilder.DropTable(
                name: "Character_ActiveSkill");

            migrationBuilder.DropTable(
                name: "Character_ArenaPoints");

            migrationBuilder.DropTable(
                name: "Character_Buff");

            migrationBuilder.DropTable(
                name: "Character_DailyPoints");

            migrationBuilder.DropTable(
                name: "Character_DigimonArchiveItem");

            migrationBuilder.DropTable(
                name: "Character_Foe");

            migrationBuilder.DropTable(
                name: "Character_Friend");

            migrationBuilder.DropTable(
                name: "Character_Incubator");

            migrationBuilder.DropTable(
                name: "Character_InProgressQuest");

            migrationBuilder.DropTable(
                name: "Character_Location");

            migrationBuilder.DropTable(
                name: "Character_MapRegion");

            migrationBuilder.DropTable(
                name: "Character_Seal");

            migrationBuilder.DropTable(
                name: "Character_XaiInfo");

            migrationBuilder.DropTable(
                name: "Config_BitsDropReward");

            migrationBuilder.DropTable(
                name: "Config_Clone");

            migrationBuilder.DropTable(
                name: "Config_FruitSize");

            migrationBuilder.DropTable(
                name: "Config_Hash");

            migrationBuilder.DropTable(
                name: "Config_Hatch");

            migrationBuilder.DropTable(
                name: "Config_ItemDropReward");

            migrationBuilder.DropTable(
                name: "Config_KillSpawnSourceMob");

            migrationBuilder.DropTable(
                name: "Config_KillSpawnTargetMob");

            migrationBuilder.DropTable(
                name: "Config_MobExpReward");

            migrationBuilder.DropTable(
                name: "Config_MobLocation");

            migrationBuilder.DropTable(
                name: "Config_SummonBitsDropReward");

            migrationBuilder.DropTable(
                name: "Config_SummonMobExpReward");

            migrationBuilder.DropTable(
                name: "Config_SummonMobItemDropReward");

            migrationBuilder.DropTable(
                name: "Config_SummonMobLocation");

            migrationBuilder.DropTable(
                name: "Config_User");

            migrationBuilder.DropTable(
                name: "Config_WelcomeMessage");

            migrationBuilder.DropTable(
                name: "Digimon_AttributeExperience");

            migrationBuilder.DropTable(
                name: "Digimon_Buff");

            migrationBuilder.DropTable(
                name: "Digimon_DigicloneHistory");

            migrationBuilder.DropTable(
                name: "Digimon_EvolutionSkill");

            migrationBuilder.DropTable(
                name: "Digimon_Location");

            migrationBuilder.DropTable(
                name: "Event_AttendanceReward");

            migrationBuilder.DropTable(
                name: "Event_TimeReward");

            migrationBuilder.DropTable(
                name: "Guild_Authority");

            migrationBuilder.DropTable(
                name: "Guild_Historic");

            migrationBuilder.DropTable(
                name: "Guild_Member");

            migrationBuilder.DropTable(
                name: "Guild_Skill");

            migrationBuilder.DropTable(
                name: "Routine_Routine");

            migrationBuilder.DropTable(
                name: "Security_ChatMessage");

            migrationBuilder.DropTable(
                name: "Security_LoginTry");

            migrationBuilder.DropTable(
                name: "Shared_ItemAccessoryStatus");

            migrationBuilder.DropTable(
                name: "Shared_ItemSocketStatus");

            migrationBuilder.DropTable(
                name: "Shop_Location");

            migrationBuilder.DropTable(
                name: "Arena_Ranking");

            migrationBuilder.DropTable(
                name: "Asset_AccessoryRoll");

            migrationBuilder.DropTable(
                name: "Asset_ArenaDailyItemRewards");

            migrationBuilder.DropTable(
                name: "Asset_Container");

            migrationBuilder.DropTable(
                name: "Asset_EvolutionLine");

            migrationBuilder.DropTable(
                name: "Asset_ExtraEvolution");

            migrationBuilder.DropTable(
                name: "Asset_ItemCraft");

            migrationBuilder.DropTable(
                name: "Asset_MapRegionList");

            migrationBuilder.DropTable(
                name: "Asset_NpcColiseum");

            migrationBuilder.DropTable(
                name: "Asset_NpcPortalsAmount");

            migrationBuilder.DropTable(
                name: "Asset_QuestReward");

            migrationBuilder.DropTable(
                name: "Asset_ScanDetail");

            migrationBuilder.DropTable(
                name: "Asset_SkillCode");

            migrationBuilder.DropTable(
                name: "Character_BuffList");

            migrationBuilder.DropTable(
                name: "Character_DigimonArchive");

            migrationBuilder.DropTable(
                name: "Character_Progress");

            migrationBuilder.DropTable(
                name: "Character_SealList");

            migrationBuilder.DropTable(
                name: "Config_Fruit");

            migrationBuilder.DropTable(
                name: "Config_MobDropReward");

            migrationBuilder.DropTable(
                name: "Config_KillSpawn");

            migrationBuilder.DropTable(
                name: "Config_SummonMobDropReward");

            migrationBuilder.DropTable(
                name: "Digimon_BuffList");

            migrationBuilder.DropTable(
                name: "Digimon_Digiclone");

            migrationBuilder.DropTable(
                name: "Digimon_Evolution");

            migrationBuilder.DropTable(
                name: "Shared_Item");

            migrationBuilder.DropTable(
                name: "Shop_ConsignedShop");

            migrationBuilder.DropTable(
                name: "Asset_Evolution");

            migrationBuilder.DropTable(
                name: "Asset_ExtraEvolutionInformation");

            migrationBuilder.DropTable(
                name: "Asset_NpcPortal");

            migrationBuilder.DropTable(
                name: "Asset_Quest");

            migrationBuilder.DropTable(
                name: "Config_Mob");

            migrationBuilder.DropTable(
                name: "Config_SummonMob");

            migrationBuilder.DropTable(
                name: "Digimon_Digimon");

            migrationBuilder.DropTable(
                name: "Shared_ItemList");

            migrationBuilder.DropTable(
                name: "Asset_ExtraEvolutionNpc");

            migrationBuilder.DropTable(
                name: "Asset_Npc");

            migrationBuilder.DropTable(
                name: "Config_Map");

            migrationBuilder.DropTable(
                name: "Config_Summon");

            migrationBuilder.DropTable(
                name: "Account_Account");

            migrationBuilder.DropTable(
                name: "Character_Tamer");

            migrationBuilder.DropTable(
                name: "Guild_Guild");
        }
    }
}
