using DigitalWorldOnline.Commons.Interfaces;

namespace DigitalWorldOnline.Commons.Enums.PacketProcessor
{
    public enum GameServerPacketEnum
    {
        /// <summary>
        /// Unknown packet
        /// </summary>
        Unknown = -99,

        /// <summary>
        /// To avoid connection break/interrupt, the client sends this often.
        /// </summary>
        KeepConnection = -3,

        /// <summary>
        /// Request connection with the server.
        /// </summary>
        Connection = -1,

        /// <summary>
        /// Equips a new title on the tamer.
        /// </summary>
        SetTitle = 15,

        /// <summary>
        /// Loads all the other game-related information.
        /// </summary>
        ComplementarInformation = 1001,
            
        /// <summary>
        /// Sent at every tamer or digimon movimentation.
        /// </summary>
        TamerMovimentation = 1004,
        
        /// <summary>
        /// Sends a message to the target chat.
        /// </summary>
        ChatMessage = 1008,
        
        /// <summary>
        /// Sends a private message to the target character.
        /// </summary>
        WhisperMessage = 1009,
        
        /// <summary>
        /// Sends the current partner to perform an attack into the target.
        /// </summary>
        PartnerAttack = 1013,
        
        /// <summary>
        /// Sends the current partner to perform an skill into the target.
        /// </summary>
        PartnerSkill = 1015,

        /// <summary>
        /// Updates the current tamer target information.
        /// </summary>
        UpdateTarget = 1016,

        /// <summary>
        /// Request confirm dialog after partner defeat.
        /// </summary>
        DieConfirm = 1022,

        /// <summary>
        /// Evolves the current partner
        /// </summary>
        PartnerEvolution = 1028,

        /// <summary>
        /// Calls the current partner back to the tamer.
        /// </summary>
        PartnerStop = 1033,

        /// <summary>
        /// Insert egg on incubator.
        /// </summary>
        HatchInsertEgg = 1036,

        /// <summary>
        /// Try to increase the curent incubator egg.
        /// </summary>
        HatchIncrease = 1037,
        
        /// <summary>
        /// Ecloacs the current egg.
        /// </summary>
        HatchFinish = 1038,

        /// <summary>
        /// Removes the egg from the incubator.
        /// </summary>
        HatchRemoveEgg = 1039,
        
        /// <summary>
        /// Switches the current partner.
        /// </summary>
        PartnerSwitch = 1041,
        
        /// <summary>
        /// Deletes the selected partner.
        /// </summary>
        PartnerDelete = 1042,

        /// <summary>
        /// Unlocks the target map region.
        /// </summary>
        UnlockRegion = 1051,

        /// <summary>
        /// Evolution unlock.
        /// </summary>
        EvolutionUnlock = 1055,

        /// <summary>
        /// Sends a shout message to the current map.
        /// </summary>
        ShoutMessage = 1056,
        
        /// <summary>
        /// Jumps to another map.
        /// </summary>
        JumpBooster = 1057,

        /// <summary>
        /// Tamer fun actions (dance, yellow, etc).
        /// </summary>
        TamerReaction = 1058,

        /// <summary>
        /// Unlocks the ride mode of the target evolution.
        /// </summary>
        EvolutionRideUnlock = 1063,

        /// <summary>
        /// Sends a global megaphone message.
        /// </summary>
        MegaphoneMessage = 1074,

        /// <summary>
        /// Client requests a UI/data refresh (pGame::ReloadData).
        /// </summary>
        ReloadData = 1080,

        /// <summary>
        /// Sends the digiclone results
        /// </summary>
        PartnerDigiclone = 1075,
        
        /// <summary>
        /// Sends the digiclone reset
        /// </summary>
        PartnerDigicloneReset = 1083,

        /// <summary>
        /// Sends the Skill Level Up
        /// </summary>
        SkillLevelUp = 1104,

        /// <summary>
        /// Sends the Summon Player
        /// </summary>
        TamerSummon = 1114,

        /// <summary>
        /// pSkill::AddSkillChip (1118) — server→client only.  Sent after a successful
        /// chip-consume registers a memory skill on the partner's evolution.  Not a
        /// receive-side packet, but kept here so logs disambiguate it from "no processor".
        /// </summary>
        MemorySkillAdd = 1118,

        /// <summary>
        /// pSkill::RemoveSkillChip — client requests deletion of a memory skill from
        /// the partner's current evolution.  Payload: u4 SkillCode.
        /// </summary>
        MemorySkillRemove = 1119,

        /// <summary>
        /// pSkill::UseSkillChip — client requests cast of an attached memory skill.
        /// Payload: u4 DigimonUID, u1 EvoStep, u4 SkillCode, u4 TargetUID.
        /// </summary>
        MemorySkillUse = 1120,


        /// <summary>
        /// Tamer Change Character Name
        /// </summary>
        TamerChangeName = 1311,

        /// <summary>
        /// Tamer Change Character Model
        /// </summary>
        TamerChangeModel = 1314,

        /// <summary>
        /// Turns partner into ride mode.
        /// </summary>
        PartnerRideModeStart = 1325,

        /// <summary>
        /// Ends partner ride mode.
        /// </summary>
        PartnerRideModeStop = 1326,

        /// <summary>
        /// Send  TamerSkillRequest.
        /// </summary>
        TamerSkillRequest = 1327,

        ActiveTamerCashSkillRemove = 1332,

        /// <summary>
        /// Sets the target seal as favorite
        /// </summary>
        SetSealFavorite = 1334,

        /// <summary>
        /// Send Request Trade
        /// </summary>
        TradeRequestSend = 1501,

        /// <summary>
        /// Trade Request Accept
        /// </summary>
        TradeRequestAccept = 1502,

        /// <summary>
        /// Trade Confirmation 
        /// </summary>
        TradeConfirmation = 1503,

        /// <summary>
        /// Trade Inventory Unlock 
        /// </summary>
        TradeInventoryUnlock = 1505,

        /// <summary>
        /// Trade Refuse 
        /// </summary>
        TradeRefuse = 1506,

        /// <summary>
        /// Trade Add Item
        /// </summary>
        TradeAddItem = 1508,

        /// <summary>
        /// Trade Add Money
        /// </summary>
        TradeAddMoney = 1509,

        /// <summary>
        /// Shows the incoming personal shop window.
        /// </summary>
        PersonalShopPrepare = 1510,

        /// <summary>
        /// Open the tamer shop.
        /// </summary>
        TamerShopOpen = 1511,

        /// <summary>
        /// Shows the target consigned shop items.
        /// </summary>
        ConsignedShopView = 1515,

        /// <summary>
        /// Open the consigned shop.
        /// </summary>
        ConsignedShopOpen = 1516,

        /// <summary>
        /// Retrieves the items from consigned shop and disables the shop.
        /// </summary>
        ConsignedShopRetrieve = 1517,

        /// <summary>
        /// Purchase's an item from the consigned shop item list.
        /// </summary>
        ConsignedShopPurchaseItem = 1518,

        /// <summary>
        /// Trade Add Money
        /// </summary>
        TradeRemoveItem = 1531,

        /// <summary>
        /// Retrieves item and bits from the consigned shop warehouse.
        /// </summary>
        ConsignedWarehouseRetrieve = 1521,

        /// <summary>
        /// Open the running consigned shop item list.
        /// </summary>
        ConsignedWarehouse = 1523,

        /// <summary>
        /// Trade Inventory lock 
        /// </summary>
        TradeInventorylock = 1532,

        /// <summary>
        /// Client → server: request to switch the current map's channel.
        /// Maps to <c>pGame::ChangeChannel</c> = <c>nScope::Game (1000) + 50</c>.
        /// Payload: <c>u4 nChannelIDX</c>.  (Previously misnamed
        /// <c>ChannelSwitchConfirm = 1703</c>, which is actually <c>pSvr::GoBackGate</c>.)
        /// </summary>
        ChangeChannel = 1050,

        /// <summary>
        /// Client → server: fired by <c>cCliGame::SendChangeServer</c>
        /// (<c>pSvr::Change = 1703</c>) when <c>LoadingContents::_DataLoadComplete</c>
        /// finishes a portal/MapSwap-triggered load.  No payload.
        ///
        /// Server response: re-add the tamer to the (possibly new-channel) map
        /// instance and resend <c>InitialInfoPacket</c> so the client's freshly
        /// reset state gets re-initialised.  See
        /// <c>PostLoadCompletePacketProcessor</c>.
        ///
        /// Without this handler, any flow that pushes a <c>MapSwapPacket</c>
        /// (channel switch, GM /summon /warp, die-respawn) leaves the client
        /// stuck on the loading screen because the server silently drops 1703.
        /// </summary>
        PostLoadComplete = 1703,

        /// <summary>
        /// Loads the base information about the tamer and digimons.
        /// </summary>
        InitialInformation = 1706,

        /// <summary>
        /// Teleports the tamer to another location.
        /// </summary>
        WarpGate = 1709,

        /// <summary>
        /// Bidirectional: client → server request for the channel list (no payload),
        /// server → client reply carrying <c>(u1 idx, u1 load) × N + u1 0xFF</c>.
        /// Maps to <c>pSvr::ChannelInfo</c> in <c>common_vs2019/pServer.h</c>.
        /// Counting from <c>Begin = nScope::Svr (1700)</c>: ClusterList=1701,
        /// SelectCluster=1702, Change=1703, GoBackGate=1704, GoBackAccount=1705,
        /// AccessCode=1706, KillGate=1707, KillSession=1708, SelectPortal=1709,
        /// SelectPortalFailure=1710, LocalPortal=1711, TryLogin=1712,
        /// <b>ChannelInfo=1713</b>.  A prior "fix" miscounted and used 1712 —
        /// reverted here.
        /// </summary>
        Channels = 1713,

        /// <summary>
        /// Sends a guild message to the entire guild.
        /// </summary>
        GuildMessage = 2114,

        /// <summary>
        /// Sends an invite to party.
        /// </summary>
        PartyRequestSend = 2301,

        /// <summary>
        /// Sends a response to the party invite.
        /// </summary>
        PartyRequestResponse = 2302,
        
        /// <summary>
        /// Sends a message to the entire party.
        /// </summary>
        PartyMessage = 2304,
        
        /// <summary>
        /// Kicks a member of the party.
        /// </summary>
        PartyMemberKick = 2306,
        
        /// <summary>
        /// Member quit from the party.
        /// </summary>
        PartyMemberLeave = 2307,
        
        /// <summary>
        /// Changes the party leader.
        /// </summary>
        PartyLeaderChange = 2308,
        
        /// <summary>
        /// Changes the party config.
        /// </summary>
        PartyConfigChange = 2309,

        /// <summary>
        /// Loads the friend information.
        /// </summary>
        FriendInformation = 3129,

        /// <summary>
        /// Sets the target seal as leader
        /// </summary>
        SetSealLeader = 3232,

        /// <summary>
        /// Removes the current seal leader
        /// </summary>
        RemoveSealLeader = 3233,

        /// <summary>
        /// Adds a digimon to archive/storage.
        /// </summary>
        DigimonArchiveInsert = 3201,
        
        /// <summary>
        /// Loads the digimon archive/storage.
        /// </summary>
        DigimonArchive = 3204,
        
        /// <summary>
        /// Loads the remaining membership timer.
        /// </summary>
        MembershipInformation = 3414,

        /// <summary>
        ///  HatchSpiritEvolution.
        /// </summary>
        HatchSpiritEvolution = 3239,

        /// <summary>
        ///  Spirit Craft.
        /// </summary>
        SpiritCraft = 3240,

        /// <summary>
        /// D-Skill expansion: raise per-skill cap on a partner using a Type-202 item.
        /// pDigimon::DigimonSkillLimitOpen — gated client-side by SDM_DIGIMONSKILL_LV_EXPEND_20181206.
        /// </summary>
        DigimonSkillLimitOpen = 3245,

        /// <summary>
        /// Moves an item to another position/storage.
        /// </summary>
        MoveItem = 3904,

        /// <summary>
        /// Split inventory items
        /// </summary>
        SplitItem = 3907,

        /// <summary>
        /// Removes and item from the inventory
        /// </summary>
        ItemRemove = 3909,

        /// <summary>
        /// Sends the tamer to loot the item on the ground.
        /// </summary>
        LootItem = 3910,
        
        /// <summary>
        /// Purchases an item from the NPC store.
        /// </summary>
        NpcItemPurchase = 3915,
        
        /// <summary>
        /// Sells an item to the NPC store.
        /// </summary>
        NpcItemSell = 3916,

        /// <summary>
        /// Item Socket In Attribute
        /// </summary>
        ItemSocketIn = 3926,

        /// <summary>
        /// Item Socket Out Attribute
        /// </summary>
        ItemSocketOut = 3927,

        /// <summary>
        /// Sells an item to the NPC store.
        /// </summary>
        ItemSocketIdentify = 3929,

        /// <summary>
        /// Opens the target seal
        /// </summary>
        OpenSeal = 3971,
        
        /// <summary>
        /// Closes the target seal
        /// </summary>
        CloseSeal = 3972,
        
        /// <summary>
        /// Repurchase an item from the NPC
        /// </summary>
        RepurchaseItem = 3978,

        /// <summary>
        /// Crafts an item.
        /// </summary>
        ItemCraft = 3982,
        
        /// <summary>
        /// Sorts the inventory items.
        /// </summary>
        InventorySort = 3986,

        /// <summary>
        /// Dungeon Warp packet.
        /// </summary>
        WarpGateDungeon = 4119,

        /// <summary>
        /// Arena current Stage Next.
        /// </summary>
        DungeonArenaStageNext = 4126,

        /// <summary>
        /// Arena surrender request.
        /// </summary>
        DungeonArenaSurrender = 4127,

        /// <summary>
        /// Arena daily ranking load.
        /// </summary>
        ArenaDailyRankingLoad = 4130,

        /// <summary>
        /// Arena daily insert points.
        /// </summary>
        ArenaDailyInsertPoints = 4131,

        /// <summary>
        /// Update the quest status.
        /// </summary>
        QuestUpdate = 11001,

        /// <summary>
        /// Accepts a quest.
        /// </summary>
        QuestAccept = 11002,
        
        /// <summary>
        /// Give up a quest.
        /// </summary>
        QuestGiveUp = 11003,

        /// <summary>
        /// Deliver a quest.
        /// </summary>
        QuestDeliver = 11004,

        /// <summary>
        /// Receives a new achievement.
        /// </summary>
        ProgressUpdate = 11007,


        /// <summary>
        /// Consumes an item.
        /// </summary>
        ConsumeItem = 3901,

        /// <summary>
        /// sends the loading Tamer Account Warehouse.
        /// </summary>
        LoadAccountWarehouse = 3930,

        /// <summary>
        /// sends the loading Tamer Account Warehouse.
        /// </summary>
        RetrivieAccountWarehouseItem = 3931,
        /// <summary>
        /// sends the loading Tamer Gift Storage.
        /// </summary>
        LoadGiftStorage = 3935,

        /// <summary>
        /// sends the Gift Storage Item Retrieve.
        /// </summary>
        GiftStorageItemRetrieve = 3936,

        /// <summary>
        /// pEvent::Attendance — duplex packet: client→server "claim attendance" request
        /// (no payload), server→client claim result with <c>n4 nResCode</c> and optional
        /// <c>u4 nGiveItemNo + n4 nWorkDayHistory</c> on success. Triggered by the player
        /// clicking the attendance button at <c>BGSprite.cpp:696</c>.
        /// </summary>
        AttendanceRequest = 3107,

        /// <summary>
        /// pEvent::HotTimeEvent — server→client panel push: state, current/next table indices,
        /// already-claimed flag, time-left to next state edge. Driven by Event.bin §5 records.
        /// </summary>
        HotTimeEventInfo = 3134,

        /// <summary>
        /// pEvent::HotTimeItemRequest — duplex packet: client→server "claim" request,
        /// server→client claim result (n1 nResult per nsHotTimeResult).
        /// </summary>
        HotTimeItemRequest = 3135,

        /// <summary>
        /// pCashShop::BuyRequest (Steam pre-purchase). v487 SendCashShopBuyRequest_Steam
        /// pushes <c>n1 itemCount, n4 totalPrice, n4 cashType=2, itemCount × n4 productID</c>.
        /// On non-Steam servers this is a stub that always replies success so the client
        /// falls into the regular MultiBuy flow.
        /// </summary>
        CashShopBuyRequest = 3401,

        /// <summary>
        /// pCashShop::Gift — single-item gift to peer tamer. C→S:
        /// <c>n4 price, n4 productIDX, wstring peerTamerName, WORD trailingProtocolDup</c>
        /// (the trailing WORD is a copy-paste bug in <c>SendGiftCashItem</c> that pushes
        /// the protocol number into the body; server consumes + ignores).
        /// </summary>
        CashShopGiftRequest = 3403,

        /// <summary>
        /// pCashShop::Balance — duplex. C→S empty; S→C
        /// <c>n4 result · n4 bonusCash · n4 haveCash</c>. Fired by the cash shop window
        /// on open and after each successful purchase to refresh the display.
        /// </summary>
        CashShopBalanceRequest = 3404,

        /// <summary>
        /// pCashShop::BuyHistory — duplex. C→S empty; S→C
        /// <c>n1 result · n2 count · count × n4 productID</c>. Used by the cash shop
        /// catalog UI to dim already-purchased products.
        /// </summary>
        CashShopBuyHistoryRequest = 3412,

        /// <summary>
        /// pCashShop::MultiBuy — multi-item checkout. C→S
        /// <c>n1 itemCnt, n4 totalPrice, u8 orderID, itemCnt × n4 productID</c>;
        /// S→C <c>u2 result, n4 realCash, n4 bonusCash, n1 successCnt + n4×successCnt,
        /// n1 failedCnt + n4×failedCnt</c>. The single-item Buy (3402) is unused in v487
        /// — both `SendBuyCashItem` and the dispatch route through MultiBuy.
        /// </summary>
        CashShopMultiBuyRequest = 3413,
        /// <summary>
        /// Join event queue. (Custom)
        /// </summary>
        JoinEventQueue = 3124,
        
        /// <summary>
        /// Loads NPC repurchase list.
        /// </summary>
        LoadNpcRepurchaseList = 3979,
        
        /// <summary>
        /// Removes an active buff manually.
        /// </summary>
        RemoveBuff = 4005,

        /// <summary>
        /// Updates the current guild message.
        /// </summary>
        GuildNoticeUpdate = 2126,

        /// <summary>
        /// Updates the current guild historic.
        /// </summary>
        GuildHistoric = 2128,

        /// <summary>
        /// Updates an authority title description.
        /// </summary>
        GuildTitleChange = 2129,

        /// <summary>
        /// Creates a new guild.
        /// </summary>
        CreateGuild = 2101,

        /// <summary>
        /// Deletes a guild.
        /// </summary>
        GuildDelete = 2102,

        /// <summary>
        /// Sends an invite to join the guild.
        /// </summary>
        GuildInvite = 2109,

        /// <summary>
        /// Refuses a guild invite.
        /// </summary>
        GuildInviteDeny = 2105,

        /// <summary>
        /// Accepts a guild invite.
        /// </summary>
        GuildInviteAccept = 2103,

        /// <summary>
        /// Guild member authority change.
        /// </summary>
        GuildAuthorityChangeToMaster = 2119,

        /// <summary>
        /// Guild member authority change.
        /// </summary>
        GuildAuthorityChangeToSubMaster = 2118,

        /// <summary>
        /// Guild member authority change.
        /// </summary>
        GuildAuthorityChangeToDatsMember = 2117,

        /// <summary>
        /// Guild member authority change.
        /// </summary>
        GuildAuthorityChangeToMember = 2116,

        /// <summary>
        /// Guild member authority change.
        /// </summary>
        GuildAuthorityChangeToNewMember = 2115,

        /// <summary>
        /// Guild member quit.
        /// </summary>
        GuildMemberLeave = 2107,

        /// <summary>
        /// Guild member kick.
        /// </summary>
        GuildMemberKick = 2106,
        
        /// <summary>
        /// Load encyclopedia data.
        /// </summary>
        EncyclopediaLoad = 3234,

        /// <summary>
        /// Return an item.
        /// </summary>
        ItemReturn = 3923,

        /// <summary>
        /// Insert backup disk on incubator.
        /// </summary>
        HatchInsertBackup = 3946,
        
        /// <summary>
        /// Removes the backup disk from incubator.
        /// </summary>
        HatchRemoveBackup = 3947,

        /// <summary>
        /// Identify an item.
        /// </summary>
        ItemIdentify = 3968,
            
        /// <summary>
        /// Reroll item status.
        /// </summary>
        ItemReroll = 3969,

        /// <summary>
        /// Guild member kick.
        /// </summary>
        ItemScan = 3987,

        /// <summary>
        /// Arena all ranking request info.
        /// </summary>
        ArenaRankingAllRequestInfo = 16023,

        /// <summary>
        /// Trancendence Partner  Exp Result.
        /// </summary>
        TranscendenceReceiveExpResult = 16039,

        /// <summary>
        /// Item Recharge NPC Result.
        /// </summary>
        TimeChargeResult = 16042,
    }
    
    
}
