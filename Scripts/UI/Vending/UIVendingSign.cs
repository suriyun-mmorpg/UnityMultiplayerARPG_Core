using UnityEngine;

namespace MultiplayerARPG
{
    public class UIVendingSign : MonoBehaviour
    {
        public GameObject uiRoot;
        public TextWrapper textTitle;

        private BasePlayerCharacterEntity _entity;

        private void OnEnable()
        {
            _entity = GetComponentInParent<BasePlayerCharacterEntity>();
            if (_entity == null)
                return;
            _entity.Vending.onVendingDataChange += UpdateUI;
            UpdateUI(_entity.Vending.Data);
        }

        private void OnDisable()
        {
            if (_entity == null)
                return;
            _entity.Vending.onVendingDataChange -= UpdateUI;
        }

        public void UpdateUI(VendingData data)
        {
            if (uiRoot != null)
                uiRoot.SetActive(data.isStarted);
            if (textTitle != null)
                textTitle.text = data.title;
        }

        public void OnClickVending()
        {
            BaseUISceneGameplay.Singleton.ShowVending(_entity);
        }
    }
}