using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform rootTransform;

    private Canvas cacheCanvas;
    public Canvas CacheCanvas
    {
        get
        {
            if (cacheCanvas == null)
            {
                cacheCanvas = GetComponentInParent<Canvas>();
                // Find root canvas, will use it to set as parent while dragging
                if (cacheCanvas != null)
                    cacheCanvas = cacheCanvas.rootCanvas;
            }
            return cacheCanvas;
        }
    }

    private List<Graphic> cacheGraphics;
    public List<Graphic> CacheGraphics
    {
        get
        {
            if (cacheGraphics == null)
            {
                cacheGraphics = new List<Graphic>();
                Graphic[] graphics = rootTransform.GetComponentsInChildren<Graphic>();
                foreach (Graphic graphic in graphics)
                {
                    if (graphic.raycastTarget)
                        cacheGraphics.Add(graphic);
                }
            }
            return cacheGraphics;
        }
    }

    public virtual bool CanDrag { get { return true; } }

    [System.NonSerialized]
    public bool isDropped;

    private int defaultSiblingIndex;
    private Transform defaultParent;
    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalScale;
    private Button attachedButton;

    protected virtual void Start()
    {
        if (rootTransform == null)
            rootTransform = transform;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        defaultSiblingIndex = rootTransform.GetSiblingIndex();
        defaultParent = rootTransform.parent;
        defaultLocalPosition = rootTransform.localPosition;
        defaultLocalScale = rootTransform.localScale;

        if (!CanDrag)
            return;

        isDropped = false;
        rootTransform.SetParent(CacheCanvas.transform);
        rootTransform.SetAsLastSibling();

        // Disable button to not trigger on click event after drag
        attachedButton = rootTransform.GetComponent<Button>();
        if (attachedButton != null)
            attachedButton.enabled = false;

        // Don't raycast while dragging to avoid it going to obstruct drop area
        foreach (Graphic graphic in CacheGraphics)
        {
            graphic.raycastTarget = false;
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        rootTransform.position = Input.mousePosition;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        rootTransform.SetParent(defaultParent);
        rootTransform.SetSiblingIndex(defaultSiblingIndex);
        rootTransform.localPosition = defaultLocalPosition;
        rootTransform.localScale = defaultLocalScale;

        // Enable button to allow on click event after drag
        if (attachedButton != null)
            attachedButton.enabled = true;
        
        // Enable raycast graphics
        foreach (Graphic graphic in CacheGraphics)
        {
            graphic.raycastTarget = true;
        }
    }
}
