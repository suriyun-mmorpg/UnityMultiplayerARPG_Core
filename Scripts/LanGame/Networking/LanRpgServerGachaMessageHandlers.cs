using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerGachaMessageHandlers : MonoBehaviour, IServerGachaMessageHandlers
    {
        public async UniTaskVoid HandleRequestGachaInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseGachaInfoMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGachaInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseGachaInfoMessage()
            {
                cash = playerCharacter.UserCash,
                gachaIds = new List<int>(GameInstance.Gachas.Keys),
            });

            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestOpenGacha(RequestHandlerData requestHandler, RequestOpenGachaMessage request, RequestProceedResultDelegate<ResponseOpenGachaMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            Gacha gacha;
            if (!GameInstance.Gachas.TryGetValue(request.dataId, out gacha))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }

            int price = request.openMode == GachaOpenMode.Multiple ? gacha.MultipleModeOpenPrice : gacha.SingleModeOpenPrice;
            int cash = playerCharacter.UserCash;
            if (cash < price)
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH,
                });
                return;
            }

            int openCount = request.openMode == GachaOpenMode.Multiple ? gacha.MultipleModeOpenCount : 1;
            List<ItemAmount> itemAmounts = gacha.GetRandomedItems(openCount);
            if (playerCharacter.IncreasingItemsWillOverwhelming(itemAmounts))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_WILL_OVERWHELMING,
                });
                return;
            }
            // Decrease cash amount
            cash -= price;
            playerCharacter.UserCash = cash;
            // Increase character items
            playerCharacter.IncreaseItems(itemAmounts);
            playerCharacter.FillEmptySlots();
            // Send response message
            result.Invoke(AckResponseCode.Success, new ResponseOpenGachaMessage());
        }
    }
}
