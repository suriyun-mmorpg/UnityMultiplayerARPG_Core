using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ATTRIBUTE_FILE, menuName = GameDataMenuConsts.ATTRIBUTE_MENU, order = GameDataMenuConsts.ATTRIBUTE_ORDER)]
    public partial class Attribute : BaseGameData
    {

        [Category("Attribute Settings")]
        [SerializeField]
        private float battlePointScore = 10;
        public float BattlePointScore
        {
            get { return battlePointScore; }
        }

        [SerializeField]
        private CharacterStats statsIncreaseEachLevel;
        public CharacterStats StatsIncreaseEachLevel
        {
            get { return statsIncreaseEachLevel; }
        }

        [SerializeField]
        private ResistanceIncremental[] increaseResistances = new ResistanceIncremental[0];
        public ResistanceIncremental[] IncreaseResistances
        {
            get { return increaseResistances; }
        }

        [SerializeField]
        private ArmorIncremental[] increaseArmors = new ArmorIncremental[0];
        public ArmorIncremental[] IncreaseArmors
        {
            get { return increaseArmors; }
        }

        [SerializeField]
        private DamageIncremental[] increaseDamages = new DamageIncremental[0];
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        [Tooltip("If this value more than 0 it will limit max amount of this attribute by this value")]
        public int maxAmount;
        public bool cannotReset = false;

        public bool CanIncreaseAmount(IPlayerCharacterData character, int amount, out UITextKeys gameMessage, bool checkStatPoint = true)
        {
            gameMessage = UITextKeys.NONE;
            if (character == null)
                return false;

            if (maxAmount > 0 && amount >= maxAmount)
            {
                gameMessage = UITextKeys.UI_ERROR_ATTRIBUTE_REACHED_MAX_AMOUNT;
                return false;
            }

            if (checkStatPoint && character.StatPoint <= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_STAT_POINT;
                return false;
            }

            return true;
        }
    }

    [System.Serializable]
    public struct AttributeAmount
    {
        public Attribute attribute;
        public float amount;
    }

    [System.Serializable]
    public struct AttributeRandomAmount
    {
        public Attribute attribute;
        public float minAmount;
        public float maxAmount;
        [Range(0, 1f)]
        public float applyRate;

        public bool Apply(System.Random random)
        {
            return random.NextDouble() <= applyRate;
        }

        public AttributeAmount GetRandomedAmount(System.Random random)
        {
            return new AttributeAmount()
            {
                attribute = attribute,
                amount = random.RandomFloat(minAmount, maxAmount),
            };
        }
    }

    [System.Serializable]
    public struct AttributeIncremental
    {
        public Attribute attribute;
        public IncrementalFloat amount;
    }
}
