using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using LiteNetLibManager;
using LiteNetLib;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public abstract partial class BaseMonsterCharacterEntity : BaseCharacterEntity
    {
        public const float TELEPORT_TO_SUMMONER_DELAY = 5f;

        public readonly Dictionary<BaseCharacterEntity, ReceivedDamageRecord> receivedDamageRecords = new Dictionary<BaseCharacterEntity, ReceivedDamageRecord>();

        [Category("Character Settings")]
        [SerializeField]
        [FormerlySerializedAs("monsterCharacter")]
        protected MonsterCharacter characterDatabase;
        [Tooltip("If this is `TRUE` it will use `overrideCharacteristic` as its characteristic instead of `characterDatabase.Characteristic`")]
        [SerializeField]
        protected bool isOverrideCharacteristic;
        [SerializeField]
        protected MonsterCharacteristic overrideCharacteristic;
        [SerializeField]
        protected float destroyDelay = 2f;
        [SerializeField]
        protected float destroyRespawnDelay = 5f;

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldUInt summonerObjectId = new SyncFieldUInt();
        [SerializeField]
        protected SyncFieldByte summonType = new SyncFieldByte();

        public override string EntityTitle
        {
            get
            {
                string title = base.EntityTitle;
                return !string.IsNullOrEmpty(title) ? title : characterDatabase.Title;
            }
        }

        private BaseCharacterEntity summoner;
        public BaseCharacterEntity Summoner
        {
            get
            {
                if (summoner == null)
                {
                    LiteNetLibIdentity identity;
                    if (Manager.Assets.TryGetSpawnedObject(summonerObjectId.Value, out identity))
                        summoner = identity.GetComponent<BaseCharacterEntity>();
                }
                return summoner;
            }
            protected set
            {
                summoner = value;
                if (IsServer)
                    summonerObjectId.Value = summoner != null ? summoner.ObjectId : 0;
            }
        }

        public SummonType SummonType
        {
            get { return (SummonType)summonType.Value; }
            protected set { summonType.Value = (byte)value; }
        }

        public bool IsSummoned
        {
            get { return SummonType != SummonType.None; }
        }

        public GameSpawnArea<BaseMonsterCharacterEntity> SpawnArea { get; protected set; }

        public BaseMonsterCharacterEntity SpawnPrefab { get; protected set; }

        public short SpawnLevel { get; protected set; }

        public Vector3 SpawnPosition { get; protected set; }

        public MonsterCharacter CharacterDatabase
        {
            get { return characterDatabase; }
            set { characterDatabase = value; }
        }

        public bool IsOverrideCharacteristic
        {
            get { return isOverrideCharacteristic; }
            set { isOverrideCharacteristic = value; }
        }

        public MonsterCharacteristic OverrideCharacteristic
        {
            get { return overrideCharacteristic; }
            set { overrideCharacteristic = value; }
        }

        public MonsterCharacteristic Characteristic
        {
            get { return IsOverrideCharacteristic ? OverrideCharacteristic : CharacterDatabase.Characteristic; }
        }

        public override int DataId
        {
            get { return CharacterDatabase.DataId; }
            set { }
        }

        public float DestroyDelay
        {
            get { return destroyDelay; }
        }

        public float DestroyRespawnDelay
        {
            get { return destroyRespawnDelay; }
        }

        protected override bool UpdateEntityComponents
        {
            get
            {
                if (IsServer && Identity.CountSubscribers() == 0)
                    return false;
                return true;
            }
        }

        protected bool isDestroyed;
        protected readonly HashSet<string> looters = new HashSet<string>();
        protected readonly List<CharacterItem> droppingItems = new List<CharacterItem>();
        protected float lastTeleportToSummonerTime = 0f;

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacters(CharacterDatabase);
        }

        public override EntityInfo GetInfo()
        {
            return new EntityInfo(
                EntityTypes.Monster,
                ObjectId,
                ObjectId.ToString(),
                DataId,
                0, 0, 0,
                IsInSafeArea,
                Summoner);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.monsterTag;
            gameObject.layer = CurrentGameInstance.monsterLayer;
            isDestroyed = false;
        }

        protected override void EntityUpdate()
        {
            if (IsServer && Identity.CountSubscribers() == 0)
            {
                // Don't updates while there is no subscrubers
                return;
            }
            Profiler.BeginSample("BaseMonsterCharacterEntity - Update");
            base.EntityUpdate();
            if (IsServer)
            {
                if (IsSummoned)
                {
                    if (!Summoner || Summoner.IsDead())
                    {
                        // Summoner disappear so destroy it
                        UnSummon();
                    }
                    else
                    {
                        float currentTime = Time.unscaledTime;
                        if (Vector3.Distance(EntityTransform.position, Summoner.EntityTransform.position) > CurrentGameInstance.maxFollowSummonerDistance &&
                            currentTime - lastTeleportToSummonerTime > TELEPORT_TO_SUMMONER_DELAY)
                        {
                            // Teleport to summoner if too far from summoner
                            Teleport(GameInstance.Singleton.GameplayRule.GetSummonPosition(Summoner), GameInstance.Singleton.GameplayRule.GetSummonRotation(Summoner));
                            lastTeleportToSummonerTime = currentTime;
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        public override void SendServerState()
        {
            if (IsServer && Identity.CountSubscribers() == 0)
            {
                // Don't updates while there is no subscrubers
                return;
            }
            base.SendServerState();
        }

        protected void InitStats()
        {
            if (!IsServer)
                return;
            isDestroyed = false;
            if (Level <= 0)
                Level = CharacterDatabase.DefaultLevel;
            ForceMakeCaches();
            CharacterStats stats = this.GetCaches().Stats;
            CurrentHp = (int)stats.hp;
            CurrentMp = (int)stats.mp;
            CurrentStamina = (int)stats.stamina;
            CurrentFood = (int)stats.food;
            CurrentWater = (int)stats.water;
        }

        public void SetSpawnArea(GameSpawnArea<BaseMonsterCharacterEntity> spawnArea, BaseMonsterCharacterEntity spawnPrefab, short spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = spawnPrefab;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            summonerObjectId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            summonerObjectId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            summonType.deliveryMethod = DeliveryMethod.ReliableOrdered;
            summonType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public override void OnSetup()
        {
            // Force set `MovementSecure` to `ServerAuthoritative` for all monsters
            MovementSecure = MovementSecure.ServerAuthoritative;

            base.OnSetup();

            if (IsClient)
            {
                // Setup relates elements
                if (CurrentGameInstance.monsterCharacterMiniMapObjects != null && CurrentGameInstance.monsterCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.monsterCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                // UI which show monster information
                if (CurrentGameInstance.monsterCharacterUI != null)
                    InstantiateUI(CurrentGameInstance.monsterCharacterUI);
            }

            // Initial default data
            InitStats();
            if (SpawnArea == null)
                SpawnPosition = EntityTransform.position;
        }

        public void SetAttackTarget(IDamageableEntity target)
        {
            if (target == null || target.Entity == Entity ||
                target.IsDead() || !target.CanReceiveDamageFrom(GetInfo()))
                return;
            SetTargetEntity(target.Entity);
        }

        public override float GetMoveSpeed()
        {
            if (ExtraMovementState == ExtraMovementState.IsWalking)
                return CharacterDatabase.WanderMoveSpeed;
            return base.GetMoveSpeed();
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType damageAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel, CharacterBuff buff, bool isDamageOverTime = false)
        {
            RecordRecivingDamage(instigator, totalDamage);
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, damageAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);
        }

        public override void OnBuffHpDecrease(EntityInfo causer, int amount)
        {
            RecordRecivingDamage(causer, amount);
            base.OnBuffHpDecrease(causer, amount);
        }

        public void RecordRecivingDamage(EntityInfo instigator, int damage)
        {
            BaseCharacterEntity attackerCharacter;
            if (instigator.TryGetEntity(out attackerCharacter))
            {
                // If summoned by someone, summoner is attacker
                if (attackerCharacter != null &&
                    attackerCharacter is BaseMonsterCharacterEntity &&
                    (attackerCharacter as BaseMonsterCharacterEntity).IsSummoned)
                    attackerCharacter = (attackerCharacter as BaseMonsterCharacterEntity).Summoner;

                // Add received damage entry
                if (attackerCharacter != null)
                {
                    ReceivedDamageRecord receivedDamageRecord = new ReceivedDamageRecord();
                    receivedDamageRecord.totalReceivedDamage = damage;
                    if (receivedDamageRecords.ContainsKey(attackerCharacter))
                    {
                        receivedDamageRecord = receivedDamageRecords[attackerCharacter];
                        receivedDamageRecord.totalReceivedDamage += damage;
                    }
                    receivedDamageRecord.lastReceivedDamageTime = Time.unscaledTime;
                    receivedDamageRecords[attackerCharacter] = receivedDamageRecord;
                }
            }
        }

        public override void GetAttackingData(
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            // Monster animation always main-hand (right-hand) animation
            isLeftHand = false;
            // Monster animation always main-hand (right-hand) animation
            animActionType = AnimActionType.AttackRightHand;
            // Monster will not have weapon type so set dataId to `0`, then random attack animation from default attack animtions
            animationDataId = 0;
            // Monster will not have weapon data
            weapon = CharacterItem.Create(CurrentGameInstance.MonsterWeaponItem.DataId);
        }

        public override void GetUsingSkillData(
            BaseSkill skill,
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int animationDataId,
            out CharacterItem weapon)
        {
            // Monster animation always main-hand (right-hand) animation
            isLeftHand = false;
            // Monster animation always main-hand (right-hand) animation
            animActionType = AnimActionType.AttackRightHand;
            // Monster will not have weapon type so set dataId to `0`, then random attack animation from default attack animtions
            animationDataId = 0;
            // Monster will not have weapon data
            weapon = CharacterItem.Create(CurrentGameInstance.MonsterWeaponItem.DataId);
            // Prepare skill data
            if (skill == null)
                return;
            // Get activate animation type which defined at character model
            SkillActivateAnimationType useSkillActivateAnimationType = CharacterModel.UseSkillActivateAnimationType(skill);
            // Prepare animation
            if (useSkillActivateAnimationType == SkillActivateAnimationType.UseAttackAnimation && skill.IsAttack)
            {
                // Assign data id
                animationDataId = 0;
                // Assign animation action type
                animActionType = AnimActionType.AttackRightHand;
            }
            else if (useSkillActivateAnimationType == SkillActivateAnimationType.UseActivateAnimation)
            {
                // Assign data id
                animationDataId = skill.DataId;
                // Assign animation action type
                animActionType = AnimActionType.SkillRightHand;
            }
        }

        public override float GetAttackDistance(bool isLeftHand)
        {
            return CharacterDatabase.DamageInfo.GetDistance();
        }

        public override float GetAttackFov(bool isLeftHand)
        {
            return CharacterDatabase.DamageInfo.GetFov();
        }

        public override void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);

            // If this summoned by someone, don't give reward to killer
            if (IsSummoned)
                return;

            Reward reward = CurrentGameplayRule.MakeMonsterReward(CharacterDatabase, Level);
            // Temp data which will be in-use in loop
            BaseCharacterEntity tempCharacterEntity;
            BasePlayerCharacterEntity tempPlayerCharacterEntity;
            BaseMonsterCharacterEntity tempMonsterCharacterEntity;
            // Last player is last player who kill the monster
            // Whom will have permission to pickup an items before other
            BasePlayerCharacterEntity lastPlayer = null;
            BaseCharacterEntity attackerCharacter;
            if (lastAttacker.TryGetEntity(out attackerCharacter))
            {
                if (attackerCharacter is BaseMonsterCharacterEntity)
                {
                    tempMonsterCharacterEntity = attackerCharacter as BaseMonsterCharacterEntity;
                    if (tempMonsterCharacterEntity.Summoner != null &&
                        tempMonsterCharacterEntity.Summoner is BasePlayerCharacterEntity)
                    {
                        // Set its summoner as main enemy
                        lastAttacker = tempMonsterCharacterEntity.Summoner.GetInfo();
                        lastAttacker.TryGetEntity(out attackerCharacter);
                    }
                }
                lastPlayer = attackerCharacter as BasePlayerCharacterEntity;
            }
            GuildData tempGuildData;
            PartyData tempPartyData;
            bool givenRewardExp;
            bool givenRewardCurrency;
            float shareGuildExpRate;
            if (receivedDamageRecords.Count > 0)
            {
                float tempHighRewardRate = 0f;
                foreach (BaseCharacterEntity enemy in receivedDamageRecords.Keys)
                {
                    if (enemy == null)
                        continue;

                    tempCharacterEntity = enemy;
                    givenRewardExp = false;
                    givenRewardCurrency = false;
                    shareGuildExpRate = 0f;

                    ReceivedDamageRecord receivedDamageRecord = receivedDamageRecords[tempCharacterEntity];
                    float rewardRate = (float)receivedDamageRecord.totalReceivedDamage / (float)this.GetCaches().MaxHp;
                    if (rewardRate > 1f)
                        rewardRate = 1f;

                    if (tempCharacterEntity is BaseMonsterCharacterEntity)
                    {
                        tempMonsterCharacterEntity = tempCharacterEntity as BaseMonsterCharacterEntity;
                        if (tempMonsterCharacterEntity.Summoner != null &&
                            tempMonsterCharacterEntity.Summoner is BasePlayerCharacterEntity)
                        {
                            // Set its summoner as main enemy
                            tempCharacterEntity = tempMonsterCharacterEntity.Summoner;
                        }
                    }

                    if (tempCharacterEntity is BasePlayerCharacterEntity)
                    {
                        bool makeMostDamage = false;
                        tempPlayerCharacterEntity = tempCharacterEntity as BasePlayerCharacterEntity;
                        // Clear looters list when it is found new player character who make most damages
                        if (rewardRate > tempHighRewardRate)
                        {
                            tempHighRewardRate = rewardRate;
                            looters.Clear();
                            makeMostDamage = true;
                        }
                        // Try find guild data from player character
                        if (tempPlayerCharacterEntity.GuildId > 0 && GameInstance.ServerGuildHandlers.TryGetGuild(tempPlayerCharacterEntity.GuildId, out tempGuildData))
                        {
                            // Calculation amount of Exp which will be shared to guild
                            shareGuildExpRate = (float)tempGuildData.ShareExpPercentage(tempPlayerCharacterEntity.Id) * 0.01f;
                            // Will share Exp to guild when sharing amount more than 0
                            if (shareGuildExpRate > 0)
                            {
                                // Increase guild exp
                                GameInstance.ServerGuildHandlers.IncreaseGuildExp(tempPlayerCharacterEntity, (int)(reward.exp * shareGuildExpRate * rewardRate));
                            }
                        }
                        // Try find party data from player character
                        if (tempPlayerCharacterEntity.PartyId > 0 && GameInstance.ServerPartyHandlers.TryGetParty(tempPlayerCharacterEntity.PartyId, out tempPartyData))
                        {
                            List<BasePlayerCharacterEntity> sharingExpMembers = new List<BasePlayerCharacterEntity>();
                            List<BasePlayerCharacterEntity> sharingItemMembers = new List<BasePlayerCharacterEntity>();
                            BasePlayerCharacterEntity nearbyPartyMember;
                            foreach (string memberId in tempPartyData.GetMemberIds())
                            {
                                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out nearbyPartyMember) && !nearbyPartyMember.IsDead())
                                {
                                    if (tempPartyData.shareExp)
                                    {
                                        if (GameInstance.Singleton.partyShareExpDistance <= 0f || Vector3.Distance(tempPlayerCharacterEntity.EntityTransform.position, nearbyPartyMember.EntityTransform.position) <= GameInstance.Singleton.partyShareExpDistance)
                                            sharingExpMembers.Add(nearbyPartyMember);
                                    }
                                    if (tempPartyData.shareItem)
                                    {
                                        if (GameInstance.Singleton.partyShareItemDistance <= 0f || Vector3.Distance(tempPlayerCharacterEntity.EntityTransform.position, nearbyPartyMember.EntityTransform.position) <= GameInstance.Singleton.partyShareItemDistance)
                                            sharingItemMembers.Add(nearbyPartyMember);
                                    }
                                }
                            }
                            int countNearbyPartyMembers;
                            // Share EXP to party members
                            countNearbyPartyMembers = sharingExpMembers.Count;
                            for (int i = 0; i < sharingExpMembers.Count; ++i)
                            {
                                nearbyPartyMember = sharingExpMembers[i];
                                // If share exp, every party member will receive devided exp
                                // If not share exp, character who make damage will receive non-devided exp
                                nearbyPartyMember.RewardExp(reward, (1f - shareGuildExpRate) / (float)countNearbyPartyMembers * rewardRate, RewardGivenType.PartyShare);
                            }
                            // Share Items to party members
                            countNearbyPartyMembers = sharingItemMembers.Count;
                            for (int i = 0; i < sharingItemMembers.Count; ++i)
                            {
                                nearbyPartyMember = sharingItemMembers[i];
                                // If share item, every party member will receive devided gold
                                // If not share item, character who make damage will receive non-devided gold
                                if (makeMostDamage)
                                {
                                    // Make other member in party able to pickup items
                                    looters.Add(nearbyPartyMember.Id);
                                }
                                nearbyPartyMember.RewardCurrencies(reward, 1f / (float)countNearbyPartyMembers * rewardRate, RewardGivenType.PartyShare);
                            }
                            // Shared exp has been given, so do not give it to character again
                            if (tempPartyData.shareExp)
                                givenRewardExp = true;
                            // Shared gold has been given, so do not give it to character again
                            if (tempPartyData.shareItem)
                                givenRewardCurrency = true;
                        }

                        // Add reward to current character in damage record list
                        if (!givenRewardExp)
                        {
                            // Will give reward when it was not given
                            int petIndex = tempPlayerCharacterEntity.IndexOfSummon(SummonType.PetItem);
                            if (petIndex >= 0)
                            {
                                tempMonsterCharacterEntity = tempPlayerCharacterEntity.Summons[petIndex].CacheEntity;
                                if (tempMonsterCharacterEntity != null)
                                {
                                    // Share exp to pet, set multiplier to 0.5, because it will be shared to player
                                    tempMonsterCharacterEntity.RewardExp(reward, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster);
                                }
                                // Set multiplier to 0.5, because it was shared to monster
                                tempPlayerCharacterEntity.RewardExp(reward, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster);
                            }
                            else
                            {
                                // No pet, no share, so rate is 1f
                                tempPlayerCharacterEntity.RewardExp(reward, (1f - shareGuildExpRate) * rewardRate, RewardGivenType.KillMonster);
                            }
                        }

                        if (!givenRewardCurrency)
                        {
                            // Will give reward when it was not given
                            tempPlayerCharacterEntity.RewardCurrencies(reward, rewardRate, RewardGivenType.KillMonster);
                        }

                        if (makeMostDamage)
                        {
                            // Make current character able to pick up item because it made most damage
                            looters.Add(tempPlayerCharacterEntity.Id);
                        }
                    }   // End is `BasePlayerCharacterEntity` condition
                }   // End for-loop
            }   // End count recived damage record count
            receivedDamageRecords.Clear();
            // Clear dropping items, it will fills in `OnRandomDropItem` function
            droppingItems.Clear();
            // Drop items
            CharacterDatabase.RandomItems(OnRandomDropItem);

            switch (CurrentGameInstance.monsterDeadDropItemMode)
            {
                case DeadDropItemMode.DropOnGround:
                    for (int i = 0; i < droppingItems.Count; ++i)
                    {
                        ItemDropEntity.DropItem(this, droppingItems[i], looters);
                    }
                    break;
                case DeadDropItemMode.CorpseLooting:
                    if (droppingItems.Count > 0)
                        ItemsContainerEntity.DropItems(CurrentGameInstance.monsterCorpsePrefab, this, droppingItems, looters, CurrentGameInstance.monsterCorpseAppearDuration);
                    break;
            }

            if (lastPlayer != null)
            {
                // Increase kill progress
                lastPlayer.OnKillMonster(this);
            }

            if (!IsSummoned)
            {
                // If not summoned by someone, destroy and respawn it
                DestroyAndRespawn();
            }

            // Clear looters because they are already set to dropped items
            looters.Clear();
        }

        private void OnRandomDropItem(BaseItem item, short amount)
        {
            // Drop item to the ground
            if (amount > item.MaxStack)
                amount = item.MaxStack;
            droppingItems.Add(CharacterItem.Create(item, 1, amount));
        }

        public virtual void DestroyAndRespawn()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (isDestroyed)
                return;
            // Mark as destroyed
            isDestroyed = true;
            // Respawning later
            if (SpawnArea != null)
                SpawnArea.Spawn(SpawnPrefab, SpawnLevel, DestroyDelay + DestroyRespawnDelay);
            else if (Identity.IsSceneObject)
                RespawnRoutine(DestroyDelay + DestroyRespawnDelay).Forget();
            // Destroy this entity
            NetworkDestroy(DestroyDelay);
        }

        private async UniTaskVoid RespawnRoutine(float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            InitStats();
            Teleport(SpawnPosition, EntityTransform.rotation);
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
            OnRespawn();
        }

        public void Summon(BaseCharacterEntity summoner, SummonType summonType, short level)
        {
            Summoner = summoner;
            SummonType = summonType;
            Level = level;
            InitStats();
        }

        public void UnSummon()
        {
            // TODO: May play teleport effects
            NetworkDestroy();
        }
    }

    public struct ReceivedDamageRecord
    {
        public float lastReceivedDamageTime;
        public int totalReceivedDamage;
    }
}
