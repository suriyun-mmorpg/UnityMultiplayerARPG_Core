using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
    {
        public async UniTaskVoid HandleRequestDepositGuildGold(RequestHandlerData requestHandler, RequestDepositGuildGoldMessage request, RequestProceedResultDelegate<ResponseDepositGuildGoldMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            playerCharacter.Gold -= request.gold;
            guild.gold += request.gold;
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.Invoke(AckResponseCode.Success, new ResponseDepositGuildGoldMessage());
        }

        public async UniTaskVoid HandleRequestDepositUserGold(RequestHandlerData requestHandler, RequestDepositUserGoldMessage request, RequestProceedResultDelegate<ResponseDepositUserGoldMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            playerCharacter.Gold -= request.gold;
            playerCharacter.UserGold = playerCharacter.UserGold.Increase(request.gold);
        }

        public async UniTaskVoid HandleRequestWithdrawGuildGold(RequestHandlerData requestHandler, RequestWithdrawGuildGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawGuildGoldMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            if (guild.gold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            guild.gold -= request.gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawGuildGoldMessage());
        }

        public async UniTaskVoid HandleRequestWithdrawUserGold(RequestHandlerData requestHandler, RequestWithdrawUserGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawUserGoldMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.UserGold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            playerCharacter.UserGold -= request.gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawUserGoldMessage());
        }
    }
}
