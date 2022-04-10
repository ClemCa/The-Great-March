using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromptMenu : MonoBehaviour
{
    private static PromptMenu _instance;
    private Prompt.PromptData _data;
    private bool _enabled = false;
    private RectTransform _rectTransform;
    private float _visibility;
    private CanvasGroup _canvasGroup;

    void Start()
    {
        _instance = this;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        _canvasGroup.alpha = _visibility;
        if (!_enabled)
        {
            if(_visibility > 0)
            {
                _visibility -= Time.deltaTime * 5;
                transform.position = Input.mousePosition - new Vector3(1, 1);
            }
            else
            {
                transform.position = new Vector3(0, 0);
            }
            return;
        }
        _visibility = 1;
        transform.position = Input.mousePosition - new Vector3(1,1);
        var rect = transform.FindDeep("Description").GetComponent<RectTransform>();

        if (_data.FacilityMenu != null)
        {
            var info = Registry.Instance.GetFacilityInfo(_data.FacilityMenu.Facility);
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = info.Name;
            transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = info.Description;
            transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "Produces 1 " + Registry.Instance.GetResourceName(info.AssociatedResource) + " every " + info.Cooldown.ToString()+"s";
            transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            rect.anchorMin = rect.anchorMin.SetY(0.15);
            return;
        }
        if (_data.TransformationFacilityMenu != null)
        {
            var info = Registry.Instance.GetFacilityInfo(_data.TransformationFacilityMenu.Facility.Value);
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = info.Name;
            transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = info.Description;
            string content = "";
            for(int i = 0; i < info.InputResources.Length; i++)
            {
                if (i == 0)
                    content += info.InputResources[0];
                else if (i == info.InputResources.Length - 1)
                    content += " and " + info.InputResources[i];
                else
                    content += ", " + info.InputResources[i];
            }
            transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "Consumes " + info.Cost + " " + content;
            transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "Produces " + info.Production + " " + (info.Advanced ? Registry.Instance.GetResourceName(info.OutputResourceTransformation).ToString() : Registry.Instance.GetResourceName(info.OutputResource).ToString()) + " every " + info.Cooldown.ToString() + "s";
            rect.anchorMin = rect.anchorMin.SetY(0.25);
            return;
        }
        if (_data.ResourceSelection != null)
        {
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = Registry.Instance.GetResourceName(_data.ResourceSelection.GetResource());
            transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = Registry.Instance.GetResourceDescription(_data.ResourceSelection.GetResource());
            transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            rect.anchorMin = rect.anchorMin.SetY(0);
            return;
        }
        if (_data.ResourceMenu != null)
        {
            var name = _data.ResourceMenu.AdvancedResources ? Registry.Instance.GetResourceName(_data.ResourceMenu.AdvancedResourceType) : Registry.Instance.GetResourceName(_data.ResourceMenu.ResourceType);
            var description = _data.ResourceMenu.AdvancedResources ? Registry.Instance.GetResourceDescription(_data.ResourceMenu.AdvancedResourceType) : Registry.Instance.GetResourceDescription(_data.ResourceMenu.ResourceType);
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = name;
            transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = description;
            transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            rect.anchorMin = rect.anchorMin.SetY(0);
            rect.offsetMax = rect.offsetMax.SetY(0);
            return;
        }
        if (_data.SlotMenu != null)
        {
            if (_data.SlotMenu.HasFacility())
            {
                var info = Registry.Instance.GetFacilityInfo(Planet.Selected.GetFacility(_data.SlotMenu.ResourceType));
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = info.Name;
                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = info.Description;
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "Produces 1 " + Registry.Instance.GetResourceName(info.AssociatedResource) + " every " + info.Cooldown.ToString() + "s";
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                rect.anchorMin = rect.anchorMin.SetY(0.15);
            }
            else
            {
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = Registry.Instance.GetResourceName(_data.SlotMenu.ResourceType);
                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = Registry.Instance.GetResourceDescription(_data.SlotMenu.ResourceType);
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                rect.anchorMin = rect.anchorMin.SetY(0);
            }
            return;
        }
        if (_data.WildcardMenu != null)
        {
            if (_data.WildcardMenu.Facility.HasValue)
            {
                var info = Registry.Instance.GetFacilityInfo(_data.WildcardMenu.Facility.Value);
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = info.Name;
                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = info.Description;
                string content = "";
                for (int i = 0; i < info.InputResources.Length; i++)
                {
                    if (i == 0)
                        content += info.InputResources[0];
                    else if (i == info.InputResources.Length - 1)
                        content += " and " + info.InputResources[i];
                    else
                        content += ", " + info.InputResources[i];
                }
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "Consumes " + info.Cost + " " + content;
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "Produces " + info.Production + " " + (info.Advanced ? Registry.Instance.GetResourceName(info.OutputResourceTransformation).ToString() : Registry.Instance.GetResourceName(info.OutputResource).ToString()) + " every " + info.Cooldown.ToString() + "s";
                rect.anchorMin = rect.anchorMin.SetY(0.25);
            }
            else
            {
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = "Plot of land";
                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = "An empty plot of land";
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                rect.anchorMin = rect.anchorMin.SetY(0);
            }
            return;
        }
        transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Title;
        transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Description;
        transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
        transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
        rect.anchorMin = rect.anchorMin.SetY(0);
    }

    public static void Show(Prompt.PromptData data)
    {
        _instance._data = data;
        _instance._enabled = true;
    }
    public static void Hide()
    {
        _instance._enabled = false;
    }
}
