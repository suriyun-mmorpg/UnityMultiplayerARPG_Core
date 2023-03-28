using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public void OnKillMonster(BaseMonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (int i = 0; i < Quests.Count; ++i)
            {
                CharacterQuest quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }

        public override void Killed(EntityInfo lastAttacker)
        {
            // Dead time
            LastDeadTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Dead penalty
            float expLostPercentage = CurrentGameInstance.GameplayRule.GetExpLostPercentageWhenDeath(this);
            GuildData guildData;
            if (GameInstance.ServerGuildHandlers.TryGetGuild(GuildId, out guildData))
                expLostPercentage -= expLostPercentage * guildData.DecreaseExpLostPercentage;
            if (expLostPercentage <= 0f)
                expLostPercentage = 0f;
            int exp = Exp;
            exp -= (int)(this.GetNextLevelExp() * expLostPercentage / 100f);
            if (exp <= 0)
                exp = 0;
            Exp = exp;

            // Clear data
            NpcAction.ClearNpcDialogData();

            // Add killer to looters
            HashSet<string> looters = new HashSet<string>();
            string killerObjectId;
            if (lastAttacker.SummonerObjectId > 0)
                killerObjectId = lastAttacker.SummonerId;
            else
                killerObjectId = lastAttacker.Id;
            if (!string.IsNullOrEmpty(killerObjectId))
                looters.Add(killerObjectId);

            // Add this character to make players able to get items from their own corpses immediately
            looters.Add(Id);

            // Drop an items
            List<CharacterItem> droppingItems = new List<CharacterItem>();

            if (CurrentMapInfo.PlayerDeadDropsEquipWeapons)
            {
                for (int i = 0; i < SelectableWeaponSets.Count; ++i)
                {
                    EquipWeapons updatingEquipWeapons = SelectableWeaponSets[i].Clone();
                    if (!CurrentMapInfo.ExcludeItemFromDropping(SelectableWeaponSets[i].GetRightHandItem()))
                    {
                        droppingItems.Add(SelectableWeaponSets[i].rightHand);
                        updatingEquipWeapons.rightHand = CharacterItem.Empty;
                    }
                    if (!CurrentMapInfo.ExcludeItemFromDropping(SelectableWeaponSets[i].GetLeftHandItem()))
                    {
                        droppingItems.Add(SelectableWeaponSets[i].leftHand);
                        updatingEquipWeapons.leftHand = CharacterItem.Empty;
                    }
                    SelectableWeaponSets[i] = updatingEquipWeapons;
                }
            }

            if (CurrentMapInfo.PlayerDeadDropsEquipItems)
            {
                for (int i = EquipItems.Count - 1; i >= 0; --i)
                {
                    if (!EquipItems[i].IsEmptySlot() && !CurrentMapInfo.ExcludeItemFromDropping(EquipItems[i].GetItem()))
                    {
                        droppingItems.Add(EquipItems[i]);
                        EquipItems.RemoveAt(i);
                    }
                }
            }

            if (CurrentMapInfo.PlayerDeadDropsNonEquipItems)
            {
                for (int i = NonEquipItems.Count - 1; i >= 0; --i)
                {
                    if (!NonEquipItems[i].IsEmptySlot() && !CurrentMapInfo.ExcludeItemFromDropping(NonEquipItems[i].GetItem()))
                    {
                        droppingItems.Add(NonEquipItems[i]);
                        NonEquipItems.RemoveAt(i);
                    }
                }
            }

            int dropCount = 0;
            for (int i = droppingItems.Count - 1; i >= 0; --i)
            {
                if (droppingItems[i].NotEmptySlot())
                    ++dropCount;
                else
                    droppingItems.RemoveAt(i);
            }


            if (dropCount > 0)
            {
                this.FillEmptySlots();
                switch (CurrentGameInstance.playerDeadDropItemMode)
                {
                    case DeadDropItemMode.DropOnGround:
                        for (int i = 0; i < droppingItems.Count; ++i)
                        {
                            ItemDropEntity.DropItem(this, RewardGivenType.PlayerDead, droppingItems[i], looters);
                        }
                        break;
                    case DeadDropItemMode.CorpseLooting:
                        if (droppingItems.Count > 0)
                            ItemsContainerEntity.DropItems(CurrentGameInstance.monsterCorpsePrefab, this, RewardGivenType.PlayerDead, droppingItems, looters, CurrentGameInstance.playerCorpseAppearDuration);
                        break;
                }
            }

            base.Killed(lastAttacker);

#if UNITY_EDITOR || UNITY_SERVER
            if (BaseGameNetworkManager.CurrentMapInfo.AutoRespawnWhenDead)
                GameInstance.ServerCharacterHandlers.Respawn(0, this);
#endif
        }
    }
}
