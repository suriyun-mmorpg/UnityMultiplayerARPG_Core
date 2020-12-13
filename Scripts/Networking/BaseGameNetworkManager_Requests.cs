using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        #region Cash shop requests
        public bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashShopBuy, data, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageBuyValidation, data, responseDelegate: callback);
        }
        #endregion

        #region Mail requests
        public bool RequestMailList(RequestMailListMessage data, ResponseDelegate<ResponseMailListMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MailList, data, responseDelegate: callback);
        }

        public bool RequestReadMail(RequestReadMailMessage data, ResponseDelegate<ResponseReadMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ReadMail, data, responseDelegate: callback);
        }

        public bool RequestClaimMailItems(RequestClaimMailItemsMessage data, ResponseDelegate<ResponseClaimMailItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ClaimMailItems, data, responseDelegate: callback);
        }

        public bool RequestDeleteMail(RequestDeleteMailMessage data, ResponseDelegate<ResponseDeleteMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.DeleteMail, data, responseDelegate: callback);
        }

        public bool RequestSendMail(RequestSendMailMessage data, ResponseDelegate<ResponseSendMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SendMail, data, responseDelegate: callback);
        }
        #endregion

        #region Storage requests
        public bool RequestGetStorageItems(RequestGetStorageItemsMessage data, ResponseDelegate<ResponseGetStorageItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.GetStorageItems, data, responseDelegate: callback);
        }

        public bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemFromStorage, data, responseDelegate: callback);
        }

        public bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemToStorage, data, responseDelegate: callback);
        }

        public bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SwapOrMergeStorageItem, data, responseDelegate: callback);
        }
        #endregion

        #region Inventory requests
        public bool RequestSwapOrMergeItem(RequestSwapOrMergeItemMessage data, ResponseDelegate<ResponseSwapOrMergeItemMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SwapOrMergeItem, data, responseDelegate: callback);
        }

        public bool RequestEquipWeapon(RequestEquipWeaponMessage data, ResponseDelegate<ResponseEquipWeaponMessage> callback)
        {
            return ClientSendRequest(ReqTypes.EquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestEquipArmor(RequestEquipArmorMessage data, ResponseDelegate<ResponseEquipArmorMessage> callback)
        {
            return ClientSendRequest(ReqTypes.EquipArmor, data, responseDelegate: callback);
        }

        public bool RequestUnEquipWeapon(RequestUnEquipWeaponMessage data, ResponseDelegate<ResponseUnEquipWeaponMessage> callback)
        {
            return ClientSendRequest(ReqTypes.UnEquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestUnEquipArmor(RequestUnEquipArmorMessage data, ResponseDelegate<ResponseUnEquipArmorMessage> callback)
        {
            return ClientSendRequest(ReqTypes.UnEquipArmor, data, responseDelegate: callback);
        }
        #endregion

        #region Party
        public bool RequestCreateParty(RequestCreatePartyMessage data, ResponseDelegate<ResponseCreatePartyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CreateParty, data, responseDelegate: callback);
        }

        public bool RequestChangePartyLeader(RequestChangePartyLeaderMessage data, ResponseDelegate<ResponseChangePartyLeaderMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangePartyLeader, data, responseDelegate: callback);
        }

        public bool RequestChangePartySetting(RequestChangePartySettingMessage data, ResponseDelegate<ResponseChangePartySettingMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangePartySetting, data, responseDelegate: callback);
        }

        public bool RequestSendPartyInvitation(RequestSendPartyInvitationMessage data, ResponseDelegate<ResponseSendPartyInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SendPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptPartyInvitation(RequestAcceptPartyInvitationMessage data, ResponseDelegate<ResponseAcceptPartyInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.AcceptPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclinePartyInvitation(RequestDeclinePartyInvitationMessage data, ResponseDelegate<ResponseDeclinePartyInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.DeclinePartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromParty(RequestKickMemberFromPartyMessage data, ResponseDelegate<ResponseKickMemberFromPartyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.KickMemberFromParty, data, responseDelegate: callback);
        }

        public bool RequestLeaveParty(RequestLeavePartyMessage data, ResponseDelegate<ResponseLeavePartyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.LeaveParty, data, responseDelegate: callback);
        }
        #endregion

        #region Guild
        public bool RequestCreateGuild(RequestCreateGuildMessage data, ResponseDelegate<ResponseCreateGuildMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CreateGuild, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildLeader(RequestChangeGuildLeaderMessage data, ResponseDelegate<ResponseChangeGuildLeaderMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangeGuildLeader, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildMessage(RequestChangeGuildMessageMessage data, ResponseDelegate<ResponseChangeGuildMessageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangeGuildMessage, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildRole(RequestChangeGuildRoleMessage data, ResponseDelegate<ResponseChangeGuildRoleMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangeGuildRole, data, responseDelegate: callback);
        }

        public bool RequestChangeMemberGuildRole(RequestChangeMemberGuildRoleMessage data, ResponseDelegate<ResponseChangeMemberGuildRoleMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ChangeMemberGuildRole, data, responseDelegate: callback);
        }

        public bool RequestSendGuildInvitation(RequestSendGuildInvitationMessage data, ResponseDelegate<ResponseSendGuildInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SendGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptGuildInvitation(RequestAcceptGuildInvitationMessage data, ResponseDelegate<ResponseAcceptGuildInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.AcceptGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclineGuildInvitation(RequestDeclineGuildInvitationMessage data, ResponseDelegate<ResponseDeclineGuildInvitationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.DeclineGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromGuild(RequestKickMemberFromGuildMessage data, ResponseDelegate<ResponseKickMemberFromGuildMessage> callback)
        {
            return ClientSendRequest(ReqTypes.KickMemberFromGuild, data, responseDelegate: callback);
        }

        public bool RequestLeaveGuild(RequestLeaveGuildMessage data, ResponseDelegate<ResponseLeaveGuildMessage> callback)
        {
            return ClientSendRequest(ReqTypes.LeaveGuild, data, responseDelegate: callback);
        }

        public bool RequestIncreaseGuildSkillLevel(RequestIncreaseGuildSkillLevelMessage data, ResponseDelegate<ResponseIncreaseGuildSkillLevelMessage> callback)
        {
            return ClientSendRequest(ReqTypes.IncreaseGuildSkillLevel, data, responseDelegate: callback);
        }
        #endregion
    }
}
