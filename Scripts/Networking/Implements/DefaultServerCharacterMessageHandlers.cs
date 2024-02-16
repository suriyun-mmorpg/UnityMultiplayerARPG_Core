using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerCharacterMessageHandlers : MonoBehaviour, IServerCharacterMessageHandlers
    {
        public UniTaskVoid HandleRequestIncreaseAttributeAmount(RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request, RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!playerCharacter.AddAttribute(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = gameMessage,
                });
                return default;
            }
            playerCharacter.StatPoint -= 1;
            result.InvokeSuccess(new ResponseIncreaseAttributeAmountMessage());
            return default;
        }

        public UniTaskVoid HandleRequestIncreaseSkillLevel(RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!playerCharacter.AddSkill(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = gameMessage,
                });
                return default;
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
            return default;
        }

        public UniTaskVoid HandleRequestRespawn(RequestHandlerData requestHandler, RequestRespawnMessage request, RequestProceedResultDelegate<ResponseRespawnMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (playerCharacter.CurrentHp > 0)
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_DEAD,
                });
                return default;
            }
            GameInstance.ServerCharacterHandlers.Respawn(request.option, playerCharacter);
            result.InvokeSuccess(new ResponseRespawnMessage());
            return default;
        }

        public UniTaskVoid HandleRequestSetIcon(RequestHandlerData requestHandler, RequestSetIconMessage request, RequestProceedResultDelegate<ResponseSetIconMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerIcons.TryGetValue(request.dataId, out PlayerIcon data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetIconMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return default;
            }
            playerCharacter.IconDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetIconMessage()
            {
                dataId = request.dataId,
            });
            return default;
        }

        public UniTaskVoid HandleRequestSetFrame(RequestHandlerData requestHandler, RequestSetFrameMessage request, RequestProceedResultDelegate<ResponseSetFrameMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerFrames.TryGetValue(request.dataId, out PlayerFrame data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetFrameMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return default;
            }
            playerCharacter.FrameDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetFrameMessage()
            {
                dataId = request.dataId,
            });
            return default;
        }

        public UniTaskVoid HandleRequestSetTitle(RequestHandlerData requestHandler, RequestSetTitleMessage request, RequestProceedResultDelegate<ResponseSetTitleMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            // TODO: Implement data unlocking
            if (!GameInstance.PlayerTitles.TryGetValue(request.dataId, out PlayerTitle data) || data.IsLocked)
            {
                result.InvokeError(new ResponseSetTitleMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return default;
            }
            playerCharacter.TitleDataId = request.dataId;
            result.InvokeSuccess(new ResponseSetTitleMessage()
            {
                dataId = request.dataId,
            });
            return default;
        }
    }
}
