using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerInventoryMessageHandlers : MonoBehaviour, IServerInventoryMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        public UniTaskVoid HandleRequestSwapOrMergeItem(RequestHandlerData requestHandler, RequestSwapOrMergeItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeItemMessage> result)
        {
            throw new System.NotImplementedException();
        }

        public UniTaskVoid HandleRequestEquipArmor(RequestHandlerData requestHandler, RequestEquipArmorMessage request, RequestProceedResultDelegate<ResponseEquipArmorMessage> result)
        {
            throw new System.NotImplementedException();
        }

        public UniTaskVoid HandleRequestEquipWeapon(RequestHandlerData requestHandler, RequestEquipWeaponMessage request, RequestProceedResultDelegate<ResponseEquipWeaponMessage> result)
        {
            throw new System.NotImplementedException();
        }

        public UniTaskVoid HandleRequestUnEquipArmor(RequestHandlerData requestHandler, RequestUnEquipArmorMessage request, RequestProceedResultDelegate<ResponseUnEquipArmorMessage> result)
        {
            throw new System.NotImplementedException();
        }

        public UniTaskVoid HandleRequestUnEquipWeapon(RequestHandlerData requestHandler, RequestUnEquipWeaponMessage request, RequestProceedResultDelegate<ResponseUnEquipWeaponMessage> result)
        {
            throw new System.NotImplementedException();
        }
    }
}
