namespace MultiplayerARPG
{
    public partial class GameNetworkingConsts
    {
        // Networking messages
        public const ushort GameMessage = 100;
        public const ushort Warp = 101;
        public const ushort Chat = 102;
        public const ushort UpdatePartyMember = 103;
        public const ushort UpdateParty = 104;
        public const ushort UpdateGuildMember = 105;
        public const ushort UpdateGuild = 106;
        public const ushort UpdateFriends = 107;
        public const ushort UpdateMapInfo = 108;
        public const ushort NotifyOnlineCharacter = 109;
        public const ushort NotifyRewardExp = 110;
        public const ushort NotifyRewardGold = 111;
        public const ushort NotifyRewardItem = 112;
        public const ushort UpdateTimeOfDay = 113;
        public const ushort NotifyStorageOpened = 114;
        public const ushort NotifyStorageClosed = 115;
        public const ushort NotifyStorageItemsUpdated = 116;
        public const ushort NotifyPartyInvitation = 117;
        public const ushort NotifyGuildInvitation = 118;
        // Networking requests/responses
        public const ushort CashShopInfo = 100;
        public const ushort CashShopBuy = 101;
        public const ushort CashPackageInfo = 102;
        public const ushort CashPackageBuyValidation = 103;
        public const ushort MailList = 104;
        public const ushort ReadMail = 105;
        public const ushort ClaimMailItems = 106;
        public const ushort DeleteMail = 107;
        public const ushort SendMail = 108;
        public const ushort MoveItemFromStorage = 109;
        public const ushort MoveItemToStorage = 110;
        public const ushort SwapOrMergeStorageItem = 111;
        public const ushort SwapOrMergeItem = 112;
        public const ushort EquipWeapon = 113;
        public const ushort EquipArmor = 114;
        public const ushort UnEquipWeapon = 115;
        public const ushort UnEquipArmor = 116;
        public const ushort CreateParty = 117;
        public const ushort ChangePartyLeader = 118;
        public const ushort ChangePartySetting = 119;
        public const ushort SendPartyInvitation = 120;
        public const ushort AcceptPartyInvitation = 121;
        public const ushort DeclinePartyInvitation = 122;
        public const ushort KickMemberFromParty = 123;
        public const ushort LeaveParty = 124;
        public const ushort CreateGuild = 125;
        public const ushort ChangeGuildLeader = 126;
        public const ushort ChangeGuildMessage = 127;
        public const ushort ChangeGuildRole = 128;
        public const ushort ChangeMemberGuildRole = 129;
        public const ushort SendGuildInvitation = 130;
        public const ushort AcceptGuildInvitation = 131;
        public const ushort DeclineGuildInvitation = 132;
        public const ushort KickMemberFromGuild = 133;
        public const ushort LeaveGuild = 134;
        public const ushort IncreaseGuildSkillLevel = 135;
        public const ushort FindCharacters = 136;
        public const ushort GetFriends = 137;
        public const ushort AddFriend = 138;
        public const ushort RemoveFriend = 139;
        public const ushort DepositUserGold = 140;
        public const ushort WithdrawUserGold = 141;
        public const ushort DepositGuildGold = 142;
        public const ushort WithdrawGuildGold = 143;
        public const ushort OpenStorage = 144;
        public const ushort CloseStorage = 145;
        public const ushort SwitchEquipWeaponSet = 146;
        public const ushort IncreaseAttributeAmount = 147;
        public const ushort IncreaseSkillLevel = 148;
        public const ushort DismantleItem = 149;
        public const ushort DismantleItems = 150;
        public const ushort EnhanceSocketItem = 151;
        public const ushort RefineItem = 152;
        public const ushort RemoveEnhancerFromItem = 153;
        public const ushort RepairItem = 154;
        public const ushort RepairEquipItems = 155;
        public const ushort SellItem = 156;
        public const ushort SellItems = 157;
        public const ushort SendFriendRequest = 158;
        public const ushort AcceptFriendRequest = 159;
        public const ushort DeclineFriendRequest = 160;
        public const ushort SendGuildRequest = 161;
        public const ushort AcceptGuildRequest = 162;
        public const ushort DeclineGuildRequest = 163;
        // Entity movement
        public const ushort MovementInput = 165;
        public const ushort SyncTransform = 167;
        public const ushort Teleport = 168;
        public const ushort StopMove = 169;
        public const ushort Jump = 170;
        // New notify type
        public const ushort NotifyRewardCurrency = 171;
        // 1.63c
        public const ushort Respawn = 172;
        // 1.64
        public const ushort MailNotification = 174;
        public const ushort GetFriendRequests = 175;
        public const ushort GetGuildRequests = 176;
        public const ushort FindGuilds = 177;
        // 1.67
        public const ushort ChangeGuildMessage2 = 178;
        public const ushort ChangeGuildOptions = 179;
        public const ushort ChangeGuildAutoAcceptRequests = 184;
        public const ushort ClaimAllMailsItems = 185;
        public const ushort DeleteAllMails = 186;
        // Gacha
        public const ushort GachaInfo = 187;
        public const ushort OpenGacha = 188;
        // 1.73
        public const ushort GetGuildInfo = 189;
        // 1.74
        public const ushort HitRegistration = 190;
    }
}
