using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class BaseCharacterModel : GameEntityModel, IMoveableModel, IHittableModel, IJumppableModel
    {
        [HideInInspector]
        public CharacterModelManager modelManager;
        public bool isMainModel { get { return modelManager != null && modelManager.MainModel == this; } }

        [Header("Equipment Containers")]
        public EquipmentContainer[] equipmentContainers;
        [InspectorButton("SetEquipmentContainersBySetters")]
        public bool setEquipmentContainersBySetters;

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
        private Dictionary<string, EquipmentContainer> cacheEquipmentModelContainers = null;
        /// <summary>
        /// Dictionary[equipSocket(String), container(EquipmentModelContainer)]
        /// </summary>
        public Dictionary<string, EquipmentContainer> CacheEquipmentModelContainers
        {
            get
            {
                if (cacheEquipmentModelContainers == null)
                {
                    cacheEquipmentModelContainers = new Dictionary<string, EquipmentContainer>();
                    foreach (EquipmentContainer equipmentContainer in equipmentContainers)
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

        // Equipment entities that will be used to play weapon effects
        protected BaseEquipmentEntity rightHandEquipmentEntity;
        protected BaseEquipmentEntity leftHandEquipmentEntity;
        protected Dictionary<string, List<BaseEquipmentEntity>> equipmentEntities = new Dictionary<string, List<BaseEquipmentEntity>>();

        // Protected fields
        public EquipWeapons equipWeapons { get; protected set; }
        public IList<CharacterItem> equipItems { get; protected set; }
        public IList<CharacterBuff> buffs { get; protected set; }
        public bool isDead { get; protected set; }
        public float moveAnimationSpeedMultiplier { get; protected set; }
        public MovementState movementState { get; protected set; }

        // Optimize garbage collector
        protected readonly List<string> tempAddingKeys = new List<string>();
        protected readonly List<string> tempCachedKeys = new List<string>();
        protected GameObject tempEquipmentObject;
        protected BaseEquipmentEntity tempEquipmentEntity;

        protected override void Awake()
        {
            base.Awake();
            SetIsDead(false);
            SetMoveAnimationSpeedMultiplier(1);
            SetMovementState(MovementState.IsGrounded);
        }

        internal virtual void SwitchModel(BaseCharacterModel previousModel)
        {
            DestroyCacheModels();
            DestroyCacheEffects();

            if (modelManager != null && !isMainModel)
            {
                // Sub-model will use some data same as main model
                hiddingObjects = modelManager.MainModel.hiddingObjects;
                effectContainers = modelManager.MainModel.effectContainers;
                equipmentContainers = modelManager.MainModel.equipmentContainers;
            }

            if (previousModel != null)
            {
                previousModel.DestroyCacheModels();
                previousModel.DestroyCacheEffects();
                SetEquipWeapons(previousModel.equipWeapons);
                SetEquipItems(previousModel.equipItems);
                SetBuffs(previousModel.buffs);
                SetIsDead(previousModel.isDead);
                SetMoveAnimationSpeedMultiplier(previousModel.moveAnimationSpeedMultiplier);
                SetMovementState(previousModel.movementState);
            }
        }

        protected override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            base.OnDrawGizmos();
            if (equipmentContainers != null)
            {
                foreach (EquipmentContainer equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.transform == null) continue;
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(equipmentContainer.transform.position, 0.1f);
                    Handles.Label(equipmentContainer.transform.position, equipmentContainer.equipSocket + "(Equipment)");
                }
            }
#endif
        }

        [ContextMenu("Set Equipment Containers By Setters")]
        public void SetEquipmentContainersBySetters()
        {
            EquipmentContainerSetter[] setters = GetComponentsInChildren<EquipmentContainerSetter>();
            if (setters != null && setters.Length > 0)
            {
                foreach (EquipmentContainerSetter setter in setters)
                {
                    setter.ApplyToCharacterModel(this);
                }
            }
        }

        private void CreateCacheModel(string equipPosition, Dictionary<string, GameObject> models)
        {
            DestroyCacheModel(equipPosition);
            if (models == null)
                return;
            foreach (string key in models.Keys)
            {
                EquipmentContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(key, out container))
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
                foreach (string key in oldModels.Keys)
                {
                    Destroy(oldModels[key]);
                    EquipmentContainer container;
                    if (!CacheEquipmentModelContainers.TryGetValue(key, out container))
                        continue;
                    container.SetActiveDefaultModel(true);
                }
                cacheModels.Remove(equipPosition);
            }
        }

        private void DestroyCacheModels()
        {
            List<string> equipPositions = new List<string>(cacheModels.Keys);
            foreach (string equipPosition in equipPositions)
            {
                DestroyCacheModel(equipPosition);
            }
        }

        public virtual void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            this.equipWeapons = equipWeapons;

            Item rightHandWeapon = equipWeapons.GetRightHandEquipmentItem();
            Item leftHandWeapon = equipWeapons.GetLeftHandEquipmentItem();

            // Clear equipped item models
            tempAddingKeys.Clear();
            if (rightHandWeapon != null)
                tempAddingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandWeapon != null)
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

            if (rightHandWeapon != null && rightHandWeapon.IsWeapon())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels, equipWeapons.rightHand.level, out rightHandEquipmentEntity);
            if (leftHandWeapon != null && leftHandWeapon.IsWeapon())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels, equipWeapons.leftHand.level, out leftHandEquipmentEntity);
            if (leftHandWeapon != null && leftHandWeapon.IsShield())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.equipmentModels, equipWeapons.leftHand.level, out leftHandEquipmentEntity);
        }

        public virtual void SetEquipItems(IList<CharacterItem> equipItems)
        {
            this.equipItems = equipItems;
            // Clear equipped item models
            tempAddingKeys.Clear();
            if (equipItems != null)
            {
                foreach (CharacterItem equipItem in equipItems)
                {
                    Item armorItem = equipItem.GetArmorItem();
                    if (armorItem != null)
                        tempAddingKeys.Add(armorItem.EquipPosition);
                }
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

            if (equipItems != null)
            {
                foreach (CharacterItem equipItem in equipItems)
                {
                    Item armorItem = equipItem.GetArmorItem();
                    if (armorItem == null)
                        continue;
                    BaseEquipmentEntity tempEquipmentEntity;
                    if (tempAddingKeys.Contains(armorItem.EquipPosition))
                        InstantiateEquipModel(armorItem.EquipPosition, armorItem.equipmentModels, equipItem.level, out tempEquipmentEntity);
                }
            }
        }

        public void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels, int level, out BaseEquipmentEntity equipmentEntity)
        {
            equipmentEntity = null;

            if (!equipmentEntities.ContainsKey(equipPosition))
                equipmentEntities.Add(equipPosition, new List<BaseEquipmentEntity>());
            equipmentEntities[equipPosition].Clear();

            if (equipmentModels == null || equipmentModels.Length == 0)
                return;

            Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
            {
                if (string.IsNullOrEmpty(equipmentModel.equipSocket) || equipmentModel.model == null)
                    continue;
                EquipmentContainer container;
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
                    equipmentEntities[equipPosition].Add(tempEquipmentEntity);
                    if (equipmentEntity == null)
                        equipmentEntity = tempEquipmentEntity;
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

        private void DestroyCacheEffects()
        {
            List<string> buffIds = new List<string>(cacheEffects.Keys);
            foreach (string buffId in buffIds)
            {
                DestroyCacheEffect(buffId);
            }
        }

        public virtual void SetBuffs(IList<CharacterBuff> buffs)
        {
            this.buffs = buffs;
            // Temp old keys
            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheEffects.Keys);
            // Prepare data
            tempAddingKeys.Clear();
            string tempKey;
            // Loop new buffs to prepare adding keys
            if (buffs != null)
            {
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

        public bool GetRightHandReloadAnimation(
            WeaponType weaponType,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetRightHandReloadAnimation(weaponType.DataId, out triggerDuration, out totalDuration);
        }

        public bool GetLeftHandReloadAnimation(
            WeaponType weaponType,
            out float triggerDuration,
            out float totalDuration)
        {
            return GetLeftHandReloadAnimation(weaponType.DataId, out triggerDuration, out totalDuration);
        }

        public SkillActivateAnimationType UseSkillActivateAnimationType(Skill skill)
        {
            return UseSkillActivateAnimationType(skill.DataId);
        }

        public List<BaseEquipmentEntity> GetEquipmentEntities(string equipPosition)
        {
            if (!equipmentEntities.ContainsKey(equipPosition))
                equipmentEntities.Add(equipPosition, new List<BaseEquipmentEntity>());
            return equipmentEntities[equipPosition];
        }

        public BaseEquipmentEntity GetRightHandEquipmentEntity()
        {
            return rightHandEquipmentEntity;
        }

        public BaseEquipmentEntity GetLeftHandEquipmentEntity()
        {
            return leftHandEquipmentEntity;
        }

        public Transform GetRightHandMissileDamageTransform()
        {
            if (rightHandEquipmentEntity != null)
                return rightHandEquipmentEntity.missileDamageTransform;
            return null;
        }

        public Transform GetLeftHandMissileDamageTransform()
        {
            if (leftHandEquipmentEntity != null)
                return leftHandEquipmentEntity.missileDamageTransform;
            return null;
        }

        public void PlayWeaponLaunchEffect(AnimActionType animActionType)
        {
            if ((animActionType == AnimActionType.AttackRightHand || animActionType == AnimActionType.SkillRightHand) && rightHandEquipmentEntity != null)
                rightHandEquipmentEntity.PlayWeaponLaunchEffect();
            if ((animActionType == AnimActionType.AttackLeftHand || animActionType == AnimActionType.SkillLeftHand) && leftHandEquipmentEntity != null)
                leftHandEquipmentEntity.PlayWeaponLaunchEffect();
        }

        public virtual void AddingNewModel(GameObject newModel) { }

        public void SetIsDead(bool isDead)
        {
            this.isDead = isDead;
        }

        public void SetMoveAnimationSpeedMultiplier(float moveAnimationSpeedMultiplier)
        {
            this.moveAnimationSpeedMultiplier = moveAnimationSpeedMultiplier;
        }

        public void SetMovementState(MovementState movementState)
        {
            this.movementState = movementState;
            PlayMoveAnimation();
        }

        /// <summary>
        /// Use this function to play hit animation when receive damage
        /// </summary>
        public virtual void PlayHitAnimation() { }

        /// <summary>
        /// Use this function to play jump animation
        /// </summary>
        public virtual void PlayJumpAnimation() { }

        public abstract void PlayMoveAnimation();
        public abstract Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f);
        public abstract Coroutine PlaySkillCastClip(int dataId, float duration);
        public abstract void StopActionAnimation();
        public abstract void StopSkillCastAnimation();
        public abstract bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetRandomLeftHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float triggerDuration, out float totalDuration);
        public abstract bool GetSkillActivateAnimation(int dataId, out float triggerDuration, out float totalDuration);
        public abstract bool GetRightHandReloadAnimation(int dataId, out float triggerDuration, out float totalDuration);
        public abstract bool GetLeftHandReloadAnimation(int dataId, out float triggerDuration, out float totalDuration);
        public abstract SkillActivateAnimationType UseSkillActivateAnimationType(int dataId);
    }

    [System.Serializable]
    public struct EquipmentContainer
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
