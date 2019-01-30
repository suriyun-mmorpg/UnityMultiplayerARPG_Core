using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibTransform))]
    public abstract partial class BaseMonsterCharacterEntity : BaseCharacterEntity
    {
        public readonly Dictionary<BaseCharacterEntity, ReceivedDamageRecord> receivedDamageRecords = new Dictionary<BaseCharacterEntity, ReceivedDamageRecord>();

        [Header("Monster Character Settings")]
        public MonsterCharacter monsterCharacter;

        [Header("Monster Character - Sync Fields")]
        [SerializeField]
        protected SyncFieldPackedUInt summonerObjectId = new SyncFieldPackedUInt();
        [SerializeField]
        protected SyncFieldByte summonType = new SyncFieldByte();

        public override string CharacterName
        {
            get { return monsterCharacter == null ? LanguageManager.GetUnknowTitle() : monsterCharacter.Title; }
            set { }
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

        private BaseCharacterEntity summoner;
        public BaseCharacterEntity Summoner
        {
            get
            {
                if (summoner == null)
                {
                    if (Manager.Assets.SpawnedObjects.ContainsKey(summonerObjectId.Value))
                        summoner = Manager.Assets.SpawnedObjects[summonerObjectId.Value].GetComponent<BaseCharacterEntity>();
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
        
        public SummonType SummonType { get { return (SummonType)summonType.Value; } protected set { summonType.Value = (byte)value; } }
        public bool IsSummoned { get { return SummonType != SummonType.None; } }

        public MonsterSpawnArea spawnArea { get; private set; }
        public Vector3 spawnPosition { get; private set; }
        public override int DataId { get { return monsterCharacter.DataId; } set { } }
        public override BaseCharacter Database { get { return monsterCharacter; } }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.monsterTag;
            MigrateDatabase();
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            InitStats();
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (IsSummoned)
            {
                if (Summoner != null)
                {
                    if (Vector3.Distance(CacheTransform.position, Summoner.CacheTransform.position) > gameInstance.maxFollowSummonerDistance)
                    {
                        // Teleport to summoner if too far from summoner
                        CacheNetTransform.Teleport(Summoner.GetSummonPosition(), Summoner.GetSummonRotation());
                    }
                }
                else
                {
                    // Summoner disappear so destroy it
                    UnSummon();
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (database != null && !(database is MonsterCharacter))
            {
                Debug.LogError("[BaseMonsterCharacterEntity] " + name + " Database must be `MonsterCharacter`");
                database = null;
                EditorUtility.SetDirty(this);
            }
            if (MigrateDatabase())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateDatabase()
        {
            bool hasChanges = false;
            if (database != null && database is MonsterCharacter)
            {
                monsterCharacter = database as MonsterCharacter;
                database = null;
                hasChanges = true;
            }
            return hasChanges;
        }

        protected void InitStats()
        {
            if (IsServer)
            {
                if (spawnArea == null)
                    spawnPosition = CacheTransform.position;

                CharacterStats stats = this.GetStats();
                CurrentHp = (int)stats.hp;
                CurrentMp = (int)stats.mp;
                CurrentStamina = (int)stats.stamina;
                CurrentFood = (int)stats.food;
                CurrentWater = (int)stats.water;
            }
        }

        public void SetSpawnArea(MonsterSpawnArea spawnArea, Vector3 spawnPosition)
        {
            this.spawnArea = spawnArea;
            this.spawnPosition = spawnPosition;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            CacheNetTransform.ownerClientCanSendTransform = false;
            summonerObjectId.sendOptions = SendOptions.ReliableOrdered;
            summonerObjectId.forOwnerOnly = false;
            summonType.sendOptions = SendOptions.ReliableOrdered;
            summonType.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();

            // Setup relates elements
            if (gameInstance.monsterCharacterMiniMapObjects != null && gameInstance.monsterCharacterMiniMapObjects.Length > 0)
            {
                foreach (GameObject obj in gameInstance.monsterCharacterMiniMapObjects)
                {
                    if (obj == null) continue;
                    Instantiate(obj, MiniMapUITransform.position, MiniMapUITransform.rotation, MiniMapUITransform);
                }
            }

            if (gameInstance.monsterCharacterUI != null)
                InstantiateUI(gameInstance.monsterCharacterUI);

            InitStats();
        }

        public virtual void SetAttackTarget(BaseCharacterEntity target)
        {
            if (target == null || target.IsDead())
                return;
            // Already have target so don't set target
            BaseCharacterEntity oldTarget;
            if (TryGetTargetEntity(out oldTarget) && !oldTarget.IsDead())
                return;
            // Set target to attack
            SetTargetEntity(target);
        }

        public override bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            if (attacker == null)
                return false;

            BaseCharacterEntity characterEntity = attacker as BaseCharacterEntity;
            if (characterEntity == null)
                return false;

            if (isInSafeArea || characterEntity.isInSafeArea)
            {
                // If this character or another character is in safe area so it cannot receive damage
                return false;
            }
            // If another character is not ally assume that it can receive damage
            return !IsAlly(characterEntity);
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;

            if (IsSummoned)
            {
                // If summoned by someone, will have same allies with summoner
                return characterEntity == Summoner || characterEntity.IsAlly(Summoner);
            }
            if (characterEntity is BaseMonsterCharacterEntity)
            {
                // If another monster has same allyId so it is ally
                BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
                if (monsterCharacterEntity != null)
                {
                    if (monsterCharacterEntity.IsSummoned)
                        return IsAlly(monsterCharacterEntity.Summoner);
                    return monsterCharacterEntity.monsterCharacter.allyId == monsterCharacter.allyId;
                }
            }
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;

            if (IsSummoned)
            {
                // If summoned by someone, will have same enemies with summoner
                return characterEntity != Summoner && characterEntity.IsEnemy(Summoner);
            }
            // Attack only player by default
            return characterEntity is BasePlayerCharacterEntity;
        }

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            BaseCharacterEntity attackerCharacter = attacker as BaseCharacterEntity;

            // If character is not dead, try to attack
            if (!IsDead())
            {
                BaseCharacterEntity targetEntity;
                if (!TryGetTargetEntity(out targetEntity))
                {
                    // If no target enemy, set target enemy as attacker
                    SetAttackTarget(attackerCharacter);
                }
                else if (attackerCharacter != targetEntity && Random.value > 0.5f)
                {
                    // Random 50% to change target when receive damage from anyone
                    SetAttackTarget(attackerCharacter);
                }
            }
        }

        public override void GetAttackingData(
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out bool isLeftHand,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            animActionType = AnimActionType.AttackRightHand;

            // Monster will not have weapon type so set dataId to `0`, then random attack animation from default attack animtions
            dataId = 0;

            // Monster attack always right hand
            isLeftHand = false;

            // Monster will not have weapon data
            weapon = null;

            // Random attack animation
            CharacterModel.GetRandomRightHandAttackAnimation(dataId, out animationIndex, out triggerDuration, out totalDuration);

            // Assign damage data
            damageInfo = monsterCharacter.damageInfo;

            // Assign damage amounts
            allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
            DamageElement damageElement = monsterCharacter.damageAmount.damageElement;
            MinMaxFloat damageAmount = monsterCharacter.damageAmount.amount.GetAmount(Level);
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            allDamageAmounts.Add(damageElement, damageAmount);
        }

        public override float GetAttackDistance()
        {
            return monsterCharacter.damageInfo.GetDistance();
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType damageAmountType, int damage)
        {
            BaseCharacterEntity attackerCharacter = attacker as BaseCharacterEntity;

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

            base.ReceivedDamage(attackerCharacter, damageAmountType, damage);

            // If dead destroy / respawn
            if (IsDead())
            {
                CurrentHp = 0;
                if (!IsSummoned)
                {
                    // If not summoned by someone, destroy and respawn it
                    DestroyAndRespawn();
                }
            }
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);

            // If this summoned by someone, don't give reward to killer
            if (IsSummoned)
                return;

            int randomedExp = Random.Range(monsterCharacter.randomExpMin, monsterCharacter.randomExpMax);
            int randomedGold = Random.Range(monsterCharacter.randomGoldMin, monsterCharacter.randomGoldMax);
            HashSet<uint> looters = new HashSet<uint>();
            BasePlayerCharacterEntity lastPlayer = lastAttacker as BasePlayerCharacterEntity;
            GuildData tempGuildData;
            PartyData tempPartyData;
            BasePlayerCharacterEntity tempPlayerCharacter;
            BaseMonsterCharacterEntity tempMonsterCharacter;
            if (receivedDamageRecords.Count > 0)
            {
                float tempHighRewardRate = 0f;
                foreach (BaseCharacterEntity enemy in receivedDamageRecords.Keys)
                {
                    ReceivedDamageRecord receivedDamageRecord = receivedDamageRecords[enemy];
                    float rewardRate = (float)receivedDamageRecord.totalReceivedDamage / (float)CacheMaxHp;
                    int rewardExp = (int)(randomedExp * rewardRate);
                    int rewardGold = (int)(randomedGold * rewardRate);
                    if (rewardRate > 1f)
                        rewardRate = 1f;
                    if (enemy is BasePlayerCharacterEntity)
                    {
                        bool makeMostDamage = false;
                        tempPlayerCharacter = enemy as BasePlayerCharacterEntity;
                        // Clear looters list when it is found new player character who make most damages
                        if (rewardRate > tempHighRewardRate)
                        {
                            tempHighRewardRate = rewardRate;
                            looters.Clear();
                            makeMostDamage = true;
                        }
                        // Try find guild data from player character
                        if (tempPlayerCharacter.GuildId > 0 && gameManager.TryGetGuild(tempPlayerCharacter.GuildId, out tempGuildData))
                        {
                            // Calculation amount of Exp which will be shared to guild
                            int shareRewardExp = (int)(rewardExp * (float)tempGuildData.ShareExpPercentage(tempPlayerCharacter.Id) / 100f);
                            // Will share Exp to guild when sharing amount more than 0
                            if (shareRewardExp > 0)
                            {
                                gameManager.IncreaseGuildExp(tempPlayerCharacter, shareRewardExp);
                                rewardExp -= shareRewardExp;
                            }
                        }
                        // Try find party data from player character
                        if (tempPlayerCharacter.PartyId > 0 && gameManager.TryGetParty(tempPlayerCharacter.PartyId, out tempPartyData))
                        {
                            BasePlayerCharacterEntity partyPlayerCharacter;
                            // Loop party member to fill looter list / increase gold / increase exp
                            foreach (SocialCharacterData member in tempPartyData.GetMembers())
                            {
                                if (gameManager.TryGetPlayerCharacterById(member.id, out partyPlayerCharacter))
                                {
                                    // If share exp, every party member will receive devided exp
                                    // If not share exp, character who make damage will receive non-devided exp
                                    if (tempPartyData.shareExp)
                                        partyPlayerCharacter.RewardExp(rewardExp / tempPartyData.CountMember(), RewardGivenType.PartyShare);

                                    // If share item, every party member will receive devided gold
                                    // If not share item, character who make damage will receive non-devided gold
                                    if (tempPartyData.shareItem)
                                    {
                                        if (makeMostDamage)
                                            looters.Add(partyPlayerCharacter.ObjectId);
                                        partyPlayerCharacter.RewardGold(rewardGold / tempPartyData.CountMember(), RewardGivenType.PartyShare);
                                    }
                                }
                            }
                            // Shared exp, has increased so do not increase it again
                            if (tempPartyData.shareExp)
                                rewardExp = 0;
                            // Shared gold, has increased so do not increase it again
                            if (tempPartyData.shareItem)
                                rewardGold = 0;
                        }
                        // Add reward to current character in damage record list
                        int petIndex = tempPlayerCharacter.IndexOfSummon(SummonType.Pet);
                        if (petIndex >= 0)
                        {
                            tempMonsterCharacter = tempPlayerCharacter.Summons[petIndex].CacheEntity;
                            if (tempMonsterCharacter != null)
                            {
                                // Share exp to pet
                                tempMonsterCharacter.RewardExp(rewardExp, RewardGivenType.KillMonster);
                            }
                        }
                        tempPlayerCharacter.RewardExp(rewardExp, RewardGivenType.KillMonster);
                        if (makeMostDamage)
                            looters.Add(tempPlayerCharacter.ObjectId);
                        tempPlayerCharacter.RewardGold(rewardGold, RewardGivenType.KillMonster);
                    }
                }
            }
            receivedDamageRecords.Clear();
            foreach (ItemDrop randomItem in monsterCharacter.randomItems)
            {
                if (Random.value <= randomItem.dropRate)
                {
                    Item item = randomItem.item;
                    short amount = randomItem.amount;
                    if (item != null && GameInstance.Items.ContainsKey(item.DataId))
                    {
                        // Drop item to the ground
                        if (amount > item.maxStack)
                            amount = item.maxStack;
                        CharacterItem dropData = CharacterItem.Create(item, 1);
                        dropData.amount = amount;
                        ItemDropEntity.DropItem(this, dropData, looters);
                    }
                }
            }
            if (lastPlayer != null)
                lastPlayer.OnKillMonster(this);
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;

            base.Respawn();
            StopMove();
            CacheNetTransform.Teleport(spawnPosition, CacheTransform.rotation);
        }

        public void DestroyAndRespawn()
        {
            if (!IsServer)
                return;

            if (spawnArea != null)
                spawnArea.Spawn(monsterCharacter.deadHideDelay + monsterCharacter.deadRespawnDelay);
            else
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(monsterCharacter.deadHideDelay);
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(monsterCharacter.deadHideDelay + monsterCharacter.deadRespawnDelay);
            InitStats();
            Manager.Assets.NetworkSpawn(Identity.HashAssetId, spawnPosition, Quaternion.Euler(Vector3.up * Random.Range(0, 360)), Identity.ObjectId, Identity.ConnectionId);
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

        public override void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker)
        {
            if ((Summoner != null && Summoner == ally) || monsterCharacter.characteristic == MonsterCharacteristic.Assist)
                SetAttackTarget(attacker);
        }

        public abstract void StopMove();
    }

    public struct ReceivedDamageRecord
    {
        public float lastReceivedDamageTime;
        public int totalReceivedDamage;
    }
}
