using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindVisualizerOptions : MonoBehaviour
{
    [SerializeField] private string[] options;
    [SerializeField] private int maxOptions = 8;
    [SerializeField] private int page;
    [SerializeField] private List<TMPro.TMP_Text> texts;
    [SerializeField] private TMPro.TMP_Text backText;
    [SerializeField] private GameObject spawn;

    public Action<string> Navigation;


    public string[] Options
    {
        get
        {
            return options;
        }
        set
        {
            options = value;
            Refresh();
        }
    }

    public string BackText
    {
        set
        {
            backText.text = value;
        }
    }

    public void CallOption(TMPro.TMP_Text text)
    {
        CallOption(text.text);
    }

    public void CallOption(string option)
    {
        if (option == "Next")
        {
            page++;
            Refresh();
        }
        else if (option == "Previous")
        {
            page--;
            Refresh();
        }
        else
        {
            Navigation.Invoke(option);
        }
    }

    private void Refresh()
    {
        if (options.Length <= maxOptions)
            page = 0;
        else
        {
            var left = options.Length - (maxOptions - 1);
            var pageDelta = maxOptions - 2;
            var minima = 1f / pageDelta * 1.5f; // this is to use to check for the last next,
                                                // multiplied by 1.5 because floats aren't precise
                                                // and having a number for r in-between is impossible
            var r = left / (float)pageDelta; // number of pages to display the rest
            if (r - Mathf.Floor(r) <= minima)
                r = Mathf.Floor(r);
            else
                r = Mathf.Ceil(r);
            page = (int)Mathf.Min(r, page);
        }
        int start = page == 0 ? 0 : maxOptions - 2 + (page - 1) * (maxOptions - 2);
        int end = start + maxOptions;
        int used = 0;
        for(int i = start; i < end; i++)
        {
            if (i == start && page != 0)
                SetText(i - start, "Previous");
            else if (i == end - 1 && i < options.Length - 1)
                SetText(i - start, "Next");
            else if (i < options.Length)
                SetText(i - start, options[i]);
            else
                continue;
            used++;
        }
        if(used < texts.Count)
        {
            for(int i = texts.Count - 1; i >= used; i--)
            {
                Destroy(texts[i].transform.parent.gameObject);
                texts.RemoveAt(i);
            }
        }
    }

    private void SetText(int id, string text)
    {
        if(texts.Count <= id)
        {
            var txt = Instantiate(spawn, transform).GetComponentInChildren<TMPro.TMP_Text>();
            texts.Add(txt);
        }
        texts[id].text = text;
    }
}
