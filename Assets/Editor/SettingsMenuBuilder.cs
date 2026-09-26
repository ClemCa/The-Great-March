using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds Assets/Prefabs/UI/SettingsMenu.prefab. Run from the Tools menu.
/// </summary>
public static class SettingsMenuBuilder
{
    private const string PrefabPath = "Assets/Prefabs/UI/SettingsMenu.prefab";

    [MenuItem("Tools/Build Settings Menu Prefab")]
    public static void Build()
    {
        var sprites = LoadSprites();
        _sprites = sprites;
        var font = TMP_Settings.defaultFontAsset;

        var root = NewUI("SettingsMenu", null);
        var rootRt = root.GetComponent<RectTransform>();
        Stretch(rootRt);

        var backdrop = NewUI("Backdrop", root.transform);
        Stretch(backdrop.GetComponent<RectTransform>());
        var backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = new Color(0f, 0f, 0f, 0.65f);

        var menu = root.AddComponent<SettingsMenu>();

        var panel = NewUI("Panel", root.transform);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(1000f, 760f);
        var panelImage = panel.AddComponent<Image>();
        panelImage.sprite = sprites.background;
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(0.10f, 0.10f, 0.13f, 0.98f);

        var title = NewText(panel.transform, "Title", "Settings", 40, TextAlignmentOptions.Center, font);
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(600f, 56f));

        var close = MakeButton(panel.transform, "Close", "X", sprites, font, 44f, 44f);
        Anchor(close.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(44f, 44f));

        var tabs = NewUI("Tabs", panel.transform);
        var tabsRt = tabs.GetComponent<RectTransform>();
        Anchor(tabsRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -120f), new Vector2(920f, 52f));
        var tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 10f;
        tabLayout.childAlignment = TextAnchor.MiddleLeft;
        tabLayout.childControlWidth = false;
        tabLayout.childControlHeight = true;
        tabLayout.childForceExpandWidth = false;
        tabLayout.childForceExpandHeight = false;

        string[] sections = { "Audio", "Display", "Gameplay", "Dialogue" };
        for (int i = 0; i < sections.Length; i++)
        {
            var tab = MakeButton(tabs.transform, "Tab_" + sections[i], sections[i], sprites, font, 180f, 48f);
            var le = tab.AddComponent<LayoutElement>();
            le.preferredWidth = 180f;
            le.minWidth = 180f;
        }

        var content = NewUI("Content", panel.transform);
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 0f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.offsetMin = new Vector2(40f, 40f);
        contentRt.offsetMax = new Vector2(-40f, -170f);

        var audio = MakePanel(content.transform, "Panel_Audio");
        MakeSliderRow(audio.transform, "Master", 0f, 1f, false);
        MakeSliderRow(audio.transform, "UI", 0f, 1f, false);
        MakeSliderRow(audio.transform, "SFX", 0f, 1f, false);
        MakeSliderRow(audio.transform, "Music", 0f, 1f, false);

        var display = MakePanel(content.transform, "Panel_Display");
        MakeToggleRow(display.transform, "Fullscreen", sprites);
        MakeToggleRow(display.transform, "VSync", sprites);
        MakeCycleRow(display.transform, "Resolution", sprites, font);
        MakeCycleRow(display.transform, "Quality", sprites, font);
        MakeCycleRow(display.transform, "Antialiasing", sprites, font);
        MakeCycleRow(display.transform, "AntialiasingQuality", sprites, font);

        var gameplay = MakePanel(content.transform, "Panel_Gameplay");
        MakeToggleRow(gameplay.transform, "ShowPrompt", sprites);
        MakeToggleRow(gameplay.transform, "BackgroundSound", sprites);

        var dialogue = MakePanel(content.transform, "Panel_Dialogue");
        MakeCycleRow(dialogue.transform, "Mode", sprites, font);
        MakeCycleRow(dialogue.transform, "Provider", sprites, font);
        MakeNote(dialogue.transform, "WebGL_Note", SettingsMenu.WebGlOllamaNote, font);
        MakeInputRow(dialogue.transform, "BaseUrl", sprites, font, false);
        MakeInputRow(dialogue.transform, "Model", sprites, font, false);
        MakeInputRow(dialogue.transform, "ApiKey", sprites, font, true);
        MakeSliderRow(dialogue.transform, "Temperature", 0f, 2f, false);
        MakeSliderRow(dialogue.transform, "Verbatim", 0f, 50f, true);
        MakeSliderRow(dialogue.transform, "Summarized", 0f, 200f, true);
        MakeToggleRow(dialogue.transform, "Thoughts", sprites);

        // inactive by default
        root.SetActive(true);

        System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("Built " + PrefabPath);
    }

    #region controls

    private static GameObject MakePanel(Transform parent, string name)
    {
        var panel = NewUI(name, parent);
        Stretch(panel.GetComponent<RectTransform>());
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return panel;
    }

    private static GameObject MakeRow(Transform parent, string label, float labelWidth, TMP_FontAsset font, float height)
    {
        var row = NewUI("Row_" + label, parent);
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = height;
        rowLe.preferredHeight = height;

        var labelGo = NewText(row.transform, label + "_Label", label, 24, TextAlignmentOptions.Left, font);
        var labelLe = labelGo.gameObject.AddComponent<LayoutElement>();
        labelLe.preferredWidth = labelWidth;
        labelLe.minWidth = labelWidth;
        return row;
    }

    private static void MakeSliderRow(Transform parent, string label, float min, float max, bool whole)
    {
        var font = TMP_Settings.defaultFontAsset;
        var row = MakeRow(parent, label, 240f, font, 46f);
        var sliderGo = MakeSlider(row.transform, label + "_Control", whole);
        var slider = sliderGo.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = whole;
        slider.value = min;

        var value = NewText(row.transform, label + "_Value", "", 24, TextAlignmentOptions.Right, font);
        var valueLe = value.gameObject.AddComponent<LayoutElement>();
        valueLe.preferredWidth = 140f;
        valueLe.minWidth = 140f;
    }

    private static void MakeToggleRow(Transform parent, string label, Sprites sprites)
    {
        var font = TMP_Settings.defaultFontAsset;
        var row = MakeRow(parent, label, 240f, font, 46f);
        var toggleGo = MakeToggle(row.transform, label + "_Control", sprites);
        var le = toggleGo.AddComponent<LayoutElement>();
        le.preferredWidth = 44f;
        le.minWidth = 44f;
    }

    private static void MakeCycleRow(Transform parent, string label, Sprites sprites, TMP_FontAsset font)
    {
        var row = MakeRow(parent, label, 240f, font, 46f);
        MakeButton(row.transform, label + "_Control", "", sprites, font, 300f, 40f);
        var value = NewText(row.transform, label + "_Value", "", 24, TextAlignmentOptions.Left, font);
        var valueLe = value.gameObject.AddComponent<LayoutElement>();
        valueLe.preferredWidth = 260f;
        valueLe.minWidth = 260f;
    }

    private static void MakeNote(Transform parent, string name, string text, TMP_FontAsset font)
    {
        var go = NewUI(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.color = new Color(1f, 0.76f, 0.4f, 1f);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 84f;
        le.minHeight = 40f;

        go.SetActive(false);
    }

    private static void MakeInputRow(Transform parent, string label, Sprites sprites, TMP_FontAsset font, bool password)
    {
        var row = MakeRow(parent, label, 240f, font, 46f);
        var inputGo = MakeInput(row.transform, label + "_Control", sprites, font, password);
        var le = inputGo.AddComponent<LayoutElement>();
        le.preferredWidth = 560f;
        le.minWidth = 300f;
    }

    private static GameObject MakeButton(Transform parent, string name, string label, Sprites sprites, TMP_FontAsset font, float width, float height)
    {
        var go = NewUI(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        var image = go.AddComponent<Image>();
        image.sprite = sprites.standard;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.22f, 0.22f, 0.26f, 1f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.9f);
        button.colors = colors;

        var text = NewText(go.transform, "Text (TMP)", label, 24, TextAlignmentOptions.Center, font);
        Stretch(text.rectTransform, 8f, 8f, 4f, 4f);
        return go;
    }

    private static GameObject MakeToggle(Transform parent, string name, Sprites sprites)
    {
        var go = NewUI(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40f, 40f);

        var background = NewUI("Background", go.transform);
        var bgRt = background.GetComponent<RectTransform>();
        Anchor(bgRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f));
        var bgImage = background.AddComponent<Image>();
        bgImage.sprite = sprites.background;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = new Color(0.18f, 0.18f, 0.22f, 1f);

        var checkmark = NewUI("Checkmark", background.transform);
        Stretch(checkmark.GetComponent<RectTransform>(), 4f, 4f, 4f, 4f);
        var checkImage = checkmark.AddComponent<Image>();
        checkImage.sprite = sprites.checkmark;
        checkImage.color = new Color(0.45f, 0.75f, 1f, 1f);

        var toggle = go.AddComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = true;
        return go;
    }

    private static GameObject MakeSlider(Transform parent, string name, bool whole)
    {
        var sprites = _sprites;
        var go = NewUI(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 26f);

        var background = NewUI("Background", go.transform);
        var bgRt = background.GetComponent<RectTransform>();
        Anchor(bgRt, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var bgImage = background.AddComponent<Image>();
        bgImage.sprite = sprites.background;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = new Color(0.16f, 0.16f, 0.2f, 1f);

        var fillArea = NewUI("Fill Area", go.transform);
        var fillAreaRt = fillArea.GetComponent<RectTransform>();
        Anchor(fillAreaRt, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(5f, 0f), new Vector2(-15f, 0f));
        var fill = NewUI("Fill", fillArea.transform);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.sizeDelta = new Vector2(10f, 0f);
        var fillImage = fill.AddComponent<Image>();
        fillImage.sprite = sprites.standard;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.35f, 0.6f, 0.9f, 1f);

        var handleArea = NewUI("Handle Slide Area", go.transform);
        var handleAreaRt = handleArea.GetComponent<RectTransform>();
        Anchor(handleAreaRt, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        handleAreaRt.offsetMin = new Vector2(10f, 0f);
        handleAreaRt.offsetMax = new Vector2(-10f, 0f);
        var handle = NewUI("Handle", handleArea.transform);
        var handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, 0f);
        var handleImage = handle.AddComponent<Image>();
        handleImage.sprite = sprites.knob;
        handleImage.color = new Color(0.85f, 0.85f, 0.9f, 1f);

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.wholeNumbers = whole;
        return go;
    }

    private static GameObject MakeInput(Transform parent, string name, Sprites sprites, TMP_FontAsset font, bool password)
    {
        var go = NewUI(name, parent);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 40f);
        var image = go.AddComponent<Image>();
        image.sprite = sprites.inputField;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.16f, 0.16f, 0.2f, 1f);

        var textArea = NewUI("Text Area", go.transform);
        var textAreaRt = textArea.GetComponent<RectTransform>();
        Stretch(textAreaRt, 10f, 10f, 6f, 6f);
        textArea.AddComponent<RectMask2D>();

        var placeholder = NewText(textArea.transform, "Placeholder", "Enter text...", 22, TextAlignmentOptions.Left, font);
        Stretch(placeholder.rectTransform);
        placeholder.color = new Color(1f, 1f, 1f, 0.35f);
        placeholder.fontStyle = FontStyles.Italic;

        var text = NewText(textArea.transform, "Text", "", 22, TextAlignmentOptions.Left, font);
        Stretch(text.rectTransform);
        text.color = Color.white;

        var input = go.AddComponent<TMP_InputField>();
        input.textViewport = textAreaRt;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        input.characterLimit = 260;
        return go;
    }

    #endregion

    #region primitives

    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null)
            go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text NewText(Transform parent, string name, string text, float size, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = NewUI(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        return tmp;
    }

    private static void Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    private struct Sprites
    {
        public Sprite standard;
        public Sprite background;
        public Sprite inputField;
        public Sprite knob;
        public Sprite checkmark;
        public Sprite dropdown;
        public Sprite mask;
    }

    private static Sprites _sprites;

    private static Sprites LoadSprites()
    {
        return new Sprites
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }

    #endregion
}
