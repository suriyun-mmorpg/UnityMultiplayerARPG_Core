using System.Collections.Generic;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacterModel : GameEntityModel, IMoveableModel, IHittableModel, IJumppableModel, IPickupableModel, IDeadlyModel
    {
        public BaseCharacterModel MainModel { get; set; }
        public bool IsMainModel { get { return MainModel == this; } }
        public bool IsTpsModel { get; set; }
        public bool IsFpsModel { get; set; }

        [Header("Model Switching Settings")]
        [SerializeField]
        protected GameObject[] activateObjectsWhenSwitchModel = new GameObject[0];
        public GameObject[] ActivateObjectsWhenSwitchModel
        {
            get { return activateObjectsWhenSwitchModel; }
            set { activateObjectsWhenSwitchModel = value; }
        }

        [SerializeField]
        protected GameObject[] deactivateObjectsWhenSwitchModel = new GameObject[0];
        public GameObject[] DeactivateObjectsWhenSwitchModel
        {
            get { return deactivateObjectsWhenSwitchModel; }
            set { deactivateObjectsWhenSwitchModel = value; }
        }

        [SerializeField]
        protected VehicleCharacterModel[] vehicleModels = new VehicleCharacterModel[0];
        public VehicleCharacterModel[] VehicleModels
        {
            get { return vehicleModels; }
            set { vehicleModels = value; }
        }

        [Header("Equipment Containers")]
        [SerializeField]
        protected EquipmentContainer[] equipmentContainers = new EquipmentContainer[0];
        public EquipmentContainer[] EquipmentContainers
        {
            get { return equipmentContainers; }
            set { equipmentContainers = value; }
        }

        [SerializeField]
        protected UnityLayer equipmentLayer;
        public int EquipmentLayer
        {
            get { return equipmentLayer.LayerIndex; }
            set { equipmentLayer = new UnityLayer(value); }
        }

#if UNITY_EDITOR
        [InspectorButton(nameof(SetEquipmentContainersBySetters))]
        public bool setEquipmentContainersBySetters = false;
        [InspectorButton(nameof(DeactivateInstantiatedObjects))]
        public bool deactivateInstantiatedObjects = false;
        [InspectorButton(nameof(ActivateInstantiatedObject))]
        public bool activateInstantiatedObject = false;
#endif

        public CharacterModelManager Manager { get; private set; }

        /// <summary>
        /// { vehicleType(Int32), vehicleCharacterModel(VehicleCharacterModel) }
        /// </summary>
        public Dictionary<int, VehicleCharacterModel> CacheVehicleModels { get; private set; }

        /// <summary>
        /// { equipSocket(String), container(EquipmentModelContainer) }
        /// </summary>
        public Dictionary<string, EquipmentContainer> CacheEquipmentModelContainers { get; private set; }

        /// <summary>
        /// { equipPosition(String), { equipSocket(String), model(GameObject) } }
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, GameObject>> cacheModels = new Dictionary<string, Dictionary<string, GameObject>>();

        /// <summary>
        /// { equipPosition(String), [ effect(GameEffect) ] }
        /// </summary>
        private readonly Dictionary<string, List<GameEffect>> cacheEffects = new Dictionary<string, List<GameEffect>>();

        // Equipment entities that will be used to play weapon effects
        protected BaseEquipmentEntity rightHandEquipmentEntity;
        protected BaseEquipmentEntity leftHandEquipmentEntity;
        protected readonly Dictionary<string, List<BaseEquipmentEntity>> equipmentEntities = new Dictionary<string, List<BaseEquipmentEntity>>();

        // Item Ids
        /// <summary>
        /// { equipPosition(String), itemDataId(Int32) }
        /// </summary>
        protected readonly Dictionary<string, int> itemIds = new Dictionary<string, int>();

        // Protected fields
        public EquipWeapons equipWeapons { get; protected set; }
        public IList<CharacterItem> equipItems { get; protected set; }
        public IList<CharacterBuff> buffs { get; protected set; }
        public bool isDead { get; protected set; }
        public float moveAnimationSpeedMultiplier { get; protected set; }
        public MovementState movementState { get; protected set; }
        public ExtraMovementState extraMovementState { get; protected set; }
        public Vector2 direction2D { get; protected set; }
        public bool isFreezeAnimation { get; protected set; }

        // Public events
        public System.Action<string> onEquipmentModelsInstantiated;
        public System.Action<string> onEquipmentModelsDestroyed;

        // Optimize garbage collector
        protected readonly List<string> tempAddingKeys = new List<string>();
        protected readonly List<string> tempCachedKeys = new List<string>();

        protected override void Awake()
        {
            base.Awake();

            Manager = GetComponent<CharacterModelManager>();
            if (Manager == null)
                Manager = GetComponentInParent<CharacterModelManager>();
            // Can't find manager, this component may attached to non-character entities, so assume that this character model is main model
            if (Manager == null)
            {
                MainModel = this;
            }
            else
            {
                EffectLayer = Manager.Entity.gameObject.layer;
                EquipmentLayer = Manager.Entity.gameObject.layer;
            }

            CacheVehicleModels = new Dictionary<int, VehicleCharacterModel>();
            if (IsMainModel && vehicleModels != null && vehicleModels.Length > 0)
            {
                foreach (VehicleCharacterModel vehicleModel in vehicleModels)
                {
                    if (!vehicleModel.vehicleType) continue;
                    for (int i = 0; i < vehicleModel.modelsForEachSeats.Length; ++i)
                    {
                        vehicleModel.modelsForEachSeats[i].MainModel = this;
                        vehicleModel.modelsForEachSeats[i].IsTpsModel = IsTpsModel;
                        vehicleModel.modelsForEachSeats[i].IsFpsModel = IsFpsModel;
                    }
                    CacheVehicleModels[vehicleModel.vehicleType.DataId] = vehicleModel;
                }
            }

            CacheEquipmentModelContainers = new Dictionary<string, EquipmentContainer>();
            foreach (EquipmentContainer equipmentContainer in equipmentContainers)
            {
                if (equipmentContainer.transform != null && !CacheEquipmentModelContainers.ContainsKey(equipmentContainer.equipSocket))
                    CacheEquipmentModelContainers[equipmentContainer.equipSocket] = equipmentContainer;
            }
        }

        IEnumerator SetLayers()
        {
            yield return new WaitForEndOfFrame();
        }

        internal virtual void CopyCacheDataTo(
            Dictionary<string, Dictionary<string, GameObject>> destModels,
            Dictionary<string, List<GameEffect>> destEffects,
            ref BaseEquipmentEntity destRightHandEquipmentEntity,
            ref BaseEquipmentEntity destLeftHandEquipmentEntity,
            Dictionary<string, List<BaseEquipmentEntity>> destEquipmentEntities,
            Dictionary<string, int> destItemIds)
        {
            destModels.Clear();
            foreach (var entry in cacheModels)
            {
                destModels.Add(entry.Key, entry.Value);
            }
            destEffects.Clear();
            foreach (var entry in cacheEffects)
            {
                destEffects.Add(entry.Key, entry.Value);
            }
            destRightHandEquipmentEntity = rightHandEquipmentEntity;
            destLeftHandEquipmentEntity = leftHandEquipmentEntity;
            destEquipmentEntities.Clear();
            foreach (var entry in equipmentEntities)
            {
                destEquipmentEntities.Add(entry.Key, entry.Value);
            }
            destItemIds.Clear();
            foreach (var entry in itemIds)
            {
                destItemIds.Add(entry.Key, entry.Value);
            }
        }

        private void UpdateObjectsWhenSwitch()
        {
            if (activateObjectsWhenSwitchModel != null &&
                activateObjectsWhenSwitchModel.Length > 0)
            {
                foreach (GameObject obj in activateObjectsWhenSwitchModel)
                {
                    if (!obj.activeSelf)
                        obj.SetActive(true);
                }
            }
            if (deactivateObjectsWhenSwitchModel != null &&
                deactivateObjectsWhenSwitchModel.Length > 0)
            {
                foreach (GameObject obj in deactivateObjectsWhenSwitchModel)
                {
                    if (obj.activeSelf)
                        obj.SetActive(false);
                }
            }
        }

        private void RevertObjectsWhenSwitch()
        {
            if (activateObjectsWhenSwitchModel != null &&
                activateObjectsWhenSwitchModel.Length > 0)
            {
                foreach (GameObject obj in activateObjectsWhenSwitchModel)
                {
                    if (obj.activeSelf)
                        obj.SetActive(false);
                }
            }
            if (deactivateObjectsWhenSwitchModel != null &&
                deactivateObjectsWhenSwitchModel.Length > 0)
            {
                foreach (GameObject obj in deactivateObjectsWhenSwitchModel)
                {
                    if (!obj.activeSelf)
                        obj.SetActive(true);
                }
            }
        }

        internal virtual void SwitchModel(BaseCharacterModel previousModel)
        {
            if (!IsMainModel)
            {
                // Sub-model will use some data same as main model
                hiddingObjects = MainModel.hiddingObjects;
                hiddingRenderers = MainModel.hiddingRenderers;
                effectContainers = MainModel.effectContainers;
                equipmentContainers = MainModel.equipmentContainers;
            }

            if (previousModel != null)
            {
                previousModel.OnSwitchingToAnotherModel();
                previousModel.RevertObjectsWhenSwitch();
                previousModel.CopyCacheDataTo(
                    cacheModels,
                    cacheEffects,
                    ref rightHandEquipmentEntity,
                    ref leftHandEquipmentEntity,
                    equipmentEntities,
                    itemIds);
                SetIsDead(previousModel.isDead);
                SetDefaultAnimations();
                SetEquipWeapons(previousModel.equipWeapons);
                SetEquipItems(previousModel.equipItems);
                SetBuffs(previousModel.buffs);
                SetMoveAnimationSpeedMultiplier(previousModel.moveAnimationSpeedMultiplier);
                SetMovementState(previousModel.movementState, previousModel.extraMovementState, previousModel.direction2D, previousModel.isFreezeAnimation);
            }
            else
            {
                SetDefaultAnimations();
                SetEquipWeapons(equipWeapons);
                SetEquipItems(equipItems);
                SetBuffs(buffs);
            }

            UpdateObjectsWhenSwitch();
            OnSwitchedToThisModel();
        }

        internal virtual void OnSwitchingToAnotherModel()
        {

        }

        internal virtual void OnSwitchedToThisModel()
        {

        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (equipmentContainers != null)
            {
                foreach (EquipmentContainer equipmentContainer in equipmentContainers)
                {
                    if (equipmentContainer.transform == null) continue;
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawWireSphere(equipmentContainer.transform.position, 0.1f);
                    Gizmos.DrawSphere(equipmentContainer.transform.position, 0.03f);
                    Handles.Label(equipmentContainer.transform.position, equipmentContainer.equipSocket + "(Equipment)");
                }
            }
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Set Equipment Containers By Setters", false, 1000301)]
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
            this.InvokeInstanceDevExtMethods("SetEquipmentContainersBySetters");
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Deactivate Instantiated Objects", false, 1000302)]
        public void DeactivateInstantiatedObjects()
        {
            if (equipmentContainers != null && equipmentContainers.Length > 0)
            {
                for (int i = 0; i < equipmentContainers.Length; ++i)
                {
                    equipmentContainers[i].DeactivateInstantiatedObjects();
                    equipmentContainers[i].SetActiveDefaultModel(true);
                }
            }
        }

        [ContextMenu("Activate Instantiated Object", false, 1000303)]
        public void ActivateInstantiatedObject()
        {
            if (equipmentContainers != null && equipmentContainers.Length > 0)
            {
                for (int i = 0; i < equipmentContainers.Length; ++i)
                {
                    equipmentContainers[i].SetActiveDefaultModel(false);
                    equipmentContainers[i].ActivateInstantiatedObject(equipmentContainers[i].activatingInstantiateObjectIndex);
                }
            }
        }
#endif

        private void DestroyCacheModel(string equipPosition)
        {
            if (string.IsNullOrEmpty(equipPosition))
                return;

            Dictionary<string, GameObject> oldModels;
            if (cacheModels.TryGetValue(equipPosition, out oldModels) &&
                oldModels != null)
            {
                EquipmentContainer tempContainer;
                foreach (string equipSocket in oldModels.Keys)
                {
                    if (oldModels[equipSocket] != null)
                        Destroy(oldModels[equipSocket]);
                    if (!CacheEquipmentModelContainers.TryGetValue(equipSocket, out tempContainer))
                        continue;
                    tempContainer.DeactivateInstantiatedObjects();
                    tempContainer.SetActiveDefaultModel(true);
                }
                cacheModels.Remove(equipPosition);
            }
            if (itemIds.ContainsKey(equipPosition))
                itemIds.Remove(equipPosition);
            if (onEquipmentModelsDestroyed != null)
                onEquipmentModelsDestroyed.Invoke(equipPosition);
        }

        private void DestroyCacheModels()
        {
            foreach (string equipPosition in cacheModels.Keys)
            {
                DestroyCacheModel(equipPosition);
            }
        }

        public virtual void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            this.equipWeapons = equipWeapons;

            IEquipmentItem rightHandItem = equipWeapons.GetRightHandEquipmentItem();
            IEquipmentItem leftHandItem = equipWeapons.GetLeftHandEquipmentItem();

            // Clear equipped item models
            tempAddingKeys.Clear();
            if (rightHandItem != null)
                tempAddingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandItem != null)
                tempAddingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (string equipPosition in tempCachedKeys)
            {
                // Destroy cache model by the position which not existed in new equipment position (unequipped items)
                if (!tempAddingKeys.Contains(equipPosition) &&
                    (equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                    equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                    DestroyCacheModel(equipPosition);
            }

            if (rightHandItem != null && rightHandItem.IsWeapon())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandItem.DataId, equipWeapons.rightHand.level, rightHandItem.EquipmentModels, out rightHandEquipmentEntity);
            if (leftHandItem != null && leftHandItem.IsWeapon())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandItem.DataId, equipWeapons.leftHand.level, (leftHandItem as IWeaponItem).OffHandEquipmentModels, out leftHandEquipmentEntity);
            if (leftHandItem != null && leftHandItem.IsShield())
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandItem.DataId, equipWeapons.leftHand.level, leftHandItem.EquipmentModels, out leftHandEquipmentEntity);
        }

        public virtual void SetEquipItems(IList<CharacterItem> equipItems)
        {
            this.equipItems = equipItems;
            IArmorItem armorItem;
            tempAddingKeys.Clear();
            if (equipItems != null && equipItems.Count > 0)
            {
                foreach (CharacterItem equipItem in equipItems)
                {
                    armorItem = equipItem.GetArmorItem();
                    if (armorItem == null) continue;
                    tempAddingKeys.Add(armorItem.GetEquipPosition());
                }
            }

            tempCachedKeys.Clear();
            tempCachedKeys.AddRange(cacheModels.Keys);
            foreach (string equipPosition in tempCachedKeys)
            {
                // Destroy cache model by the position which not existed in new equipment position (unequipped items)
                if (!tempAddingKeys.Contains(equipPosition) &&
                    !equipPosition.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                    !equipPosition.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    DestroyCacheModel(equipPosition);
            }

            if (equipItems != null)
            {
                foreach (CharacterItem equipItem in equipItems)
                {
                    armorItem = equipItem.GetArmorItem();
                    if (armorItem == null) continue;
                    if (tempAddingKeys.Contains(armorItem.GetEquipPosition()))
                        InstantiateEquipModel(armorItem.GetEquipPosition(), armorItem.DataId, equipItem.level, armorItem.EquipmentModels, out _);
                }
            }
        }

        public void InstantiateEquipModel(string equipPosition, int itemDataId, int itemLevel, EquipmentModel[] equipmentModels, out BaseEquipmentEntity foundEquipmentEntity)
        {
            foundEquipmentEntity = null;

            if (!equipmentEntities.ContainsKey(equipPosition))
                equipmentEntities.Add(equipPosition, new List<BaseEquipmentEntity>());

            // Temp variables
            int i;
            GameObject tempEquipmentObject;
            BaseEquipmentEntity tempEquipmentEntity;

            // Same item Id, just change equipment level don't destroy and re-create
            if (itemIds.ContainsKey(equipPosition) && itemIds[equipPosition] == itemDataId)
            {
                for (i = 0; i < equipmentEntities[equipPosition].Count; ++i)
                {
                    tempEquipmentEntity = equipmentEntities[equipPosition][i];
                    tempEquipmentEntity.Setup(this, equipPosition, itemLevel);
                    if (foundEquipmentEntity == null)
                        foundEquipmentEntity = tempEquipmentEntity;
                }
                return;
            }

            DestroyCacheModel(equipPosition);
            itemIds[equipPosition] = itemDataId;
            equipmentEntities[equipPosition].Clear();

            if (equipmentModels == null || equipmentModels.Length == 0)
                return;

            Dictionary<string, GameObject> tempInstantiatingModels = new Dictionary<string, GameObject>();
            EquipmentContainer tempContainer;
            EquipmentModel tempEquipmentModel;
            for (i = 0; i < equipmentModels.Length; ++i)
            {
                tempEquipmentModel = equipmentModels[i];
                if (string.IsNullOrEmpty(tempEquipmentModel.equipSocket))
                    continue;
                if (!CacheEquipmentModelContainers.TryGetValue(tempEquipmentModel.equipSocket, out tempContainer))
                    continue;
                if (tempEquipmentModel.useInstantiatedObject)
                {
                    // Activate the instantiated object
                    if (!tempContainer.ActivateInstantiatedObject(tempEquipmentModel.instantiatedObjectIndex))
                        continue;
                    tempContainer.SetActiveDefaultModel(false);
                    tempEquipmentObject = tempContainer.instantiatedObjects[tempEquipmentModel.instantiatedObjectIndex];
                    tempEquipmentEntity = tempEquipmentObject.GetComponent<BaseEquipmentEntity>();
                    tempInstantiatingModels.Add(tempEquipmentModel.equipSocket, null);
                }
                else
                {
                    if (tempEquipmentModel.model == null)
                        continue;
                    // Instantiate model, setup transform and activate game object
                    tempContainer.DeactivateInstantiatedObjects();
                    tempContainer.SetActiveDefaultModel(false);
                    tempEquipmentObject = Instantiate(tempEquipmentModel.model, tempContainer.transform);
                    tempEquipmentObject.transform.localPosition = tempEquipmentModel.localPosition;
                    tempEquipmentObject.transform.localEulerAngles = tempEquipmentModel.localEulerAngles;
                    tempEquipmentObject.transform.localScale = tempEquipmentModel.localScale.Equals(Vector3.zero) ? Vector3.one : tempEquipmentModel.localScale;
                    tempEquipmentObject.gameObject.SetActive(true);
                    tempEquipmentObject.gameObject.SetLayerRecursively(EquipmentLayer, true);
                    tempEquipmentObject.RemoveComponentsInChildren<Collider>(false);
                    tempEquipmentEntity = tempEquipmentObject.GetComponent<BaseEquipmentEntity>();
                    AddingNewModel(tempEquipmentObject, tempContainer);
                    tempInstantiatingModels.Add(tempEquipmentModel.equipSocket, tempEquipmentObject);
                }
                // Setup equipment entity (if exists)
                if (tempEquipmentEntity != null)
                {
                    tempEquipmentEntity.Setup(this, equipPosition, itemLevel);
                    equipmentEntities[equipPosition].Add(tempEquipmentEntity);
                    if (foundEquipmentEntity == null)
                        foundEquipmentEntity = tempEquipmentEntity;
                }
            }
            // Cache Models
            cacheModels[equipPosition] = tempInstantiatingModels;
            if (onEquipmentModelsInstantiated != null)
                onEquipmentModelsInstantiated.Invoke(equipPosition);
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
            foreach (string buffId in cacheEffects.Keys)
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
            // Loop new buffs to prepare adding keys
            if (buffs != null && buffs.Count > 0)
            {
                string tempKey;
                foreach (CharacterBuff buff in buffs)
                {
                    // Buff effects
                    tempKey = buff.GetKey();
                    if (!tempCachedKeys.Contains(tempKey))
                    {
                        // If old buffs not contains this buff, add this buff effect
                        InstantiateBuffEffect(tempKey, buff.GetBuff().effects);
                        tempCachedKeys.Add(tempKey);
                    }
                    tempAddingKeys.Add(tempKey);
                    // Ailment effects
                    switch (buff.GetBuff().ailment)
                    {
                        case AilmentPresets.Stun:
                            tempKey = nameof(AilmentPresets.Stun);
                            if (!tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.StunEffects);
                                tempCachedKeys.Add(tempKey);
                            }
                            tempAddingKeys.Add(tempKey);
                            break;
                        case AilmentPresets.Mute:
                            tempKey = nameof(AilmentPresets.Mute);
                            if (!tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.MuteEffects);
                                tempCachedKeys.Add(tempKey);
                            }
                            tempAddingKeys.Add(tempKey);
                            break;
                        case AilmentPresets.Freeze:
                            tempKey = nameof(AilmentPresets.Freeze);
                            if (!tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.FreezeEffects);
                                tempCachedKeys.Add(tempKey);
                            }
                            tempAddingKeys.Add(tempKey);
                            break;
                    }
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
            int randomSeed,
            out int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            return GetRandomRightHandAttackAnimation(weaponType.DataId, randomSeed, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public bool GetRandomLeftHandAttackAnimation(
            WeaponType weaponType,
            int randomSeed,
            out int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            return GetRandomLeftHandAttackAnimation(weaponType.DataId, randomSeed, out animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public bool GetSkillActivateAnimation(
            BaseSkill skill,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            return GetSkillActivateAnimation(skill.DataId, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public bool GetRightHandReloadAnimation(
            WeaponType weaponType,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            return GetRightHandReloadAnimation(weaponType.DataId, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public bool GetLeftHandReloadAnimation(
            WeaponType weaponType,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            return GetLeftHandReloadAnimation(weaponType.DataId, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public SkillActivateAnimationType UseSkillActivateAnimationType(BaseSkill skill)
        {
            return GetSkillActivateAnimationType(skill.DataId);
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

        public void PlayEquippedWeaponLaunch(bool isLeftHand)
        {
            if (!isLeftHand && rightHandEquipmentEntity != null)
                rightHandEquipmentEntity.PlayLaunch();
            if (isLeftHand && leftHandEquipmentEntity != null)
                leftHandEquipmentEntity.PlayLaunch();
        }

        public void PlayEquippedWeaponReload(bool isLeftHand)
        {
            if (!isLeftHand && rightHandEquipmentEntity != null)
                rightHandEquipmentEntity.PlayReload();
            if (isLeftHand && leftHandEquipmentEntity != null)
                leftHandEquipmentEntity.PlayReload();
        }

        public void PlayEquippedWeaponCharge(bool isLeftHand)
        {
            if (!isLeftHand && rightHandEquipmentEntity != null)
                rightHandEquipmentEntity.PlayCharge();
            if (isLeftHand && leftHandEquipmentEntity != null)
                leftHandEquipmentEntity.PlayCharge();
        }

        public virtual void AddingNewModel(GameObject newModel, EquipmentContainer equipmentContainer) { }

        public void SetIsDead(bool isDead)
        {
            this.isDead = isDead;
        }

        public void SetMoveAnimationSpeedMultiplier(float moveAnimationSpeedMultiplier)
        {
            this.moveAnimationSpeedMultiplier = moveAnimationSpeedMultiplier;
        }

        public void SetMovementState(MovementState movementState, ExtraMovementState extraMovementState, Vector2 direction2D, bool isFreezeAnimation)
        {
            if (!Application.isPlaying)
                return;
            this.movementState = movementState;
            this.extraMovementState = extraMovementState;
            this.direction2D = direction2D;
            this.isFreezeAnimation = isFreezeAnimation;
            PlayMoveAnimation();
        }

        public virtual void SetDefaultAnimations()
        {
            SetIsDead(false);
            SetMoveAnimationSpeedMultiplier(1f);
            SetMovementState(MovementState.IsGrounded, ExtraMovementState.None, Vector2.down, false);
        }

        /// <summary>
        /// Use this function to play hit animation when receive damage
        /// </summary>
        public virtual void PlayHitAnimation() { }

        /// <summary>
        /// Use this to get jump animation duration
        /// </summary>
        /// <returns></returns>
        public virtual float GetJumpAnimationDuration()
        {
            return 0f;
        }

        /// <summary>
        /// Use this function to play jump animation
        /// </summary>
        public virtual void PlayJumpAnimation() { }

        /// <summary>
        /// Use this function to play pickup animation
        /// </summary>
        public virtual void PlayPickupAnimation() { }

        public abstract void PlayMoveAnimation();
        public abstract void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f);
        public abstract void PlaySkillCastClip(int dataId, float duration);
        public abstract void PlayWeaponChargeClip(int dataId, bool isLeftHand);
        public abstract void StopActionAnimation();
        public abstract void StopSkillCastAnimation();
        public abstract void StopWeaponChargeAnimation();
        public abstract bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract SkillActivateAnimationType GetSkillActivateAnimationType(int dataId);
    }

    [System.Serializable]
    public struct EquipmentContainer
    {
        public string equipSocket;
        public Transform transform;
        public GameObject defaultModel;
        public GameObject[] instantiatedObjects;
#if UNITY_EDITOR
        [Header("Testing tools")]
        [Tooltip("Index of instantiate object which you want to test activation by character model's context menu")]
        public int activatingInstantiateObjectIndex;
#endif

        public void SetActiveDefaultModel(bool isActive)
        {
            if (defaultModel == null || defaultModel.activeSelf == isActive)
                return;
            defaultModel.SetActive(isActive);
        }

        public void DeactivateInstantiatedObjects()
        {
            if (instantiatedObjects == null || instantiatedObjects.Length == 0)
                return;
            // Deactivate all objects
            foreach (GameObject instantiatedObject in instantiatedObjects)
            {
                if (instantiatedObject == null || !instantiatedObject.activeSelf) continue;
                instantiatedObject.SetActive(false);
            }
        }

        public bool ActivateInstantiatedObject(int index)
        {
            if (instantiatedObjects == null || instantiatedObjects.Length == 0)
                return false;
            // Deactivate all objects
            DeactivateInstantiatedObjects();
            if (index < 0 || instantiatedObjects.Length <= index)
                return false;
            // Activate only one object
            if (instantiatedObjects[index] == null || instantiatedObjects[index].activeSelf)
                return false;
            instantiatedObjects[index].SetActive(true);
            return true;
        }
    }
}
