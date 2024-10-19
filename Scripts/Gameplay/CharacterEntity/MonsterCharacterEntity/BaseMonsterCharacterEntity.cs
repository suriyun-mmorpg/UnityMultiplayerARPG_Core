using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

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
        protected Faction faction;
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

        public bool IsSummonedAndSummonerExisted
        {
            get { return IsSummoned && Summoner != null; }
        }

        public GameSpawnArea<BaseMonsterCharacterEntity> SpawnArea { get; protected set; }

        public BaseMonsterCharacterEntity SpawnPrefab { get; protected set; }

        public GameSpawnArea<BaseMonsterCharacterEntity>.AddressablePrefab SpawnAddressablePrefab { get; protected set; }

        public int SpawnLevel { get; protected set; }

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

        public Faction Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        public override int DataId
        {
            get { return CharacterDatabase.DataId; }
            set { }
        }

        public override int FactionId
        {
            get
            {
                if (Faction == null)
                    return 0;
                return Faction.DataId;
            }
            set { }
        }

        public float DestroyDelay
        {
            get { return destroyDelay; }
            set { destroyDelay = value; }
        }

        public float DestroyRespawnDelay
        {
            get { return destroyRespawnDelay; }
            set { destroyRespawnDelay = value; }
        }

        protected bool _isDestroyed;
        protected readonly HashSet<string> _looters = new HashSet<string>();
        protected readonly List<CharacterItem> _droppingItems = new List<CharacterItem>();
        protected float _lastTeleportToSummonerTime = 0f;
        protected int _beforeDamageReceivedHp;

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
                FactionId,
                0 /* Party ID */,
                0 /* Guild ID */,
                IsInSafeArea,
                Summoner);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.monsterTag;
            gameObject.layer = CurrentGameInstance.monsterLayer;
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("BaseMonsterCharacterEntity - Update");
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
                            currentTime - _lastTeleportToSummonerTime > TELEPORT_TO_SUMMONER_DELAY)
                        {
                            // Teleport to summoner if too far from summoner
                            Teleport(GameInstance.Singleton.GameplayRule.GetSummonPosition(Summoner), GameInstance.Singleton.GameplayRule.GetSummonRotation(Summoner), false);
                            _lastTeleportToSummonerTime = currentTime;
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        public override void SendServerState(long writeTimestamp)
        {
            if (!IsUpdateEntityComponents)
            {
                // Don't updates while there is no subscrubers
                return;
            }
            base.SendServerState(writeTimestamp);
        }

        public virtual void InitStats()
        {
            _isDestroyed = false;
            if (Level <= 0)
                Level = CharacterDatabase.DefaultLevel;
            ForceMakeCaches();
            CharacterStats stats = CachedData.Stats;
            CurrentHp = (int)stats.hp;
            CurrentMp = (int)stats.mp;
            CurrentStamina = (int)stats.stamina;
            CurrentFood = (int)stats.food;
            CurrentWater = (int)stats.water;
        }

        public void SetSpawnArea(GameSpawnArea<BaseMonsterCharacterEntity> spawnArea, BaseMonsterCharacterEntity spawnPrefab, int spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = spawnPrefab;
            SpawnAddressablePrefab = null;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        public virtual void SetSpawnArea(GameSpawnArea<BaseMonsterCharacterEntity> spawnArea, GameSpawnArea<BaseMonsterCharacterEntity>.AddressablePrefab spawnAddressablePrefab, int spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = null;
            SpawnAddressablePrefab = spawnAddressablePrefab;
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
            base.OnSetup();

            if (IsClient)
            {
                // Instantiates monster objects
                if (CurrentGameInstance.monsterCharacterObjects != null && CurrentGameInstance.monsterCharacterObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.monsterCharacterObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, EntityTransform.position, EntityTransform.rotation, EntityTransform);
                    }
                }
                // Instantiates monster minimap objects
                if (CurrentGameInstance.monsterCharacterMiniMapObjects != null && CurrentGameInstance.monsterCharacterMiniMapObjects.Length > 0)
                {
                    foreach (GameObject obj in CurrentGameInstance.monsterCharacterMiniMapObjects)
                    {
                        if (obj == null) continue;
                        Instantiate(obj, MiniMapUiTransform.position, MiniMapUiTransform.rotation, MiniMapUiTransform);
                    }
                }
                // Instantiates monster character UI
                if (CurrentGameInstance.monsterCharacterUI != null)
                {
                    InstantiateUI(CurrentGameInstance.monsterCharacterUI);
                }
            }
            if (SpawnArea == null)
                SpawnPosition = EntityTransform.position;
            if (IsServer)
                InitStats();
        }

        public void SetAttackTarget(IDamageableEntity target)
        {
            if (target.GetObjectId() == Entity.ObjectId || target.IsDead() || !target.CanReceiveDamageFrom(GetInfo()))
            {
                // Can't attack
                return;
            }
            SetTargetEntity(target.Entity);
        }

        public override float GetMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            if (extraMovementState == ExtraMovementState.IsWalking)
                return CharacterDatabase.WanderMoveSpeed;
            return base.GetMoveSpeed(movementState, extraMovementState);
        }

        public override void ReceivingDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            _beforeDamageReceivedHp = CurrentHp;
            base.ReceivingDamage(position, fromPosition, instigator, damageAmounts, weapon, skill, skillLevel);
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType damageAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime = false)
        {
            RecordRecivingDamage(instigator, totalDamage);
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, damageAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);
        }

        public override void OnBuffHpDecrease(EntityInfo causer, int amount)
        {
            _beforeDamageReceivedHp = CurrentHp;
            base.OnBuffHpDecrease(causer, amount);
            RecordRecivingDamage(causer, amount);
        }

        public void RecordRecivingDamage(EntityInfo instigator, int damage)
        {
            if (instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter))
            {
                // If summoned by someone, summoner is attacker
                if (attackerCharacter is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.IsSummonedAndSummonerExisted)
                    attackerCharacter = monsterCharacterEntity.Summoner;

                // Add received damage entry
                if (attackerCharacter != null)
                {
                    if (damage > _beforeDamageReceivedHp)
                        damage = _beforeDamageReceivedHp;
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

        public override async void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);

            // If this summoned by someone, don't give reward to killer
            if (IsSummoned)
                return;

            Reward reward = CurrentGameplayRule.MakeMonsterReward(CharacterDatabase, Level);
            // Giving gold and exp to players
            GivingRewardToKillers(FindLastAttackerPlayer(lastAttacker), reward, out float itemDropRate);
            receivedDamageRecords.Clear();
            // Clear dropping items, it will fills in `OnRandomDropItem` function
            _droppingItems.Clear();
            // Drop items
            CharacterDatabase.RandomItems(OnRandomDropItem, itemDropRate);

            switch (CurrentGameInstance.monsterDeadDropItemMode)
            {
                case RewardingItemMode.DropOnGround:
                    for (int i = 0; i < _droppingItems.Count; ++i)
                    {
                        ItemDropEntity.Drop(this, RewardGivenType.KillMonster, _droppingItems[i], _looters).Forget();
                    }
                    break;
                case RewardingItemMode.CorpseLooting:
                    if (_droppingItems.Count > 0)
                    {
                        ItemsContainerEntity loadedPrefab = await CurrentGameInstance.GetLoadedMonsterCorpsePrefab();
                        if (loadedPrefab != null)
                            ItemsContainerEntity.DropItems(loadedPrefab, this, RewardGivenType.KillMonster, _droppingItems, _looters, CurrentGameInstance.monsterCorpseAppearDuration);
                    }
                    break;
                case RewardingItemMode.Immediately:
                    // NOTE: might have to think about how the item should be sent, now it will be sent randomly to `_looters`
                    List<string> shufflingLooters = new List<string>(_looters);
                    shufflingLooters.Shuffle();
                    int looterIndex = 0;
                    for (int i = 0; i < _droppingItems.Count; ++i)
                    {
                        if (looterIndex >= shufflingLooters.Count)
                        {
                            looterIndex = 0;
                        }
                        BasePlayerCharacterEntity looterEntity = null;
                        while (shufflingLooters.Count > 0 && !GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(shufflingLooters[looterIndex], out looterEntity))
                        {
                            shufflingLooters.RemoveAt(looterIndex);
                            looterEntity = null;
                            continue;
                        }
                        if (shufflingLooters.Count <= 0)
                        {
                            // Prevent no looters error
                            break;
                        }
                        looterIndex++;
                        if (looterEntity != null)
                        {
                            // Increase items directly to character inventory
                            if (looterEntity.IncreasingItemsWillOverwhelming(_droppingItems[i].dataId, _droppingItems[i].amount))
                            {
                                GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(looterEntity.Id, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                                continue;
                            }
                            looterEntity.IncreaseItems(_droppingItems[i]);
                        }
                    }
                    break;
            }

            if (!reward.NoExp() && CurrentGameInstance.monsterExpRewardingMode == RewardingMode.DropOnGround)
            {
                ExpDropEntity.Drop(this, 1f, RewardGivenType.KillMonster, Level, Level, reward.exp, _looters).Forget();
            }

            if (!reward.NoGold() && CurrentGameInstance.monsterGoldRewardingMode == RewardingMode.DropOnGround)
            {
                GoldDropEntity.Drop(this, 1f, RewardGivenType.KillMonster, Level, Level, reward.gold, _looters).Forget();
            }

            if (!reward.NoCurrencies() && CurrentGameInstance.monsterCurrencyRewardingMode == RewardingMode.DropOnGround)
            {
                foreach (CurrencyAmount currencyAmount in reward.currencies)
                {
                    if (currencyAmount.currency == null || currencyAmount.amount <= 0)
                        continue;
                    CurrencyDropEntity.Drop(this, 1f, RewardGivenType.KillMonster, Level, Level, currencyAmount.currency, currencyAmount.amount, _looters).Forget();
                }
            }

            if (!IsSummoned)
            {
                // If not summoned by someone, destroy and respawn it
                DestroyAndRespawn();
            }

            // Clear looters because they are already set to dropped items
            _looters.Clear();
        }

        /// <summary>
        /// Last attacker player is last player who kill the monster
        /// </summary>
        /// <param name="lastAttacker"></param>
        /// <returns></returns>
        protected virtual BasePlayerCharacterEntity FindLastAttackerPlayer(EntityInfo lastAttacker)
        {
            if (!lastAttacker.TryGetEntity(out BaseCharacterEntity attackerCharacter))
                return null;
            if (attackerCharacter is BaseMonsterCharacterEntity monsterCharacterEntity &&
                monsterCharacterEntity.Summoner != null &&
                monsterCharacterEntity.Summoner is BasePlayerCharacterEntity)
            {
                // Set its summoner as main enemy
                lastAttacker = monsterCharacterEntity.Summoner.GetInfo();
                lastAttacker.TryGetEntity(out attackerCharacter);
            }
            return attackerCharacter as BasePlayerCharacterEntity;
        }

        protected virtual void GivingRewardToKillers(BasePlayerCharacterEntity lastPlayer, Reward reward, out float itemDropRate)
        {
            itemDropRate = 1f;
            if (receivedDamageRecords.Count <= 0)
                return;

            BaseCharacterEntity tempCharacterEntity;
            bool givenRewardExp;
            bool givenRewardGold;
            bool givenRewardCurrencies;
            float tempHighRewardRate = 0f;
            foreach (BaseCharacterEntity enemy in receivedDamageRecords.Keys)
            {
                if (enemy == null)
                    continue;

                tempCharacterEntity = enemy;
                givenRewardExp = false;
                givenRewardGold = false;
                givenRewardCurrencies = false;

                ReceivedDamageRecord receivedDamageRecord = receivedDamageRecords[tempCharacterEntity];
                float rewardRate = (float)receivedDamageRecord.totalReceivedDamage / (float)CachedData.MaxHp;
                if (rewardRate > 1f)
                    rewardRate = 1f;

                if (tempCharacterEntity is BaseMonsterCharacterEntity tempMonsterCharacterEntity && tempMonsterCharacterEntity.Summoner != null && tempMonsterCharacterEntity.Summoner is BasePlayerCharacterEntity)
                {
                    // Set its summoner as main enemy
                    tempCharacterEntity = tempMonsterCharacterEntity.Summoner;
                }

                if (tempCharacterEntity is BasePlayerCharacterEntity tempPlayerCharacterEntity)
                {
                    bool makeMostDamage = false;
                    bool isLastAttacker = lastPlayer != null && lastPlayer.ObjectId == tempPlayerCharacterEntity.ObjectId;
                    if (isLastAttacker)
                    {
                        // Increase kill progress
                        tempPlayerCharacterEntity.OnKillMonster(this);
                    }
                    // Clear looters list when it is found new player character who make most damages
                    if (rewardRate > tempHighRewardRate)
                    {
                        tempHighRewardRate = rewardRate;
                        _looters.Clear();
                        makeMostDamage = true;
                        // Make this player character to be able to pick up item because it made most damage
                        _looters.Add(tempPlayerCharacterEntity.Id);
                        // And also change item drop rate
                        itemDropRate = 1f + tempPlayerCharacterEntity.CachedData.Stats.itemDropRate;
                    }
                    GivingRewardToGuild(tempPlayerCharacterEntity, reward, rewardRate, out float shareGuildExpRate);
                    GivingRewardToParty(tempPlayerCharacterEntity, isLastAttacker, reward, rewardRate, shareGuildExpRate, makeMostDamage, out givenRewardExp, out givenRewardGold, out givenRewardCurrencies);

                    // Add reward to current character in damage record list
                    if (CurrentGameInstance.monsterExpRewardingMode == RewardingMode.Immediately && !givenRewardExp)
                    {
                        // Will give reward when it was not given
                        int petIndex = tempPlayerCharacterEntity.IndexOfSummon(SummonType.PetItem);
                        if (petIndex >= 0 && tempPlayerCharacterEntity.Summons[petIndex].CacheEntity != null)
                        {
                            tempMonsterCharacterEntity = tempPlayerCharacterEntity.Summons[petIndex].CacheEntity;
                            // Share exp to pet, set multiplier to 0.5, because it will be shared to player
                            tempMonsterCharacterEntity.RewardExp(reward.exp, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster, Level, Level);
                            // Set multiplier to 0.5, because it was shared to monster
                            tempPlayerCharacterEntity.RewardExp(reward.exp, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster, Level, Level);
                        }
                        else
                        {
                            // No pet, no share, so rate is 1f
                            tempPlayerCharacterEntity.RewardExp(reward.exp, (1f - shareGuildExpRate) * rewardRate, RewardGivenType.KillMonster, Level, Level);
                        }
                    }

                    if (CurrentGameInstance.monsterGoldRewardingMode == RewardingMode.Immediately && !givenRewardGold)
                    {
                        // Will give reward when it was not given
                        tempPlayerCharacterEntity.RewardGold(reward.gold, rewardRate, RewardGivenType.KillMonster, Level, Level);
                    }

                    if (CurrentGameInstance.monsterCurrencyRewardingMode == RewardingMode.Immediately && !givenRewardCurrencies)
                    {
                        // Will give reward when it was not given
                        tempPlayerCharacterEntity.RewardCurrencies(reward.currencies, rewardRate, RewardGivenType.KillMonster, Level, Level);
                    }
                }
            }
        }

        protected virtual void GivingRewardToGuild(BasePlayerCharacterEntity playerCharacterEntity, Reward reward, float rewardRate, out float shareGuildExpRate)
        {
            shareGuildExpRate = 0f;
            if (CurrentGameInstance.monsterExpRewardingMode != RewardingMode.Immediately)
                return;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacterEntity.GuildId, out GuildData tempGuildData) || tempGuildData == null)
                return;
            // Calculation amount of Exp which will be shared to guild
            shareGuildExpRate = (float)tempGuildData.ShareExpPercentage(playerCharacterEntity.Id) * 0.01f;
            // Will share Exp to guild when sharing amount more than 0
            if (shareGuildExpRate > 0)
            {
                // Increase guild exp
                GameInstance.ServerGuildHandlers.IncreaseGuildExp(playerCharacterEntity, Mathf.CeilToInt(reward.exp * shareGuildExpRate * rewardRate));
            }
        }

        protected virtual void GivingRewardToParty(BasePlayerCharacterEntity playerCharacterEntity, bool isLastAttacker, Reward reward, float rewardRate, float shareGuildExpRate, bool makeMostDamage, out bool givenRewardExp, out bool givenRewardGold, out bool givenRewardCurrencies)
        {
            givenRewardExp = false;
            givenRewardGold = false;
            givenRewardCurrencies = false;
            if (!GameInstance.ServerPartyHandlers.TryGetParty(playerCharacterEntity.PartyId, out PartyData tempPartyData) || tempPartyData == null)
            {
                // No joined party
                return;
            }
            List<BasePlayerCharacterEntity> sharingExpMembers = new List<BasePlayerCharacterEntity>();
            List<BasePlayerCharacterEntity> sharingItemMembers = new List<BasePlayerCharacterEntity>();
            BasePlayerCharacterEntity nearbyPartyMember;
            foreach (string memberId in tempPartyData.GetMemberIds())
            {
                if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out nearbyPartyMember) || nearbyPartyMember == null || nearbyPartyMember.IsDead())
                {
                    // No reward for offline or dead members
                    continue;
                }
                if (tempPartyData.shareExp && ShouldShareExp(playerCharacterEntity, nearbyPartyMember))
                {
                    // Add party member to sharing exp list, to share exp to this party member later
                    sharingExpMembers.Add(nearbyPartyMember);
                }
                if (tempPartyData.shareItem && ShouldShareItem(playerCharacterEntity, nearbyPartyMember))
                {
                    // Add party member to sharing item list, to share item to this party member later
                    sharingItemMembers.Add(nearbyPartyMember);
                }
                if (isLastAttacker && playerCharacterEntity.ObjectId != nearbyPartyMember.ObjectId)
                {
                    // Increase kill progress
                    nearbyPartyMember.OnKillMonster(this);
                }
            }
            float countNearbyPartyMembers;
            // Share EXP to party members
            countNearbyPartyMembers = sharingExpMembers.Count;
            if (CurrentGameInstance.monsterExpRewardingMode == RewardingMode.Immediately && !reward.NoExp())
            {
                for (int i = 0; i < sharingExpMembers.Count; ++i)
                {
                    nearbyPartyMember = sharingExpMembers[i];
                    // If share exp, every party member will receive devided exp
                    // If not share exp, character who make damage will receive non-devided exp
                    int petIndex = nearbyPartyMember.IndexOfSummon(SummonType.PetItem);
                    if (petIndex >= 0)
                    {
                        BaseMonsterCharacterEntity monsterCharacterEntity = nearbyPartyMember.Summons[petIndex].CacheEntity;
                        if (monsterCharacterEntity != null)
                        {
                            // Share exp to pet, set multiplier to 0.5, because it will be shared to player
                            monsterCharacterEntity.RewardExp(reward.exp, (1f - shareGuildExpRate) / countNearbyPartyMembers * 0.5f * rewardRate, RewardGivenType.PartyShare, playerCharacterEntity.Level, Level);
                        }
                        // Set multiplier to 0.5, because it was shared to monster
                        nearbyPartyMember.RewardExp(reward.exp, (1f - shareGuildExpRate) / countNearbyPartyMembers * 0.5f * rewardRate, RewardGivenType.PartyShare, playerCharacterEntity.Level, Level);
                    }
                    else
                    {
                        // No pet, no share, so rate is 1f
                        nearbyPartyMember.RewardExp(reward.exp, (1f - shareGuildExpRate) / countNearbyPartyMembers * rewardRate, playerCharacterEntity.ObjectId == nearbyPartyMember.ObjectId ? RewardGivenType.KillMonster : RewardGivenType.PartyShare, Level, Level);
                    }
                }
            }
            // Share Items to party members
            countNearbyPartyMembers = sharingItemMembers.Count;
            if ((CurrentGameInstance.monsterGoldRewardingMode == RewardingMode.Immediately && !reward.NoGold()) || (CurrentGameInstance.monsterCurrencyRewardingMode == RewardingMode.Immediately && reward.NoCurrencies()))
            {
                for (int i = 0; i < sharingItemMembers.Count; ++i)
                {
                    nearbyPartyMember = sharingItemMembers[i];
                    // If share item, every party member will receive devided gold
                    // If not share item, character who make damage will receive non-devided gold
                    if (makeMostDamage)
                    {
                        // Make other member in party able to pickup items
                        _looters.Add(nearbyPartyMember.Id);
                    }
                    float multiplier = 1f / countNearbyPartyMembers * rewardRate;
                    RewardGivenType rewardGivenType = playerCharacterEntity.ObjectId == nearbyPartyMember.ObjectId ? RewardGivenType.KillMonster : RewardGivenType.PartyShare;
                    if (CurrentGameInstance.monsterGoldRewardingMode == RewardingMode.Immediately)
                        nearbyPartyMember.RewardGold(reward.gold, multiplier, rewardGivenType, Level, Level);
                    if (CurrentGameInstance.monsterCurrencyRewardingMode == RewardingMode.Immediately)
                        nearbyPartyMember.RewardCurrencies(reward.currencies, multiplier, rewardGivenType, Level, Level);
                }
            }

            // Shared exp has been given, so do not give it to character again
            if (tempPartyData.shareExp)
            {
                givenRewardExp = true;
            }

            // Shared gold has been given, so do not give it to character again
            if (tempPartyData.shareItem)
            {
                givenRewardGold = true;
                givenRewardCurrencies = true;
            }
        }

        private bool ShouldShareExp(BasePlayerCharacterEntity attacker, BasePlayerCharacterEntity member)
        {
            return GameInstance.Singleton.partyShareExpDistance <= 0f || Vector3.Distance(attacker.EntityTransform.position, member.EntityTransform.position) <= GameInstance.Singleton.partyShareExpDistance;
        }

        private bool ShouldShareItem(BasePlayerCharacterEntity attacker, BasePlayerCharacterEntity member)
        {
            return GameInstance.Singleton.partyShareItemDistance <= 0f || Vector3.Distance(attacker.EntityTransform.position, member.EntityTransform.position) <= GameInstance.Singleton.partyShareItemDistance;
        }

        private void OnRandomDropItem(BaseItem item, int level, int amount)
        {
            int maxStack = item.MaxStack;
            while (amount > 0)
            {
                int stackSize = Mathf.Min(maxStack, amount);
                _droppingItems.Add(CharacterItem.Create(item, level, stackSize));
                amount -= stackSize;
            }
        }

        public virtual void DestroyAndRespawn()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (_isDestroyed)
                return;
            // Mark as destroyed
            _isDestroyed = true;
            // Destroy this entity
            NetworkDestroy(DestroyDelay);
            // Respawning later
            if (SpawnArea != null)
                SpawnArea.Spawn(SpawnPrefab, SpawnAddressablePrefab, SpawnLevel, DestroyDelay + DestroyRespawnDelay, DestroyRespawnDelay);
            else if (Identity.IsSceneObject)
                RespawnRoutine(DestroyDelay + DestroyRespawnDelay).Forget();
        }

        /// <summary>
        /// This function will be called if this object is placed in scene networked object
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private async UniTaskVoid RespawnRoutine(float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            Teleport(SpawnPosition, EntityTransform.rotation, false);
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
            OnRespawn();
        }

        public void Summon(BaseCharacterEntity summoner, SummonType summonType, int level)
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

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            if (damageAmounts == null)
            {
                Logging.LogWarning($"{name}({nameof(BaseCharacterEntity)}) damage amounts dictionary is null, this should not occurring.");
                combatAmountType = CombatAmountType.Miss;
                totalDamage = 0;
                return;
            }

            if (instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter))
            {
                // Notify enemy spotted when received damage from enemy
                NotifyEnemySpotted(attackerCharacter);

                // Notify enemy spotted when damage taken to enemy
                attackerCharacter.NotifyEnemySpotted(this);
            }

            if (!CurrentGameInstance.GameplayRule.RandomAttackHitOccurs(fromPosition, attackerCharacter, this, damageAmounts, weapon, skill, skillLevel, randomSeed, out bool isCritical, out bool isBlocked))
            {
                // Don't hit (Miss)
                combatAmountType = CombatAmountType.Miss;
                totalDamage = 0;
                return;
            }

            // Calculate damages
            combatAmountType = CombatAmountType.NormalDamage;
            float calculatingTotalDamage = 0f;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                calculatingTotalDamage += damageElement.GetDamageReducedByResistance(CachedData.Resistances, CachedData.Armors,
                    CurrentGameInstance.GameplayRule.RandomAttackDamage(fromPosition, attackerCharacter, this, damageElement, damageAmounts[damageElement], weapon, skill, skillLevel, randomSeed));
            }

            if (attackerCharacter != null)
            {
                // If critical occurs
                if (isCritical)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.CriticalDamage;
                }
                // If block occurs
                if (isBlocked)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.BlockedDamage;
                }
            }

            // Apply damages
            totalDamage = CurrentGameInstance.GameplayRule.GetTotalDamage(fromPosition, instigator, this, calculatingTotalDamage, weapon, skill, skillLevel);
            if (totalDamage < 0)
                totalDamage = 0;

            IWeaponItem weaponItem = weapon.GetWeaponItem();
            if (position == HitBoxPosition.Head && 
                !CharacterDatabase.IsHeadshotInstantDeathProtected &&
                weaponItem != null && weaponItem.WeaponType != null &&
                weaponItem.WeaponType.DamageInfo.IsHeadshotInstantDeath())
            {
                // Headshot!, one hit dead
                combatAmountType = CombatAmountType.CriticalDamage;
                if (totalDamage < CurrentHp)
                    totalDamage = CurrentHp;
            }

            CurrentHp -= totalDamage;
        }
    }

    public struct ReceivedDamageRecord
    {
        public float lastReceivedDamageTime;
        public int totalReceivedDamage;
    }
}
