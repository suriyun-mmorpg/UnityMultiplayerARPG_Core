using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static readonly HashSet<GameObject> DraggingObjects = new HashSet<GameObject>();
    public enum ScrollRectAllowing
    {
        None,
        AllowVerticalScrolling,
        AllowHorizontalScrolling,
    }

    public Transform rootTransform;
    public ScrollRectAllowing scrollRectAllowing;
    public ScrollRect scrollRect;
    public UnityEvent onStart = new UnityEvent();
    public UnityEvent onBeginDrag = new UnityEvent();
    public UnityEvent onEndDrag = new UnityEvent();

    public Canvas CacheCanvas { get; protected set; }

    public List<Graphic> CacheGraphics { get; protected set; }

    public virtual bool CanDrag { get { return true; } }

    public virtual bool CanDrop { get { return CanDrag && !IsDropped && !IsScrolling; } }

    public bool IsScrolling { get; protected set; }

    public bool IsDropped { get; set; }

    private int _defaultSiblingIndex;
    private Transform _defaultParent;
    private Vector3 _defaultLocalPosition;
    private Vector3 _defaultLocalScale;
    private Button _attachedButton;

    protected virtual void Start()
    {
        if (rootTransform == null)
            rootTransform = transform;

        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>();

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

        onStart.Invoke();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRect != null)
        {
            if (scrollRectAllowing == ScrollRectAllowing.AllowVerticalScrolling &&
                Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
            {
                IsScrolling = true;
                scrollRect.SendMessage("OnBeginDrag", eventData);
                return;
            }

            if (scrollRectAllowing == ScrollRectAllowing.AllowHorizontalScrolling &&
                Mathf.Abs(eventData.delta.y) < Mathf.Abs(eventData.delta.x))
            {
                IsScrolling = true;
                scrollRect.SendMessage("OnBeginDrag", eventData);
                return;
            }
        }

        _defaultSiblingIndex = rootTransform.GetSiblingIndex();
        _defaultParent = rootTransform.parent;
        _defaultLocalPosition = rootTransform.localPosition;
        _defaultLocalScale = rootTransform.localScale;

        if (!CanDrag)
            return;

        DraggingObjects.Add(gameObject);
        IsDropped = false;
        rootTransform.SetParent(CacheCanvas.transform);
        rootTransform.SetAsLastSibling();

        // Disable button to not trigger on click event after drag
        _attachedButton = rootTransform.GetComponent<Button>();
        if (_attachedButton != null)
            _attachedButton.enabled = false;

        // Don't raycast while dragging to avoid it going to obstruct drop area
        foreach (Graphic graphic in CacheGraphics)
        {
            graphic.raycastTarget = false;
        }

        onBeginDrag.Invoke();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (IsScrolling)
        {
            scrollRect.SendMessage("OnDrag", eventData);
            return;
        }
        if (!CanDrag)
            return;
        rootTransform.position = eventData.position;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (IsScrolling)
        {
            scrollRect.SendMessage("OnEndDrag", eventData);
            IsScrolling = false;
            return;
        }
        DraggingObjects.Remove(gameObject);
        rootTransform.SetParent(_defaultParent);
        rootTransform.SetSiblingIndex(_defaultSiblingIndex);
        rootTransform.localPosition = _defaultLocalPosition;
        rootTransform.localScale = _defaultLocalScale;

        // Enable button to allow on click event after drag
        if (_attachedButton != null)
            _attachedButton.enabled = true;

        // Enable raycast graphics
        foreach (Graphic graphic in CacheGraphics)
        {
            graphic.raycastTarget = true;
        }

        onEndDrag.Invoke();
    }
}
