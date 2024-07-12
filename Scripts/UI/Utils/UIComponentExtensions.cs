using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public static class UIComponentExtensions
    {
        public static async void SetImageGameDataIcon(this Image image, BaseGameData gameData, bool deactivateIfNotSprite = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            Sprite sprite = null;
            if (gameData)
                sprite = await gameData.GetIcon();
            image.SetImageSprite(sprite, deactivateIfNotSprite);
#endif
        }

        public static async void SetImageNpcDialogIcon(this Image image, BaseNpcDialog npcDialog, bool deactivateIfNotSprite = true)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            Sprite sprite = null;
            if (npcDialog)
                sprite = await npcDialog.GetIcon();
            image.SetImageSprite(sprite, deactivateIfNotSprite);
#endif
        }

        public static void SetImageSprite(this Image image, Sprite sprite, bool deactivateIfNotSprite = true)
        {
            if (!image)
                return;
            image.gameObject.SetActive(!deactivateIfNotSprite || sprite != null);
            image.sprite = sprite;
        }

        public static async void PlayNpcDialogVoice(this AudioSource source, MonoBehaviour uiRoot, BaseNpcDialog npcDialog)
        {
            source.Stop();
            AudioClip clip = await npcDialog.GetVoice();
            source.clip = clip;
            if (clip != null && uiRoot.enabled)
                source.Play();
        }
    }
}
