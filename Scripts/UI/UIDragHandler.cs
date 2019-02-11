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

    private int defaultSiblingIndex;
    private Transform defaultParent;
    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalScale;
    private Button attachedButton;

    public void OnBeginDrag(PointerEventData eventData)
    {
        defaultSiblingIndex = rootTransform.GetSiblingIndex();
        defaultParent = rootTransform.parent;
        defaultLocalPosition = rootTransform.localPosition;
        defaultLocalScale = rootTransform.localScale;

        rootTransform.SetParent(CacheCanvas.transform);
        rootTransform.SetAsLastSibling();

        attachedButton = rootTransform.GetComponent<Button>();
        if (attachedButton != null)
            attachedButton.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rootTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        rootTransform.SetParent(defaultParent);
        rootTransform.SetSiblingIndex(defaultSiblingIndex);
        rootTransform.localPosition = defaultLocalPosition;
        rootTransform.localScale = defaultLocalScale;

        if (attachedButton != null)
            attachedButton.enabled = true;
    }
}
