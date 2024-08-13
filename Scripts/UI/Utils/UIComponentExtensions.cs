using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public static class UIComponentExtensions
    {
        public static async void SetImageGameDataIcon(this Image image, BaseGameData gameData, bool deactivateIfNoContent = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            Sprite sprite = null;
            if (gameData)
                sprite = await gameData.GetIcon();
            image.SetImageSprite(sprite, deactivateIfNoContent);
#endif
        }

        public static async void SetRawImageExternalTexture(this RawImage rawImage, string url, Texture2D defaultTexture = null, bool deactivateIfNoContent = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            if (!rawImage)
                return;
            Texture2D texture = null;
            // TODO: May improve it by change URL format before load
            if (string.IsNullOrWhiteSpace(url))
            {
                texture = await ExternalTextureManager.Load(url);
                if (texture == null)
                    texture = defaultTexture;
            }
            rawImage.gameObject.SetActive(!deactivateIfNoContent || texture != null);
            rawImage.texture = texture;
#endif
        }

        public static async void SetImageNpcDialogIcon(this Image image, BaseNpcDialog npcDialog, bool deactivateIfNoContent = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            Sprite sprite = null;
            if (npcDialog)
                sprite = await npcDialog.GetIcon();
            image.SetImageSprite(sprite, deactivateIfNoContent);
#endif
        }

        public static void SetImageSprite(this Image image, Sprite sprite, bool deactivateIfNoContent = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            if (!image)
                return;
            image.gameObject.SetActive(!deactivateIfNoContent || sprite != null);
            image.sprite = sprite;
#endif
        }

        public static async void PlayNpcDialogVoice(this AudioSource source, MonoBehaviour uiRoot, BaseNpcDialog npcDialog)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            if (!source)
                return;
            source.Stop();
            AudioClip clip = await npcDialog.GetVoice();
            source.clip = clip;
            if (clip != null && uiRoot.enabled)
                source.Play();
#endif
        }
    }
}
