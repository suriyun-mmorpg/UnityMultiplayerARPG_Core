using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public static void EnhanceSocketRightHandItem(IPlayerCharacterData character, int enhancerId, int socketIndex, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, character.EquipWeapons.rightHand, enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void EnhanceSocketLeftHandItem(IPlayerCharacterData character, int enhancerId, int socketIndex, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, character.EquipWeapons.leftHand, enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void EnhanceSocketEquipItem(IPlayerCharacterData character, int index, int enhancerId, int socketIndex, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItemByList(character, character.EquipItems, index, enhancerId, socketIndex, out gameMessageType);
        }

        public static void EnhanceSocketNonEquipItem(IPlayerCharacterData character, int index, int enhancerId, int socketIndex, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItemByList(character, character.NonEquipItems, index, enhancerId, socketIndex, out gameMessageType);
        }

        private static void EnhanceSocketItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, int enhancerId, int socketIndex, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, list[index], enhancerId, socketIndex, (enhancedSocketItem) =>
            {
                list[index] = enhancedSocketItem;
            }, out gameMessageType);
        }

        private static void EnhanceSocketItem(IPlayerCharacterData character, CharacterItem enhancingItem, int enhancerId, int socketIndex, System.Action<CharacterItem> onEnhanceSocket, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (enhancingItem.IsEmptySlot())
            {
                // Cannot enhance socket because character item is empty
                gameMessageType = GameMessage.Type.CannotEnhanceSocket;
                return;
            }
            IEquipmentItem equipmentItem = enhancingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot enhance socket because it's not equipment item
                gameMessageType = GameMessage.Type.CannotEnhanceSocket;
                return;
            }
            byte maxSocket = GameInstance.Singleton.GameplayRule.GetItemMaxSocket(character, enhancingItem);
            if (maxSocket <= 0)
            {
                // Cannot enhance socket because equipment has no socket(s)
                gameMessageType = GameMessage.Type.NoEmptySocket;
                return;
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
                    gameMessageType = GameMessage.Type.SocketNotEmpty;
                    return;
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
                        gameMessageType = GameMessage.Type.NoEmptySocket;
                        return;
                    }
                }
            }
            BaseItem enhancerItem;
            if (!GameInstance.Items.TryGetValue(enhancerId, out enhancerItem) || !enhancerItem.IsSocketEnhancer())
            {
                // Cannot enhance socket because enhancer id is invalid
                gameMessageType = GameMessage.Type.CannotEnhanceSocket;
                return;
            }
            if (!character.HasOneInNonEquipItems(enhancerId))
            {
                // Cannot enhance socket because there is no item
                gameMessageType = GameMessage.Type.NotEnoughSocketEnchaner;
                return;
            }
            character.DecreaseItems(enhancerId, 1, GameInstance.Singleton.IsLimitInventorySlot);
            character.FillEmptySlots();
            enhancingItem.Sockets[socketIndex] = enhancerId;
            onEnhanceSocket.Invoke(enhancingItem);
        }

        public static void RemoveEnhancerFromRightHandItem(IPlayerCharacterData character, int socketIndex, bool returnEnhancer, out GameMessage.Type gameMessageType)
        {
            RemoveEnhancerFromItem(character, character.EquipWeapons.rightHand, socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RemoveEnhancerFromLeftHandItem(IPlayerCharacterData character, int socketIndex, bool returnEnhancer, out GameMessage.Type gameMessageType)
        {
            RemoveEnhancerFromItem(character, character.EquipWeapons.leftHand, socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void RemoveEnhancerFromEquipItem(IPlayerCharacterData character, int index, int socketIndex, bool returnEnhancer, out GameMessage.Type gameMessageType)
        {
            RemoveEnhancerFromItemByList(character, character.EquipItems, index, socketIndex, returnEnhancer, out gameMessageType);
        }

        public static void RemoveEnhancerFromNonEquipItem(IPlayerCharacterData character, int index, int socketIndex, bool returnEnhancer, out GameMessage.Type gameMessageType)
        {
            RemoveEnhancerFromItemByList(character, character.NonEquipItems, index, socketIndex, returnEnhancer, out gameMessageType);
        }

        private static void RemoveEnhancerFromItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, int socketIndex, bool returnEnhancer, out GameMessage.Type gameMessageType)
        {
            RemoveEnhancerFromItem(character, list[index], socketIndex, returnEnhancer, (enhancedSocketItem) =>
            {
                list[index] = enhancedSocketItem;
            }, out gameMessageType);
        }

        private static void RemoveEnhancerFromItem(IPlayerCharacterData character, CharacterItem enhancedItem, int socketIndex, bool returnEnhancer, System.Action<CharacterItem> onRemoveEnhancer, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (enhancedItem.IsEmptySlot())
            {
                gameMessageType = GameMessage.Type.CannotRemoveEnhancer;
                return;
            }
            if (enhancedItem.Sockets.Count == 0 || socketIndex >= enhancedItem.Sockets.Count)
            {
                gameMessageType = GameMessage.Type.CannotRemoveEnhancer;
                return;
            }
            if (enhancedItem.Sockets[socketIndex] == 0)
            {
                gameMessageType = GameMessage.Type.NoEnhancer;
                return;
            }
            int enhancerId = enhancedItem.Sockets[socketIndex];
            if (returnEnhancer)
            {
                if (character.IncreasingItemsWillOverwhelming(enhancerId, 1))
                {
                    gameMessageType = GameMessage.Type.CannotCarryAnymore;
                    return;
                }
                character.IncreaseItems(CharacterItem.Create(enhancerId));
                character.FillEmptySlots();
            }
            enhancedItem.Sockets[socketIndex] = 0;
            onRemoveEnhancer.Invoke(enhancedItem);
        }
    }
}