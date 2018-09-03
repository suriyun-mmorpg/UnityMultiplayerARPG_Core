using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterModel : RpgEntityModel
    {
        [Header("Equipment Containers")]
        public EquipmentModelContainer[] equipmentContainers;

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }
        private Dictionary<string, EquipmentModelContainer> cacheEquipmentModelContainers = null;
        /// <summary>
        /// Dictionary[equipSocket(String), container(EquipmentModelContainer)]
        /// </summary>
        public Dictionary<string, EquipmentModelContainer> CacheEquipmentModelContainers
        {
            get
            {
                if (cacheEquipmentModelContainers == null)
                {
                    cacheEquipmentModelContainers = new Dictionary<string, EquipmentModelContainer>();
                    foreach (var equipmentContainer in equipmentContainers)
                    {
                        if (equipmentContainer.transform != null && !cacheEquipmentModelContainers.ContainsKey(equipmentContainer.equipSocket))
                            cacheEquipmentModelContainers[equipmentContainer.equipSocket] = equipmentContainer;
                    }
                }
                return cacheEquipmentModelContainers;
            }
        }

        /// <summary>
        /// Dictionary[equipPosition(String), Dictionary[equipSocket(String), model(GameObject)]]
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, GameObject>> cacheModels = new Dictionary<string, Dictionary<string, GameObject>>();

        /// <summary>
        /// Dictionary[equipPosition(String), List[effect(GameEffect)]]
        /// </summary>
        private readonly Dictionary<string, List<GameEffect>> cacheEffects = new Dictionary<string, List<GameEffect>>();

        private void CreateCacheModel(string equipPosition, Dictionary<string, GameObject> models)
        {
            DestroyCacheModel(equipPosition);
            if (models == null)
                return;
            foreach (var model in models)
            {
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
                    continue;
                if (container.defaultModel != null)
                    container.defaultModel.SetActive(false);
            }
            cacheModels[equipPosition] = models;
        }

        private void DestroyCacheModel(string equipPosition)
        {
            Dictionary<string, GameObject> oldModels;
            if (!string.IsNullOrEmpty(equipPosition) && cacheModels.TryGetValue(equipPosition, out oldModels) && oldModels != null)
            {
                foreach (var model in oldModels)
                {
                    Destroy(model.Value);
                    EquipmentModelContainer container;
                    if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
                        continue;
                    if (container.defaultModel != null)
                        container.defaultModel.SetActive(true);
                }
                cacheModels.Remove(equipPosition);
            }
        }

        public void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            var rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
            var leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
            var leftHandShield = equipWeapons.leftHand.GetShieldItem();

            // Clear equipped item models
            var keepingKeys = new List<string>();
            if (rightHandWeapon != null)
                keepingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandWeapon != null || leftHandShield != null)
                keepingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

            var keys = new List<string>(cacheModels.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key) &&
                    (key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                    key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                    DestroyCacheModel(key);
            }

            if (rightHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels);
            if (leftHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels);
            if (leftHandShield != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandShield.equipmentModels);
        }

        public void SetEquipItems(IList<CharacterItem> equipItems)
        {
            // Clear equipped item models
            var keepingKeys = new List<string>();
            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem != null)
                    keepingKeys.Add(armorItem.EquipPosition);
            }

            var keys = new List<string>(cacheModels.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    DestroyCacheModel(key);
            }

            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;
                var equipPosition = armorItem.EquipPosition;
                if (keepingKeys.Contains(equipPosition))
                    InstantiateEquipModel(equipPosition, armorItem.equipmentModels);
            }
        }

        public void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels)
        {
            if (equipmentModels == null || equipmentModels.Length == 0)
                return;
            var models = new Dictionary<string, GameObject>();
            foreach (var equipmentModel in equipmentModels)
            {
                var equipSocket = equipmentModel.equipSocket;
                var model = equipmentModel.model;
                if (string.IsNullOrEmpty(equipSocket) || model == null)
                    continue;
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(equipSocket, out container))
                    continue;
                var newModel = Instantiate(model, container.transform);
                newModel.transform.localPosition = Vector3.zero;
                newModel.transform.localEulerAngles = Vector3.zero;
                newModel.transform.localScale = Vector3.one;
                newModel.gameObject.SetActive(true);
                newModel.gameObject.SetLayerRecursively(gameInstance.characterLayer.LayerIndex, true);
                newModel.RemoveComponentsInChildren<Collider>(false);
                AddingNewModel(newModel);
                models.Add(equipSocket, newModel);
            }
            CreateCacheModel(equipPosition, models);
        }

        private void CreateCacheEffect(string buffId, List<GameEffect> effects)
        {
            DestroyCacheEffect(buffId);
            if (effects == null)
                return;
            cacheEffects[buffId] = effects;
        }

        private void DestroyCacheEffect(string buffId)
        {
            List<GameEffect> oldEffects;
            if (!string.IsNullOrEmpty(buffId) && cacheEffects.TryGetValue(buffId, out oldEffects) && oldEffects != null)
            {
                foreach (var effect in oldEffects)
                {
                    effect.DestroyEffect();
                }
                cacheEffects.Remove(buffId);
            }
        }

        public void SetBuffs(IList<CharacterBuff> buffs)
        {
            var keepingKeys = new List<string>();
            var addingKeys = new List<string>();
            foreach (var buff in buffs)
            {
                var buffId = buff.id;
                keepingKeys.Add(buffId);
                addingKeys.Add(buffId);
            }

            var keys = new List<string>(cacheEffects.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key))
                    DestroyCacheEffect(key);
                else
                    addingKeys.Remove(key);
            }

            foreach (var buff in buffs)
            {
                var buffId = buff.id;
                if (addingKeys.Contains(buffId))
                {
                    var buffData = buff.GetBuff();
                    InstantiateBuffEffect(buffId, buffData.effects);
                }
            }
        }

        public void InstantiateBuffEffect(string buffId, GameEffect[] buffEffects)
        {
            if (buffEffects == null || buffEffects.Length == 0)
                return;
            var effects = InstantiateEffect(buffEffects);
            CreateCacheEffect(buffId, effects);
        }

        public bool GetRandomRightHandAttackAnimation(
            WeaponType weaponType,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetRandomRightHandAttackAnimation(weaponType.DataId, out animationIndex, out triggerDuration, out totalDuration);
        }

        public bool GetRandomLeftHandAttackAnimation(
            WeaponType weaponType,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetRandomLeftHandAttackAnimation(weaponType.DataId, out animationIndex, out triggerDuration, out totalDuration);
        }

        public bool GetRandomSkillCastAnimation(
            WeaponType weaponType,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetRandomSkillCastAnimation(weaponType.DataId, out animationIndex, out triggerDuration, out totalDuration);
        }

        public bool HasSkillCastAnimations(Skill skill)
        {
            return HasSkillCastAnimations(skill.DataId);
        }

        public virtual void AddingNewModel(GameObject newModel) { }
        public abstract void UpdateAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier = 1f);
        public abstract Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f);
        public abstract void PlayHurtAnimation();
        public abstract void PlayJumpAnimation();
        public abstract bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetRandomLeftHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetRandomSkillCastAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool HasSkillCastAnimations(int dataId);
    }

    [System.Serializable]
    public struct EquipmentModelContainer
    {
        public string equipSocket;
        public GameObject defaultModel;
        public Transform transform;
    }
}
