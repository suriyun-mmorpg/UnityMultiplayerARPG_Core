using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SOCKET_ENHANCER_ITEM_FILE, menuName = GameDataMenuConsts.SOCKET_ENHANCER_ITEM_MENU, order = GameDataMenuConsts.SOCKET_ENHANCER_ITEM_ORDER)]
    public partial class SocketEnhancerItem : BaseItem, ISocketEnhancerItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.SocketEnhancer; }
        }

        [Category(3, "Buff/Bonus Settings")]
        [SerializeField]
        private EquipmentBonus socketEnhanceEffect = default;
        public EquipmentBonus SocketEnhanceEffect
        {
            get { return socketEnhanceEffect; }
        }

        [SerializeField]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacking = new StatusEffectApplying[0];
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacking
        {
            get { return selfStatusEffectsWhenAttacking; }
        }

        [SerializeField]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacking = new StatusEffectApplying[0];
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacking
        {
            get { return enemyStatusEffectsWhenAttacking; }
        }

        [SerializeField]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacked = new StatusEffectApplying[0];
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacked
        {
            get { return selfStatusEffectsWhenAttacked; }
        }

        [SerializeField]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacked = new StatusEffectApplying[0];
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacked
        {
            get { return enemyStatusEffectsWhenAttacked; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddStatusEffects(SelfStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(EnemyStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(SelfStatusEffectsWhenAttacked);
            GameInstance.AddStatusEffects(EnemyStatusEffectsWhenAttacked);
        }
    }
}
