using LiteNetLib;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public const byte ACTION_TO_SERVER_DATA_CHANNEL = 1;
        public const byte ACTION_TO_CLIENT_DATA_CHANNEL = 1;

        public bool ValidateRequestAttack(bool isLeftHand)
        {
            if (!CanAttack())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            CharacterItem weapon = this.GetAvailableWeapon(ref isLeftHand);
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (!ValidateAmmo(weapon))
            {
                QueueGameMessage(UITextKeys.UI_ERROR_NO_AMMO);
                // Play empty sfx
                if (weaponItem != null)
                    AudioManager.PlaySfxClipAtAudioSource(weaponItem.EmptyClip, CharacterModel.GenericAudioSource);
                return false;
            }
            return true;
        }

        public bool CallServerAttack(bool isLeftHand, AimPosition aimPosition)
        {
            if (!ValidateRequestAttack(isLeftHand))
                return false;
            RPC(ServerAttack, ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, aimPosition);
            return true;
        }

        public bool ValidateRequestUseSKill(int dataId, bool isLeftHand)
        {
            if (!CanUseSkill())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill) ||
                !this.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return false;

            UITextKeys gameMessage;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessage))
            {
                QueueGameMessage(gameMessage);
                return false;
            }
            return true;
        }

        public bool CallServerUseSkill(int dataId, bool isLeftHand, AimPosition aimPosition)
        {
            if (!ValidateRequestUseSKill(dataId, isLeftHand))
                return false;
            RPC(ServerUseSkill, ACTION_TO_SERVER_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, dataId, isLeftHand, aimPosition);
            return true;
        }

        public bool CallAllPlayAttackAnimation(bool isLeftHand, byte animationIndex, int randomSeed, AimPosition aimPosition)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayAttackAnimation, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex, randomSeed, aimPosition);
            return true;
        }

        public bool CallAllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, AimPosition aimPosition)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayUseSkillAnimation, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
            return true;
        }

        public bool CallAllPlayChargeAnimation(bool isLeftHand)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayChargeAnimation, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
            return true;
        }

        public bool CallAllStopChargeAnimation()
        {
            if (this.IsDead())
                return false;
            RPC(AllStopChargeAnimation, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        public bool CallAllPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            if (this.IsDead())
                return false;
            RPC(AllPlayReloadAnimation, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand, reloadingAmmoAmount);
            return true;
        }

        public bool CallServerSkillCastingInterrupt()
        {
            if (this.IsDead())
                return false;
            RPC(ServerSkillCastingInterrupt, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        public bool CallAllOnSkillCastingInterrupt()
        {
            if (this.IsDead())
                return false;
            RPC(AllOnSkillCastingInterrupt, ACTION_TO_CLIENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
            return true;
        }

        public bool CallServerPickupItem(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupItem, objectId);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerPickupItemFromContainer(uint objectId, int index)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupItemFromContainer, objectId, index);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerPickupNearbyItems()
        {
            if (!CanDoActions())
                return false;
            RPC(ServerPickupNearbyItems);
            CallAllPlayPickupAnimation();
            return true;
        }

        public bool CallServerDropItem(short nonEquipIndex, short amount)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerDropItem, nonEquipIndex, amount);
            return true;
        }

        public bool CallAllOnDead()
        {
            RPC(AllOnDead);
            return true;
        }

        public bool CallAllOnRespawn()
        {
            RPC(AllOnRespawn);
            return true;
        }

        public bool CallAllOnLevelUp()
        {
            RPC(AllOnLevelUp);
            return true;
        }

        public bool CallServerUnSummon(uint objectId)
        {
            RPC(ServerUnSummon, objectId);
            return true;
        }

        public bool CallServerReload(bool isLeftHand)
        {
            if (!CanDoActions())
                return false;
            if (!isLeftHand && EquipWeapons.rightHand.IsAmmoFull())
                return false;
            if (isLeftHand && EquipWeapons.leftHand.IsAmmoFull())
                return false;
            RPC(ServerReload, isLeftHand);
            return true;
        }

        public bool CallServerStartWeaponCharge(bool isLeftHand)
        {
            RPC(ServerStartWeaponCharge, isLeftHand);
            return true;
        }

        public bool CallServerStopWeaponCharge()
        {
            RPC(ServerStopWeaponCharge);
            return true;
        }
    }
}
