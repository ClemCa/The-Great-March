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
    private Vector2 _baseSize;
    [SerializeField] private Vector2 _extendedSize;

    void Awake()
    {
        _instance = this;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _baseSize = _rectTransform.sizeDelta;
    }

    void Update()
    {
        _canvasGroup.alpha = _visibility;
        if (!_enabled || !Settings.ShowPrompt)
        {
            if(_visibility > 0)
            {
                _visibility -= Time.unscaledDeltaTime * 5;
                SetPosition(Input.mousePosition - new Vector3(1, 1));
            }
            else
            {
                transform.position = new Vector3(0, 0);
            }
            return;
        }
        _visibility = 1;
        SetPosition(Input.mousePosition - new Vector3(1,1));
        var rect = transform.FindDeep("Description").GetComponent<RectTransform>();

        if (_data.FacilityMenu != null)
        {
            var info = Registry.Instance.GetFacilityInfo(_data.FacilityMenu.Facility);
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = info.Name;
            transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = info.Description;
            transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "Produces 1 " + Registry.Instance.GetResourceName(info.AssociatedResource) + " every " + info.Cooldown.ToString()+"s";
            transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
            rect.anchorMin = rect.anchorMin.SetY(0.15);
            _rectTransform.sizeDelta = _baseSize;
            return;
        }
        if (_data.TransformationFacilityMenu != null)
        {
            var info = Registry.Instance.GetFacilityInfo(_data.TransformationFacilityMenu.Facility.Value);
            if(_data.Extended || _data.TransformationFacilityMenu.Facility.Value == Registry.TransformationFacilities.Factory)
            {
                _rectTransform.sizeDelta = _extendedSize;
            }
            else
            {
                _rectTransform.sizeDelta = _baseSize;
            }
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
        _rectTransform.sizeDelta = _baseSize;
        if (_data.ResourceSelection != null)
        {
            var name = _data.ResourceSelection.GetResourceType() ? _data.ResourceSelection.GetAdvancedResource().ToString() : _data.ResourceSelection.GetResource().ToString();
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = name + " (" + (_data.ResourceSelection.GetResourceType() ? Planet.Selected.GetResource(_data.ResourceSelection.GetAdvancedResource()) : Planet.Selected.GetResource(_data.ResourceSelection.GetResource())) + ")";
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
            transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = name + " ("+(_data.ResourceMenu.AdvancedResources ? Planet.Selected.GetResource(_data.ResourceMenu.AdvancedResourceType) : Planet.Selected.GetResource(_data.ResourceMenu.ResourceType))+")";
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
        if (_data.ShipChoice != null)
        {
            if (_data.ShipChoice.RefuelButton.interactable && _data.ShipChoice.RefuelText.text.StartsWith("Refuel"))
            {
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = "Refuel";
                var required = Registry.Instance.GetRequiredFuel(_data.ShipChoice.Ships[_data.ShipChoice.Selected]);
                var simulated = Planet.Selected.SimulateFuel(required);
                var gas = simulated[0] == 0 ? "" : Registry.Instance.GetResourceName(Registry.Resources.Gas) + ": -" + simulated[0];
                var oil = simulated[1] == 0 ? "" : Registry.Instance.GetResourceName(Registry.Resources.Oil) + ": -" + simulated[1];
                var hydrogen = simulated[2] == 0 ? "" : Registry.Instance.GetResourceName(Registry.Resources.Hydrogen) + ": -" + simulated[2];
                var highfuel = simulated[3] == 0 ? "" : Registry.Instance.GetResourceName(Registry.AdvancedResources.HighEfficiencyFuel) + ": -" + simulated[3];
                var hydrogenbat = simulated[4] == 0 ? "" : Registry.Instance.GetResourceName(Registry.AdvancedResources.HydrogenBattery) + ": -" + simulated[4];

                var txt = "";
                if (gas != "")
                    txt += gas;
                if (oil != "")
                    txt += txt != "" ? "\n" + oil : oil;
                if (hydrogen != "")
                    txt += txt != "" ? "\n" + hydrogen : hydrogen;
                if (highfuel != "")
                    txt += txt != "" ? "\n" + highfuel : highfuel;
                if (hydrogenbat != "")
                    txt += txt != "" ? "\n" + hydrogenbat : hydrogenbat;

                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = txt;
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                rect.anchorMin = rect.anchorMin.SetY(0.1);
            }
            else if (_data.ShipChoice.RefuelText.color == Color.red)
            {
                transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = "Refuel";
                transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = "Missing fuel";
                transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = "";
                rect.anchorMin = rect.anchorMin.SetY(0);
            }
            else
            {
                _enabled = false;
            }
            return;
        }
        transform.FindDeep("Title").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Title;
        transform.FindDeep("Description").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Description;
        transform.FindDeep("Info1").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Info1;
        transform.FindDeep("Info2").GetComponentInChildren<TMPro.TMP_Text>().text = _data.Info2;
        if(_data.Info1 == "" && _data.Info2 == "")
            rect.anchorMin = rect.anchorMin.SetY(0);
        else if (_data.Info1 == "")
            rect.anchorMin = rect.anchorMin.SetY(0.15);
        else
            rect.anchorMin = rect.anchorMin.SetY(0.25);
    }

    private void SetPosition(Vector2 position)
    {
        var size = _rectTransform.sizeDelta * _rectTransform.lossyScale;
        _rectTransform.position = position.Clamp(new Vector2(size.x, size.y / 2), new Vector2(Screen.width, Screen.height - size.y / 2));
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
