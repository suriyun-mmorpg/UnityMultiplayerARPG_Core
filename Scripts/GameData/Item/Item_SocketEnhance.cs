using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class Item
    {
        public static void EnhanceSocketRightHandItem(IPlayerCharacterData character, int enhancerId, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, character.EquipWeapons.rightHand, enhancerId, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.rightHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void EnhanceSocketLeftHandItem(IPlayerCharacterData character, int enhancerId, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, character.EquipWeapons.leftHand, enhancerId, (enhancedSocketItem) =>
            {
                EquipWeapons equipWeapon = character.EquipWeapons;
                equipWeapon.leftHand = enhancedSocketItem;
                character.EquipWeapons = equipWeapon;
            }, out gameMessageType);
        }

        public static void EnhanceSocketEquipItem(IPlayerCharacterData character, int index, int enhancerId, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItemByList(character, character.EquipItems, index, enhancerId, out gameMessageType);
        }

        public static void EnhanceSocketNonEquipItem(IPlayerCharacterData character, int index, int enhancerId, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItemByList(character, character.NonEquipItems, index, enhancerId, out gameMessageType);
        }

        private static void EnhanceSocketItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, int enhancerId, out GameMessage.Type gameMessageType)
        {
            EnhanceSocketItem(character, list[index], enhancerId, (enhancedSocketItem) =>
            {
                list[index] = enhancedSocketItem;
            }, out gameMessageType);
        }

        private static void EnhanceSocketItem(IPlayerCharacterData character, CharacterItem enhancingItem, int enhancerId, System.Action<CharacterItem> onEnhanceSocket, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotEnhanceSocket;
            if (enhancingItem.IsEmptySlot())
            {
                // Cannot enhance socket because character item is empty
                return;
            }
            Item equipmentItem = enhancingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot enhance socket because it's not equipment item
                return;
            }
            if (equipmentItem.maxSocket <= 0)
            {
                // Cannot enhance socket because equipment has no socket(s)
                return;
            }
            if (enhancingItem.Sockets.Count >= equipmentItem.maxSocket)
            {
                // Cannot enhance socket because socket is full
                return;
            }
            Item enhancerItem;
            if (!GameInstance.Items.TryGetValue(enhancerId, out enhancerItem) || !enhancerItem.IsSocketEnhancer())
            {
                // Cannot enhance socket because enhancer id is invalid
                return;
            }
            if (!character.DecreaseItems(enhancerItem.DataId, 1))
            {
                // Cannot enhance socket because there is no item
                gameMessageType = GameMessage.Type.NotEnoughSocketEnchaner;
                return;
            }
            enhancingItem.Sockets.Add(enhancerId);
            onEnhanceSocket.Invoke(enhancingItem);
        }
    }
}