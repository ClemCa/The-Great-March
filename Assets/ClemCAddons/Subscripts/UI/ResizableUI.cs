using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using Luminosity.IO;

public class ResizableUI : MonoBehaviour
{
    [Header("Awareness")]
    [SerializeField] private bool _useContentSize;
    [Header("Reduction")]
    [SerializeField] private bool _canReduce = true;
    [SerializeField, LabelOverride("Max Reduction %", 0, 100)] private float _maxReduction = 75;
    [Header("Others")]
    [SerializeField] private bool _canLeaveScreen = false;
    [SerializeField, LabelOverride("Detection Margin %", 0, 100)] private float _detectionMargin = 5f;
    [SerializeField, LabelOverride("Limit Margin %", 0, 100)] private float _limitMargin = 0f;
    private Vector2 _renderingSize;
    private float _scaleFactor;
    private RectTransform _rect;
    private Canvas _rootCanvas;
    private Vector2 _baseSize;
    private Vector2 _basePosition;
    private Directions _directions;
    private float _xPerc = 1;
    private float _yPerc = 1;
    private Vector2 anchorMin;
    private Vector2 anchorMax;

    public Vector2 DetectionMargin { get => Vector2.one * (_detectionMargin / 100); }
    public Vector2 LimitMargin { get => Vector2.one * (_limitMargin / 100); }
    public float MaxReduction { get => 1-_maxReduction/100; }
    public Vector2 BaseSize { get => _baseSize; }
    public Vector2 HalfSize { get => _baseSize / 2; }


    void Start()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect == null)
            Debug.LogError("Must be placed on an UI component");
        _rootCanvas = _rect.FindParentWithComponent(typeof(Canvas)).GetComponent<Canvas>().rootCanvas;
        _renderingSize = _rootCanvas.renderingDisplaySize;
        _scaleFactor = _rootCanvas.scaleFactor;
        _baseSize = new Vector2(1410, 301); //  _rect.sizeDelta * _scaleFactor;
        _basePosition = new Vector2(0, 150); // _rect.anchoredPosition
        anchorMin = _rect.anchorMin;
        anchorMax = _rect.anchorMax;
    }
    void OnGUI()
    {
        Update();
    }
    void Update()
    {
        var t = true.OnceIfTrueGate(194);
        if (t)
            Start();
        if (_rect == null)
            return;
        if (_useContentSize)
            SetSizeToChildren(); // set _baseSize to target
        //END DEBUG
        if (_canReduce)
        {
            UpdateOffsets();                        // distance       screen size       half size of the item, offsets the position checked
            _xPerc = GetPointPercentage(_directions.left, _renderingSize.x, _baseSize.x * _scaleFactor).Max(-DetectionMargin.x*2)
                .Min(GetPointPercentage(_directions.right, _renderingSize.x, _baseSize.x * _scaleFactor).Max(-DetectionMargin.x*2));
            _yPerc = GetPointPercentage(_directions.bottom, _renderingSize.y, _baseSize.y * _scaleFactor).Max(-DetectionMargin.y * 2)
                .Min(GetPointPercentage(_directions.top, _renderingSize.y, _baseSize.y * _scaleFactor).Max(-DetectionMargin.y * 2));
            if (_xPerc < DetectionMargin.x || _yPerc < DetectionMargin.y)
            {
                _xPerc = _xPerc / DetectionMargin.x; // scale to [-?,1]
                _yPerc = _yPerc / DetectionMargin.y; // scale to [-?,1] 
                _xPerc = _xPerc + (1 - _xPerc) / 2f;  // add difference back, making it scale twice as fast (/2 to scale just as fast)
                _yPerc = _yPerc + (1 - _yPerc) / 2f;  // add difference back, making it scale twice as fast (/2 to scale just as fast)
                _xPerc = (_xPerc.Max(0) * (1 - MaxReduction) + MaxReduction); // apply max reduction
                _yPerc = (_yPerc.Max(0) * (1 - MaxReduction) + MaxReduction); // apply max reduction
                _rect.sizeDelta = Vector2.Lerp(_rect.sizeDelta, new Vector2(_baseSize.x * _xPerc.Min(_yPerc), _baseSize.y * _xPerc.Min(_yPerc)), Time.deltaTime * 4);
            }
            else
                _rect.sizeDelta = Vector2.Lerp(_rect.sizeDelta, _baseSize, Time.deltaTime*4);
        }
        else
        {
            _rect.sizeDelta = BaseSize;
        }
        if (_canLeaveScreen || (_xPerc > MaxReduction && _yPerc > MaxReduction))
        {
            _rect.anchoredPosition = _basePosition;
            return;
        }
        // stop it from leaving screen
        var left = GetPointDistance(_directions.left, _renderingSize.x);
        var right = GetPointDistance(_directions.right, _renderingSize.x);
        var bottom = GetPointDistance(_directions.bottom, _renderingSize.y);
        var top = GetPointDistance(_directions.top, _renderingSize.y);
        var distanceX = left < right ? -(left.Min(0)) : right.Min(0);
        var distanceY = bottom < top ? -(bottom.Min(0)) : top.Min(0);
        var border = _renderingSize * MaxReduction;
        _rect.anchoredPosition = _basePosition + new Vector2(distanceX*2 - (border.x * Mathf.Sign(distanceX)), distanceY*2 - (border.y * Mathf.Sign(distanceY))) * _scaleFactor;
    }


    private void UpdateOffsets()
    {
        var anchorMin = _rect.anchorMin;
        var anchorMax = _rect.anchorMax;
        var position = transform.parent.position - _basePosition.ToVector3() + Vector3.zero.SetY(_renderingSize.y);
        _directions.left = position.x - (_baseSize.x * _scaleFactor * anchorMin.x);
        _directions.right = position.x + (_baseSize.x * _scaleFactor * anchorMax.x);
        _directions.bottom = position.y - (_baseSize.y * _scaleFactor * anchorMin.y);
        _directions.top = position.y + (_baseSize.y * _scaleFactor * anchorMax.y);
    }
    private float GetPointDistance(float point, float frame)
    {
        if (point > frame / 2)
            point = frame - point;
        return point;
    }
    private float GetPointPercentage(float point, float frame, float size)
    {   
        // if point < 50%, point / frame size, if point > 50%, needs to get the difference with the frame size first;
        if (point > frame / 2)
            point = frame - point;
        return point / (frame - size) * 2;
    }
    private Vector2 GetChildrenSize(Transform transformR = null)
    {
        if (transformR == null)
            transformR = transform;
        RectTransform children = transformR.GetComponentInChildren<RectTransform>();
        float size_x = 0, size_y = 0;
        foreach (RectTransform child in children)
        {
            if (child.gameObject.activeInHierarchy == true)
            {
                Vector2 scale = child.sizeDelta;
                Vector2 childSize = GetChildrenSize(child);
                size_x = childSize.x.Max(false, true, size_x, scale.x);
                size_y = childSize.y.Max(false, true, size_y, scale.y);
            }
        }
        return new Vector2(size_x, size_y);
    }
    private void SetSizeToChildren()
    {
        _baseSize = GetChildrenSize();
    }

    private struct Directions
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
    }
}
