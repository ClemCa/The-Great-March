using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReorderButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform parentToReorder;
    private ScrollRect parentRect;
    private Transform originalParent;
    private LayoutGroup parentLayoutGroup;
    private bool down;
    private RectTransform rectTransform;
    private Vector3 basePos;
    private Vector3 baseWorldPos;
    private float yOffset;

    public void OnPointerDown(PointerEventData eventData)
    {
        baseWorldPos = rectTransform.position.SetY(Input.mousePosition.y);
        yOffset = baseWorldPos.y - rectTransform.position.y;
        transform.SetParent(parentRect.transform.parent, true);
        down = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.SetParent(originalParent, true);
        down = false;
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalParent = rectTransform.parent;
        basePos = rectTransform.localPosition;
        parentRect = transform.FindParentWithComponent(typeof(ScrollRect)).GetComponent<ScrollRect>();
        parentLayoutGroup = transform.FindParentWithComponent(typeof(LayoutGroup)).GetComponent<LayoutGroup>();
        parentToReorder = parentLayoutGroup.transform.Find(t => t.FindDeep(r => r.gameObject.GetInstanceID() == originalParent.gameObject.GetInstanceID()) != null).GetComponent<RectTransform>();
    }


    void Update()
    {
        if (!down)
        {
            rectTransform.localPosition = basePos;
            return;
        }
        rectTransform.position = rectTransform.position.SetY(Input.mousePosition.y - yOffset);
        var diff = baseWorldPos.y - Input.mousePosition.y;
        diff = diff / rectTransform.GetWorldRect().size.y;
        RefreshPosition(Mathf.FloorToInt(diff.Abs()) * diff.SignInt());
    }

    private void RefreshPosition(int positions)
    {
        if (positions == 0)
            return;
        int currentIndex = parentToReorder.GetSiblingIndex();
        if (currentIndex + positions < 0)
            return;
        if (currentIndex + positions >= parentToReorder.parent.childCount)
            positions = parentToReorder.parent.childCount - 1 - currentIndex;
        if (positions == 0)
            return;

        parentToReorder.SetSiblingIndex(parentToReorder.GetSiblingIndex() + positions);
        baseWorldPos += Vector3.down * positions * rectTransform.GetWorldRect().size.y;
    }
}
