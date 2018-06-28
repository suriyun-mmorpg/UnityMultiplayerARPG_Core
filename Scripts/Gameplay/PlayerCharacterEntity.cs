using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class PlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [HideInInspector]
        public WarpPortalEntity warpingPortal;
        public BasePlayerCharacterController controllerPrefab;

        #region Sync data
        [SerializeField]
        protected SyncFieldShort statPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldShort skillPoint = new SyncFieldShort();
        [SerializeField]
        protected SyncFieldInt gold = new SyncFieldInt();
        // List
        [SerializeField]
        protected SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
        [SerializeField]
        protected SyncListCharacterQuest quests = new SyncListCharacterQuest();
        #endregion

        #region Sync data actions
        public System.Action<int> onShowNpcDialog;
        public System.Action<short> onStatPointChange;
        public System.Action<short> onSkillPointChange;
        public System.Action<int> onGoldChange;
        // List
        public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
        public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
        #endregion

        #region Interface implementation
        public short StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
        public short SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
        public int Gold { get { return gold.Value; } set { gold.Value = value; } }
        public string CurrentMapName { get { return SceneManager.GetActiveScene().name; } set { } }
        public Vector3 CurrentPosition
        {
            get { return CacheTransform.position; }
            set
            {
                CacheNetTransform.Teleport(value, CacheTransform.rotation);
                CacheTransform.position = value;
            }
        }
        public string RespawnMapName { get; set; }
        public Vector3 RespawnPosition { get; set; }
        public int LastUpdate { get; set; }

        public IList<CharacterHotkey> Hotkeys
        {
            get { return hotkeys; }
            set
            {
                hotkeys.Clear();
                foreach (var entry in value)
                    hotkeys.Add(entry);
            }
        }

        public IList<CharacterQuest> Quests
        {
            get { return quests; }
            set
            {
                quests.Clear();
                foreach (var entry in value)
                    quests.Add(entry);
            }
        }
        #endregion

        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Movement Settings")]
        public float groundingDistance = 0.1f;
        public float jumpHeight = 2f;
        public float gravityRate = 1f;
        public float angularSpeed = 120f;
        #endregion

        #region Protected data
        public Queue<Vector3> navPaths { get; protected set; }
        public Vector3 moveDirection { get; protected set; }
        public bool isJumping { get; protected set; }
        public bool isGrounded { get; protected set; }
        public NpcDialog currentNpcDialog { get; protected set; }

        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }
        #endregion

        #region Cache components
        private Rigidbody cacheRigidbody;
        public Rigidbody CacheRigidbody
        {
            get
            {
                if (cacheRigidbody == null)
                    cacheRigidbody = GetComponent<Rigidbody>();
                return cacheRigidbody;
            }
        }

        private LiteNetLibTransform cacheNetTransform;
        public LiteNetLibTransform CacheNetTransform
        {
            get
            {
                if (cacheNetTransform == null)
                    cacheNetTransform = GetComponent<LiteNetLibTransform>();
                return cacheNetTransform;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            CacheRigidbody.useGravity = false;
            gameObject.tag = gameInstance.playerTag;
            StopMove();
        }

        protected override void Start()
        {
            base.Start();
            if (IsOwnerClient)
            {
                if (BasePlayerCharacterController.Singleton == null)
                {
                    var controller = Instantiate(controllerPrefab);
                    controller.CharacterEntity = this;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsServer && !IsOwnerClient)
                return;

            var velocity = CacheRigidbody.velocity;
            if (!IsDead())
            {
                var moveDirectionMagnitude = moveDirection.magnitude;
                if (!IsPlayingActionAnimation() && moveDirectionMagnitude != 0)
                {
                    if (moveDirectionMagnitude > 1)
                        moveDirection = moveDirection.normalized;

                    var targetVelocity = moveDirection * CacheMoveSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -CacheMoveSpeed, CacheMoveSpeed);
                    velocityChange.y = 0;
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -CacheMoveSpeed, CacheMoveSpeed);
                    CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                    // Calculate rotation on client only, will send update to server later
                    CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.fixedDeltaTime);
                }

                BaseCharacterEntity tempCharacterEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempCharacterEntity))
                {
                    var targetDirection = (tempCharacterEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (targetDirection.magnitude != 0f)
                    {
                        var fromRotation = CacheTransform.rotation.eulerAngles;
                        var lookAtRotation = Quaternion.LookRotation(targetDirection).eulerAngles;
                        lookAtRotation = new Vector3(fromRotation.x, lookAtRotation.y, fromRotation.z);
                        CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.Euler(lookAtRotation), angularSpeed * Time.fixedDeltaTime);
                    }
                }
                // Jump
                if (isGrounded && isJumping)
                {
                    CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    isJumping = false;
                }
            }

            if (Mathf.Abs(velocity.y) > groundingDistance)
                isGrounded = false;

            // We apply gravity manually for more tuning control
            CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (!IsServer && !IsOwnerClient)
                return;

            if (navPaths != null)
            {
                if (navPaths.Count > 0)
                {
                    var target = navPaths.Peek();
                    target = new Vector3(target.x, 0, target.z);
                    var currentPosition = CacheTransform.position;
                    currentPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
                    moveDirection = (target - currentPosition).normalized;
                    if (Vector3.Distance(target, currentPosition) < stoppingDistance)
                        navPaths.Dequeue();
                }
                else
                    StopMove();
            }
        }

        public virtual void StopMove()
        {
            navPaths = null;
            moveDirection = Vector3.zero;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
                manager.WarpCharacter(this, RespawnMapName, RespawnPosition);
        }

        public override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is MonsterCharacterEntity;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is MonsterCharacterEntity;
        }

        #region Setup functions
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            statPoint.sendOptions = SendOptions.ReliableOrdered;
            statPoint.forOwnerOnly = true;
            skillPoint.sendOptions = SendOptions.ReliableOrdered;
            skillPoint.forOwnerOnly = true;
            gold.sendOptions = SendOptions.ReliableOrdered;
            gold.forOwnerOnly = false;

            hotkeys.forOwnerOnly = true;
            quests.forOwnerOnly = true;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Setup network components
            CacheNetTransform.ownerClientCanSendTransform = true;
            CacheNetTransform.ownerClientNotInterpolate = false;
            // On data changes events
            statPoint.onChange += OnStatPointChange;
            skillPoint.onChange += OnSkillPointChange;
            gold.onChange += OnGoldChange;
            // On list changes events
            hotkeys.onOperation += OnHotkeysOperation;
            quests.onOperation += OnQuestsOperation;
            // Register Network functions
            RegisterNetFunction("SwapOrMergeItem", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((fromIndex, toIndex) => NetFuncSwapOrMergeItem(fromIndex, toIndex)));
            RegisterNetFunction("AddAttribute", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((attributeIndex, amount) => NetFuncAddAttribute(attributeIndex, amount)));
            RegisterNetFunction("AddSkill", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((skillIndex, amount) => NetFuncAddSkill(skillIndex, amount)));
            RegisterNetFunction("Respawn", new LiteNetLibFunction(NetFuncRespawn));
            RegisterNetFunction("AssignHotkey", new LiteNetLibFunction<NetFieldString, NetFieldByte, NetFieldInt>((hotkeyId, type, dataId) => NetFuncAssignHotkey(hotkeyId, type, dataId)));
            RegisterNetFunction("NpcActivate", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncNpcActivate(objectId)));
            RegisterNetFunction("ShowNpcDialog", new LiteNetLibFunction<NetFieldInt>((npcDialogId) => NetFuncShowNpcDialog(npcDialogId)));
            RegisterNetFunction("SelectNpcDialogMenu", new LiteNetLibFunction<NetFieldInt>((menuIndex) => NetFuncSelectNpcDialogMenu(menuIndex)));
            RegisterNetFunction("BuyNpcItem", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((itemIndex, amount) => NetFuncBuyNpcItem(itemIndex, amount)));
            RegisterNetFunction("EnterWarp", new LiteNetLibFunction(() => NetFuncEnterWarp()));
            RegisterNetFunction("Build", new LiteNetLibFunction<NetFieldInt, NetFieldVector3, NetFieldQuaternion, NetFieldUInt>((itemIndex, position, rotation, parentObjectId) => NetFuncBuild(itemIndex, position, rotation, parentObjectId)));
            RegisterNetFunction("DestroyBuild", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncDestroyBuild(objectId)));
            RegisterNetFunction("SellItem", new LiteNetLibFunction<NetFieldInt, NetFieldShort>((nonEquipIndex, amount) => NetFuncSellItem(nonEquipIndex, amount)));
        }
        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // On data changes events
            statPoint.onChange -= OnStatPointChange;
            skillPoint.onChange -= OnSkillPointChange;
            gold.onChange += OnGoldChange;
            // On list changes events
            hotkeys.onOperation -= OnHotkeysOperation;
            quests.onOperation -= OnQuestsOperation;

            if (IsOwnerClient && BasePlayerCharacterController.Singleton != null)
                Destroy(BasePlayerCharacterController.Singleton.gameObject);
        }

        #region Net functions callbacks
        protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                fromIndex < 0 ||
                fromIndex >= NonEquipItems.Count ||
                toIndex < 0 ||
                toIndex >= NonEquipItems.Count)
                return;

            var fromItem = NonEquipItems[fromIndex];
            var toItem = NonEquipItems[toIndex];
            if (!fromItem.IsValid() || !toItem.IsValid())
                return;

            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    NonEquipItems[fromIndex] = CharacterItem.Empty;
                    NonEquipItems[toIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    NonEquipItems[fromIndex] = fromItem;
                    NonEquipItems[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                NonEquipItems[fromIndex] = toItem;
                NonEquipItems[toIndex] = fromItem;
            }
        }

        protected void NetFuncAddAttribute(int attributeIndex, short amount)
        {
            if (IsDead() ||
                attributeIndex < 0 ||
                attributeIndex >= Attributes.Count ||
                amount <= 0 ||
                amount > StatPoint)
                return;

            var attribute = Attributes[attributeIndex];
            if (!attribute.CanIncrease(this))
                return;

            attribute.Increase(amount);
            Attributes[attributeIndex] = attribute;

            StatPoint -= amount;
        }

        protected void NetFuncAddSkill(int skillIndex, short amount)
        {
            if (IsDead() ||
                skillIndex < 0 ||
                skillIndex >= Skills.Count ||
                amount <= 0 ||
                amount > SkillPoint)
                return;

            var skill = Skills[skillIndex];
            if (!skill.CanLevelUp(this))
                return;

            skill.LevelUp(amount);
            Skills[skillIndex] = skill;

            SkillPoint -= amount;
        }

        protected void NetFuncRespawn()
        {
            Respawn();
        }

        protected void NetFuncAssignHotkey(string hotkeyId, byte type, int dataId)
        {
            var characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = (HotkeyType)type;
            characterHotkey.dataId = dataId;
            var hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
        }

        protected void NetFuncNpcActivate(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;

            LiteNetLibIdentity identity;
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out identity))
                return;

            var npcEntity = identity.GetComponent<NpcEntity>();
            if (npcEntity == null)
                return;

            if (Vector3.Distance(CacheTransform.position, npcEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            currentNpcDialog = npcEntity.startDialog;
            if (currentNpcDialog != null)
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncShowNpcDialog(int npcDialogDataId)
        {
            if (onShowNpcDialog != null)
                onShowNpcDialog(npcDialogDataId);
        }

        protected void NetFuncSelectNpcDialogMenu(int menuIndex)
        {
            if (currentNpcDialog == null)
                return;
            var menus = currentNpcDialog.menus;
            NpcDialogMenu selectedMenu;
            switch (currentNpcDialog.type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex < 0 || menuIndex >= menus.Length)
                        return;
                    selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(this) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        currentNpcDialog = null;
                        RequestShowNpcDialog(0);
                        return;
                    }
                    currentNpcDialog = selectedMenu.dialog;
                    RequestShowNpcDialog(currentNpcDialog.DataId);
                    break;
                case NpcDialogType.Quest:
                    NetFuncSelectNpcDialogQuestMenu(menuIndex);
                    break;
            }
        }

        protected void NetFuncSelectNpcDialogQuestMenu(int menuIndex)
        {
            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Quest || currentNpcDialog.quest == null)
            {
                currentNpcDialog = null;
                RequestShowNpcDialog(0);
                return;
            }
            switch (menuIndex)
            {
                case NpcDialog.QUEST_ACCEPT_MENU_INDEX:
                    NetFuncAcceptQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questAcceptedDialog;
                    break;
                case NpcDialog.QUEST_DECLINE_MENU_INDEX:
                    currentNpcDialog = currentNpcDialog.questDeclinedDialog;
                    break;
                case NpcDialog.QUEST_ABANDON_MENU_INDEX:
                    NetFuncAbandonQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questAbandonedDialog;
                    break;
                case NpcDialog.QUEST_COMPLETE_MENU_INDEX:
                    NetFuncCompleteQuest(currentNpcDialog.quest.DataId);
                    currentNpcDialog = currentNpcDialog.questCompletedDailog;
                    break;
            }
            if (currentNpcDialog == null)
                RequestShowNpcDialog(0);
            else
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncBuyNpcItem(int itemIndex, short amount)
        {
            if (currentNpcDialog == null)
                return;
            var sellItems = currentNpcDialog.sellItems;
            if (sellItems == null || itemIndex < 0 || itemIndex >= sellItems.Length)
                return;
            var sellItem = sellItems[itemIndex];
            if (Gold < sellItem.sellPrice * amount)
            {
                // TODO: May send not enough gold message
                return;
            }
            var dataId = sellItem.item.DataId;
            if (IncreasingItemsWillOverwhelming(dataId, amount))
            {
                // TODO: May send overwhelming message
                return;
            }
            Gold -= sellItem.sellPrice * amount;
            this.IncreaseItems(dataId, 1, amount);
        }

        protected void NetFuncAcceptQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        protected void NetFuncAbandonQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = quests[indexOfQuest];
            if (characterQuest.isComplete)
                return;
            quests.RemoveAt(indexOfQuest);
        }

        protected void NetFuncCompleteQuest(int questDataId)
        {
            var indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            var characterQuest = quests[indexOfQuest];
            if (!characterQuest.IsAllTasksDone(this))
                return;
            if (characterQuest.isComplete)
                return;
            var tasks = quest.tasks;
            foreach (var task in tasks)
            {
                switch (task.taskType)
                {
                    case QuestTaskType.CollectItem:
                        this.DecreaseItems(task.itemAmount.item.DataId, task.itemAmount.amount);
                        break;
                }
            }
            IncreaseExp(quest.rewardExp);
            IncreaseGold(quest.rewardGold);
            var rewardItems = quest.rewardItems;
            if (rewardItems != null && rewardItems.Length > 0)
            {
                foreach (var rewardItem in rewardItems)
                {
                    if (rewardItem.item != null && rewardItem.amount > 0)
                        this.IncreaseItems(rewardItem.item.DataId, 1, rewardItem.amount);
                }
            }
            characterQuest.isComplete = true;
            if (!quest.canRepeat)
                quests[indexOfQuest] = characterQuest;
            else
                quests.RemoveAt(indexOfQuest);
        }

        protected void NetFuncEnterWarp()
        {
            if (IsDead() || IsPlayingActionAnimation() || warpingPortal == null)
                return;

            warpingPortal.EnterWarp(this);
        }

        protected void NetFuncBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (IsDead() ||
                IsPlayingActionAnimation() ||
                index < 0 ||
                index >= NonEquipItems.Count)
                return;

            BuildingObject buildingObject;
            var nonEquipItem = NonEquipItems[index];
            if (!nonEquipItem.IsValid() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().buildingObject == null ||
                !GameInstance.BuildingObjects.TryGetValue(nonEquipItem.GetBuildingItem().buildingObject.DataId, out buildingObject) ||
                !this.DecreaseItemsByIndex(index, 1))
                return;

            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
            {
                var buildingSaveData = new BuildingSaveData();
                buildingSaveData.Id = GenericUtils.GetUniqueId();
                buildingSaveData.ParentId = string.Empty;
                LiteNetLibIdentity entity;
                if (Manager.Assets.TryGetSpawnedObject(parentObjectId, out entity))
                {
                    var parentBuildingEntity = entity.GetComponent<BuildingEntity>();
                    if (parentBuildingEntity != null)
                        buildingSaveData.ParentId = parentBuildingEntity.Id;
                }
                buildingSaveData.DataId = buildingObject.DataId;
                buildingSaveData.CurrentHp = buildingObject.maxHp;
                buildingSaveData.Position = position;
                buildingSaveData.Rotation = rotation;
                buildingSaveData.CreatorId = Id;
                buildingSaveData.CreatorName = CharacterName;
                manager.CreateBuildingEntity(buildingSaveData, false);
            }
        }

        protected void NetFuncDestroyBuild(uint objectId)
        {
            if (IsDead() ||
                IsPlayingActionAnimation())
                return;

            LiteNetLibIdentity identity;
            if (Manager.Assets.TryGetSpawnedObject(objectId, out identity))
            {
                var manager = Manager as BaseGameNetworkManager;
                var buildingEntity = identity.GetComponent<BuildingEntity>();
                if (buildingEntity != null && buildingEntity.CreatorId.Equals(Id) && manager != null)
                    manager.DestroyBuildingEntity(buildingEntity.Id);
            }
        }

        protected void NetFuncSellItem(int index, short amount)
        {
            if (IsDead() ||
                index < 0 ||
                index >= nonEquipItems.Count)
                return;

            if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Shop)
                return;

            var nonEquipItem = nonEquipItems[index];
            if (!nonEquipItem.IsValid() || amount > nonEquipItem.amount)
                return;

            var item = nonEquipItem.GetItem();
            var level = nonEquipItem.level;
            if (this.DecreaseItemsByIndex(index, amount))
                Gold += item.sellPrice * amount;
        }
        #endregion

        #region Net functions callers
        public void RequestSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
        }

        public void RequestAddAttribute(int attributeIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex, amount);
        }

        public void RequestAddSkill(int skillIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex, amount);
        }

        public void RequestRespawn()
        {
            CallNetFunction("Respawn", FunctionReceivers.Server);
        }

        public void RequestAssignHotkey(string hotkeyId, HotkeyType type, int dataId)
        {
            CallNetFunction("AssignHotkey", FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
        }

        public void RequestNpcActivate(uint objectId)
        {
            if (IsDead())
                return;
            CallNetFunction("NpcActivate", FunctionReceivers.Server, objectId);
        }

        public void RequestShowNpcDialog(int npcDialogDataId)
        {
            if (IsDead())
                return;
            CallNetFunction("ShowNpcDialog", ConnectId, npcDialogDataId);
        }

        public void RequestSelectNpcDialogMenu(int menuIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SelectNpcDialogMenu", FunctionReceivers.Server, menuIndex);
        }

        public void RequestBuyNpcItem(int itemIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("BuyNpcItem", FunctionReceivers.Server, itemIndex, amount);
        }

        public void RequestEnterWarp()
        {
            if (IsDead() || IsPlayingActionAnimation() || warpingPortal == null)
                return;
            CallNetFunction("EnterWarp", FunctionReceivers.Server);
        }

        public void RequestBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("Build", FunctionReceivers.Server, index, position, rotation, parentObjectId);
        }

        public void RequestDestroyBuilding(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("DestroyBuild", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestSellItem(int nonEquipIndex, short amount)
        {
            if (IsDead() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("SellItem", FunctionReceivers.Server, nonEquipIndex, amount);
        }
        #endregion

        #region Sync data changes callback
        protected virtual void OnStatPointChange(short statPoint)
        {
            if (onStatPointChange != null)
                onStatPointChange.Invoke(statPoint);
        }

        protected virtual void OnSkillPointChange(short skillPoint)
        {
            if (onSkillPointChange != null)
                onSkillPointChange.Invoke(skillPoint);
        }

        protected virtual void OnGoldChange(int gold)
        {
            if (onGoldChange != null)
                onGoldChange.Invoke(gold);
        }

        protected virtual void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onHotkeysOperation != null)
                onHotkeysOperation.Invoke(operation, index);
        }

        protected virtual void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (onQuestsOperation != null)
                onQuestsOperation.Invoke(operation, index);
        }
        #endregion

        protected void SetMovePaths(Vector3 position)
        {
            var navPath = new NavMeshPath();
            if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
            {
                navPaths = new Queue<Vector3>(navPath.corners);
                // Dequeue first path it's not require for future movement
                navPaths.Dequeue();
            }
        }

        public void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            moveDirection = direction;
            if (moveDirection.magnitude == 0 && isGrounded)
                CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (!isJumping)
                isJumping = isGrounded && isJump;
        }

        public void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position);
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);
            currentNpcDialog = null;
        }

        public virtual void IncreaseGold(int gold)
        {
            if (!IsServer)
                return;
            Gold += gold;
        }

        public virtual void OnKillMonster(MonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (var i = 0; i < Quests.Count; ++i)
            {
                var quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }
    }
}
