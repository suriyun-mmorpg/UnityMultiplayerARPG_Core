using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIFollowWorldObject))]
[RequireComponent(typeof(Text))]
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

    private Text cacheText;
    public Text CacheText
    {
        get
        {
            if (cacheText == null)
                cacheText = GetComponent<Text>();
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
            var positiveSign = showPositiveSign && amount > 0 ? "+" : "";
            CacheText.text = string.Format(format, (positiveSign + amount.ToString("N0")));
        }
    }

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }
}
