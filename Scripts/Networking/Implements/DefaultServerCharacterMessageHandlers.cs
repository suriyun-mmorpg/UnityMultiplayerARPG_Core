using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerCharacterMessageHandlers : MonoBehaviour, IServerCharacterMessageHandlers
    {
        public async UniTaskVoid HandleRequestIncreaseAttributeAmount(RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request, RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!playerCharacter.AddAttribute(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            playerCharacter.StatPoint -= 1;
            result.InvokeSuccess(new ResponseIncreaseAttributeAmountMessage());
        }

        public async UniTaskVoid HandleRequestIncreaseSkillLevel(RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!playerCharacter.AddSkill(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            int indexOfSkill = playerCharacter.IndexOfSkill(request.dataId);
            CharacterSkill characterSkill = playerCharacter.Skills[indexOfSkill];
            BaseSkill skill = characterSkill.GetSkill();
            int learnLevel = characterSkill.level - 1;
            float requireSkillPoint = skill.GetRequireCharacterSkillPoint(learnLevel);
            int requireGold = skill.GetRequireCharacterGold(learnLevel);
            Dictionary<Currency, int> requireCurrencies = skill.GetRequireCurrencyAmounts(learnLevel);
            Dictionary<BaseItem, int> requireItems = skill.GetRequireItemAmounts(learnLevel);
            playerCharacter.SkillPoint -= requireSkillPoint;
            playerCharacter.Gold -= requireGold;
            playerCharacter.DecreaseCurrencies(requireCurrencies);
            playerCharacter.DecreaseItems(requireItems);
            result.InvokeSuccess(new ResponseIncreaseSkillLevelMessage());
        }

        public async UniTaskVoid HandleRequestRespawn(RequestHandlerData requestHandler, RequestRespawnMessage request, RequestProceedResultDelegate<ResponseRespawnMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp > 0)
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_DEAD,
                });
                return;
            }
            GameInstance.ServerCharacterHandlers.Respawn(request.option, playerCharacter);
            result.InvokeSuccess(new ResponseRespawnMessage());
        }

        public async UniTaskVoid HandleRequestAvailableIcons(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableIconsMessage> result)
        {
            await UniTask.Yield();
            // TODO: Implement data unlocking
            List<int> iconIds = new List<int>();
            foreach (PlayerIcon icon in GameInstance.PlayerIcons.Values)
            {
                if (!icon.IsLocked)
                    iconIds.Add(icon.DataId);
            }
            result.InvokeSuccess(new ResponseAvailableIconsMessage()
            {
                iconIds = iconIds.ToArray(),
            });
        }

        public async UniTaskVoid HandleRequestAvailableFrames(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableFramesMessage> result)
        {
            await UniTask.Yield();
            // TODO: Implement data unlocking
            List<int> frameIds = new List<int>();
            foreach (PlayerFrame frame in GameInstance.PlayerFrames.Values)
            {
                if (!frame.IsLocked)
                    frameIds.Add(frame.DataId);
            }
            result.InvokeSuccess(new ResponseAvailableFramesMessage()
            {
                frameIds = frameIds.ToArray(),
            });
        }

        public async UniTaskVoid HandleRequestAvailableTitles(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableTitlesMessage> result)
        {
            await UniTask.Yield();
            // TODO: Implement data unlocking
            List<int> titleIds = new List<int>();
            foreach (PlayerTitle title in GameInstance.PlayerTitles.Values)
            {
                if (!title.IsLocked)
                    titleIds.Add(title.DataId);
            }
            result.InvokeSuccess(new ResponseAvailableTitlesMessage()
            {
                titleIds = titleIds.ToArray(),
            });
        }

        public async UniTaskVoid HandleRequestSetIcon(RequestHandlerData requestHandler, RequestSetIconMessage request, RequestProceedResultDelegate<ResponseSetIconMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerIcons.TryGetValue(request.dataId, out PlayerIcon data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.IconDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetIconMessage()
            {
                dataId = request.dataId,
            });
        }

        public async UniTaskVoid HandleRequestSetFrame(RequestHandlerData requestHandler, RequestSetFrameMessage request, RequestProceedResultDelegate<ResponseSetFrameMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerFrames.TryGetValue(request.dataId, out PlayerFrame data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.FrameDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetFrameMessage()
            {
                dataId = request.dataId,
            });
        }

        public async UniTaskVoid HandleRequestSetTitle(RequestHandlerData requestHandler, RequestSetTitleMessage request, RequestProceedResultDelegate<ResponseSetTitleMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerTitles.TryGetValue(request.dataId, out PlayerTitle data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.TitleDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetTitleMessage()
            {
                dataId = request.dataId,
            });
        }
    }
}
