using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public static bool EnhanceSocketRightHandItem(IPlayerCharacterData character, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
            return EnhanceSocketItem(character, character.EquipWeapons.rightHand, enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool EnhanceSocketLeftHandItem(IPlayerCharacterData character, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
            return EnhanceSocketItem(character, character.EquipWeapons.leftHand, enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool EnhanceSocketEquipItem(IPlayerCharacterData character, int index, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
            return EnhanceSocketItemByList(character, character.EquipItems, index, enhancerId, socketIndex, out gameMessage);
        }

        public static bool EnhanceSocketNonEquipItem(IPlayerCharacterData character, int index, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
            return EnhanceSocketItemByList(character, character.NonEquipItems, index, enhancerId, socketIndex, out gameMessage);
        }

        private static bool EnhanceSocketItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, int enhancerId, int socketIndex, out UITextKeys gameMessage)
        {
            return EnhanceSocketItem(character, list[index], enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                list[index] = enhancedSocketItem;
            }, out gameMessage);
        }

        private static bool EnhanceSocketItem(IPlayerCharacterData character, CharacterItem enhancingItem, int enhancerId, int socketIndex, System.Action<CharacterItem> onEnhanceSocket, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (enhancingItem.IsEmptySlot())
            {
                // Cannot enhance socket because character item is empty
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            IEquipmentItem equipmentItem = enhancingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot enhance socket because it's not equipment item
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_EQUIPMENT;
                return false;
            }
            byte maxSocket = GameInstance.Singleton.GameplayRule.GetItemMaxSocket(character, enhancingItem);
            if (maxSocket <= 0)
            {
                // Cannot enhance socket because equipment has no socket(s)
                gameMessage = UITextKeys.UI_ERROR_NO_EMPTY_SOCKET;
                return false;
            }
            while (enhancingItem.Sockets.Count < maxSocket)
            {
                // Add empty slots
                enhancingItem.Sockets.Add(0);
            }
            if (socketIndex >= 0)
            {
                // Put enhancer to target socket
                if (socketIndex >= enhancingItem.Sockets.Count || enhancingItem.Sockets[socketIndex] != 0)
                {
                    gameMessage = UITextKeys.UI_ERROR_SOCKET_NOT_EMPTY;
                    return false;
                }
            }
            else
            {
                // Put enhancer to any empty socket
                for (int index = 0; index < enhancingItem.Sockets.Count; ++index)
                {
                    if (enhancingItem.Sockets[index] == 0)
                    {
                        socketIndex = index;
                        break;
                    }
                    if (index == enhancingItem.Sockets.Count - 1)
                    {
                        gameMessage = UITextKeys.UI_ERROR_NO_EMPTY_SOCKET;
                        return false;
                    }
                }
            }
            BaseItem enhancerItem;
            if (!GameInstance.Items.TryGetValue(enhancerId, out enhancerItem) || !enhancerItem.IsSocketEnhancer())
            {
                // Cannot enhance socket because enhancer id is invalid
                gameMessage = UITextKeys.UI_ERROR_CANNOT_ENHANCE_SOCKET;
                return false;
            }
            if (!character.HasOneInNonEquipItems(enhancerId))
            {
                // Cannot enhance socket because there is no item
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_SOCKET_ENCHANER;
                return false;
            }
            character.DecreaseItems(enhancerId, 1);
            character.FillEmptySlots();
            enhancingItem.Sockets[socketIndex] = enhancerId;
            onEnhanceSocket.Invoke(enhancingItem);
            return true;
        }

        public static bool RemoveEnhancerFromRightHandItem(IPlayerCharacterData character, int socketIndex, bool returnEnhancer, out UITextKeys gameMessage)
        {
            return RemoveEnhancerFromItem(character, character.EquipWeapons.rightHand, socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool RemoveEnhancerFromLeftHandItem(IPlayerCharacterData character, int socketIndex, bool returnEnhancer, out UITextKeys gameMessage)
        {
            return RemoveEnhancerFromItem(character, character.EquipWeapons.leftHand, socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessage);
        }

        public static bool RemoveEnhancerFromEquipItem(IPlayerCharacterData character, int index, int socketIndex, bool returnEnhancer, out UITextKeys gameMessage)
        {
            return RemoveEnhancerFromItemByList(character, character.EquipItems, index, socketIndex, returnEnhancer, out gameMessage);
        }

        public static bool RemoveEnhancerFromNonEquipItem(IPlayerCharacterData character, int index, int socketIndex, bool returnEnhancer, out UITextKeys gameMessage)
        {
            return RemoveEnhancerFromItemByList(character, character.NonEquipItems, index, socketIndex, returnEnhancer, out gameMessage);
        }

        private static bool RemoveEnhancerFromItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, int socketIndex, bool returnEnhancer, out UITextKeys gameMessage)
        {
            return RemoveEnhancerFromItem(character, list[index], socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                list[index] = enhancedSocketItem;
            }, out gameMessage);
        }

        private static bool RemoveEnhancerFromItem(IPlayerCharacterData character, CharacterItem enhancedItem, int socketIndex, bool returnEnhancer, System.Action<CharacterItem> onRemoveEnhancer, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (enhancedItem.IsEmptySlot())
            {
                gameMessage = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                return false;
            }
            if (enhancedItem.Sockets.Count == 0 || socketIndex >= enhancedItem.Sockets.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ENHANCER_ITEM_INDEX;
                return false;
            }
            if (enhancedItem.Sockets[socketIndex] == 0)
            {
                gameMessage = UITextKeys.UI_ERROR_NO_ENHANCER;
                return false;
            }
            int enhancerId = enhancedItem.Sockets[socketIndex];
            if (returnEnhancer)
            {
                if (character.IncreasingItemsWillOverwhelming(enhancerId, 1))
                {
                    gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                    return false;
                }
                character.IncreaseItems(CharacterItem.Create(enhancerId));
                character.FillEmptySlots();
            }
            enhancedItem.Sockets[socketIndex] = 0;
            onRemoveEnhancer.Invoke(enhancedItem);
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenRemoveEnhancer(character);
            return true;
        }
    }
}