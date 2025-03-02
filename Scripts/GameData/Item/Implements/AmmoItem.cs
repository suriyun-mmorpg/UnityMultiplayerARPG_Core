using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using Insthync.UnityEditorUtils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.AMMO_ITEM_FILE, menuName = GameDataMenuConsts.AMMO_ITEM_MENU, order = GameDataMenuConsts.AMMO_ITEM_ORDER)]
    public partial class AmmoItem : BaseItem, IAmmoItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Ammo; }
        }

        [Category(2, "Ammo Settings")]
        [SerializeField]
        [Tooltip("Ammo type data")]
        private AmmoType ammoType = null;
        public AmmoType AmmoType
        {
            get { return ammoType; }
        }
        
#if UNITY_EDITOR || !UNITY_SERVER
        [Category(3, "In-Scene Objects/Appearance")]
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableEquipModel))]
        protected GameObject equipModel;
#endif
        [SerializeField]
        protected AssetReferenceGameObject addressableEquipModel = null;
#endif

#if UNITY_EDITOR || !UNITY_SERVER
        public async UniTask<GameObject> GetAmmoAttachModel()
        {
            GameObject equipModel = null;
#if !EXCLUDE_PREFAB_REFS
            equipModel = this.equipModel;
#endif
            return await addressableEquipModel.GetOrLoadAssetAsyncOrUsePrefab(equipModel);
        }

        public IItem SetAmmoAttachModel(GameObject equipModel)
        {
#if !EXCLUDE_PREFAB_REFS
            this.equipModel = equipModel;
#endif
            return this;
        }
#endif

        [SerializeField]
        [Tooltip("Increasing damages stats while attacking by weapon which put this item")]
        private DamageIncremental[] increaseDamages = new DamageIncremental[0];
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAmmoTypes(AmmoType);
        }
    }
}
