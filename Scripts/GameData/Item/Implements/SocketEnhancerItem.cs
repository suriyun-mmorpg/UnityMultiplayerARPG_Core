using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        [Category(0, "Item Settings")]
        [SerializeField]
        private SocketEnhancerType socketEnhancerType = SocketEnhancerType.Type1;
        public SocketEnhancerType SocketEnhancerType
        {
            get { return socketEnhancerType; }
        }

        [Category(3, "Buff/Bonus Settings")]
        [SerializeField]
        private EquipmentBonus socketEnhanceEffect = default;
        public EquipmentBonus SocketEnhanceEffect
        {
            get { return socketEnhanceEffect; }
        }

#if UNITY_EDITOR || !UNITY_SERVER
        [Category(4, "In-Scene Objects/Appearance")]
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableEquipModel))]
        protected GameObject equipModel;
#endif
        [SerializeField]
        protected AssetReferenceGameObject addressableEquipModel = null;
#endif

        [SerializeField]
        private BaseWeaponAbility[] weaponAbilities = new BaseWeaponAbility[0];

        public BaseWeaponAbility[] WeaponAbilities
        {
            get { return weaponAbilities; }
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

#if UNITY_EDITOR || !UNITY_SERVER
        public async UniTask<GameObject> GetEquipModel()
        {
            GameObject equipModel = null;
#if !EXCLUDE_PREFAB_REFS
            equipModel = this.equipModel;
#endif
            return await addressableEquipModel.GetOrLoadAssetAsyncOrUsePrefab(equipModel);
        }

        public IItem SetEquipModel(GameObject equipModel)
        {
#if !EXCLUDE_PREFAB_REFS
            this.equipModel = equipModel;
#endif
            return this;
        }
#endif

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
