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
    private ObservableCollection<CharacterBuff> buffs;
    private ObservableCollection<CharacterItem> equipItems;
    private ObservableCollection<CharacterItem> nonEquipItems;

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
            foreach (var entry in value)
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
            foreach (var entry in value)
                skills.Add(entry);
            shouldMakeCache = true;
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
            foreach (var entry in value)
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
            foreach (var entry in value)
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
            foreach (var entry in value)
                nonEquipItems.Add(entry);
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
        cacheStats = this.GetStats();
        cacheAttributes = this.GetAttributes();
        cacheSkills = this.GetSkills();
        cacheResistances = this.GetResistances();
        cacheIncreaseDamages = this.GetIncreaseDamages();
        cacheMaxHp = (int)cacheStats.hp;
        cacheMaxMp = (int)cacheStats.mp;
        cacheMaxStamina = (int)cacheStats.stamina;
        cacheMaxFood = (int)cacheStats.food;
        cacheMaxWater = (int)cacheStats.water;
        cacheTotalItemWeight = this.GetTotalItemWeight();
        cacheAtkSpeed = cacheStats.atkSpeed;
        cacheMoveSpeed = cacheStats.moveSpeed;
        shouldMakeCache = false;
    }
}
