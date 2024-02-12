using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UISocialCharacter))]
    public class UISocialCharacterView : MonoBehaviour
    {
        public int index = -1;
        public string id = string.Empty;
        public RawImage view;
        public GameObject[] noCharacterPlaceholders = new GameObject[0];
        private UISocialCharacter uiSocialCharacter;
        private RenderTexture renderTexture;

        private void Start()
        {
            uiSocialCharacter = GetComponent<UISocialCharacter>();
            SetShowNoCharacterPlaceHolders(true);
        }

        private void OnEnable()
        {
            Clear();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void Update()
        {
            if (index != transform.GetSiblingIndex() || !id.Equals(uiSocialCharacter.Data.id))
            {
                index = transform.GetSiblingIndex();
                id = uiSocialCharacter.Data.id;
                SetShowNoCharacterPlaceHolders(true);
                view.enabled = false;
                RenderTexture renderTexture;
                UISocialCharacterViewManager.Singleton.LoadCharacter(index, id, out renderTexture, (requestHandler, responseCode, response) =>
                {
                    SetShowNoCharacterPlaceHolders(responseCode != LiteNetLibManager.AckResponseCode.Success);
                    view.enabled = responseCode == LiteNetLibManager.AckResponseCode.Success;
                    view.texture = this.renderTexture;
                });
                this.renderTexture = renderTexture;
            }
        }

        public void Clear()
        {
            if (index >= 0)
                UISocialCharacterViewManager.Singleton.ClearContainer(index);
            index = -1;
            id = string.Empty;
        }

        public void SetShowNoCharacterPlaceHolders(bool isShow)
        {
            for (int i = 0; i < noCharacterPlaceholders.Length; ++i)
            {
                noCharacterPlaceholders[i].SetActive(isShow);
            }
        }
    }
}
