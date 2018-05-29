using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

public class RpgNetworkEntity : LiteNetLibBehaviour
{
    public string title;
    public Text textTitle;

    public virtual string Title { get { return title; } }

    private Transform cacheTransform;
    public Transform CacheTransform
    {
        get
        {
            if (cacheTransform == null)
                cacheTransform = GetComponent<Transform>();
            return cacheTransform;
        }
    }

    protected virtual void Awake() { }

    protected virtual void Start() { }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() { }

    protected virtual void Update() { }

    protected virtual void LateUpdate()
    {
        if (textTitle != null)
            textTitle.text = Title;
    }

    protected virtual void FixedUpdate() { }
}
