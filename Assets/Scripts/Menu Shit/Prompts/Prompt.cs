using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class Prompt : MonoBehaviour
{
    [SerializeField] private PromptData _data;
    private bool _hover = false;

    [Serializable]
    public class PromptData
    {
        public string Title;
        public string Description;
        public FacilityMenu FacilityMenu;
        public TransformationFacilityMenu TransformationFacilityMenu;
        public ResourceSelection ResourceSelection;
        public ResourceMenu ResourceMenu;
        public SlotMenu SlotMenu;
        public WildcardMenu WildcardMenu;
    }

    void Update()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> r = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, r);
        if (r.Count != 0 && (r[0].gameObject == gameObject || r[0].gameObject.transform.IsChildOf(transform)))
        {
            SetHover(true);
        }
        else
        {
            SetHover(false);
        }
    }

    private void SetHover(bool value)
    {
        if(value != _hover)
        {
            if(value)
                PromptMenu.Show(_data);
            else
                PromptMenu.Hide();
            _hover = value;
        }
    }
}
