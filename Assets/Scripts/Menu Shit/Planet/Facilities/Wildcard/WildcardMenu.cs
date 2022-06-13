using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ClemCAddons;
using UnityEngine.UI;

public class WildcardMenu : MonoBehaviour, IPointerClickHandler
{
    private Registry.TransformationFacilities? _facility;
    private int _id = 0;

    public Registry.TransformationFacilities? Facility { get => _facility; }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_facility.HasValue && TransformationFacilityMenu.OrderedFacilities.Count <= _id)
        {
            MenuAudioManager.Instance.PlayClick();
            SubMenu.GetInstance(SubMenu.SubMenuMode.TransformationFacilitySubMenu).Flip(transform.FindParentDeep("PlanetMenu").Find("Inventory Section"), GetInstanceID());
        }
    }
    void Start()
    {
        if (Planet.Selected && _facility.HasValue)
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetTransformationFacilitySprite(_facility.Value);
        else
            Clear();
    }

    void Update()
    {
        if (Planet.Selected != null && _facility.HasValue)
        {
            transform.GetChild(0).GetComponent<Image>().sprite = Registry.Instance.GetTransformationFacilitySprite(_facility.Value);
            var info = Registry.Instance.GetFacilityInfo(_facility.Value);
            transform.Find("Output").GetComponent<Image>().enabled = true;
            if (info.InputResources.Length == 2)
            {
                transform.Find("SourceSingle").GetComponent<Image>().enabled = false;
                transform.Find("Source0").GetComponent<Image>().enabled = true;
                transform.Find("Source1").GetComponent<Image>().enabled = true;
                transform.Find("Source0").GetComponent<Image>().sprite = Registry.Instance.GetResourceSprite(info.InputResources[0]);
                transform.Find("Source1").GetComponent<Image>().sprite = Registry.Instance.GetResourceSprite(info.InputResources[1]);
            }
            else
            {
                transform.Find("SourceSingle").GetComponent<Image>().enabled = true;
                transform.Find("Source0").GetComponent<Image>().enabled = false;
                transform.Find("Source1").GetComponent<Image>().enabled = false;
                transform.Find("SourceSingle").GetComponent<Image>().sprite = Registry.Instance.GetResourceSprite(info.InputResources[0]);
            }
            if(info.Advanced)
                transform.Find("Output").GetComponent<Image>().sprite = Registry.Instance.GetAdvancedResourceSprite(info.OutputResourceTransformation);
            else
                transform.Find("Output").GetComponent<Image>().sprite = Registry.Instance.GetResourceSprite(info.OutputResource);

            transform.Find("Progression").GetComponent<RectTransform>().sizeDelta =
                        new Vector2(transform.Find("Progression").GetComponent<RectTransform>().sizeDelta.x,
                        GetComponent<RectTransform>().rect.height * Planet.Selected.GetFactoryProgression(_facility.Value));
            return;
        }
        Clear();
    }
    private void Clear()
    {
        transform.Find("SourceSingle").GetComponent<Image>().enabled = false;
        transform.Find("Source0").GetComponent<Image>().enabled = false;
        transform.Find("Source1").GetComponent<Image>().enabled = false;
        transform.Find("Output").GetComponent<Image>().enabled = false;
        transform.GetChild(0).GetComponent<Image>().sprite = Registry.Instance.GetWildcardSprite();
        transform.Find("Progression").GetComponent<RectTransform>().sizeDelta =
                    new Vector2(transform.Find("Progression").GetComponent<RectTransform>().sizeDelta.x,
                    GetComponent<RectTransform>().rect.height * 0);
    }
    public void SetFacility(Registry.TransformationFacilities facility)
    {
        _facility = facility;
    }
    public void SetID(int id)
    {
        _id = id;
    }
}
