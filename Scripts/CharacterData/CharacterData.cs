using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class CharacterData : ICharacterData
    {
        private string id;
        private int dataId;
        private int entityId;
        private string characterName;
        private short level;
        private byte equipWeaponSet;
        private ObservableCollection<EquipWeapons> selectableEquipWeapons;

        private ObservableCollection<CharacterAttribute> attributes;
        private ObservableCollection<CharacterSkill> skills;
        private List<CharacterSkillUsage> skillUsages;
        private ObservableCollection<CharacterBuff> buffs;
        private ObservableCollection<CharacterItem> equipItems;
        private ObservableCollection<CharacterItem> nonEquipItems;
        private ObservableCollection<CharacterSummon> summons;

        public string Id { get { return id; } set { id = value; } }
        public int DataId
        {
            get { return dataId; }
            set
            {
                dataId = value;
                this.MarkToMakeCaches();
            }
        }
        public int EntityId
        {
            get { return entityId; }
            set
            {
                entityId = value;
                this.MarkToMakeCaches();
            }
        }
        public string CharacterName
        {
            get { return characterName; }
            set { characterName = value; }
        }
        public string Title
        {
            get { return CharacterName; }
            set { CharacterName = value; }
        }
        public short Level
        {
            get { return level; }
            set
            {
                level = value;
                this.MarkToMakeCaches();
            }
        }
        public int Exp { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }
        public int CurrentStamina { get; set; }
        public int CurrentFood { get; set; }
        public int CurrentWater { get; set; }

        public EquipWeapons EquipWeapons
        {
            get
            {
                if (EquipWeaponSet < SelectableWeaponSets.Count)
                    return SelectableWeaponSets[EquipWeaponSet];
                return new EquipWeapons();
            }
            set
            {
                this.FillWeaponSetsIfNeeded(EquipWeaponSet);
                SelectableWeaponSets[EquipWeaponSet] = value;
            }
        }

        public byte EquipWeaponSet
        {
            get { return equipWeaponSet; }
            set
            {
                equipWeaponSet = value;
                this.MarkToMakeCaches();
            }
        }

        public IList<EquipWeapons> SelectableWeaponSets
        {
            get
            {
                if (selectableEquipWeapons == null)
                {
                    selectableEquipWeapons = new ObservableCollection<EquipWeapons>();
                    selectableEquipWeapons.CollectionChanged += List_CollectionChanged;
                }
                return selectableEquipWeapons;
            }
            set
            {
                if (selectableEquipWeapons == null)
                {
                    selectableEquipWeapons = new ObservableCollection<EquipWeapons>();
                    selectableEquipWeapons.CollectionChanged += List_CollectionChanged;
                }
                selectableEquipWeapons.Clear();
                foreach (EquipWeapons entry in value)
                    selectableEquipWeapons.Add(entry);
            }
        }

        public IList<CharacterAttribute> Attributes
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new ObservableCollection<CharacterAttribute>();
                    attributes.CollectionChanged += List_CollectionChanged;
                }
                return attributes;
            }
            set
            {
                if (attributes == null)
                {
                    attributes = new ObservableCollection<CharacterAttribute>();
                    attributes.CollectionChanged += List_CollectionChanged;
                }
                attributes.Clear();
                foreach (CharacterAttribute entry in value)
                    attributes.Add(entry);
            }
        }

        public IList<CharacterSkill> Skills
        {
            get
            {
                if (skills == null)
                {
                    skills = new ObservableCollection<CharacterSkill>();
                    skills.CollectionChanged += List_CollectionChanged;
                }
                return skills;
            }
            set
            {
                if (skills == null)
                {
                    skills = new ObservableCollection<CharacterSkill>();
                    skills.CollectionChanged += List_CollectionChanged;
                }
                skills.Clear();
                foreach (CharacterSkill entry in value)
                    skills.Add(entry);
            }
        }

        public IList<CharacterSkillUsage> SkillUsages
        {
            get
            {
                if (skillUsages == null)
                    skillUsages = new List<CharacterSkillUsage>();
                return skillUsages;
            }
            set
            {
                if (skillUsages == null)
                    skillUsages = new List<CharacterSkillUsage>();
                skillUsages.Clear();
                foreach (CharacterSkillUsage entry in value)
                    skillUsages.Add(entry);
            }
        }

        public IList<CharacterBuff> Buffs
        {
            get
            {
                if (buffs == null)
                {
                    buffs = new ObservableCollection<CharacterBuff>();
                    buffs.CollectionChanged += List_CollectionChanged;
                }
                return buffs;
            }
            set
            {
                if (buffs == null)
                {
                    buffs = new ObservableCollection<CharacterBuff>();
                    buffs.CollectionChanged += List_CollectionChanged;
                }
                buffs.Clear();
                foreach (CharacterBuff entry in value)
                    buffs.Add(entry);
            }
        }

        public IList<CharacterItem> EquipItems
        {
            get
            {
                if (equipItems == null)
                {
                    equipItems = new ObservableCollection<CharacterItem>();
                    equipItems.CollectionChanged += List_CollectionChanged;
                }
                return equipItems;
            }
            set
            {
                if (equipItems == null)
                {
                    equipItems = new ObservableCollection<CharacterItem>();
                    equipItems.CollectionChanged += List_CollectionChanged;
                }
                equipItems.Clear();
                foreach (CharacterItem entry in value)
                    equipItems.Add(entry);
            }
        }

        public IList<CharacterItem> NonEquipItems
        {
            get
            {
                if (nonEquipItems == null)
                {
                    nonEquipItems = new ObservableCollection<CharacterItem>();
                    nonEquipItems.CollectionChanged += List_CollectionChanged;
                }
                return nonEquipItems;
            }
            set
            {
                if (nonEquipItems == null)
                {
                    nonEquipItems = new ObservableCollection<CharacterItem>();
                    nonEquipItems.CollectionChanged += List_CollectionChanged;
                }
                nonEquipItems.Clear();
                foreach (CharacterItem entry in value)
                    nonEquipItems.Add(entry);
            }
        }

        public IList<CharacterSummon> Summons
        {
            get
            {
                if (summons == null)
                {
                    summons = new ObservableCollection<CharacterSummon>();
                    summons.CollectionChanged += List_CollectionChanged;
                }
                return summons;
            }
            set
            {
                if (summons == null)
                {
                    summons = new ObservableCollection<CharacterSummon>();
                    summons.CollectionChanged += List_CollectionChanged;
                }
                summons.Clear();
                foreach (CharacterSummon entry in value)
                    summons.Add(entry);
            }
        }
        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.MarkToMakeCaches();
        }
    }
}
