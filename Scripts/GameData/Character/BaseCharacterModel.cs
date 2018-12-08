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

        // Optimize garbage collector
        private readonly List<string> tempKeepingKeys = new List<string>();
        private readonly List<string> tempAddingKeys = new List<string>();
        private readonly List<string> tempCachedKeys = new List<string>();
        private GameObject tempEquipmentObject;
        private BaseEquipmentModel tempEquipmentModel;

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
                container.SetActiveDefaultModel(false);
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
                    container.SetActiveDefaultModel(true);
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
            tempKeepingKeys.Clear();
            if (rightHandWeapon != null)
                tempKeepingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandWeapon != null || leftHandShield != null)
                tempKeepingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (var key in tempCachedKeys)
            {
                if (!tempKeepingKeys.Contains(key) &&
                    (key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                    key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                    DestroyCacheModel(key);
            }

            if (rightHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels, equipWeapons.rightHand.level);
            if (leftHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels, equipWeapons.leftHand.level);
            if (leftHandShield != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandShield.equipmentModels, equipWeapons.leftHand.level);
        }

        public void SetEquipItems(IList<CharacterItem> equipItems)
        {
            // Clear equipped item models
            tempKeepingKeys.Clear();
            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem != null)
                    tempKeepingKeys.Add(armorItem.EquipPosition);
            }

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (var key in tempCachedKeys)
            {
                if (!tempKeepingKeys.Contains(key) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    DestroyCacheModel(key);
            }

            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;
                if (tempKeepingKeys.Contains(armorItem.EquipPosition))
                    InstantiateEquipModel(armorItem.EquipPosition, armorItem.equipmentModels, equipItem.level);
            }
        }

        public void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels, int level)
        {
            if (equipmentModels == null || equipmentModels.Length == 0)
                return;
            var models = new Dictionary<string, GameObject>();
            foreach (var equipmentModel in equipmentModels)
            {
                if (string.IsNullOrEmpty(equipmentModel.equipSocket) || equipmentModel.model == null)
                    continue;
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(equipmentModel.equipSocket, out container))
                    continue;
                tempEquipmentObject = Instantiate(equipmentModel.model, container.transform);
                tempEquipmentObject.transform.localPosition = Vector3.zero;
                tempEquipmentObject.transform.localEulerAngles = Vector3.zero;
                tempEquipmentObject.transform.localScale = Vector3.one;
                tempEquipmentObject.gameObject.SetActive(true);
                tempEquipmentObject.gameObject.SetLayerRecursively(gameInstance.characterLayer.LayerIndex, true);
                tempEquipmentObject.RemoveComponentsInChildren<Collider>(false);
                tempEquipmentModel = tempEquipmentObject.GetComponent<BaseEquipmentModel>();
                if (tempEquipmentModel != null)
                    tempEquipmentModel.Level = level;
                AddingNewModel(tempEquipmentObject);
                models.Add(equipmentModel.equipSocket, tempEquipmentObject);
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
            tempKeepingKeys.Clear();
            tempAddingKeys.Clear();
            foreach (var buff in buffs)
            {
                var key = buff.GetKey();
                tempKeepingKeys.Add(key);
                tempAddingKeys.Add(key);
            }

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheEffects.Keys);
            foreach (var key in tempCachedKeys)
            {
                if (!tempKeepingKeys.Contains(key))
                    DestroyCacheEffect(key);
                else
                    tempAddingKeys.Remove(key);
            }

            foreach (var buff in buffs)
            {
                var key = buff.GetKey();
                if (tempAddingKeys.Contains(key))
                {
                    var buffData = buff.GetBuff();
                    InstantiateBuffEffect(key, buffData.effects);
                }
            }
        }

        public void InstantiateBuffEffect(string buffId, GameEffect[] buffEffects)
        {
            if (buffEffects == null || buffEffects.Length == 0)
                return;
            CreateCacheEffect(buffId, InstantiateEffect(buffEffects));
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

        public void SetActiveDefaultModel(bool isActive)
        {
            if (defaultModel == null)
                return;

            defaultModel.SetActive(isActive);
            foreach (var renderer in defaultModel.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = isActive;
            }
        }
    }
}
