using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Utilities;
using ClemCAddons.Player;
using UnityEditor;

namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public partial class Node : MonoBehaviour
        {
            [SerializeField] private NodeContent _content = new NodeContent();
            [SerializeField] private Color _colorInEditor = new Color(1, 1, 1, 0.1f);
            [SerializeField] private bool _useSafeZone = true;
            [SerializeField, DrawIf("_useSafeZone", true, ComparisonType.Equals)] private float _safeZoneSize;
            [SerializeField, DrawIf("_useSafeZone", true, ComparisonType.Equals)] private Color _safeColor = new Color(1,1,1,0.3f);
            [SerializeField] private bool _isRectangular = false;
            [SerializeField, DrawIf("_isRectangular", true, ComparisonType.Equals)] private Vector3 _dimensions = new Vector3();
            [SerializeField, DrawIf("_isRectangular", true, ComparisonType.Equals)] private Vector3 _safeDimensions = new Vector3();
            private bool _visible = false;
            public bool Visible { get => _visible; set => _visible = value; }
            public NodeContent Content { get => _content; }
            public Vector3 Dimensions { get => _dimensions; }
            public bool IsRectangular { get => _isRectangular; set => _isRectangular = value; }
            public float SafeZoneSize { get => _safeZoneSize; set => _safeZoneSize = value; }
            public Vector3 SafeDimensions { get => _safeDimensions; set => _safeDimensions = value; }
            public bool UseSafeZone { get => _useSafeZone; set => _useSafeZone = value; }

            void Update()
            {
                if (Application.isPlaying)
                {
                    _content.Update();
                }
            }
        }

        [ExecuteInEditMode]
        public partial class Node : MonoBehaviour
        {
            private KeyValuePair<Vector3, Quaternion> _nodePrediction = new KeyValuePair<Vector3, Quaternion>();
            public KeyValuePair<Vector3, Quaternion> NodePrediction
            {
                get
                {
                    return _nodePrediction;
                }
                set
                {
                    _nodePrediction = value;
                }
            }
            void OnDrawGizmos()
            {
                _content.Update();
                var r = FindObjectOfType<NodeHelperSettings>();
                var settings = r != null ? r.Settings : new NodeHelpSettings(false,false);
                CharacterMovement player = FindObjectOfType<CharacterMovement>();
                NodeBasedCamera cam = FindObjectOfType<NodeBasedCamera>();
                if(cam == null)
                    return;
                Color color = _visible&&settings.ShowSelection? Color.green : _content.type == NodeType.Relative ? Color.red : Color.blue;
                var values = Nodes.CalculateNode(cam.DefaultCameraPosition(player.transform.position), cam.DefaultCameraQuaternion, player.transform.position, _content);
                if (settings.ShowNodes)
                {
                    EditorTools.DrawSphereInEditor(transform.position, 0.25f, color);
                }
                if (settings.AlwaysShowNodeDirection || (!_isRectangular && _content.range >= Vector3.Distance(player.transform.position, transform.position))||(_isRectangular&& GameTools.IsInRectangle(this,player.transform.position)))
                {
                    NodePrediction = Nodes.CalculateNode(cam.DefaultCameraPosition(transform.position), cam.DefaultCameraQuaternion, transform.position, _content);
                    if (settings.ShowNodeDirection)
                    {
                        EditorTools.DrawLineInEditor(transform.position, values.Key, color);
                        EditorTools.DrawSphereInEditor(values.Key, 0.05f, Color.yellow);
                    }
                    if (settings.ShowNodePredictiveDirection)
                    {
                        EditorTools.DrawLineInEditor(transform.position, NodePrediction.Key, Color.blue);
                        EditorTools.DrawSphereInEditor(NodePrediction.Key, 0.05f, Color.blue);
                    }
                    if (settings.ShowNodePaths)
                    {
                        var toLink = Nodes.FindNodes(transform.position, _content.range);
                        for(int i = 0; i < toLink.Key.Length; i++)
                        {
                            EditorTools.DrawLineInEditor(transform.position, toLink.Key[i], Color.cyan);
                            EditorTools.DrawLineInEditor(NodePrediction.Key, toLink.Value[i].NodePrediction.Key, Color.cyan);
                        }
                    }
                }
                if (settings.ShowRange)
                {
                    var colorToUse = _colorInEditor;
                    var safeColorToUse = _safeColor;
#if(UNITY_EDITOR)
                    if (Selection.Contains(gameObject))
                    {
                        colorToUse.a *= 2;
                        safeColorToUse.a *= 2;
                    }
#endif
                    if (_isRectangular)
                    {
                        var pos = transform.position;
                        pos.y += _dimensions.y;
                        EditorTools.DrawCubeInEditor(pos, _dimensions, _colorInEditor);
                        pos.y = transform.position.y + _safeDimensions.y;
                        EditorTools.DrawCubeInEditor(pos, _safeDimensions, _safeColor);
                    }
                    else
                    {
                        EditorTools.DrawSphereInEditor(transform.position, _content.range, _colorInEditor);
                    }
                }
                if (settings.ShowSafeRange && _useSafeZone && !_isRectangular)
                {
                    EditorTools.DrawSphereInEditor(transform.position, _safeZoneSize, _safeColor);
                }
            }
        }
    }
}