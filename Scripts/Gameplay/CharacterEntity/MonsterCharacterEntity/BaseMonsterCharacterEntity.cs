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
        [SerializeField]
        private MonsterCharacter monsterCharacter;
        [SerializeField]
        private float destroyDelay = 2f;
        [SerializeField]
        private float destroyRespawnDelay = 5f;
        [HideInInspector, System.NonSerialized]
        public bool isWandering;

        [Header("Monster Character - Sync Fields")]
        [SerializeField]
        protected SyncFieldPackedUInt summonerObjectId = new SyncFieldPackedUInt();
        [SerializeField]
        protected SyncFieldByte summonType = new SyncFieldByte();

        public override string Title
        {
            get
            {
                // Return title (Can set in prefab) if it is not empty
                if (!string.IsNullOrEmpty(base.Title))
                    return base.Title;
                return MonsterDatabase == null ? LanguageManager.GetUnknowTitle() : MonsterDatabase.Title;
            }
            set { }
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
        public MonsterCharacter MonsterDatabase { get { return monsterCharacter; } }
        public override int DataId { get { return MonsterDatabase.DataId; } set { } }
        public float DestroyDelay { get { return destroyDelay; } }
        public float DestroyRespawnDelay { get { return destroyRespawnDelay; } }

        private readonly HashSet<uint> looters = new HashSet<uint>();

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.monsterTag;
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

        protected void InitStats()
        {
            if (!IsServer)
                return;

            if (spawnArea == null)
                spawnPosition = CacheTransform.position;

            if (Level <= 0)
                Level = MonsterDatabase.defaultLevel;

            CharacterStats stats = this.GetStats();
            CurrentHp = (int)stats.hp;
            CurrentMp = (int)stats.mp;
            CurrentStamina = (int)stats.stamina;
            CurrentFood = (int)stats.food;
            CurrentWater = (int)stats.water;
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
                    return monsterCharacterEntity.MonsterDatabase.allyId == MonsterDatabase.allyId;
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

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead() || !CanReceiveDamageFrom(attacker))
                return;

            base.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
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
            weapon = null;
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
            weapon = null;
            // Prepare skill data
            if (skill == null)
                return;
            // Get activate animation type which defined at character model
            SkillActivateAnimationType useSkillActivateAnimationType = CharacterModel.UseSkillActivateAnimationType(skill);
            // Prepare animation
            if (useSkillActivateAnimationType == SkillActivateAnimationType.UseAttackAnimation && skill.IsAttack())
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
            return MonsterDatabase.damageInfo.GetDistance();
        }

        public override float GetAttackFov(bool isLeftHand)
        {
            return MonsterDatabase.damageInfo.GetFov();
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType damageAmountType, int damage)
        {
            BaseCharacterEntity attackerCharacterEntity = attacker as BaseCharacterEntity;

            // If summoned by someone, summoner is attacker
            if (attackerCharacterEntity != null &&
                attackerCharacterEntity is BaseMonsterCharacterEntity &&
                (attackerCharacterEntity as BaseMonsterCharacterEntity).IsSummoned)
                attackerCharacterEntity = (attackerCharacterEntity as BaseMonsterCharacterEntity).Summoner;

            // Add received damage entry
            if (attackerCharacterEntity != null)
            {
                ReceivedDamageRecord receivedDamageRecord = new ReceivedDamageRecord();
                receivedDamageRecord.totalReceivedDamage = damage;
                if (receivedDamageRecords.ContainsKey(attackerCharacterEntity))
                {
                    receivedDamageRecord = receivedDamageRecords[attackerCharacterEntity];
                    receivedDamageRecord.totalReceivedDamage += damage;
                }
                receivedDamageRecord.lastReceivedDamageTime = Time.unscaledTime;
                receivedDamageRecords[attackerCharacterEntity] = receivedDamageRecord;
            }

            base.ReceivedDamage(attackerCharacterEntity, damageAmountType, damage);
        }

        public override sealed void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);

            // If this summoned by someone, don't give reward to killer
            if (IsSummoned)
                return;

            Reward reward = gameplayRule.MakeMonsterReward(MonsterDatabase);
            BasePlayerCharacterEntity lastPlayer = null;
            if (lastAttacker != null)
                lastPlayer = lastAttacker as BasePlayerCharacterEntity;
            GuildData tempGuildData;
            PartyData tempPartyData;
            BasePlayerCharacterEntity tempPlayerCharacterEntity;
            BaseMonsterCharacterEntity tempMonsterCharacterEntity;
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

                    givenRewardExp = false;
                    givenRewardCurrency = false;
                    shareGuildExpRate = 0f;

                    ReceivedDamageRecord receivedDamageRecord = receivedDamageRecords[enemy];
                    float rewardRate = (float)receivedDamageRecord.totalReceivedDamage / (float)this.GetCaches().MaxHp;
                    if (rewardRate > 1f)
                        rewardRate = 1f;

                    if (enemy is BasePlayerCharacterEntity)
                    {
                        bool makeMostDamage = false;
                        tempPlayerCharacterEntity = enemy as BasePlayerCharacterEntity;
                        // Clear looters list when it is found new player character who make most damages
                        if (rewardRate > tempHighRewardRate)
                        {
                            tempHighRewardRate = rewardRate;
                            looters.Clear();
                            makeMostDamage = true;
                        }
                        // Try find guild data from player character
                        if (tempPlayerCharacterEntity.GuildId > 0 && gameManager.TryGetGuild(tempPlayerCharacterEntity.GuildId, out tempGuildData))
                        {
                            // Calculation amount of Exp which will be shared to guild
                            shareGuildExpRate = (float)tempGuildData.ShareExpPercentage(tempPlayerCharacterEntity.Id) * 0.01f;
                            // Will share Exp to guild when sharing amount more than 0
                            if (shareGuildExpRate > 0)
                            {
                                // Increase guild exp
                                gameManager.IncreaseGuildExp(tempPlayerCharacterEntity, (int)(reward.exp * shareGuildExpRate * rewardRate));
                            }
                        }
                        // Try find party data from player character
                        if (tempPlayerCharacterEntity.PartyId > 0 && gameManager.TryGetParty(tempPlayerCharacterEntity.PartyId, out tempPartyData))
                        {
                            BasePlayerCharacterEntity partyPlayerCharacterEntity;
                            // Loop party member to fill looter list / increase gold / increase exp
                            foreach (SocialCharacterData member in tempPartyData.GetMembers())
                            {
                                if (gameManager.TryGetPlayerCharacterById(member.id, out partyPlayerCharacterEntity))
                                {
                                    // If share exp, every party member will receive devided exp
                                    // If not share exp, character who make damage will receive non-devided exp
                                    if (tempPartyData.shareExp)
                                        partyPlayerCharacterEntity.RewardExp(reward, (1f - shareGuildExpRate) / (float)tempPartyData.CountMember() * rewardRate, RewardGivenType.PartyShare);

                                    // If share item, every party member will receive devided gold
                                    // If not share item, character who make damage will receive non-devided gold
                                    if (tempPartyData.shareItem)
                                    {
                                        if (makeMostDamage)
                                        {
                                            // Make other member in party able to pickup items
                                            looters.Add(partyPlayerCharacterEntity.ObjectId);
                                        }
                                        partyPlayerCharacterEntity.RewardCurrencies(reward, 1f / (float)tempPartyData.CountMember() * rewardRate, RewardGivenType.PartyShare);
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
                            int petIndex = tempPlayerCharacterEntity.IndexOfSummon(SummonType.Pet);
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
                            looters.Add(tempPlayerCharacterEntity.ObjectId);
                        }
                    }   // End is `BasePlayerCharacterEntity` condition
                }   // End for-loop
            }   // End count recived damage record count
            receivedDamageRecords.Clear();
            // Drop items
            MonsterDatabase.RandomItems(OnRandomDropItem);
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
            ItemDropEntity.DropItem(this, CharacterItem.Create(item, 1, amount), looters);
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
                spawnArea.Spawn(DestroyDelay + DestroyRespawnDelay);
            else
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(DestroyDelay);
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(DestroyDelay + DestroyRespawnDelay);
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
            if ((Summoner != null && Summoner == ally) || MonsterDatabase.characteristic == MonsterCharacteristic.Assist)
                SetAttackTarget(attacker);
        }
    }

    public struct ReceivedDamageRecord
    {
        public float lastReceivedDamageTime;
        public int totalReceivedDamage;
    }
}
