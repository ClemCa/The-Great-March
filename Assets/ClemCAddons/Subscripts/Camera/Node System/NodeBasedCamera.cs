using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ClemCAddons;
using ClemCAddons.Utilities;
using ClemCAddons.Player;

namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public partial class NodeBasedCamera : MonoBehaviour
            {
                [SerializeField] private Vector3 _defaultCameraOffset = new Vector3(0, 0, 0);
                [SerializeField] private Vector3 _defaultCameraAngle = new Vector3(0, 0, 0);
                [SerializeField] private float _nodeDifferentAngletolerance = -0.5f;
                [SerializeField] private float _nodeSameDirectionTolerance = 0.8f;
                [SerializeField] private float _nodeFixedTolerance = 0.5f;
                [SerializeField] private float _nodePercSmoothing = 0.2f;
                [SerializeField] private float _cameraLagValue = 0.2f;
                [SerializeField] private float _rotationalLagValue = 0.2f;
                [SerializeField] private string _cameraHitTag = "Hittable";
                [SerializeField] private float _minCameraBoom = 2f;
                private Transform _player;
                private CharacterMovement _character;
                private BoxCollider _boxCollider = null;
                public Quaternion DefaultCameraQuaternion
                {
                    get
                    {
                        return Quaternion.Euler(_defaultCameraAngle);
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

                void Start()
                {
                    _player = FindObjectOfType<LaggingPoint>().transform;
                    _character = FindObjectOfType<CharacterMovement>();
                }

                // Update is called once per frame
                void Update()
                {
                    if (Application.isPlaying)
                    {
                        CamTriangulation(_player);
                    }
                }

                private void CamTriangulation(Transform player)
                {
                    Node[] nodes = (Node[])FindObjectsOfType(typeof(Node));
                    if (nodes.Length < 1)
                    {
                        var res = new NodeContent
                        {
                            position = DefaultCameraPosition(player.position),
                            rotation = new SerializableQuaternion(DefaultCameraQuaternion.eulerAngles),
                            type = NodeType.Relative
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
                    Node[] GoodNodes = temp.Key;
                    positions = temp.Value;
                    if (GoodNodes.Length < 1) // no node in range
                    {
                        var res = new NodeContent
                        {
                            position = DefaultCameraPosition(player.position),
                            rotation = new SerializableQuaternion(DefaultCameraQuaternion.eulerAngles),
                            type = NodeType.Relative
                        };
                        MoveToNode(res);
                        return;
                    }
                    else if (GoodNodes.Length < 2) // create a fake node at the edge of the range to use for smoothing out of the node's area of effect when only a single node is effective
                    {
                        NodeContent res = EdgeOut(GoodNodes[0], positions[0]);
                        MoveToNode(res);
                        return;
                    }
                    var r = (NodesDistances(positions, player.position));
                    r = NormalizeToMax(r);
                    r = GetSumTo1(r);
                    NodeContent[] val = new NodeContent[GoodNodes.Length];
                    for (int i = 0; i < GoodNodes.Length; i++)
                    {
                        val[i] = EdgeOut(GoodNodes[i], positions[i]);
                    }
                    var result = AverageNodes(val, r, player.position, positions);
                    MoveToNode(result);
                }
            private void MoveToNode(NodeContent node)
            {
                bool r = _player.position.CastTo(node.position, _character.CollisionLayer, _cameraHitTag, 1f, out RaycastHit hit, _settings.ShowCameraBoom);
                if (hit.distance < _minCameraBoom && r)
                {
                    var res = _player.position + ((node.position - _player.position).normalized * hit.distance);
                    var t = _player.position + ((node.position - _player.position).normalized * _minCameraBoom);
                    res.y = t.y;
                    node.position = r ? res : node.position;

                }
                else if (r)
                {
                    node.position = _player.position + ((node.position - _player.position).normalized * hit.distance);
                }
                else
                {
                    float[] radiuses = new float[] { 1f, 2f, 3f };
                    r = Physics.Raycast(_player.position, node.position, out hit, Vector3.Distance(_player.position, node.position), _character.CollisionLayer);
                    if (!r)
                    {
                        hit.distance = Vector3.Distance(_player.position, node.position);
                    }
                    bool r2 = _player.position.MultilayerCast(node.position, _character.CollisionLayer, _cameraHitTag, radiuses, out RaycastHit hit2, out int ID);
                    if (r2)
                    {
                        var res = Vector3.Distance(hit2.point, _player.position + (node.position - _player.position).normalized * hit2.distance);
                        res /= (radiuses[ID]);
                        node.position = _player.position + ((node.position - _player.position).normalized * hit.distance * res);
                    }
                }
                r = _player.position.CastTo(node.position, _character.CollisionLayer, _cameraHitTag, 1f, out hit, _settings.ShowCameraBoom);
                if (r)
                {
                    node.position = _player.position + (node.position - _player.position).normalized * hit.distance;
                }
                var tmpLag = _cameraLagValue;
                if (!GameTools.IsInCamera(node.position, 0.1f))
                {
                    tmpLag = _cameraLagValue * 2;
                }
                transform.position = Vector3.Lerp(transform.position, node.position, tmpLag);
                transform.rotation = Quaternion.Lerp(transform.rotation, node.rotation.Value, _rotationalLagValue);
            }

                private NodeContent AverageNodes(NodeContent[] nodes, float[] distancesTo1, Vector3 playerPos, Vector3[] nodesPosition)
                {
                    NodeContent result = new NodeContent();
                    result.Default();
                    Vector3 defaultPos = DefaultCameraPosition(playerPos);
                    Quaternion defaultRot = DefaultCameraQuaternion;
                    Vector3[] allNodesPos = new Vector3[nodes.Length];
                    Quaternion[] allNodesRot = new Quaternion[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++) // calculate all positions
                    {
                        var rr = Nodes.CalculateNode(defaultPos, defaultRot, playerPos, nodes[i]);
                        allNodesPos[i] = rr.Key;
                        allNodesRot[i] = rr.Value;
                    }
                    KeyValuePair<NodeContent[], Vector3[]> fixedNodes = FindFixed(nodes, nodesPosition);
                    var total = distancesTo1[0];       // had a weird bug I couldn't wrap my head around with basic multiplication of weight & addition that inversed
                    var currentPos = allNodesPos[0];   // the weights I couldn't wrap my head around, so I decided to do the same than the quaternions in the end
                    for (int i = 0; i < allNodesPos.Length - 1; i++)
                    {
                        var weight = CalculateLerpWeight(total, distancesTo1[i + 1]);
                        total += weight;
                        currentPos = currentPos == allNodesPos[i + 1] ? allNodesPos[i + 1] : Vector3.Lerp(currentPos, allNodesPos[i + 1], weight);
                    }
                    var r = distancesTo1[0]; // total weight so far
                    var res = allNodesRot[0];
                    for (int i = 0; i < allNodesRot.Length - 1; i++) // average the quaternions using a trick with lerp & weights, many many lines shorter than what I could find on the internet.
                    {                                              // This is worse optimisation-wise, but lerp functions aren't too badly optimized anyway, so I rather sacrifice it for readability
                        var weight = CalculateLerpWeight(r, distancesTo1[i + 1]);
                        r += weight;
                        res = GameTools.QuaternionAvoidNull(res);
                        allNodesRot[i + 1] = GameTools.QuaternionAvoidNull(allNodesRot[i + 1]);
                        res = res == allNodesRot[i + 1] ? res : Quaternion.Lerp(res, allNodesRot[i + 1], weight);
                    }
                    result.position = ClampToFixed(fixedNodes, currentPos);
                    try
                    {
                        result.rotation.Value = GameTools.QuaternionAvoidNull(res);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("Reference of rotation lost?");
                    }
                    return result;
                }

                private Vector3 ClampToFixed(KeyValuePair<NodeContent[], Vector3[]> fixedNodes, Vector3 position)
                {
                    for (int i = 0; i < fixedNodes.Key.Length; i++)
                    {
                        float distanceNodeMax = fixedNodes.Key[i].range;
                        Vector3 nodeMax = fixedNodes.Value[i] + (fixedNodes.Key[i].rotationFixed.Angle.normalized * distanceNodeMax);
                        float distanceNodePosition = Vector3.Distance(fixedNodes.Value[i], position);
                        float distanceMaxNodePosition = Vector3.Distance(nodeMax, position);
                        if (distanceNodeMax > distanceNodePosition && distanceNodeMax > distanceMaxNodePosition) // if it forms a triangle (or a line) where the distance node to max range is the highest
                        {
                            if (distanceMaxNodePosition + distanceNodePosition <= distanceNodeMax * (1 + _nodeFixedTolerance)) // if it's on the line between the node max & the node with x% tolerance.
                            {
                                position = fixedNodes.Key[i].position; // update value to position
                            }
                        } // if it isn't a triangle like that, there's nothing to do as it is beyond the range or beyond the line's limits
                    }
                    return position;
                }

                private KeyValuePair<NodeContent[], Vector3[]> FindFixed(NodeContent[] nodesToSearch, Vector3[] positions)
                {
                    NodeContent[] r = nodesToSearch;
                    Vector3[] res = positions;
                    for (int i = r.Length - 1; i >= 0; i--)
                    {
                        if (!r[i].isFixed)
                        {
                            Extensions.RemoveAt(ref r, i);
                            Extensions.RemoveAt(ref res, i);
                        }
                    }
                    return new KeyValuePair<NodeContent[], Vector3[]>(r, res);
                }

                private float CalculateLerpWeight(float a, float b)
                {
                    var result = Mathf.Min(a, b) / Mathf.Max(a, b) / 2;
                    result = a == Mathf.Min(a, b) ? result : 1 - result;
                    return result;
                }

                public Vector3 DefaultCameraPosition(Vector3 playerPos, Vector3 optionalOffset = new Vector3())
                {
                    if (optionalOffset == new Vector3())
                    {
                        optionalOffset = _defaultCameraOffset;
                    }
                    var result = playerPos + optionalOffset;
                    return result;
                }

                private KeyValuePair<Node[], Vector3[]> WeedOutBadNodes(Node[] nodes, Vector3[] positions, Vector3 playerpos)
                {
                    Node[] result = nodes;
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
                    //for (int a = 0; a < result.Length; a++) // weed out by direction (angle from players, compare how close, compare if opposite direction)
                    //{
                    //    for (int b = 0; b < result.Length; b++)
                    //    {
                    //        if (a != b)
                    //        {
                    //            var dot = Vector3.Dot(result[a].Content.rotation.Angle, result[b].Content.rotation.Angle);
                    //            var posA = distances[a];
                    //            var posB = distances[b];
                    //            if (dot <= _nodeDifferentAngletolerance) // far enough to be considered conflicting
                    //            {
                    //                if (posA < posB)
                    //                {
                    //                    toDelete.Add(b);
                    //                }
                    //                else
                    //                {
                    //                    toDelete.Add(a);
                    //                }
                    //            }
                    //            var resA = (playerpos - positions[a]).normalized;
                    //            var resB = (playerpos - positions[b]).normalized;
                    //            dot = Vector3.Dot(resA, resB);
                    //            if (dot >= _nodeSameDirectionTolerance) // the two are from the same direction
                    //            {
                    //                if (posA < posB)
                    //                {
                    //                    toDelete.Add(b);
                    //                }
                    //                else
                    //                {
                    //                    toDelete.Add(a);
                    //                }
                    //            }
                    //            if (result[a].Content.isFixed && dot >= 0.33)
                    //            {
                    //                if (posA < posB) // only if the fixed is closer
                    //                {
                    //                    toDelete.Add(b);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    for (int i = result.Length - 1; i >= 0; i--)
                    {
                        if (toDelete.IndexOf(i) != -1)
                        {
        #if (UNITY_EDITOR)
                            result[i].Visible = false;
        #endif
                            result = result.RemoveAt(i);
                            positions = positions.RemoveAt(i);
                        }
                    }
        #if (UNITY_EDITOR)
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i].Visible = true;
                    }
        #endif
                    return new KeyValuePair<Node[], Vector3[]>(result, positions);
                }

                private NodeContent EdgeOut(Node node, Vector3 pos)
                {
                    NodeContent result = Extensions.Copy(node.Content);
                    float distPerc;
                    if (!node.IsRectangular)
                    {
                        if (node.Content.range < 0)
                        {
                            Debug.LogWarning("Le range ne doit pas être < 0 sur une node circulaire");
                        }
                        if (node.UseSafeZone)
                        {
                            if (Vector3.Distance(_player.position, pos) <= node.SafeZoneSize)
                            {
                                distPerc = 1;
                            }
                            else
                            {
                                distPerc = (1 / (node.SafeZoneSize / node.Content.range)) - (Vector3.Distance(_player.position, pos) / node.Content.range / (node.SafeZoneSize / node.Content.range));
                                distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                                if (node.SafeZoneSize < 0)
                                {
                                    Debug.LogError("Si elle est activée, la safe-zone ne doit pas être < 0");
                                }
                            }
                        }
                        else
                        {
                            distPerc = 1 - (Vector3.Distance(_player.position, pos) / (node.Content.range));
                            distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                        }
                    }
                    else
                    {
                        var t1 = pos;
                        var t2 = pos;
                        if (GameTools.IsInRectangle(t2, node.SafeDimensions, _player.position))
                        {
                            distPerc = 1;
                        }
                        else
                        {
                            var pos1 = GameTools.ClosestOnCube(_player.position, t1, node.Dimensions, BoxCollider, !Application.isPlaying && _settings.ShowRectangularDebug);
                            var pos2 = GameTools.ClosestOnCube(_player.position, t2, node.SafeDimensions, BoxCollider, Application.isPlaying && _settings.ShowRectangularDebug);
                            if (!Application.isPlaying)
                            {
                                if (_settings.ShowRectangularDebug)
                                {
                                    EditorTools.DrawLineInEditor(pos1, _player.position, Color.red);
                                    EditorTools.DrawLineInEditor(pos2, _player.position, Color.red);
                                }
                            }
                            var dist1 = Vector3.Distance(pos1, _player.position);
                            var dist2 = Vector3.Distance(pos2, _player.position);
                            distPerc = dist1 / (dist2 + dist1);
                        }
                    }

                    Vector3 defaultPos = DefaultCameraPosition(_player.position);
                    Quaternion defaultRot = DefaultCameraQuaternion;
                    if (node.Content.type == NodeType.Relative)
                    {
                        result.position = defaultPos + node.Content.position;
                        result.rotation.Value = defaultRot * node.Content.rotation.Value;
                    }
                    else if (node.Content.type == NodeType.AbsolutePosition)
                    {
                        result.position = _player.position + node.Content.position;
                        result.rotation.Value = defaultRot * node.Content.rotation.Value;
                    }
                    else if (node.Content.type == NodeType.AbsoluteRotation)
                    {
                        result.position = defaultPos + node.Content.position;
                    }
                    else if (node.Content.type == NodeType.Absolute)
                    {
                        result.position = _player.position + node.Content.position;
                    }
                    var res = new NodeContent
                    {
                        position = Vector3.Lerp(DefaultCameraPosition(_player.position), result.position, distPerc),
                        rotation = new SerializableQuaternion(GameTools.QuaternionAvoidNull(Quaternion.Lerp(DefaultCameraQuaternion, result.rotation.Value, distPerc))),
                        type = NodeType.Coordinates
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

            [ExecuteInEditMode]
            public partial class NodeBasedCamera : MonoBehaviour
            {
                Vector3 _to;
                NodeHelpSettings _settings;
                void OnDrawGizmos()
                {
                    CamTriangulation(_player);
                    var r = FindObjectOfType<NodeHelperSettings>();
                    _settings = r != null ? r.Settings : new NodeHelpSettings(false, false);
                    if (_settings.ShowCamPreview)
                    {
                        _to = transform.position + (transform.forward * Vector3.Distance(transform.position, _player.position));
                        EditorTools.DrawLineInEditor(transform.position, _to, Color.black);
                        var camera = GetComponent<Camera>();
                        float dist = Vector3.Distance(transform.position, _player.position) * 1.55f;
                        Ray ray1 = camera.ScreenPointToRay(new Vector3(0, 0));
                        Ray ray2 = camera.ScreenPointToRay(new Vector3(camera.pixelWidth - 1, 0)); ;
                        Ray ray3 = camera.ScreenPointToRay(new Vector3(camera.pixelWidth - 1, camera.pixelHeight - 1));
                        Ray ray4 = camera.ScreenPointToRay(new Vector3(0, camera.pixelHeight - 1));
                        Vector3 dir1 = ray1.GetPoint(dist);
                        Vector3 dir2 = ray2.GetPoint(dist);
                        Vector3 dir3 = ray3.GetPoint(dist);
                        Vector3 dir4 = ray4.GetPoint(dist);
                        EditorTools.DrawLineInEditor(ray1.origin, dir1, Color.black);
                        EditorTools.DrawLineInEditor(ray2.origin, dir2, Color.black);
                        EditorTools.DrawLineInEditor(ray3.origin, dir3, Color.black);
                        EditorTools.DrawLineInEditor(ray4.origin, dir4, Color.black);
                        EditorTools.DrawLineInEditor(ray1.origin, ray2.origin, Color.black);
                        EditorTools.DrawLineInEditor(ray2.origin, ray3.origin, Color.black);
                        EditorTools.DrawLineInEditor(ray3.origin, ray4.origin, Color.black);
                        EditorTools.DrawLineInEditor(ray4.origin, ray1.origin, Color.black);
                        EditorTools.DrawLineInEditor(dir1, dir2, Color.black);
                        EditorTools.DrawLineInEditor(dir2, dir3, Color.black);
                        EditorTools.DrawLineInEditor(dir3, dir4, Color.black);
                        EditorTools.DrawLineInEditor(dir4, dir1, Color.black);
                        EditorTools.DrawLineInEditor(dir1, dir3, Color.black);
                        EditorTools.DrawLineInEditor(dir2, dir4, Color.black);
                    }
                }
            }
    }
}