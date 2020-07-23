using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if USE_TEXT_MESH_PRO
using TMPro;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UIFollowWorldObject))]
    [RequireComponent(typeof(TextWrapper))]
    public class UICombatText : MonoBehaviour
    {
        public float lifeTime = 2f;
        public string format = "{0}";
        public bool showPositiveSign;

        public UIFollowWorldObject CacheObjectFollower { get; private set; }

        public TextWrapper CacheText { get; private set; }

        private int amount;
        public int Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                CacheText.text = string.Format(format, (showPositiveSign && amount > 0 ? "+" : string.Empty) + amount.ToString("N0"));
            }
        }

        public bool AlreadyCachedComponents { get; private set; }

        private void Awake()
        {
            CacheComponents();
            Destroy(gameObject, lifeTime);
        }

        private void CacheComponents()
        {
            if (AlreadyCachedComponents)
                return;

            CacheObjectFollower = GetComponent<UIFollowWorldObject>();
            CacheText = gameObject.GetOrAddComponent<TextWrapper>((comp) =>
            {
                comp.unityText = GetComponent<Text>();
#if USE_TEXT_MESH_PRO
                comp.textMeshText = GetComponent<TextMeshProUGUI>();
#endif
            });
            AlreadyCachedComponents = true;
        }
    }
}
