using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public enum CombatAmountType : byte
    {
        Miss,
        NormalDamage,
        CriticalDamage,
        BlockedDamage,
        HpRecovery,
        MpRecovery,
        StaminaRecovery,
        FoodRecovery,
        WaterRecovery,
    }

    public partial class UISceneGameplay : MonoBehaviour
    {
        [System.Serializable]
        public class UIToggleUI
        {
            public UIBase ui;
            public KeyCode key;
        }

        public static UISceneGameplay Singleton { get; private set; }

        [Header("Character Releates UIs")]
        public UICharacter[] uiCharacters;
        public UIEquipItems[] uiCharacterEquipItems;
        public UINonEquipItems[] uiCharacterNonEquipItems;
        public UICharacterSkills[] uiCharacterSkills;
        public UICharacterSummons[] uiCharacterSummons;
        public UICharacterHotkeys[] uiCharacterHotkeys;
        public UICharacterQuests[] uiCharacterQuests;

        [HideInInspector]
        public UIEquipItems uiEquipItems;
        [HideInInspector]
        public UINonEquipItems uiNonEquipItems;
        [HideInInspector]
        public UICharacterSkills uiSkills;
        [HideInInspector]
        public UICharacterSummons uiSummons;
        [HideInInspector]
        public UICharacterHotkeys uiHotkeys;
        [HideInInspector]
        public UICharacterQuests uiQuests;

        [Header("Selected Target UIs")]
        public UICharacter uiTargetCharacter;
        public UIBaseGameEntity uiTargetNpc;
        public UIDamageableEntity uiTargetBuilding;
        public UIDamageableEntity uiTargetHarvestable;

        [Header("Other UIs")]
        public UINpcDialog uiNpcDialog;
        public UIRefineItem uiRefineItem;
        public UIConstructBuilding uiConstructBuilding;
        public UICurrentBuilding uiCurrentBuilding;
        public UIPlayerActivateMenu uiPlayerActivateMenu;
        public UIDealingRequest uiDealingRequest;
        public UIDealing uiDealing;
        public UIPartyInvitation uiPartyInvitation;
        public UIGuildInvitation uiGuildInvitation;
        public UIToggleUI[] toggleUis;
        [Tooltip("These GameObject (s) will ignore click / touch detection when click or touch on screen")]
        public List<GameObject> ignorePointerDetectionUis;
        [Tooltip("These UI (s) will block character controller inputs while visible")]
        public List<UIBase> blockControllerUIs;

        [Header("Combat Text")]
        public Transform combatTextTransform;
        public UICombatText uiCombatTextMiss;
        public UICombatText uiCombatTextNormalDamage;
        public UICombatText uiCombatTextCriticalDamage;
        public UICombatText uiCombatTextBlockedDamage;
        public UICombatText uiCombatTextHpRecovery;
        public UICombatText uiCombatTextMpRecovery;
        public UICombatText uiCombatTextStaminaRecovery;
        public UICombatText uiCombatTextFoodRecovery;
        public UICombatText uiCombatTextWaterRecovery;

        [Header("Events")]
        public UnityEvent onCharacterDead;
        public UnityEvent onCharacterRespawn;
        
        public System.Action<BasePlayerCharacterEntity> onUpdateCharacter;
        public System.Action<BasePlayerCharacterEntity> onUpdateEquipItems;
        public System.Action<BasePlayerCharacterEntity> onUpdateNonEquipItems;
        public System.Action<BasePlayerCharacterEntity> onUpdateSkills;
        public System.Action<BasePlayerCharacterEntity> onUpdateSummons;
        public System.Action<BasePlayerCharacterEntity> onUpdateHotkeys;
        public System.Action<BasePlayerCharacterEntity> onUpdateQuests;

        private void Awake()
        {
            Singleton = this;
            MigrateNewUIs();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateNewUIs())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateNewUIs()
        {
            bool hasChanges = false;
            if (uiEquipItems != null)
            {
                List<UIEquipItems> list = uiCharacterEquipItems == null ? new List<UIEquipItems>() : new List<UIEquipItems>(uiCharacterEquipItems);
                list.Add(uiEquipItems);
                uiCharacterEquipItems = list.ToArray();
                uiEquipItems = null;
                hasChanges = true;
            }

            if (uiNonEquipItems != null)
            {
                var list = uiCharacterNonEquipItems == null ? new List<UINonEquipItems>() : new List<UINonEquipItems>(uiCharacterNonEquipItems);
                list.Add(uiNonEquipItems);
                uiCharacterNonEquipItems = list.ToArray();
                uiNonEquipItems = null;
                hasChanges = true;
            }

            if (uiSkills != null)
            {
                var list = uiCharacterSkills == null ? new List<UICharacterSkills>() : new List<UICharacterSkills>(uiCharacterSkills);
                list.Add(uiSkills);
                uiCharacterSkills = list.ToArray();
                uiSkills = null;
                hasChanges = true;
            }

            if (uiSummons != null)
            {
                var list = uiCharacterSummons == null ? new List<UICharacterSummons>() : new List<UICharacterSummons>(uiCharacterSummons);
                list.Add(uiSummons);
                uiCharacterSummons = list.ToArray();
                uiSummons = null;
                hasChanges = true;
            }

            if (uiHotkeys != null)
            {
                var list = uiCharacterHotkeys == null ? new List<UICharacterHotkeys>() : new List<UICharacterHotkeys>(uiCharacterHotkeys);
                list.Add(uiHotkeys);
                uiCharacterHotkeys = list.ToArray();
                uiHotkeys = null;
                hasChanges = true;
            }

            if (uiQuests != null)
            {
                var list = uiCharacterQuests == null ? new List<UICharacterQuests>() : new List<UICharacterQuests>(uiCharacterQuests);
                list.Add(uiQuests);
                uiCharacterQuests = list.ToArray();
                uiQuests = null;
                hasChanges = true;
            }

            return hasChanges;
        }

        private void Update()
        {
            if (GenericUtils.IsFocusInputField())
                return;

            foreach (UIToggleUI toggleUi in toggleUis)
            {
                if (Input.GetKeyDown(toggleUi.key))
                {
                    UIBase ui = toggleUi.ui;
                    ui.Toggle();
                }
            }
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character UIs when owning character data updated
        /// </summary>
        public void UpdateCharacter()
        {
            foreach (UICharacter ui in uiCharacters)
            {
                if (ui != null)
                    ui.Data = BasePlayerCharacterController.OwningCharacter;
            }
            if (onUpdateCharacter != null)
                onUpdateCharacter.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character equip items UIs when owning character equip items updated
        /// </summary>
        public void UpdateEquipItems()
        {
            foreach (UIEquipItems ui in uiCharacterEquipItems)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateEquipItems != null)
                onUpdateEquipItems.Invoke(BasePlayerCharacterController.OwningCharacter);
        }
        
        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character non equip items UIs when owning character non equip items updated
        /// </summary>
        public void UpdateNonEquipItems()
        {
            foreach (UINonEquipItems ui in uiCharacterNonEquipItems)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateNonEquipItems != null)
                onUpdateNonEquipItems.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character skills UIs when owning character skills updated
        /// </summary>
        public void UpdateSkills()
        {
            foreach (UICharacterSkills ui in uiCharacterSkills)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateSkills != null)
                onUpdateSkills.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character summons UIs when owning character summons updated
        /// </summary>
        public void UpdateSummons()
        {
            foreach (UICharacterSummons ui in uiCharacterSummons)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateSummons != null)
                onUpdateSummons.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character hotkeys UIs when owning character hotkeys updated
        /// </summary>
        public void UpdateHotkeys()
        {
            foreach (UICharacterHotkeys ui in uiCharacterHotkeys)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateHotkeys != null)
                onUpdateHotkeys.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To update character quests UIs when owning character quests updated
        /// </summary>
        public void UpdateQuests()
        {
            foreach (UICharacterQuests ui in uiCharacterQuests)
            {
                if (ui != null)
                    ui.UpdateData(BasePlayerCharacterController.OwningCharacter);
            }
            if (onUpdateQuests != null)
                onUpdateQuests.Invoke(BasePlayerCharacterController.OwningCharacter);
        }

        /// <summary>
        /// This will be called from `BasePlayerCharacterController` class 
        /// To set selected target entity UIs
        /// </summary>
        /// <param name="entity"></param>
        public void SetTargetEntity(BaseGameEntity entity)
        {
            if (entity == null)
            {
                SetTargetCharacter(null);
                SetTargetNpc(null);
                SetTargetBuilding(null);
                SetTargetHarvestable(null);
                return;
            }

            if (entity is BaseCharacterEntity)
                SetTargetCharacter(entity as BaseCharacterEntity);
            if (entity is NpcEntity)
                SetTargetNpc(entity as NpcEntity);
            if (entity is BuildingEntity)
                SetTargetBuilding(entity as BuildingEntity);
            if (entity is HarvestableEntity)
                SetTargetHarvestable(entity as HarvestableEntity);
        }

        protected void SetTargetCharacter(BaseCharacterEntity character)
        {
            if (uiTargetCharacter == null)
                return;

            if (character == null)
            {
                uiTargetCharacter.Hide();
                return;
            }

            uiTargetCharacter.Data = character;
            uiTargetCharacter.Show();
        }

        protected void SetTargetNpc(NpcEntity npc)
        {
            if (uiTargetNpc == null)
                return;

            if (npc == null)
            {
                uiTargetNpc.Hide();
                return;
            }

            uiTargetNpc.Data = npc;
            uiTargetNpc.Show();
        }

        protected void SetTargetBuilding(BuildingEntity building)
        {
            if (uiTargetBuilding == null)
                return;

            if (building == null)
            {
                uiTargetBuilding.Hide();
                return;
            }

            uiTargetBuilding.Data = building;
            uiTargetBuilding.Show();
        }

        protected void SetTargetHarvestable(HarvestableEntity harvestable)
        {
            if (uiTargetHarvestable == null)
                return;

            if (harvestable == null)
            {
                uiTargetHarvestable.Hide();
                return;
            }

            uiTargetHarvestable.Data = harvestable;
            uiTargetHarvestable.Show();
        }

        public void SetActivePlayerCharacter(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiPlayerActivateMenu == null)
                return;

            uiPlayerActivateMenu.Data = playerCharacter;
            uiPlayerActivateMenu.Show();
        }

        public void OnClickRespawn()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestRespawn();
        }

        public void OnClickExit()
        {
            BaseGameNetworkManager.Singleton.StopHost();
        }

        public void OnCharacterDead()
        {
            onCharacterDead.Invoke();
        }

        public void OnCharacterRespawn()
        {
            onCharacterRespawn.Invoke();
        }

        public void OnShowNpcDialog(int npcDialogDataId)
        {
            if (uiNpcDialog == null)
                return;
            NpcDialog npcDialog;
            if (!GameInstance.NpcDialogs.TryGetValue(npcDialogDataId, out npcDialog))
            {
                uiNpcDialog.Hide();
                return;
            }
            uiNpcDialog.Data = npcDialog;
            uiNpcDialog.Show();
        }

        public void OnShowNpcRefine()
        {
            if (uiRefineItem == null)
                return;

            uiRefineItem.Show();
        }

        public void OnShowDealingRequest(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiDealingRequest == null)
                return;
            uiDealingRequest.Data = playerCharacter;
            uiDealingRequest.Show();
        }

        public void OnShowDealing(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiDealing == null)
                return;
            uiDealing.Data = playerCharacter;
            uiDealing.Show();
        }

        public void OnUpdateDealingState(DealingState state)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingState(state);
        }

        public void OnUpdateAnotherDealingState(DealingState state)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingState(state);
        }

        public void OnUpdateDealingGold(int gold)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingGold(gold);
        }

        public void OnUpdateAnotherDealingGold(int gold)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingGold(gold);
        }

        public void OnUpdateDealingItems(DealingCharacterItems items)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateDealingItems(items);
        }

        public void OnUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (uiDealing == null)
                return;
            uiDealing.UpdateAnotherDealingItems(items);
        }

        public void OnShowPartyInvitation(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiPartyInvitation == null)
                return;
            uiPartyInvitation.Data = playerCharacter;
            uiPartyInvitation.Show();
        }

        public void OnShowGuildInvitation(BasePlayerCharacterEntity playerCharacter)
        {
            if (uiGuildInvitation == null)
                return;
            uiGuildInvitation.Data = playerCharacter;
            uiGuildInvitation.Show();
        }

        public bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            // If it's not mobile ui, assume it's over UI
            if (ignorePointerDetectionUis != null && ignorePointerDetectionUis.Count > 0)
            {
                foreach (RaycastResult result in results)
                {
                    if (!ignorePointerDetectionUis.Contains(result.gameObject))
                        return true;
                }
            }
            else
                return results.Count > 0;
            return false;
        }

        public bool IsBlockController()
        {
            if (blockControllerUIs != null && blockControllerUIs.Count > 0)
            {
                foreach (UIBase ui in blockControllerUIs)
                {
                    if (ui.IsVisible())
                        return true;
                }
            }
            return false;
        }

        public void SpawnCombatText(Transform followingTransform, CombatAmountType combatAmountType, int amount)
        {
            switch (combatAmountType)
            {
                case CombatAmountType.Miss:
                    SpawnCombatText(followingTransform, uiCombatTextMiss, amount);
                    break;
                case CombatAmountType.NormalDamage:
                    SpawnCombatText(followingTransform, uiCombatTextNormalDamage, amount);
                    break;
                case CombatAmountType.CriticalDamage:
                    SpawnCombatText(followingTransform, uiCombatTextCriticalDamage, amount);
                    break;
                case CombatAmountType.BlockedDamage:
                    SpawnCombatText(followingTransform, uiCombatTextBlockedDamage, amount);
                    break;
                case CombatAmountType.HpRecovery:
                    SpawnCombatText(followingTransform, uiCombatTextHpRecovery, amount);
                    break;
                case CombatAmountType.MpRecovery:
                    SpawnCombatText(followingTransform, uiCombatTextMpRecovery, amount);
                    break;
                case CombatAmountType.StaminaRecovery:
                    SpawnCombatText(followingTransform, uiCombatTextStaminaRecovery, amount);
                    break;
                case CombatAmountType.FoodRecovery:
                    SpawnCombatText(followingTransform, uiCombatTextFoodRecovery, amount);
                    break;
                case CombatAmountType.WaterRecovery:
                    SpawnCombatText(followingTransform, uiCombatTextWaterRecovery, amount);
                    break;
            }
        }

        public void SpawnCombatText(Transform followingTransform, UICombatText prefab, int amount)
        {
            if (combatTextTransform != null && prefab != null)
            {
                UICombatText combatText = Instantiate(prefab, combatTextTransform);
                combatText.transform.localScale = Vector3.one;
                combatText.CacheObjectFollower.TargetObject = followingTransform;
                combatText.Amount = amount;
            }
        }
    }
}
