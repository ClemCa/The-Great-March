using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Player;
using ClemCAddons.Utilities;
using System.Linq;
using UnityEngine.Serialization;
using System;

namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public class TPSCameraWithNodeSupport : MonoBehaviour
        {
            [Header("Inputs")]
            [SerializeField] private string _horizontalMovement = "LookHorizontal";
            [SerializeField] private string _verticalMovement = "LookVertical";
            [SerializeField] private int _inputSensitivity = 200;
            [Header("Settings")]
            [SerializeField] private bool _scrollToZoom = false;
            [SerializeField] private float _scrollSensitivity = 1;
            [SerializeField] private float _distance = 2;
            [SerializeField] private float _heightOffset = 2;
            [SerializeField] private float _linearSmoothing = 0.2f;
            [SerializeField] private float _nodePercSmoothing = 0.2f;
            [SerializeField] private float _nodeFixedTolerance = 0.5f;
            [SerializeField] private float _playerMovementSpring = 0.02f;
            [SerializeField] private bool _customPlayerClass = false;
            [SerializeField, DrawIf("_customPlayerClass", true, ComparisonType.Equals)] private string _playerClassName;
            [SerializeField] private bool _useDifferentTransform = false;
            [SerializeField, DrawIf("_useDifferentTransform", true, ComparisonType.Equals)] private Transform _differentTransform;
            [SerializeField] private bool _inverseCamButton = false;
            [SerializeField, DrawIf("_inverseCamButton", true, ComparisonType.Equals)] private string _inverseCamButtonName = "InverseCam";
            [SerializeField] private bool _blockCamButton = false;
            [SerializeField, DrawIf("_blockCamButton", true, ComparisonType.Equals)] private string _blockCamButtonName = "BlockCam";
            [Header("Camera boom")]
            [SerializeField] private string _cameraHitTag = "Hittable";
            [SerializeField] private float _cameraBoomSmoothing = 0.05f;
            [SerializeField] private float _obstacleMinimumDistance = 0.1f;
            [Header("Angle Limiting")]
            [SerializeField] private float _topBottomLimit = 0.1f;

            private dynamic _player = null;
            public float _defaultDistance;
            private Vector3 _position = Vector3.forward;
            private Vector2 _currentOffset = Vector2.zero;
            private float _currentCheatOffset = 0f;
            private BoxCollider _boxCollider;
            private NodeHelpSettings _settings;
            private Vector3 _offSetTurned = Vector3.forward;
            private Vector3 _previousChange = Vector3.zero;
            private Vector3 _previousPosition = Vector3.zero;
            private Vector3 _cheatOffsetTurned = Vector3.forward;
            private float _cameraBoomD = 0f;
            private float _zoom = 0;
            private bool _camInversed;

            private bool _blocked;
            private bool isPlayerCharacterMovement
            {
                get
                {
                    return _player.GetType() == typeof(CharacterMovement);
                }
            }

            private Transform playerTransform
            {
                get
                {
                    return _useDifferentTransform ? _differentTransform : (Transform)_player.transform;
                }
            }

            public BoxCollider BoxCollider
            {
                get
                {
                    try
                    {
                        _boxCollider = FindObjectOfType<NodeHelperSettings>().GetComponentInChildren<BoxCollider>();
                    }
                    catch
                    {
                        Debug.LogWarning("Si une node rectangulaire est utilisée, le prefab NodeHelperSettings doit être dans la scène");
                    }
                    return _boxCollider;
                }
                set
                {
                    _boxCollider = value;
                }
            }

            public bool BlockCam { get => _blocked; set => _blocked = value; }
            public float Distance { get => _distance; set => _distance = value; }

            void Start()
            {
                _defaultDistance = _distance;
                if (_customPlayerClass)
                    _player = FindObjectOfType(Type.GetType(_playerClassName));
                else
                    _player = FindObjectOfType<CharacterMovement>();
                var r = FindObjectOfType<NodeHelperSettings>();
                _settings = r != null ? r.Settings : new NodeHelpSettings(false, false);
            }

            void Update()
            {
                try
                {
                    if (_blockCamButton && InputManager.GetButton(_blockCamButtonName))
                    {
                        _blocked = !_blocked;
                    }
                }
                catch
                {
                    Debug.LogWarning("Inputs are not updated, missing blocking cam debug button");
                }
                
                CamTriangulation(playerTransform);
                GetInputs();
                var change = (_position * (_distance + _zoom + Camera.allCameras[0].nearClipPlane / 2)) + Vector3.up * _heightOffset + _offSetTurned + _cheatOffsetTurned;
                if (_linearSmoothing != 0)
                { // lerp around the player, but moves with the player as point of reference
                    var position = Vector3.Lerp(_previousPosition, playerTransform.position, Time.smoothDeltaTime / _playerMovementSpring);
                    var objective = position + change;
                    bool t = (playerTransform).position.CastToLineOnly(objective, (LayerMask)(isPlayerCharacterMovement ? _player.CollisionLayer : 1 << LayerMask.NameToLayer("Default")), _cameraHitTag, out RaycastHit hit);
                    if (t)
                    {
                        _cameraBoomD = Mathf.Lerp(_cameraBoomD, hit.distance - Camera.allCameras[0].nearClipPlane/2, Time.smoothDeltaTime / _cameraBoomSmoothing);
                    }
                    else
                        _cameraBoomD = Mathf.Lerp(_cameraBoomD, (_distance + _zoom - Camera.allCameras[0].nearClipPlane/2), Time.smoothDeltaTime / _cameraBoomSmoothing);
                    transform.position = playerTransform.position + _position * (_cameraBoomD) + _offSetTurned + _cheatOffsetTurned + Vector3.up * _heightOffset;
                    _previousChange = change;
                    _previousPosition = position;
                }
                else
                {
                    var objective = playerTransform.position + change;
                    bool t = playerTransform.position.CastToLineOnly(objective, (LayerMask)(isPlayerCharacterMovement ? _player.CollisionLayer : 1 << LayerMask.NameToLayer("Default")), _cameraHitTag, out RaycastHit hit);
                    if (t)
                    {
                        transform.position = playerTransform.position.Direction(objective) * (hit.distance - Camera.allCameras[0].nearClipPlane/2);
                    }
                    else
                        transform.position = objective;
                }
                transform.LookAt(playerTransform.position + _offSetTurned + Vector3.up * _heightOffset);
            }

            private void GetInputs()
            {
                if (_scrollToZoom && !_blocked && InputManager.mouseScrollDelta.y != 0)
                    _zoom = (_zoom - InputManager.mouseScrollDelta.y * _scrollSensitivity * 10 * Time.smoothDeltaTime).Clamp(-_distance + 0.1f, _distance * 2);

                float x = _blocked ? 0 : InputManager.GetAxis(_horizontalMovement);
                float y = _blocked ? 0 : InputManager.GetAxis(_verticalMovement) * (_camInversed ? 1 : -1);



                if (y != 0)
                {
                    var t = Vector3.Slerp(_position, _position.magnitude * Vector3.up * Mathf.Sign(y), y.Abs() * 0.1f * _inputSensitivity * Time.deltaTime);
                    if (t.Distance(Vector3.up * Math.Sign(y)) > _topBottomLimit)
                        _position = t;
                }
                if (x != 0)
                    _position = _position.Rotate(Vector3.up, x * _inputSensitivity * Time.smoothDeltaTime * 5);

                var posNorm = _position.normalized;
                if (_currentOffset == Vector2.zero)
                    _offSetTurned = Vector2.zero;
                else
                    _offSetTurned = posNorm.Left() * _currentOffset.x + posNorm.Up() * _currentOffset.y;
                _cheatOffsetTurned = Vector3.up * _currentCheatOffset;

                if (_inverseCamButton && InputManager.GetButtonDown(_inverseCamButtonName))
                    _camInversed = !_camInversed;
            }

            private void CamTriangulation(Transform player)
            {
                TPSNode[] nodes = (TPSNode[])FindObjectsOfType(typeof(TPSNode));
                if (nodes.Length < 1)
                {
                    var res = new TPSNodeContent
                    {
                        offset = Vector2.zero,
                        fakeMiddle = 0f,
                        distance = _defaultDistance
                    };
                    MoveToNode(res);
                    return;
                }
                Vector3[] positions = new Vector3[nodes.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = nodes[i].transform.position;
                }
                var temp = WeedOutBadNodes(nodes, positions, player.position);
                TPSNode[] GoodNodes = temp.Key;
                positions = temp.Value;
                if (GoodNodes.Length < 1) // no node in range
                {
                    var res = new TPSNodeContent
                    {
                        offset = Vector2.zero,
                        fakeMiddle = 0f,
                        distance = _defaultDistance
                    };
                    MoveToNode(res);
                    return;
                }
                else if (GoodNodes.Length < 2) // create a fake node at the edge of the range to use for smoothing out of the node's area of effect when only a single node is effective
                {
                    TPSNodeContent res = EdgeOut(GoodNodes[0], positions[0]);
                    MoveToNode(res);
                    return;
                }
                var r = (NodesDistances(positions, player.position));
                r = NormalizeToMax(r);
                r = GetSumTo1(r);
                TPSNodeContent[] val = new TPSNodeContent[GoodNodes.Length];
                for (int i = 0; i < GoodNodes.Length; i++)
                {
                    val[i] = EdgeOut(GoodNodes[i], positions[i]);
                }
                var result = AverageNodes(val, r, player.position, positions);
                MoveToNode(result);
            }

            private void MoveToNode(TPSNodeContent node)
            {
                _currentOffset = node.offset;
                _currentCheatOffset = node.fakeMiddle;
                _distance = node.distance;
            }

            private TPSNodeContent AverageNodes(TPSNodeContent[] TPSNodes, float[] distancesTo1, Vector3 playerPos, Vector3[] TPSNodesPosition)
            {
                TPSNodeContent result = new TPSNodeContent();
                result.Default();
                Vector2[] allTPSNodesOffset = new Vector2[TPSNodes.Length];
                float[] allTPSNodesCheatOffset = new float[TPSNodes.Length];
                for (int i = 0; i < TPSNodes.Length; i++) // calculate all positions
                {
                    allTPSNodesOffset[i] = TPSNodes[i].offset;
                    allTPSNodesCheatOffset[i] = TPSNodes[i].fakeMiddle;
                }
                var total = distancesTo1[0];       // had a weird bug I couldn't wrap my head around with basic multiplication of weight & addition that inversed
                var currentOffset = allTPSNodesOffset[0];   // the weights I couldn't wrap my head around, so I decided to do the same than the quaternions in the end
                var currentCheatOffset = allTPSNodesCheatOffset[0];
                for (int i = 0; i < allTPSNodesOffset.Length - 1; i++)
                {
                    var weight = CalculateLerpWeight(total, distancesTo1[i + 1]);
                    total += weight;
                    currentOffset = currentOffset == allTPSNodesOffset[i + 1] ? allTPSNodesOffset[i + 1] : Vector2.Lerp(currentOffset, allTPSNodesOffset[i + 1], weight);
                }
                total = distancesTo1[0];
                for (int i = 0; i < allTPSNodesCheatOffset.Length - 1; i++)
                {
                    var weight = CalculateLerpWeight(total, distancesTo1[i + 1]);
                    total += weight;
                    currentCheatOffset = currentCheatOffset == allTPSNodesCheatOffset[i + 1] ? allTPSNodesCheatOffset[i + 1] : Mathf.Lerp(currentCheatOffset, allTPSNodesCheatOffset[i + 1], weight);
                }
                result.offset = currentOffset;
                result.fakeMiddle = _currentCheatOffset;
                return result;
            }

            private float CalculateLerpWeight(float a, float b)
            {
                var result = Mathf.Min(a, b) / Mathf.Max(a, b) / 2;
                result = a == Mathf.Min(a, b) ? result : 1 - result;
                return result;
            }

            private KeyValuePair<TPSNode[], Vector3[]> WeedOutBadNodes(TPSNode[] nodes, Vector3[] positions, Vector3 playerpos)
            {
                TPSNode[] result = nodes;
                float[] distances = new float[nodes.Length];
                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    if (!nodes[i].enabled)
                    {
                        distances = distances.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                        result = result.RemoveAt(i);
                    }
                }
                for (int i = result.Length - 1; i >= 0; i--) // weed out by range
                {
                    distances[i] = Vector3.Distance(positions[i], playerpos);
                    if (((distances[i] >= result[i].Content.range) && !(result[i].IsRectangular)) || (result[i].IsRectangular && !GameTools.IsInRectangle(result[i], playerpos)))
                    {
#if (UNITY_EDITOR)
                        result[i].Visible = false;
#endif
                        distances = distances.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                        result = result.RemoveAt(i);
                    }
                }
                List<int> toDelete = new List<int> { };
                for (int i = result.Length - 1; i >= 0; i--)
                {
                    if (toDelete.IndexOf(i) != -1)
                    {
                        result = result.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                    }
                }
                return new KeyValuePair<TPSNode[], Vector3[]>(result, positions);
            }

            private TPSNodeContent EdgeOut(TPSNode node, Vector3 pos)
            {
                TPSNodeContent result = Extensions.Copy(node.Content);
                float distPerc;
                if (!node.IsRectangular)
                {
                    if (node.Content.range < 0)
                    {
                        Debug.LogWarning("Le range ne doit pas �tre < 0 sur une node circulaire");
                    }
                    if (node.UseSafeZone)
                    {
                        if (Vector3.Distance(playerTransform.position, pos) <= node.SafeZoneSize)
                        {
                            distPerc = 1;
                        }
                        else
                        {
                            distPerc = (1 / (node.SafeZoneSize / node.Content.range)) - (Vector3.Distance(playerTransform.position, pos) / node.Content.range / (node.SafeZoneSize / node.Content.range));
                            distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                            if (node.SafeZoneSize < 0)
                            {
                                Debug.LogError("Si elle est activ�e, la safe-zone ne doit pas �tre < 0");
                            }
                        }
                    }
                    else
                    {
                        distPerc = 1 - (Vector3.Distance(playerTransform.position, pos) / (node.Content.range));
                        distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                    }
                }
                else
                {
                    var t1 = pos;
                    var t2 = pos;
                    if (GameTools.IsInRectangle(t2, node.SafeDimensions, playerTransform.position))
                    {
                        distPerc = 1;
                    }
                    else
                    {
                        var pos1 = GameTools.ClosestOnCube(playerTransform.position, t1, node.Dimensions, BoxCollider, !Application.isPlaying && _settings.ShowRectangularDebug);
                        var pos2 = GameTools.ClosestOnCube(playerTransform.position, t2, node.SafeDimensions, BoxCollider, Application.isPlaying && _settings.ShowRectangularDebug);
                        if (!Application.isPlaying)
                        {
                            if (_settings.ShowRectangularDebug)
                            {
                                EditorTools.DrawLineInEditor(pos1, playerTransform.position, Color.red);
                                EditorTools.DrawLineInEditor(pos2, playerTransform.position, Color.red);
                            }
                        }
                        var dist1 = Vector3.Distance(pos1, playerTransform.position);
                        var dist2 = Vector3.Distance(pos2, playerTransform.position);
                        distPerc = dist1 / (dist2 + dist1);
                    }
                }
                result.offset = node.Content.offset;
                result.fakeMiddle = node.Content.fakeMiddle;
                var res = new TPSNodeContent
                {
                    offset = Vector2.Lerp(_currentOffset, result.offset, distPerc),
                    fakeMiddle = Mathf.Lerp(_currentCheatOffset, result.fakeMiddle, distPerc),
                    distance = Mathf.Lerp(_defaultDistance, result.distance, distPerc)
                };
                return res;
            }

            private float[] GetSumTo1(float[] normalizedDistances)
            {
                float[] result = new float[normalizedDistances.Length];
                var r = normalizedDistances.Sum();
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = normalizedDistances[i] / r;
                }
                return result;
            }
            private float[] NormalizeToMax(float[] distances)
            {
                float[] result = new float[distances.Length];
                var r = distances.Max();
                for (int i = 0; i < distances.Length; i++)
                {
                    result[i] = distances[i] / r;
                }
                return result;
            }
            private float[] NodesDistances(Vector3[] nodes, Vector3 playerPos)
            {
                float[] result = new float[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    result[i] = Vector3.Distance(nodes[i], playerPos);
                }
                return result;
            }
        }
    }
}