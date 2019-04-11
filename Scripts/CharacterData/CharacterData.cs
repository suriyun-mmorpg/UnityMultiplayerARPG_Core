using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MultiplayerARPG;

[System.Serializable]
public partial class CharacterData : ICharacterData
{
    private string id;
    private int dataId;
    private int entityId;
    private string characterName;
    private short level;
    private int exp;
    private int currentHp;
    private int currentMp;
    private int currentStamina;
    private int currentFood;
    private int currentWater;
    private EquipWeapons equipWeapons;

    private ObservableCollection<CharacterAttribute> attributes;
    private ObservableCollection<CharacterSkill> skills;
    private List<CharacterSkillUsage> skillUsages;
    private ObservableCollection<CharacterBuff> buffs;
    private ObservableCollection<CharacterItem> equipItems;
    private ObservableCollection<CharacterItem> nonEquipItems;
    private ObservableCollection<CharacterSummon> summons;

    private bool shouldMakeCache = false;

    public string Id { get { return id; } set { id = value; } }
    public int DataId
    {
        get { return dataId; }
        set
        {
            dataId = value;
            shouldMakeCache = true;
        }
    }
    public int EntityId
    {
        get { return entityId; }
        set
        {
            entityId = value;
            shouldMakeCache = true;
        }
    }
    public string CharacterName { get { return characterName; } set { characterName = value; } }
    public short Level
    {
        get { return level; }
        set
        {
            level = value;
            shouldMakeCache = true;
        }
    }
    public int Exp { get { return exp; } set { exp = value; } }
    public int CurrentHp { get { return currentHp; } set { currentHp = value; } }
    public int CurrentMp { get { return currentMp; } set { currentMp = value; } }
    public int CurrentStamina { get { return currentStamina; } set { currentStamina = value; } }
    public int CurrentFood { get { return currentFood; } set { currentFood = value; } }
    public int CurrentWater { get { return currentWater; } set { currentWater = value; } }
    public EquipWeapons EquipWeapons
    {
        get { return equipWeapons; }
        set
        {
            equipWeapons = value;
            shouldMakeCache = true;
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
            shouldMakeCache = true;
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
            shouldMakeCache = true;
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
            shouldMakeCache = true;
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
            shouldMakeCache = true;
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
            shouldMakeCache = true;
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
            shouldMakeCache = true;
        }
    }

    private CharacterStats cacheStats;
    public CharacterStats CacheStats
    {
        get
        {
            MakeCaches();
            return cacheStats;
        }
    }

    private Dictionary<Attribute, short> cacheAttributes;
    public Dictionary<Attribute, short> CacheAttributes
    {
        get
        {
            MakeCaches();
            return cacheAttributes;
        }
    }

    private Dictionary<Skill, short> cacheSkills;
    public Dictionary<Skill, short> CacheSkills
    {
        get
        {
            MakeCaches();
            return cacheSkills;
        }
    }

    private Dictionary<DamageElement, float> cacheResistances;
    public Dictionary<DamageElement, float> CacheResistances
    {
        get
        {
            MakeCaches();
            return cacheResistances;
        }
    }

    private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;
    public Dictionary<DamageElement, MinMaxFloat> CacheIncreaseDamages
    {
        get
        {
            MakeCaches();
            return cacheIncreaseDamages;
        }
    }

    private Dictionary<EquipmentSet, int> cacheEquipmentSets;
    public Dictionary<EquipmentSet, int> CacheEquipmentSets
    {
        get
        {
            MakeCaches();
            return cacheEquipmentSets;
        }
    }

    private int cacheMaxHp;
    public int CacheMaxHp
    {
        get
        {
            MakeCaches();
            return cacheMaxHp;
        }
    }

    private int cacheMaxMp;
    public int CacheMaxMp
    {
        get
        {
            MakeCaches();
            return cacheMaxMp;
        }
    }

    private int cacheMaxStamina;
    public int CacheMaxStamina
    {
        get
        {
            MakeCaches();
            return cacheMaxStamina;
        }
    }

    private int cacheMaxFood;
    public int CacheMaxFood
    {
        get
        {
            MakeCaches();
            return cacheMaxFood;
        }
    }

    private int cacheMaxWater;
    public int CacheMaxWater
    {
        get
        {
            MakeCaches();
            return cacheMaxWater;
        }
    }

    private float cacheTotalItemWeight;
    public float CacheTotalItemWeight
    {
        get
        {
            MakeCaches();
            return cacheTotalItemWeight;
        }
    }

    private float cacheAtkSpeed;
    public float CacheAtkSpeed
    {
        get
        {
            MakeCaches();
            return cacheAtkSpeed;
        }
    }

    private float cacheMoveSpeed;
    public float CacheMoveSpeed
    {
        get
        {
            MakeCaches();
            return cacheMoveSpeed;
        }
    }

    private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        shouldMakeCache = true;
    }

    private void MakeCaches()
    {
        if (!shouldMakeCache)
            return;

        if (cacheAttributes == null)
            cacheAttributes = new Dictionary<Attribute, short>();
        if (cacheResistances == null)
            cacheResistances = new Dictionary<DamageElement, float>();
        if (cacheIncreaseDamages == null)
            cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        if (cacheSkills == null)
            cacheSkills = new Dictionary<Skill, short>();
        if (cacheEquipmentSets == null)
            cacheEquipmentSets = new Dictionary<EquipmentSet, int>();

        this.GetAllStats(
            out cacheStats,
            cacheAttributes,
            cacheResistances,
            cacheIncreaseDamages,
            cacheSkills,
            cacheEquipmentSets,
            out cacheMaxHp,
            out cacheMaxMp,
            out cacheMaxStamina,
            out cacheMaxFood,
            out cacheMaxWater,
            out cacheTotalItemWeight,
            out cacheAtkSpeed,
            out cacheMoveSpeed);
        shouldMakeCache = false;
    }
}
