using UnityEngine;
using UnityEngine.UI;
using Luminosity.IO;
using System;
using System.Collections.Generic;

namespace ClemCAddons
{
    namespace Utilities
    {
        public class UISlider : MonoBehaviour
        {
            [SerializeField] private Slider _slider;
            private Image[] _tiles = new Image[] { };
            int _current = 0;
            private double[] _sizes;
            private List<KeyValuePair<int, GameObject>> _tilesReferences = new List<KeyValuePair<int, GameObject>>();
            [SerializeField] private bool _enabledInputs = true;
            private Image _image;
            public delegate void sliderSubmit(int x);
            private sliderSubmit _callback;
            public sliderSubmit Callback
            {
                get
                {
                    return _callback;
                } set
                {
                    _callback = value;
                }
            }
            public bool EnabledInputs { get => _enabledInputs; set => _enabledInputs = value; }
            public Slider Slider { get => _slider; }

            void Start()
            {
                _image = GetComponent<Image>();
            }
            public delegate void UpdateReturn(int usedItemID);
            private bool first = false;
            void Update()
            {
                CheckForChildren();
                if (!first)
                {
                    first = true;
                    _slider.ExternalInitialize();
                }
                _slider.Update();
                _sizes = _slider.GetTileSizes();
                _current = _slider.GetCurrentItem();
                for (int i = 0; i < _tiles.Length; i++)
                {
                    RectTransform r = _tiles[i].GetComponent<RectTransform>();
                    r.sizeDelta = new Vector2((float)_sizes[i], (float)_sizes[i]);
                    _tiles[i].color = new Color(_tiles[i].color.r, _tiles[i].color.g, _tiles[i].color.b, _current == i ? 1 : 0.5f);
                }
                if (_enabledInputs)
                {
                    if (InputManager.GetButton("UI_Right"))
                    {
                        _slider.Slide(5 * Time.smoothDeltaTime);
                    }
                    if (InputManager.GetButton("UI_Left"))
                    {
                        _slider.Slide(-5 * Time.smoothDeltaTime);
                    }
                    if (InputManager.GetButton("UI_Submit"))
                    {
                        if (Callback != null)
                        {
                            _callback.Invoke(_slider.GetCurrentItem());
                        }
                    }
                    _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 100 / 255f);
                } else
                {
                    _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 50 / 255f);
                }
            }
            private void CheckForChildren()
            {
                Transform content = transform.Find("Viewport").Find("Content");
                if (content.ActiveChildsWithComponent(typeof(Image)) != _tiles.Length)
                {
                    _tiles = content.GetComponentsInChildren<Image>();
                    _slider.TileCount = _tiles.Length;
                    RectTransform rec = content.GetComponent<RectTransform>();
                    rec.sizeDelta = rec.sizeDelta.SetX(_slider.IdealSize);
                    Rebuild();
                }
            }
            private void Rebuild()
            {
                for(int i = 0; i < _tiles.Length; i++)
                {
                    double pos = (i - _tiles.Length / 2) * _slider.TileSize + ((_tiles.Length+1) % 2 * _slider.TileSize/2);
                    _tiles[i].rectTransform.anchoredPosition = new Vector2((float)pos,0);
                }
            }

            public void AddTile(int id, GameObject type, Sprite sprite = null, byte? itemType = null)
            {
                if(type.GetComponent<Image>() == null)
                {
                    Debug.LogError("Tiles should have an Image component");
                }
                if(type.GetComponent<RectTransform>() == null)
                {
                    Debug.LogError("Tiles should have a RectTransform component");
                }
                if (type.GetComponent<InventoryItem>() == null)
                {
                    Debug.LogError("Tiles should have an InventoryItem component");
                }
                Transform content = transform.Find("Viewport").Find("Content");
                GameObject r = Instantiate(type, content);
                _tilesReferences.Add(new KeyValuePair<int, GameObject>(id, r));
                try
                {
                    if(itemType != null)
                    {
                        r.GetComponent<InventoryItem>().SetItemType(itemType);
                    }
                    if(sprite != null)
                    {
                        r.GetComponent<Image>().sprite = sprite;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }

            public void RemoveTile(int id)
            {
                Destroy(_tilesReferences.Find(t => t.Key == id).Value);
                _tilesReferences.RemoveAll(t => t.Key == id);
            }
        }
    }
}

