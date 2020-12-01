using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static readonly HashSet<GameObject> DraggingObjects = new HashSet<GameObject>();

    public Transform rootTransform;

    public Canvas CacheCanvas { get; protected set; }

    public List<Graphic> CacheGraphics { get; protected set; }

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

        CacheCanvas = GetComponentInParent<Canvas>();
        // Find root canvas, will use it to set as parent while dragging
        if (CacheCanvas != null)
            CacheCanvas = CacheCanvas.rootCanvas;

        CacheGraphics = new List<Graphic>();
        Graphic[] graphics = rootTransform.GetComponentsInChildren<Graphic>();
        foreach (Graphic graphic in graphics)
        {
            if (graphic.raycastTarget)
                CacheGraphics.Add(graphic);
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        defaultSiblingIndex = rootTransform.GetSiblingIndex();
        defaultParent = rootTransform.parent;
        defaultLocalPosition = rootTransform.localPosition;
        defaultLocalScale = rootTransform.localScale;

        if (!CanDrag)
            return;

        DraggingObjects.Add(gameObject);
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
        DraggingObjects.Remove(gameObject);
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
