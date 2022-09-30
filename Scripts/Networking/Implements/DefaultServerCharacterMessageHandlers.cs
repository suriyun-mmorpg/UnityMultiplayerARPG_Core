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
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddAttribute(out gameMessage, request.dataId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseAttributeAmountMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            playerCharacter.StatPoint -= 1;
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseAttributeAmountMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestIncreaseSkillLevel(RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddSkill(out gameMessage, request.dataId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseSkillLevelMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            int indexOfSkill = playerCharacter.IndexOfSkill(request.dataId);
            CharacterSkill characterSkill = playerCharacter.Skills[indexOfSkill];
            BaseSkill skill = characterSkill.GetSkill();
            short learnLevel = (short)(characterSkill.level - 1);
            float requireSkillPoint = skill.GetRequireCharacterSkillPoint(learnLevel);
            int requireGold = skill.GetRequireCharacterGold(learnLevel);
            Dictionary<Currency, int> requireCurrencies = skill.GetRequireCurrencyAmounts(learnLevel);
            Dictionary<BaseItem, short> requireItems = skill.GetRequireItemAmounts(learnLevel);
            playerCharacter.SkillPoint -= requireSkillPoint;
            playerCharacter.Gold -= requireGold;
            playerCharacter.DecreaseCurrencies(requireCurrencies);
            playerCharacter.DecreaseItems(requireItems);
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseSkillLevelMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestRespawn(RequestHandlerData requestHandler, RequestRespawnMessage request, RequestProceedResultDelegate<ResponseRespawnMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.CurrentHp > 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_DEAD,
                });
                return;
            }
            GameInstance.ServerCharacterHandlers.Respawn(request.option, playerCharacter);
            result.Invoke(AckResponseCode.Success, new ResponseRespawnMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestAvailableIcons(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableIconsMessage> result)
        {
            // TODO: Implement data unlocking
            List<int> iconIds = new List<int>();
            foreach (PlayerIcon icon in GameInstance.PlayerIcons.Values)
            {
                if (!icon.isLocked)
                    iconIds.Add(icon.DataId);
            }
            result.Invoke(AckResponseCode.Success, new ResponseAvailableIconsMessage()
            {
                iconIds = iconIds.ToArray(),
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestAvailableFrames(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableFramesMessage> result)
        {
            // TODO: Implement data unlocking
            List<int> frameIds = new List<int>();
            foreach (PlayerFrame frame in GameInstance.PlayerFrames.Values)
            {
                if (!frame.isLocked)
                    frameIds.Add(frame.DataId);
            }
            result.Invoke(AckResponseCode.Success, new ResponseAvailableFramesMessage()
            {
                frameIds = frameIds.ToArray(),
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestAvailableTitles(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseAvailableTitlesMessage> result)
        {
            // TODO: Implement data unlocking
            List<int> titleIds = new List<int>();
            foreach (PlayerTitle title in GameInstance.PlayerTitles.Values)
            {
                if (!title.isLocked)
                    titleIds.Add(title.DataId);
            }
            result.Invoke(AckResponseCode.Success, new ResponseAvailableTitlesMessage()
            {
                titleIds = titleIds.ToArray(),
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestSetIcon(RequestHandlerData requestHandler, RequestSetIconMessage request, RequestProceedResultDelegate<ResponseSetIconMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerIcons.TryGetValue(request.dataId, out PlayerIcon data) || data.isLocked)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.IconDataId = request.dataId;
            result.Invoke(AckResponseCode.Success, new ResponseSetIconMessage()
            {
                dataId = request.dataId,
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestSetFrame(RequestHandlerData requestHandler, RequestSetFrameMessage request, RequestProceedResultDelegate<ResponseSetFrameMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerFrames.TryGetValue(request.dataId, out PlayerFrame data) || data.isLocked)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.FrameDataId = request.dataId;
            result.Invoke(AckResponseCode.Success, new ResponseSetFrameMessage()
            {
                dataId = request.dataId,
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid RequestSetTitle(RequestHandlerData requestHandler, RequestSetTitleMessage request, RequestProceedResultDelegate<ResponseSetTitleMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerTitles.TryGetValue(request.dataId, out PlayerTitle data) || data.isLocked)
            {
                result.Invoke(AckResponseCode.Error, new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            playerCharacter.TitleDataId = request.dataId;
            result.Invoke(AckResponseCode.Success, new ResponseSetTitleMessage()
            {
                dataId = request.dataId,
            });
            await UniTask.Yield();
        }
    }
}
