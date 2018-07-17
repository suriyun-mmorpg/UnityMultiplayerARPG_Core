using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        public struct UIToggleUI
        {
            public UIBase ui;
            public KeyCode key;
        }

        public static UISceneGameplay Singleton { get; private set; }

        public UICharacter[] uiCharacters;
        public UICharacter uiTargetCharacter;
        public UIEquipItems uiEquipItems;
        public UINonEquipItems uiNonEquipItems;
        public UICharacterSkills uiSkills;
        public UICharacterHotkeys uiHotkeys;
        public UICharacterQuests uiQuests;
        public UINpcDialog uiNpcDialog;
        public UIRefineItem uiRefineItem;
        public UIConstructBuilding uiConstructBuilding;
        public UICurrentBuilding uiCurrentBuilding;
        public UIPlayerActivateMenu uiPlayerActivateMenu;
        public UIDealingRequest uiDealingRequest;
        public UIDealing uiDealing;
        public UIToggleUI[] toggleUis;
        public List<GameObject> ignorePointerDetectionUis;

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

        #region Cache components
        private BaseGameNetworkManager cacheGameNetworkManager;
        public BaseGameNetworkManager CacheGameNetworkManager
        {
            get
            {
                if (cacheGameNetworkManager == null)
                    cacheGameNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                if (cacheGameNetworkManager == null)
                    Debug.LogWarning("[UISceneGameplay(" + name + ")] Cannot find `BaseGameNetworkManager`");
                return cacheGameNetworkManager;
            }
        }
        #endregion

        private void Awake()
        {
            Singleton = this;
        }

        private void Update()
        {
            var fields = FindObjectsOfType<InputField>();
            foreach (var field in fields)
            {
                if (field.isFocused)
                    return;
            }

            foreach (var toggleUi in toggleUis)
            {
                if (Input.GetKeyDown(toggleUi.key))
                {
                    var ui = toggleUi.ui;
                    ui.Toggle();
                }
            }
        }

        public void UpdateCharacter()
        {
            foreach (var uiCharacter in uiCharacters)
            {
                if (uiCharacter != null)
                    uiCharacter.Data = BasePlayerCharacterController.OwningCharacter;
            }
        }

        public void UpdateEquipItems()
        {
            if (uiEquipItems != null)
                uiEquipItems.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void UpdateNonEquipItems()
        {
            if (uiNonEquipItems != null)
                uiNonEquipItems.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void UpdateSkills()
        {
            if (uiSkills != null)
                uiSkills.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void UpdateHotkeys()
        {
            if (uiHotkeys != null)
                uiHotkeys.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void UpdateQuests()
        {
            if (uiQuests != null)
                uiQuests.UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void SetTargetCharacter(BaseCharacterEntity character)
        {
            if (uiTargetCharacter == null)
                return;

            if (character == null || character.IsDead())
            {
                uiTargetCharacter.Hide();
                return;
            }

            uiTargetCharacter.Data = character;
            uiTargetCharacter.Show();
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
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestRespawn();
        }

        public void OnClickExit()
        {
            if (CacheGameNetworkManager != null)
                CacheGameNetworkManager.StopHost();
        }

        public void OnCharacterDead(bool isInitialize)
        {
            onCharacterDead.Invoke();
        }

        public void OnCharacterRespawn(bool isInitialize)
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

        public bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            // If it's not mobile ui, assume it's over UI
            var overUI = false;
            if (ignorePointerDetectionUis != null && ignorePointerDetectionUis.Count > 0)
            {
                foreach (var result in results)
                {
                    if (!ignorePointerDetectionUis.Contains(result.gameObject))
                    {
                        overUI = true;
                        break;
                    }
                }
            }
            else
                overUI = results.Count > 0;
            return overUI;
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
                var combatText = Instantiate(prefab, combatTextTransform);
                combatText.transform.localScale = Vector3.one;
                combatText.CacheObjectFollower.TargetObject = followingTransform;
                combatText.Amount = amount;
            }
        }
    }
}
