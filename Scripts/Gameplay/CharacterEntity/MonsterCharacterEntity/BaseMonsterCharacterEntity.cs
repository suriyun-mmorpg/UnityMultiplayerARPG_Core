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
    public abstract partial class BaseMonsterCharacterEntity : BaseCharacterEntity
    {
        public readonly Dictionary<BaseCharacterEntity, ReceivedDamageRecord> receivedDamageRecords = new Dictionary<BaseCharacterEntity, ReceivedDamageRecord>();

        [Header("Monster Character Settings")]
        public MonsterCharacter monsterCharacter;
        public float destroyDelay = 2f;
        public float destroyRespawnDelay = 5f;
        [HideInInspector, System.NonSerialized]
        public bool isWandering;

        [Header("Monster Character - Sync Fields")]
        [SerializeField]
        protected SyncFieldPackedUInt summonerObjectId = new SyncFieldPackedUInt();
        [SerializeField]
        protected SyncFieldByte summonType = new SyncFieldByte();

        public override string DisplayCharacterName
        {
            get
            {
                // Return title (Can set in prefab) if it is not empty
                if (!string.IsNullOrEmpty(Title))
                    return Title;
                return monsterCharacter == null ? LanguageManager.GetUnknowTitle() : monsterCharacter.Title;
            }
        }

        public override short DisplayLevel
        {
            get { return Level; }
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

        public SummonType SummonType { get { return (SummonType)summonType.Value; } protected set { summonType.Value = (byte)value; } }
        public bool IsSummoned { get { return SummonType != SummonType.None; } }

        public MonsterSpawnArea spawnArea { get; protected set; }
        public Vector3 spawnPosition { get; protected set; }
        public override int DataId { get { return monsterCharacter.DataId; } set { } }

        private readonly HashSet<uint> looters = new HashSet<uint>();

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.monsterTag;
            MigrateDatabase();
            MigrateFields();
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
                        Teleport(Summoner.GetSummonPosition());
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
            if (MigrateDatabase() || MigrateFields())
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

        private bool MigrateFields()
        {
            bool hasChanges = false;
            if (monsterCharacter != null)
            {
                if (monsterCharacter.deadHideDelay > 0)
                {
                    destroyDelay = monsterCharacter.deadHideDelay;
                    monsterCharacter.deadHideDelay = -1;
                    hasChanges = true;
                }
                if (monsterCharacter.deadRespawnDelay > 0)
                {
                    destroyRespawnDelay = monsterCharacter.deadRespawnDelay;
                    monsterCharacter.deadRespawnDelay = -1;
                    hasChanges = true;
                }
#if UNITY_EDITOR
                if (hasChanges)
                    EditorUtility.SetDirty(monsterCharacter);
#endif
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
            FindGroundedPosition(spawnPosition, 512f, out spawnPosition);
            this.spawnPosition = spawnPosition;
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

        public override sealed bool IsAlly(BaseCharacterEntity characterEntity)
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

        public override sealed bool IsEnemy(BaseCharacterEntity characterEntity)
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
            ref bool isLeftHand,
            out AnimActionType animActionType,
            out int dataId,
            out int animationIndex,
            out CharacterItem weapon,
            out float triggerDuration,
            out float totalDuration,
            out DamageInfo damageInfo,
            out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            // Initialize data
            isLeftHand = false;
            animActionType = AnimActionType.AttackRightHand;

            // Monster will not have weapon type so set dataId to `0`, then random attack animation from default attack animtions
            dataId = 0;

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

        public override float GetAttackDistance(bool isLeftHand)
        {
            return monsterCharacter.damageInfo.GetDistance();
        }

        public override float GetAttackFov(bool isLeftHand)
        {
            return monsterCharacter.damageInfo.GetFov();
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
        }

        public override sealed void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);

            // If this summoned by someone, don't give reward to killer
            if (IsSummoned)
                return;

            Reward reward = gameplayRule.MakeMonsterReward(monsterCharacter);
            BasePlayerCharacterEntity lastPlayer = null;
            if (lastAttacker != null)
                lastPlayer = lastAttacker as BasePlayerCharacterEntity;
            GuildData tempGuildData;
            PartyData tempPartyData;
            BasePlayerCharacterEntity tempPlayerCharacter;
            BaseMonsterCharacterEntity tempMonsterCharacter;
            bool givenRewardExp;
            bool givenRewardCurrency;
            float shareGuildExpRate;
            if (receivedDamageRecords.Count > 0)
            {
                float tempHighRewardRate = 0f;
                foreach (BaseCharacterEntity enemy in receivedDamageRecords.Keys)
                {
                    givenRewardExp = false;
                    givenRewardCurrency = false;
                    shareGuildExpRate = 0f;

                    ReceivedDamageRecord receivedDamageRecord = receivedDamageRecords[enemy];
                    float rewardRate = (float)receivedDamageRecord.totalReceivedDamage / (float)CacheMaxHp;
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
                            shareGuildExpRate = (float)tempGuildData.ShareExpPercentage(tempPlayerCharacter.Id) * 0.01f;
                            // Will share Exp to guild when sharing amount more than 0
                            if (shareGuildExpRate > 0)
                            {
                                // Increase guild exp
                                gameManager.IncreaseGuildExp(tempPlayerCharacter, (int)(reward.exp * shareGuildExpRate * rewardRate));
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
                                        partyPlayerCharacter.RewardExp(reward, (1f - shareGuildExpRate) / (float)tempPartyData.CountMember() * rewardRate, RewardGivenType.PartyShare);

                                    // If share item, every party member will receive devided gold
                                    // If not share item, character who make damage will receive non-devided gold
                                    if (tempPartyData.shareItem)
                                    {
                                        if (makeMostDamage)
                                        {
                                            // Make other member in party able to pickup items
                                            looters.Add(partyPlayerCharacter.ObjectId);
                                        }
                                        partyPlayerCharacter.RewardCurrencies(reward, 1f / (float)tempPartyData.CountMember() * rewardRate, RewardGivenType.PartyShare);
                                    }
                                }
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
                            int petIndex = tempPlayerCharacter.IndexOfSummon(SummonType.Pet);
                            if (petIndex >= 0)
                            {
                                tempMonsterCharacter = tempPlayerCharacter.Summons[petIndex].CacheEntity;
                                if (tempMonsterCharacter != null)
                                {
                                    // Share exp to pet, set multiplier to 0.5, because it will be shared to player
                                    tempMonsterCharacter.RewardExp(reward, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster);
                                }
                                // Set multiplier to 0.5, because it was shared to monster
                                tempPlayerCharacter.RewardExp(reward, (1f - shareGuildExpRate) * 0.5f * rewardRate, RewardGivenType.KillMonster);
                            }
                            else
                            {
                                // No pet, no share, so rate is 1f
                                tempPlayerCharacter.RewardExp(reward, (1f - shareGuildExpRate) * rewardRate, RewardGivenType.KillMonster);
                            }
                        }

                        if (!givenRewardCurrency)
                        {
                            // Will give reward when it was not given
                            tempPlayerCharacter.RewardCurrencies(reward, rewardRate, RewardGivenType.KillMonster);
                        }

                        if (makeMostDamage)
                        {
                            // Make current character able to pick up item because it made most damage
                            looters.Add(tempPlayerCharacter.ObjectId);
                        }
                    }   // End is `BasePlayerCharacterEntity` condition
                }   // End for-loop
            }   // End count recived damage record count
            receivedDamageRecords.Clear();
            // Drop items
            monsterCharacter.RandomItems(OnRandomDropItem);
            // Clear looters because they are already set to dropped items
            looters.Clear();

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
        }

        private void OnRandomDropItem(Item item, short amount)
        {
            // Drop item to the ground
            if (amount > item.maxStack)
                amount = item.maxStack;
            CharacterItem dropData = CharacterItem.Create(item, 1);
            dropData.amount = amount;
            ItemDropEntity.DropItem(this, dropData, looters);
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;

            base.Respawn();
            StopMove();
            Teleport(spawnPosition);
        }

        public void DestroyAndRespawn()
        {
            if (!IsServer)
                return;

            if (spawnArea != null)
                spawnArea.Spawn(destroyDelay + destroyRespawnDelay);
            else
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(destroyDelay);
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(destroyDelay + destroyRespawnDelay);
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                spawnPosition,
                gameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
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
    }

    public struct ReceivedDamageRecord
    {
        public float lastReceivedDamageTime;
        public int totalReceivedDamage;
    }
}
