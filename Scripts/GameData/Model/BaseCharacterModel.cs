using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterModel : GameEntityModel
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
                    foreach (EquipmentModelContainer equipmentContainer in equipmentContainers)
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

        // Public fields which may used by Character Entity
        public readonly List<Transform> rightHandMissileDamageTransforms = new List<Transform>();
        public readonly List<Transform> leftHandMissileDamageTransforms = new List<Transform>();

        // Optimize garbage collector
        protected readonly List<string> tempAddingKeys = new List<string>();
        protected readonly List<string> tempCachedKeys = new List<string>();
        protected GameObject tempEquipmentObject;
        protected BaseEquipmentEntity tempEquipmentEntity;

        private void CreateCacheModel(string equipPosition, Dictionary<string, GameObject> models)
        {
            DestroyCacheModel(equipPosition);
            if (models == null)
                return;
            foreach (KeyValuePair<string, GameObject> model in models)
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
                foreach (KeyValuePair<string, GameObject> model in oldModels)
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

        public virtual void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            rightHandMissileDamageTransforms.Clear();
            leftHandMissileDamageTransforms.Clear();
            Item rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
            Item leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
            Item leftHandShield = equipWeapons.leftHand.GetShieldItem();

            // Clear equipped item models
            tempAddingKeys.Clear();
            if (rightHandWeapon != null)
                tempAddingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandWeapon != null || leftHandShield != null)
                tempAddingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (string key in tempCachedKeys)
            {
                if (!tempAddingKeys.Contains(key) &&
                    (key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                    key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                    DestroyCacheModel(key);
            }

            if (rightHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels, equipWeapons.rightHand.level, rightHandMissileDamageTransforms);
            if (leftHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels, equipWeapons.leftHand.level, leftHandMissileDamageTransforms);
            if (leftHandShield != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandShield.equipmentModels, equipWeapons.leftHand.level);
        }

        public virtual void SetEquipItems(IList<CharacterItem> equipItems)
        {
            // Clear equipped item models
            tempAddingKeys.Clear();
            foreach (CharacterItem equipItem in equipItems)
            {
                Item armorItem = equipItem.GetArmorItem();
                if (armorItem != null)
                    tempAddingKeys.Add(armorItem.EquipPosition);
            }

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (string key in tempCachedKeys)
            {
                if (!tempAddingKeys.Contains(key) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    DestroyCacheModel(key);
            }

            foreach (CharacterItem equipItem in equipItems)
            {
                Item armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;
                if (tempAddingKeys.Contains(armorItem.EquipPosition))
                    InstantiateEquipModel(armorItem.EquipPosition, armorItem.equipmentModels, equipItem.level);
            }
        }

        public void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels, int level, List<Transform> missileDamageTransforms = null)
        {
            if (equipmentModels == null || equipmentModels.Length == 0)
                return;
            Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
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
                tempEquipmentEntity = tempEquipmentObject.GetComponent<BaseEquipmentEntity>();
                if (tempEquipmentEntity != null)
                {
                    tempEquipmentEntity.Level = level;
                    if (missileDamageTransforms != null && tempEquipmentEntity.missileDamageTransform != null)
                        missileDamageTransforms.Add(tempEquipmentEntity.missileDamageTransform);
                }
                AddingNewModel(tempEquipmentObject);
                models.Add(equipmentModel.equipSocket, tempEquipmentObject);
            }
            CreateCacheModel(equipPosition, models);
        }

        private void CreateCacheEffect(string buffId, List<GameEffect> effects)
        {
            if (effects == null || cacheEffects.ContainsKey(buffId))
                return;
            cacheEffects[buffId] = effects;
        }

        private void DestroyCacheEffect(string buffId)
        {
            List<GameEffect> oldEffects;
            if (!string.IsNullOrEmpty(buffId) && cacheEffects.TryGetValue(buffId, out oldEffects) && oldEffects != null)
            {
                foreach (GameEffect effect in oldEffects)
                {
                    if (effect == null) continue;
                    effect.DestroyEffect();
                }
                cacheEffects.Remove(buffId);
            }
        }

        public virtual void SetBuffs(IList<CharacterBuff> buffs)
        {
            // Temp old keys
            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheEffects.Keys);
            // Prepare data
            tempAddingKeys.Clear();
            string tempKey;
            // Loop new buffs to prepare adding keys
            foreach (CharacterBuff buff in buffs)
            {
                tempKey = buff.GetKey();
                if (!tempCachedKeys.Contains(tempKey))
                {
                    // If old buffs not contains this buff, add this buff effect
                    InstantiateBuffEffect(tempKey, buff.GetBuff().effects);
                }
                tempAddingKeys.Add(tempKey);
            }

            // Remove effects which removed from new buffs list
            // Loop old keys to destroy removed buffs
            foreach (string key in tempCachedKeys)
            {
                if (!tempAddingKeys.Contains(key))
                {
                    // New buffs not contains old buff, remove effect
                    DestroyCacheEffect(key);
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

        public bool GetSkillActivateAnimation(
            Skill skill,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetSkillActivateAnimation(skill.DataId, out triggerDuration, out totalDuration);
        }

        public bool GetReloadAnimation(
            WeaponType weaponType,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetReloadAnimation(weaponType.DataId, out triggerDuration, out totalDuration);
        }

        public bool HasSkillAnimations(Skill skill)
        {
            return HasSkillAnimations(skill.DataId);
        }

        public Transform GetRightHandEquipmentEntity()
        {
            if (rightHandMissileDamageTransforms.Count == 0)
                return null;
            return rightHandMissileDamageTransforms[Random.Range(0, rightHandMissileDamageTransforms.Count)];
        }

        public Transform GetLeftHandEquipmentEntity()
        {
            if (leftHandMissileDamageTransforms.Count == 0)
                return null;
            return leftHandMissileDamageTransforms[Random.Range(0, leftHandMissileDamageTransforms.Count)];
        }

        public virtual void AddingNewModel(GameObject newModel) { }
        public abstract void UpdateAnimation(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier = 1f);
        public abstract Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f);
        public abstract Coroutine PlaySkillCastClip(int dataId, float duration);
        public abstract void StopActionAnimation();
        public abstract void PlayHurtAnimation();
        public abstract void PlayJumpAnimation();
        public abstract bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetRandomLeftHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetSkillActivateAnimation(int dataId, out float triggerDuration, out float totalDuration);
        public abstract bool GetReloadAnimation(int dataId, out float triggerDuration, out float totalDuration);
        public abstract bool HasSkillAnimations(int dataId);
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
            foreach (Renderer renderer in defaultModel.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = isActive;
            }
        }
    }
}
