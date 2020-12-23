using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
    {
        public async UniTaskVoid HandleRequestDepositGuildGold(RequestHandlerData requestHandler, RequestDepositGuildGoldMessage request, RequestProceedResultDelegate<ResponseDepositGuildGoldMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.GuildNotFound,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.GoldNotEnough,
                });
                return;
            }
            playerCharacter.Gold -= request.gold;
            guild.gold += request.gold;
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            BaseGameNetworkManager.Singleton.SendSetGuildGoldToClients(guild);
            result.Invoke(AckResponseCode.Success, new ResponseDepositGuildGoldMessage());
        }

        public async UniTaskVoid HandleRequestDepositUserGold(RequestHandlerData requestHandler, RequestDepositUserGoldMessage request, RequestProceedResultDelegate<ResponseDepositUserGoldMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    error = ResponseDepositUserGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    error = ResponseDepositUserGoldMessage.Error.GoldNotEnough,
                });
                return;
            }
            playerCharacter.Gold -= request.gold;
            playerCharacter.UserGold = playerCharacter.UserGold.Increase(request.gold);
        }

        public async UniTaskVoid HandleRequestWithdrawGuildGold(RequestHandlerData requestHandler, RequestWithdrawGuildGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawGuildGoldMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.GuildNotFound,
                });
                return;
            }
            if (guild.gold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.GoldNotEnough,
                });
                return;
            }
            guild.gold -= request.gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            BaseGameNetworkManager.Singleton.SendSetGuildGoldToClients(guild);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawGuildGoldMessage());
        }

        public async UniTaskVoid HandleRequestWithdrawUserGold(RequestHandlerData requestHandler, RequestWithdrawUserGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawUserGoldMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    error = ResponseWithdrawUserGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (playerCharacter.UserGold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    error = ResponseWithdrawUserGoldMessage.Error.GoldNotEnough,
                });
                return;
            }
            playerCharacter.UserGold -= request.gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawUserGoldMessage());
        }
    }
}
