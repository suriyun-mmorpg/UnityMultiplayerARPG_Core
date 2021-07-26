namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        // States
        /// <summary>
        /// This variable will be TRUE when cache data have to re-cache
        /// </summary>
        public bool IsRecaching
        {
            get
            {
                return isRecaching ||
                    selectableWeaponSetsRecachingState.isRecaching ||
                    attributesRecachingState.isRecaching ||
                    skillsRecachingState.isRecaching ||
                    buffsRecachingState.isRecaching ||
                    equipItemsRecachingState.isRecaching ||
                    nonEquipItemsRecachingState.isRecaching ||
                    summonsRecachingState.isRecaching;
            }
        }
        protected bool isRecaching;
        protected SyncListRecachingState selectableWeaponSetsRecachingState;
        protected SyncListRecachingState attributesRecachingState;
        protected SyncListRecachingState skillsRecachingState;
        protected SyncListRecachingState buffsRecachingState;
        protected SyncListRecachingState equipItemsRecachingState;
        protected SyncListRecachingState nonEquipItemsRecachingState;
        protected SyncListRecachingState summonsRecachingState;

        /// <summary>
        /// Make caches for character stats / attributes / skills / resistances / increase damages and so on immdediately
        /// </summary>
        public void ForceMakeCaches()
        {
            isRecaching = true;
            MakeCaches();
        }

        /// <summary>
        /// Make caches for character stats / attributes / skills / resistances / increase damages and so on when update calls
        /// </summary>
        protected void MakeCaches()
        {
            if (!IsRecaching)
                return;

            // Make caches with cache manager
            this.MarkToMakeCaches();

            if (selectableWeaponSetsRecachingState.isRecaching)
            {
                if (onSelectableWeaponSetsOperation != null)
                    onSelectableWeaponSetsOperation.Invoke(selectableWeaponSetsRecachingState.operation, selectableWeaponSetsRecachingState.index);
                selectableWeaponSetsRecachingState = SyncListRecachingState.Empty;
            }

            if (attributesRecachingState.isRecaching)
            {
                if (onAttributesOperation != null)
                    onAttributesOperation.Invoke(attributesRecachingState.operation, attributesRecachingState.index);
                attributesRecachingState = SyncListRecachingState.Empty;
            }

            if (skillsRecachingState.isRecaching)
            {
                if (onSkillsOperation != null)
                    onSkillsOperation.Invoke(skillsRecachingState.operation, skillsRecachingState.index);
                skillsRecachingState = SyncListRecachingState.Empty;
            }

            if (buffsRecachingState.isRecaching)
            {
                if (onBuffsOperation != null)
                    onBuffsOperation.Invoke(buffsRecachingState.operation, buffsRecachingState.index);
                buffsRecachingState = SyncListRecachingState.Empty;
            }

            if (equipItemsRecachingState.isRecaching)
            {
                if (onEquipItemsOperation != null)
                    onEquipItemsOperation.Invoke(equipItemsRecachingState.operation, equipItemsRecachingState.index);
                equipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (nonEquipItemsRecachingState.isRecaching)
            {
                if (onNonEquipItemsOperation != null)
                    onNonEquipItemsOperation.Invoke(nonEquipItemsRecachingState.operation, nonEquipItemsRecachingState.index);
                nonEquipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (summonsRecachingState.isRecaching)
            {
                if (onSummonsOperation != null)
                    onSummonsOperation.Invoke(summonsRecachingState.operation, summonsRecachingState.index);
                summonsRecachingState = SyncListRecachingState.Empty;
            }

            isRecaching = false;
        }
    }
}
