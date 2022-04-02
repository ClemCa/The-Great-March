using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClemCAddons
{
    namespace Utilities
    {
        public class InventoryItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
        {
            [Header("Enable sliding")]
            [SerializeField] private bool _slideUpToConfirm = false;
            [SerializeField] private bool _slideDownToConfirm = false;
            [Header("Slider Movements")]
            [SerializeField] private float _slideDistanceToMove = 0.5f;
            [SerializeField] private float _slideBreakthroughDistance = 1f;
            [SerializeField] private float _slideDistanceToConfirm = 2;
            [SerializeField] private Image _imitationImageForDrag;
            [Header("Call on use")]
            [SerializeField] private GameObject _methodContainer;
            [SerializeField] private string _typeName;
            [SerializeField] private string _methodToCall;
            [SerializeField] private Type _parameterType =  Type.Bool;
            [SerializeField] private bool[] _parametersB;
            [SerializeField] private float[] _parametersF;
            [SerializeField] private int[] _parametersI;
            [SerializeField] private string[] _parametersS;
            private object[] _parameters;
            private float _distanceSlided = 0;
            private byte? _storedValue;
            private Vector2 _originPosition;

            public byte? ItemType { get => _storedValue; }
            

            public enum Type
            {
                Bool,
                Float,
                Int,
                String
            }

            public InventoryItem(byte? itemType = null)
            {
                _storedValue = itemType;
            }

            public void SetItemType(byte? itemType = null)
            {
                _storedValue = itemType;
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                _originPosition = eventData.position;
            }

            public void OnDrag(PointerEventData eventData)
            {
                Vector3[] fourCornersArray = new Vector3[4];
                transform.parent.parent.parent.GetComponent<RectTransform>().GetWorldCorners(fourCornersArray);
                if (_slideUpToConfirm || _slideDownToConfirm)
                {
                    if(transform.position.x.IsBetween(fourCornersArray[0].x,fourCornersArray[3].x))
                    {
                        if (_slideUpToConfirm && _slideDownToConfirm)
                        {
                            _distanceSlided = Mathf.Abs(_originPosition.y - eventData.position.y);
                        }
                        else if (_slideUpToConfirm && _originPosition.y - eventData.position.y < 0)
                        {
                            _distanceSlided = Mathf.Abs(_originPosition.y - eventData.position.y);
                        }
                        else if (_slideDownToConfirm && _originPosition.y - eventData.position.y > 0)
                        {
                            _distanceSlided = Mathf.Abs(_originPosition.y - eventData.position.y);
                        }
                        else
                        {
                            _distanceSlided = 0;
                        }
                        if (_distanceSlided > _slideDistanceToMove && _distanceSlided < _slideDistanceToConfirm)
                        {
                            GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition.SetY
                                (
                                ((Mathf.Min(_slideBreakthroughDistance, _distanceSlided) //distance up to breakthrough
                                - _slideDistanceToMove) // distance to ignore
                                * Mathf.Sign(eventData.position.y - _originPosition.y) //direction
                                * 0.5) // slow down
                                +
                                (Mathf.Max(0, _distanceSlided- _slideBreakthroughDistance) //distance from breakthrough
                                * Mathf.Sign(eventData.position.y - _originPosition.y) //direction
                                * 4) // bonus speed
                                );
                            _imitationImageForDrag.enabled = true;
                            _imitationImageForDrag.sprite = GetComponent<Image>().sprite;
                            _imitationImageForDrag.rectTransform.position = transform.position;
                            _imitationImageForDrag.rectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;
                        } else if (_distanceSlided < _slideDistanceToMove)
                        {
                            _imitationImageForDrag.enabled = false;
                        }
                    } else
                    {
                        _imitationImageForDrag.enabled = false;
                        _distanceSlided = 0;
                        GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition.SetY(0);
                    }
                }
                if (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y) && _distanceSlided < _slideDistanceToMove)
                {
                    transform.parent.parent.GetComponentInParent<UISlider>().Slider.Slide(eventData.delta.x * -0.01);
                }
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (_distanceSlided >= _slideDistanceToConfirm)
                {
                    switch (_parameterType)
                    {
                        case Type.Bool:
                            _parameters = Array.ConvertAll(_parametersB,new Converter<bool,object>(BtoObject));
                            break;
                        case Type.Float:
                            _parameters = Array.ConvertAll(_parametersF, new Converter<float, object>(FtoObject));
                            break;
                        case Type.Int:
                            _parameters = Array.ConvertAll(_parametersI, new Converter<int, object>(ItoObject));
                            break;
                        case Type.String:
                            _parameters = Array.ConvertAll(_parametersS, new Converter<string, object>(StoObject));
                            break;
                    }
                    _methodContainer.GetComponent(_typeName).GetType().GetMethod(_methodToCall).Invoke(_methodContainer.GetComponent(_typeName), _parameters);
                }
                GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition.SetY(0);
                _imitationImageForDrag.enabled = false;
                _distanceSlided = 0;
            }

            private object BtoObject(bool pf)
            {
                return pf;
            }
            private object FtoObject(float pf)
            {
                return pf;
            }
            private object ItoObject(int pf)
            {
                return pf;
            }
            private object StoObject(string pf)
            {
                return pf;
            }

        }
    }
}