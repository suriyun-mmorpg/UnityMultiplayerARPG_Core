using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UIFollowWorldObject))]
    [RequireComponent(typeof(TextWrapper))]
    public class UICombatText : MonoBehaviour
    {
        public float lifeTime = 2f;
        public string format = "{0}";
        public bool showPositiveSign;

        private UIFollowWorldObject cacheObjectFollower;
        public UIFollowWorldObject CacheObjectFollower
        {
            get
            {
                if (cacheObjectFollower == null)
                    cacheObjectFollower = GetComponent<UIFollowWorldObject>();
                return cacheObjectFollower;
            }
        }

        private TextWrapper cacheText;
        public TextWrapper CacheText
        {
            get
            {
                if (cacheText == null)
                {
                    cacheText = GetComponent<TextWrapper>();
                    if (cacheText == null)
                    {
                        cacheText = gameObject.AddComponent<TextWrapper>();
                        cacheText.unityText = GetComponent<Text>();
                    }
                }
                return cacheText;
            }
        }

        private int amount;
        public int Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                string positiveSign = showPositiveSign && amount > 0 ? "+" : "";
                CacheText.text = string.Format(format, (positiveSign + amount.ToString("N0")));
            }
        }

        private void Awake()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}
