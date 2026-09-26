using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ClemCAddons;
using UnityEngine.UI;

public class WildcardMenu : MonoBehaviour, IPointerClickHandler
{
    private string _facility;
    private int _id = 0;

    public string Facility { get => _facility; }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_facility) && TransformationFacilityMenu.OrderedFacilities.Count <= _id)
        {
            MenuAudioManager.Instance.PlayClick();
            SubMenu.GetInstance(SubMenu.SubMenuMode.TransformationFacilitySubMenu).Flip(transform.FindParentDeep("PlanetMenu").Find("Inventory Section"), GetEntityId().GetHashCode());
        }
    }
    void Start()
    {
        if (Planet.Selected && !string.IsNullOrEmpty(_facility))
            GetComponentInChildren<Image>().sprite = Registry.Instance.GetFacilitySprite(_facility);
        else
            Clear();
    }

    void Update()
    {
        if (Planet.Selected != null && !string.IsNullOrEmpty(_facility))
        {
            transform.GetChild(0).GetComponent<Image>().sprite = Registry.Instance.GetFacilitySprite(_facility);
            var info = Registry.Instance.GetFacilityInfo(_facility);
            var inputs = info.GetEffects(Registry.FacilityEffectType.Consume);
            var outputs = info.GetEffects(Registry.FacilityEffectType.Produce);
            transform.Find("Output").GetComponent<Image>().enabled = true;
            if (inputs.Length >= 2)
            {
                transform.Find("SourceSingle").GetComponent<Image>().enabled = false;
                transform.Find("Source0").GetComponent<Image>().enabled = true;
                transform.Find("Source1").GetComponent<Image>().enabled = true;
                transform.Find("Source0").GetComponent<Image>().sprite = GetEffectSprite(inputs[0]);
                transform.Find("Source1").GetComponent<Image>().sprite = GetEffectSprite(inputs[1]);
            }
            else
            {
                transform.Find("SourceSingle").GetComponent<Image>().enabled = inputs.Length == 1;
                transform.Find("Source0").GetComponent<Image>().enabled = false;
                transform.Find("Source1").GetComponent<Image>().enabled = false;
                if (inputs.Length == 1)
                    transform.Find("SourceSingle").GetComponent<Image>().sprite = GetEffectSprite(inputs[0]);
            }
            if (outputs.Length > 0)
                transform.Find("Output").GetComponent<Image>().sprite = GetEffectSprite(outputs[0]);

            transform.Find("Progression").GetComponent<RectTransform>().sizeDelta =
                        new Vector2(transform.Find("Progression").GetComponent<RectTransform>().sizeDelta.x,
                        GetComponent<RectTransform>().rect.height * Planet.Selected.GetFactoryProgression(_facility));
            return;
        }
        Clear();
    }

    private Sprite GetEffectSprite(Registry.FacilityEffect effect)
    {
        return effect.Advanced
            ? Registry.Instance.GetAdvancedResourceSprite(effect.AdvancedResource)
            : Registry.Instance.GetResourceSprite(effect.Resource);
    }
    private void Clear()
    {
        transform.Find("SourceSingle").GetComponent<Image>().enabled = false;
        transform.Find("Source0").GetComponent<Image>().enabled = false;
        transform.Find("Source1").GetComponent<Image>().enabled = false;
        transform.Find("Output").GetComponent<Image>().enabled = false;
        if (Registry.Instance != null && transform.childCount > 0)
            transform.GetChild(0).GetComponent<Image>().sprite = Registry.Instance.GetWildcardSprite();
        transform.Find("Progression").GetComponent<RectTransform>().sizeDelta =
                    new Vector2(transform.Find("Progression").GetComponent<RectTransform>().sizeDelta.x,
                    GetComponent<RectTransform>().rect.height * 0);
    }
    public void SetFacility(string facility)
    {
        _facility = facility;
    }
    public void SetID(int id)
    {
        _id = id;
    }
}
