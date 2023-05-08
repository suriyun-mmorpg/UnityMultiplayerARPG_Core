namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public CharacterDataCache CachedData { get; protected set; }
        /// <summary>
        /// This variable will be TRUE when cache data have to re-cache
        /// </summary>
        public bool IsRecaching
        {
            get
            {
                return _isRecaching ||
                    _selectableWeaponSetsRecachingState.isRecaching ||
                    _attributesRecachingState.isRecaching ||
                    _skillsRecachingState.isRecaching ||
                    _buffsRecachingState.isRecaching ||
                    _equipItemsRecachingState.isRecaching ||
                    _nonEquipItemsRecachingState.isRecaching ||
                    _summonsRecachingState.isRecaching;
            }
        }
        protected bool _isRecaching;
        protected SyncListRecachingState _selectableWeaponSetsRecachingState;
        protected SyncListRecachingState _attributesRecachingState;
        protected SyncListRecachingState _skillsRecachingState;
        protected SyncListRecachingState _buffsRecachingState;
        protected SyncListRecachingState _equipItemsRecachingState;
        protected SyncListRecachingState _nonEquipItemsRecachingState;
        protected SyncListRecachingState _summonsRecachingState;

        /// <summary>
        /// Make caches for character stats / attributes / skills / resistances / increase damages and so on immdediately
        /// </summary>
        public void ForceMakeCaches()
        {
            _isRecaching = true;
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

            if (_selectableWeaponSetsRecachingState.isRecaching)
            {
                if (onSelectableWeaponSetsOperation != null)
                    onSelectableWeaponSetsOperation.Invoke(_selectableWeaponSetsRecachingState.operation, _selectableWeaponSetsRecachingState.index);
                _selectableWeaponSetsRecachingState = SyncListRecachingState.Empty;
            }

            if (_attributesRecachingState.isRecaching)
            {
                if (onAttributesOperation != null)
                    onAttributesOperation.Invoke(_attributesRecachingState.operation, _attributesRecachingState.index);
                _attributesRecachingState = SyncListRecachingState.Empty;
            }

            if (_skillsRecachingState.isRecaching)
            {
                if (onSkillsOperation != null)
                    onSkillsOperation.Invoke(_skillsRecachingState.operation, _skillsRecachingState.index);
                _skillsRecachingState = SyncListRecachingState.Empty;
            }

            if (_buffsRecachingState.isRecaching)
            {
                if (onBuffsOperation != null)
                    onBuffsOperation.Invoke(_buffsRecachingState.operation, _buffsRecachingState.index);
                _buffsRecachingState = SyncListRecachingState.Empty;
            }

            if (_equipItemsRecachingState.isRecaching)
            {
                if (onEquipItemsOperation != null)
                    onEquipItemsOperation.Invoke(_equipItemsRecachingState.operation, _equipItemsRecachingState.index);
                _equipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (_nonEquipItemsRecachingState.isRecaching)
            {
                if (onNonEquipItemsOperation != null)
                    onNonEquipItemsOperation.Invoke(_nonEquipItemsRecachingState.operation, _nonEquipItemsRecachingState.index);
                _nonEquipItemsRecachingState = SyncListRecachingState.Empty;
            }

            if (_summonsRecachingState.isRecaching)
            {
                if (onSummonsOperation != null)
                    onSummonsOperation.Invoke(_summonsRecachingState.operation, _summonsRecachingState.index);
                _summonsRecachingState = SyncListRecachingState.Empty;
            }

            CachedData = this.GetCaches();
            _isRecaching = false;
        }
    }
}
