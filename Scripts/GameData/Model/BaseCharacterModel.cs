using System.Collections.Generic;
using UnityEngine;
using UtilsComponents;
using Cysharp.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacterModel : GameEntityModel, IMoveableModel, IHittableModel, IJumppableModel, IPickupableModel, IDeadableModel
    {
        public BaseCharacterModel MainModel { get; set; }
        public bool IsMainModel { get { return MainModel == this; } }
        public bool IsActiveModel { get; protected set; } = false;
        public bool IsTpsModel { get; internal set; }
        public bool IsFpsModel { get; internal set; }

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

        [Header("Equipment Layer Settings")]
        [SerializeField]
        protected bool setEquipmentLayerFollowEntity = true;
        public bool SetEquipmentLayerFollowEntity
        {
            get { return setEquipmentLayerFollowEntity; }
            set { setEquipmentLayerFollowEntity = value; }
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

        public CharacterModelManager Manager { get; protected set; }

        protected Dictionary<string, EquipmentModel> equippedModels = new Dictionary<string, EquipmentModel>();
        /// <summary>
        /// { equipPosition(String), model(EquipmentModel) }
        /// </summary>
        public Dictionary<string, EquipmentModel> EquippedModels
        {
            get { return IsMainModel ? equippedModels : MainModel.equippedModels; }
            set { MainModel.equippedModels = value; }
        }

        protected Dictionary<string, GameObject> equippedModelObjects = new Dictionary<string, GameObject>();
        /// <summary>
        /// { equipSocket(String), modelObject(GameObject) }
        /// </summary>
        public Dictionary<string, GameObject> EquippedModelObjects
        {
            get { return IsMainModel ? equippedModelObjects : MainModel.equippedModelObjects; }
            set { MainModel.equippedModelObjects = value; }
        }

        public override Dictionary<string, EffectContainer> CacheEffectContainers
        {
            get { return IsMainModel ? base.CacheEffectContainers : MainModel.CacheEffectContainers; }
        }

        protected Dictionary<int, VehicleCharacterModel> cacheVehicleModels;
        /// <summary>
        /// { vehicleType(Int32), vehicleCharacterModel(VehicleCharacterModel) }
        /// </summary>
        public Dictionary<int, VehicleCharacterModel> CacheVehicleModels
        {
            get { return IsMainModel ? cacheVehicleModels : MainModel.cacheVehicleModels; }
        }

        protected Dictionary<string, EquipmentContainer> cacheEquipmentModelContainers;
        /// <summary>
        /// { equipSocket(String), container(EquipmentModelContainer) }
        /// </summary>
        public Dictionary<string, EquipmentContainer> CacheEquipmentModelContainers
        {
            get { return IsMainModel ? cacheEquipmentModelContainers : MainModel.cacheEquipmentModelContainers; }
        }

        protected Dictionary<string, List<GameEffect>> cacheEffects = new Dictionary<string, List<GameEffect>>();
        /// <summary>
        /// { equipPosition(String), [ effect(GameEffect) ] }
        /// </summary>
        public Dictionary<string, List<GameEffect>> CacheEffects
        {
            get { return IsMainModel ? cacheEffects : MainModel.cacheEffects; }
        }

        protected BaseEquipmentEntity cacheRightHandEquipmentEntity;
        public BaseEquipmentEntity CacheRightHandEquipmentEntity
        {
            get { return IsMainModel ? cacheRightHandEquipmentEntity : MainModel.cacheRightHandEquipmentEntity; }
            set { MainModel.cacheRightHandEquipmentEntity = value; }
        }

        protected BaseEquipmentEntity cacheLeftHandEquipmentEntity;
        public BaseEquipmentEntity CacheLeftHandEquipmentEntity
        {
            get { return IsMainModel ? cacheLeftHandEquipmentEntity : MainModel.cacheLeftHandEquipmentEntity; }
            set { MainModel.cacheLeftHandEquipmentEntity = value; }
        }

        public IList<EquipWeapons> SelectableWeaponSets { get; protected set; }
        public byte EquipWeaponSet { get; protected set; }
        public bool IsWeaponsSheathed { get; protected set; }
        public IList<CharacterItem> EquipItems { get; protected set; }
        public IList<CharacterBuff> Buffs { get; protected set; }
        public bool IsDead { get; protected set; }
        public float MoveAnimationSpeedMultiplier { get; protected set; }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public Vector2 Direction2D { get; protected set; }
        public bool IsFreezeAnimation { get; protected set; }

        // Public events
        public System.Action<string> onEquipmentModelsInstantiated;
        public System.Action<string> onEquipmentModelsDestroyed;

        // Optimize garbage collector
        protected bool _isCacheDataInitialized = false;
        protected readonly List<string> _tempAddingKeys = new List<string>();
        protected readonly List<string> _tempCachedKeys = new List<string>();

        protected override void Awake()
        {
            base.Awake();

            Manager = GetComponent<CharacterModelManager>();
            if (Manager == null)
                Manager = GetComponentInParent<CharacterModelManager>(true);

            // Can't find manager, this component may attached to non-character entities, so assume that this character model is main model
            if (Manager != null)
            {
                CacheEntity = Manager.Entity;
                Manager.InitTpsModel(this);
            }
            else
            {
                MainModel = this;
                InitCacheData();
                SwitchModel(null);
            }
        }

        internal void InitCacheData()
        {
            if (_isCacheDataInitialized)
                return;
            _isCacheDataInitialized = true;

            cacheVehicleModels = new Dictionary<int, VehicleCharacterModel>();
            if (vehicleModels != null && vehicleModels.Length > 0)
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
                    cacheVehicleModels[vehicleModel.vehicleType.DataId] = vehicleModel;
                }
            }

            cacheEquipmentModelContainers = new Dictionary<string, EquipmentContainer>();
            if (equipmentContainers != null && equipmentContainers.Length > 0)
            {
                foreach (EquipmentContainer equipmentContainer in equipmentContainers)
                {
                    if (!cacheEquipmentModelContainers.ContainsKey(equipmentContainer.equipSocket))
                        cacheEquipmentModelContainers[equipmentContainer.equipSocket] = equipmentContainer;
                }
            }

            cacheEffects = new Dictionary<string, List<GameEffect>>();
        }

        protected void UpdateObjectsWhenSwitch()
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

        protected void RevertObjectsWhenSwitch()
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
            if (previousModel != null)
            {
                previousModel.IsActiveModel = false;
                previousModel.OnSwitchingToAnotherModel();
                previousModel.RevertObjectsWhenSwitch();
                OnSwitchingToThisModel();
                SetDefaultAnimations();
                SetIsDead(previousModel.IsDead);
                SetMoveAnimationSpeedMultiplier(previousModel.MoveAnimationSpeedMultiplier);
                SetMovementState(previousModel.MovementState, previousModel.ExtraMovementState, previousModel.Direction2D, previousModel.IsFreezeAnimation);
                SetEquipWeapons(previousModel.SelectableWeaponSets, previousModel.EquipWeaponSet, previousModel.IsWeaponsSheathed);
                SetEquipItems(previousModel.EquipItems);
                SetBuffs(previousModel.Buffs);
                IsActiveModel = true;
                UpdateObjectsWhenSwitch();
                OnSwitchedToThisModel();
                previousModel.OnSwitchedToAnotherModel();
            }
            else
            {
                OnSwitchingToThisModel();
                SetDefaultAnimations();
                SetEquipWeapons(SelectableWeaponSets, EquipWeaponSet, IsWeaponsSheathed);
                SetEquipItems(EquipItems);
                SetBuffs(Buffs);
                IsActiveModel = true;
                UpdateObjectsWhenSwitch();
                OnSwitchedToThisModel();
            }
        }

        internal virtual void OnSwitchingToAnotherModel()
        {

        }

        internal virtual void OnSwitchedToAnotherModel()
        {

        }

        internal virtual void OnSwitchingToThisModel()
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
            if (EquipmentContainers != null && EquipmentContainers.Length > 0)
            {
                for (int i = 0; i < EquipmentContainers.Length; ++i)
                {
                    EquipmentContainers[i].DeactivateInstantiatedObjects();
                    EquipmentContainers[i].SetActiveDefaultModel(true);
                }
            }
        }

        [ContextMenu("Activate Instantiated Object", false, 1000303)]
        public void ActivateInstantiatedObject()
        {
            if (EquipmentContainers != null && EquipmentContainers.Length > 0)
            {
                for (int i = 0; i < EquipmentContainers.Length; ++i)
                {
                    EquipmentContainers[i].SetActiveDefaultModel(false);
                    EquipmentContainers[i].ActivateInstantiatedObject(EquipmentContainers[i].activatingInstantiateObjectIndex);
                }
            }
        }
#endif

        public virtual void SetEquipWeapons(IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            SelectableWeaponSets = selectableWeaponSets;
            EquipWeaponSet = equipWeaponSet;
            IsWeaponsSheathed = isWeaponsSheathed;
            UpdateEquipmentModels();
        }

        public virtual void SetEquipItems(IList<CharacterItem> equipItems)
        {
            EquipItems = equipItems;
            UpdateEquipmentModels();
        }

        private void UpdateEquipmentModels()
        {
            // Prepared data
            EquipmentContainer tempContainer;
            EquipmentModel tempEquipmentModel;
            GameObject tempEquipmentObject;
            Dictionary<string, EquipmentModel> showingModels = new Dictionary<string, EquipmentModel>();
            Dictionary<string, EquipmentModel> storingModels = new Dictionary<string, EquipmentModel>();
            HashSet<string> unequippingSockets = new HashSet<string>(EquippedModels.Keys);

            // Setup equipping models from equip items
            if (EquipItems != null && EquipItems.Count > 0)
            {
                foreach (CharacterItem equipItem in EquipItems)
                {
                    IArmorItem armorItem = equipItem.GetArmorItem();
                    if (armorItem == null)
                        continue;
                    SetupEquippingModels(showingModels, storingModels, unequippingSockets, armorItem.EquipmentModels, equipItem.dataId, equipItem.level, armorItem.GetEquipPosition());
                }
            }

            // Setup equipping models from equip weapons
            EquipWeapons equipWeapons;
            if (IsWeaponsSheathed || SelectableWeaponSets == null || SelectableWeaponSets.Count == 0)
            {
                equipWeapons = new EquipWeapons();
            }
            else
            {
                if (EquipWeaponSet >= SelectableWeaponSets.Count)
                {
                    // Issues occuring, so try to simulate data
                    // Create a new list to make sure that changes won't be applied to the source list (the source list must be readonly)
                    SelectableWeaponSets = new List<EquipWeapons>(SelectableWeaponSets);
                    while (EquipWeaponSet >= SelectableWeaponSets.Count)
                    {
                        SelectableWeaponSets.Add(new EquipWeapons());
                    }
                }
                equipWeapons = SelectableWeaponSets[EquipWeaponSet];
            }
            IEquipmentItem rightHandItem = equipWeapons.GetRightHandEquipmentItem();
            IEquipmentItem leftHandItem = equipWeapons.GetLeftHandEquipmentItem();
            if (rightHandItem != null && rightHandItem.IsWeapon())
                SetupEquippingModels(showingModels, storingModels, unequippingSockets, (rightHandItem as IWeaponItem).EquipmentModels, equipWeapons.rightHand.dataId, equipWeapons.rightHand.level, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandItem != null && leftHandItem.IsWeapon())
                SetupEquippingModels(showingModels, storingModels, unequippingSockets, (leftHandItem as IWeaponItem).OffHandEquipmentModels, equipWeapons.leftHand.dataId, equipWeapons.leftHand.level, GameDataConst.EQUIP_POSITION_LEFT_HAND);
            if (leftHandItem != null && leftHandItem.IsShield())
                SetupEquippingModels(showingModels, storingModels, unequippingSockets, (leftHandItem as IShieldItem).EquipmentModels, equipWeapons.leftHand.dataId, equipWeapons.leftHand.level, GameDataConst.EQUIP_POSITION_LEFT_HAND);

            if (SelectableWeaponSets != null && SelectableWeaponSets.Count > 0)
            {
                for (byte i = 0; i < SelectableWeaponSets.Count; ++i)
                {
                    if (IsWeaponsSheathed || i != EquipWeaponSet)
                    {
                        equipWeapons = SelectableWeaponSets[i];
                        rightHandItem = equipWeapons.GetRightHandEquipmentItem();
                        leftHandItem = equipWeapons.GetLeftHandEquipmentItem();
                        if (rightHandItem != null && rightHandItem.IsWeapon())
                            SetupEquippingModels(showingModels, storingModels, unequippingSockets, (rightHandItem as IWeaponItem).SheathModels, equipWeapons.rightHand.dataId, equipWeapons.rightHand.level, ZString.Concat(GameDataConst.EQUIP_POSITION_RIGHT_HAND, "_", i), true, i);
                        if (leftHandItem != null && leftHandItem.IsWeapon())
                            SetupEquippingModels(showingModels, storingModels, unequippingSockets, (leftHandItem as IWeaponItem).OffHandSheathModels, equipWeapons.leftHand.dataId, equipWeapons.leftHand.level, ZString.Concat(GameDataConst.EQUIP_POSITION_LEFT_HAND, "_", i), true, i);
                        if (leftHandItem != null && leftHandItem.IsShield())
                            SetupEquippingModels(showingModels, storingModels, unequippingSockets, (leftHandItem as IShieldItem).SheathModels, equipWeapons.leftHand.dataId, equipWeapons.leftHand.level, ZString.Concat(GameDataConst.EQUIP_POSITION_LEFT_HAND, "_", i), true, i);
                    }
                }
            }

            // Destroy unequipped item models, and show default models
            foreach (string unequippingSocket in unequippingSockets)
            {
                ClearEquippedModel(unequippingSocket);

                if (!CacheEquipmentModelContainers.TryGetValue(unequippingSocket, out tempContainer))
                    continue;

                tempContainer.DeactivateInstantiatedObjects();
                tempContainer.SetActiveDefaultModel(true);
            }

            foreach (string equipSocket in showingModels.Keys)
            {
                ClearEquippedModel(equipSocket);

                if (!CacheEquipmentModelContainers.TryGetValue(equipSocket, out tempContainer))
                    continue;

                tempEquipmentModel = showingModels[equipSocket];
                tempEquipmentObject = null;
                if (tempEquipmentModel.useInstantiatedObject)
                {
                    // Activate the instantiated object
                    if (!tempContainer.ActivateInstantiatedObject(tempEquipmentModel.instantiatedObjectIndex))
                        continue;
                    tempContainer.SetActiveDefaultModel(false);
                    tempEquipmentObject = tempContainer.instantiatedObjects[tempEquipmentModel.instantiatedObjectIndex];
                }
                else
                {
                    // Instantiate model, setup transform and activate game object
                    tempContainer.DeactivateInstantiatedObjects();
                    tempContainer.SetActiveDefaultModel(false);
                    if (tempContainer.transform != null)
                    {
                        tempEquipmentObject = Instantiate(tempEquipmentModel.meshPrefab, tempContainer.transform);
                        tempEquipmentObject.transform.localPosition = tempEquipmentModel.localPosition;
                        tempEquipmentObject.transform.localEulerAngles = tempEquipmentModel.localEulerAngles;
                        if (!tempEquipmentModel.doNotChangeScale)
                            tempEquipmentObject.transform.localScale = tempEquipmentModel.localScale.Equals(Vector3.zero) ? Vector3.one : tempEquipmentModel.localScale;
                        tempEquipmentObject.gameObject.SetActive(true);
                        if (SetEquipmentLayerFollowEntity)
                            tempEquipmentObject.gameObject.GetOrAddComponent<SetLayerFollowGameObject>((comp) => comp.source = CacheEntity.gameObject);
                        else
                            tempEquipmentObject.gameObject.SetLayerRecursively(EquipmentLayer, true);
                        tempEquipmentObject.RemoveComponentsInChildren<Collider>(false);
                        AddingNewModel(tempEquipmentModel, tempEquipmentObject, tempContainer);
                        EquippedModelObjects[equipSocket] = tempEquipmentObject;
                    }
                }

                // Setup equipment entity
                if (tempEquipmentObject != null)
                {
                    BaseEquipmentEntity tempEquipmentEntity = tempEquipmentObject.GetComponent<BaseEquipmentEntity>();
                    if (tempEquipmentEntity != null)
                        tempEquipmentEntity.Setup(this, tempEquipmentModel.equipPosition, tempEquipmentModel.itemLevel);
                    if (CacheRightHandEquipmentEntity == null && GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(tempEquipmentModel.equipPosition))
                        CacheRightHandEquipmentEntity = tempEquipmentEntity;
                    if (CacheLeftHandEquipmentEntity == null && GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(tempEquipmentModel.equipPosition))
                        CacheLeftHandEquipmentEntity = tempEquipmentEntity;
                }
            }
            EquippedModels = storingModels;
        }

        private void ClearEquippedModel(string equipSocket)
        {
            if (EquippedModels.TryGetValue(equipSocket, out EquipmentModel equipmentModel))
            {
                if (GameDataConst.EQUIP_POSITION_RIGHT_HAND.Equals(equipmentModel.equipPosition))
                    CacheRightHandEquipmentEntity = null;
                if (GameDataConst.EQUIP_POSITION_LEFT_HAND.Equals(equipmentModel.equipPosition))
                    CacheLeftHandEquipmentEntity = null;
            }

            if (EquippedModelObjects.TryGetValue(equipSocket, out GameObject equipmentObject))
            {
                Destroy(equipmentObject);
                EquippedModelObjects.Remove(equipSocket);
            }
        }

        private void SetupEquippingModels(Dictionary<string, EquipmentModel> showingModels, Dictionary<string, EquipmentModel> storingModels, HashSet<string> unequippingSockets, EquipmentModel[] equipmentModels, int itemDataId, int itemLevel, string equipPosition, bool isSheathModels = false, byte equipWeaponSet = 0)
        {
            if (equipmentModels == null || equipmentModels.Length == 0 || string.IsNullOrWhiteSpace(equipPosition))
                return;

            foreach (EquipmentModel model in equipmentModels)
            {
                if (string.IsNullOrEmpty(model.equipSocket) || (!model.useInstantiatedObject && !model.meshPrefab))
                {
                    // Required data are empty, skip it
                    continue;
                }

                string equipSocket = model.equipSocket;
                if (isSheathModels)
                {
                    // Can instantiates multiple sheath models into 1 socket, so create new collection name
                    equipSocket = ZString.Concat(equipSocket, "_SHEATH_", equipWeaponSet, "_", equipPosition);
                    // Create a new container for this sheath weapons
                    if (!CacheEquipmentModelContainers.ContainsKey(equipSocket) && CacheEquipmentModelContainers.ContainsKey(model.equipSocket))
                        CacheEquipmentModelContainers[equipSocket] = CacheEquipmentModelContainers[model.equipSocket];
                }

                if (!storingModels.TryGetValue(equipSocket, out EquipmentModel storedModel) || storedModel.priority < model.priority || (storedModel.equipPosition == equipPosition && storedModel.itemLevel < itemLevel))
                {
                    if (isSheathModels && model.useSpecificSheathEquipWeaponSet && model.specificSheathEquipWeaponSet != equipWeaponSet)
                    {
                        // Don't show it because `specificSheathEquipWeaponSet` is not equals to `equipWeaponSet`
                        continue;
                    }

                    if (EquippedModels.TryGetValue(equipSocket, out EquipmentModel equippedModel)
                        && equippedModel.itemDataId == itemDataId
                        && equippedModel.itemLevel == itemLevel
                        && equippedModel.priority == model.priority)
                    {
                        // Same view data, so don't destroy and don't instantiates this model object
                        showingModels.Remove(equipSocket);
                        storingModels[equipSocket] = equippedModel;
                        unequippingSockets.Remove(equipSocket);
                        continue;
                    }

                    EquipmentModel clonedModel = model.Clone();
                    clonedModel.itemDataId = itemDataId;
                    clonedModel.itemLevel = itemLevel;
                    clonedModel.equipPosition = equipPosition;
                    showingModels[equipSocket] = clonedModel;
                    storingModels[equipSocket] = clonedModel;
                    unequippingSockets.Remove(equipSocket);
                }
            }
        }

        protected void CreateCacheEffect(string buffId, List<GameEffect> effects)
        {
            if (effects == null || CacheEffects.ContainsKey(buffId))
                return;
            CacheEffects[buffId] = effects;
        }

        protected void DestroyCacheEffect(string buffId)
        {
            if (!string.IsNullOrEmpty(buffId) && CacheEffects.TryGetValue(buffId, out List<GameEffect> oldEffects) && oldEffects != null)
            {
                foreach (GameEffect effect in oldEffects)
                {
                    if (effect == null) continue;
                    effect.DestroyEffect();
                }
                CacheEffects.Remove(buffId);
            }
        }

        protected void DestroyCacheEffects()
        {
            foreach (string buffId in CacheEffects.Keys)
            {
                DestroyCacheEffect(buffId);
            }
        }

        public virtual void SetBuffs(IList<CharacterBuff> buffs)
        {
            Buffs = buffs;
            // Temp old keys
            _tempCachedKeys.Clear();
            _tempCachedKeys.AddRange(CacheEffects.Keys);
            // Prepare data
            _tempAddingKeys.Clear();
            // Loop new buffs to prepare adding keys
            if (buffs != null && buffs.Count > 0)
            {
                string tempKey;
                foreach (CharacterBuff buff in buffs)
                {
                    // Buff effects
                    tempKey = buff.GetKey();
                    if (!_tempCachedKeys.Contains(tempKey))
                    {
                        // If old buffs not contains this buff, add this buff effect
                        InstantiateBuffEffect(tempKey, buff.GetBuff().GetBuff().effects);
                        _tempCachedKeys.Add(tempKey);
                    }
                    _tempAddingKeys.Add(tempKey);
                    // Ailment effects
                    switch (buff.GetBuff().GetBuff().ailment)
                    {
                        case AilmentPresets.Stun:
                            tempKey = nameof(AilmentPresets.Stun);
                            if (!_tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.StunEffects);
                                _tempCachedKeys.Add(tempKey);
                            }
                            _tempAddingKeys.Add(tempKey);
                            break;
                        case AilmentPresets.Mute:
                            tempKey = nameof(AilmentPresets.Mute);
                            if (!_tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.MuteEffects);
                                _tempCachedKeys.Add(tempKey);
                            }
                            _tempAddingKeys.Add(tempKey);
                            break;
                        case AilmentPresets.Freeze:
                            tempKey = nameof(AilmentPresets.Freeze);
                            if (!_tempCachedKeys.Contains(tempKey))
                            {
                                InstantiateBuffEffect(tempKey, GameInstance.Singleton.FreezeEffects);
                                _tempCachedKeys.Add(tempKey);
                            }
                            _tempAddingKeys.Add(tempKey);
                            break;
                    }
                }
            }
            // Remove effects which removed from new buffs list
            // Loop old keys to destroy removed buffs
            foreach (string key in _tempCachedKeys)
            {
                if (!_tempAddingKeys.Contains(key))
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

        public BaseEquipmentEntity GetRightHandEquipmentEntity()
        {
            return CacheRightHandEquipmentEntity;
        }

        public BaseEquipmentEntity GetLeftHandEquipmentEntity()
        {
            return CacheLeftHandEquipmentEntity;
        }

        public Transform GetRightHandMissileDamageTransform()
        {
            if (CacheRightHandEquipmentEntity != null)
                return CacheRightHandEquipmentEntity.missileDamageTransform;
            return null;
        }

        public Transform GetLeftHandMissileDamageTransform()
        {
            if (CacheLeftHandEquipmentEntity != null)
                return CacheLeftHandEquipmentEntity.missileDamageTransform;
            return null;
        }

        public void PlayEquippedWeaponLaunch(bool isLeftHand)
        {
            if (!isLeftHand && CacheRightHandEquipmentEntity != null)
                CacheRightHandEquipmentEntity.PlayLaunch();
            if (isLeftHand && CacheLeftHandEquipmentEntity != null)
                CacheLeftHandEquipmentEntity.PlayLaunch();
        }

        public void PlayEquippedWeaponReload(bool isLeftHand)
        {
            if (!isLeftHand && CacheRightHandEquipmentEntity != null)
                CacheRightHandEquipmentEntity.PlayReload();
            if (isLeftHand && CacheLeftHandEquipmentEntity != null)
                CacheLeftHandEquipmentEntity.PlayReload();
        }

        public void PlayEquippedWeaponReloaded(bool isLeftHand)
        {
            if (!isLeftHand && CacheRightHandEquipmentEntity != null)
                CacheRightHandEquipmentEntity.PlayReloaded();
            if (isLeftHand && CacheLeftHandEquipmentEntity != null)
                CacheLeftHandEquipmentEntity.PlayReloaded();
        }

        public void PlayEquippedWeaponCharge(bool isLeftHand)
        {
            if (!isLeftHand && CacheRightHandEquipmentEntity != null)
                CacheRightHandEquipmentEntity.PlayCharge();
            if (isLeftHand && CacheLeftHandEquipmentEntity != null)
                CacheLeftHandEquipmentEntity.PlayCharge();
        }

        public virtual void AddingNewModel(EquipmentModel data, GameObject newModel, EquipmentContainer equipmentContainer) { }

        public void SetIsDead(bool isDead)
        {
            IsDead = isDead;
        }

        public void SetMoveAnimationSpeedMultiplier(float moveAnimationSpeedMultiplier)
        {
            MoveAnimationSpeedMultiplier = moveAnimationSpeedMultiplier;
        }

        public void SetMovementState(MovementState movementState, ExtraMovementState extraMovementState, Vector2 direction2D, bool isFreezeAnimation)
        {
            if (!Application.isPlaying)
                return;
            MovementState = movementState;
            ExtraMovementState = extraMovementState;
            Direction2D = direction2D;
            IsFreezeAnimation = isFreezeAnimation;
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
        public abstract int GetRightHandAttackRandomMax(int dataId);
        public abstract int GetLeftHandAttackRandomMax(int dataId);
        /// <summary>
        /// Get random right-hand attack animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="randomSeed"></param>
        /// <param name="animationIndex"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get random left-hand attack animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="randomSeed"></param>
        /// <param name="animationIndex"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get right-hand attack animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="animationIndex"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get left-hand attack animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="animationIndex"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get skill activate animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get right-hand reload animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        /// <summary>
        /// Get left-hand reload animation, if `triggerDurations`'s length is 0/`totalDuration` <= 0, it will wait other methods to use as `triggerDurations`/`totalDuration` (such as animtion clip event, state machine behaviour).
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="animSpeedRate"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public abstract bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration);
        public abstract SkillActivateAnimationType GetSkillActivateAnimationType(int dataId);
    }
}
