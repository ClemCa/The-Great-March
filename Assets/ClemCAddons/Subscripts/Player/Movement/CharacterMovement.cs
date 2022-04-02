using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Utilities;
using System.Diagnostics;
using ClemCAddons.CameraAndNodes;
using Debug = UnityEngine.Debug;
using UnityEngine.Serialization;

namespace ClemCAddons
{
    public enum ELevelType // PLACEHOLDER
    {
        NONE
    }
    namespace Player
    {
        public class CharacterMovement : MonoBehaviour
        {

            #region Fields

            [Header("Collision")]
            [Tooltip("The layer(s) to be considered when looking for ground")]
            [SerializeField] private LayerMask _collisionLayer = 8;

            [Header("Camera")]
            [Tooltip("Is using the TPS camera script, or the other node-based camera scirpt?")]
            [SerializeField] private bool _isUsingTPS = true;

            [Header("Gliding")]
            [Tooltip("What gravity is the player under when gliding?")]
            [SerializeField] private float _glidingGravity = 0.5f;
            [Tooltip("What are the limits to the player's fall speed?")]
            [SerializeField] private float _maxGlidingFallSpeed = 0.5f;
            [Tooltip("If over max fall speed, how fast does the player get to its max speed")]
            [SerializeField, FormerlySerializedAs("_glidingDeceleration")] private float _glidingYDeceleration = 2;
            [Tooltip("The speed the player rotates at while gliding")]
            [SerializeField] [LabelOverride("Gliding Rotation Speed")] private float _airControl = 2f;
            [Tooltip("The max speed the player can attain while gliding")]
            [SerializeField] [LabelOverride("Air Control Max Speed")] private float _airMaxMagnitude = 2f;
            [Tooltip("The acceleration of the player while gliding")]
            [SerializeField] [LabelOverride("Air Control Acceleration")] private float _airMagnitudeAcceleration = 0.3f;
            [Tooltip("The deceleration of the player while gliding")]
            [SerializeField] [LabelOverride("Air Control Deceleration")] private float _airMagnitudeDeceleration = 0.3f;


            [Header("Walking")]
            [Tooltip("The max speed the player can walk at")]
            [SerializeField] private float _maxSpeed = 10;

            [Tooltip("By how much should the player turn to match the ground's shape?")]
            [SerializeField] private float _maxGroundAdaptation = 10;
            [Tooltip("How fast does the player do this?")]
            [SerializeField] private float _groundAdaptationSpeed = 10;

            [Header("Jumping & Gravity")]
            [Tooltip("How fast and high is the player's jump?")]
            [SerializeField] private float _jumpStrength = 5;

            [Tooltip("How fast is the player allowed to fall?")]
            [SerializeField] private float _maxFallSpeed = 10;

            [Tooltip("Any higher fall height doesn't gives a bonus to a bounce")]
            [SerializeField] private float _maxBounceHeight = 10;
            [Tooltip("The height-to-bounce ratio, affects how much height boosts your bounces")]
            [SerializeField] private float _bounceFactor = 0.1f;
            [Tooltip("Multiplied to the player inputs, affects the feeling of the player's walking, serving as both acceleration and deceleration")]
            [SerializeField] private float _inputMultiplier = 10f;

            [Header("Jump Buffering")]
            [Tooltip("How long after the player left the ground can the character still jump?")]
            [SerializeField, LabelOverride("Coyote time (s)")] private float _postJumpBuffering = 0.2f;
            [Tooltip("How long before the player reaches the ground can the character prepare to jump?")]
            [SerializeField, LabelOverride("Jump Buffering (s)")] private float _preJumpBuffering = 0.2f;

            [Header("Reactive Bouncing")]
            [Tooltip("How high should the player need to fall from to bounce on any surface")]
            [SerializeField] private float _fallBounceThreshold = 4;
            [Tooltip("When falling, won't be able to bounce more than that")]
            [SerializeField] private int _fallMaxBounce = 10;
            [Tooltip("How does fall height translate into bouncing")]
            [SerializeField] private float _fallBounceRatio = 0.5f;
            [Tooltip("How close to vertical does a surface need to be to be considered vertical?")]
            [SerializeField] private float _wallBouncePrecision = 0.1f;
            [Tooltip("How straight must the player hit the wall? (0 to 1)")]
            [SerializeField] private float _wallBounceThreshold = 0.7f;
            [Tooltip("How strongly should the player bounce?")]
            [SerializeField] private float _bounceStrength = 2;
            [Tooltip("Should an object bounce on a GameObject containing those components, it will be ignored")]
            [SerializeField] private UnityEngine.Object[] _bounceScriptExceptions;
            [Tooltip("Defines how long the movements are disabled after bouncing on a wall (in ms)")]
            [SerializeField] private int _wallDisableDuration = 500;

            private Vector3 _impulse = new Vector3();
            private Vector3 _immediateImpulse = new Vector3(); // clear after applied
            private float _permanentImpulse; // does not clear
            private Vector3 _direction = new Vector3();
            private Vector3 _localVelocity;
            private float groundDistance;
            private float _fallingHeight = 0f;
            private bool _doNotMove = false;
            private bool _ignoreInputs = false;

            private Collider _collider;
            private Rigidbody _rigidbody;
            private bool _firstTimeGround;
            private bool _isOnGround;
            private bool _isFalling = true;
            private NodeBasedCamera _camera;
            private TPSCameraWithNodeSupport _tpsCamera;
            private float _jumpDelay = 0;
            private float _postJumpDelay = 0;
            private Vector3 _lastUnityPosition = new Vector3(); // bug in rigidbody causes velocity to not reset on strong slopes
            private bool _preJump;

            private Vector3 _previousDirection; // For use with gliding calculations
            private Vector3 _savedDirection;    // ^
            private float _magnitudeVelocity;   // ^

            private Vector3 _movementDirection;

            private ELevelType _currentLevelType = ELevelType.NONE;
            private bool _disableAll = false;

            #endregion Fields



            #region Properties
            public LayerMask CollisionLayer { get => _collisionLayer; set => _collisionLayer = value; }
            public Rigidbody Rigidbody
            {
                get
                {
                    if (_rigidbody == null)
                    {
                        _rigidbody = _rigidbody = GetComponent<Rigidbody>();
                    }
                    return _rigidbody;
                }
                set
                {
                    _rigidbody = value;
                }
            }
            public bool IsOnGround { get => _isOnGround; set => _isOnGround = value; }
            public bool IsFalling { get => _isFalling; set => _isFalling = value; }
            public float GroundDistance { get => groundDistance; set => groundDistance = value; }

            public bool IsWalking
            {
                get
                {
                    return (_direction.x.Abs() > 0 || _direction.z.Abs() > 0) && IsOnGround;
                }
            }

            public ELevelType CurrentLevelType
            {
                get
                {
                    return _currentLevelType;
                }
                set
                {
                    _currentLevelType = value;
                }
            }
            #endregion Properties




            void Start()
            {
                _collider = GetComponent<Collider>();
                _rigidbody = GetComponent<Rigidbody>();
                if (_isUsingTPS)
                    _tpsCamera = FindObjectOfType<TPSCameraWithNodeSupport>();
                else
                    _camera = FindObjectOfType<NodeBasedCamera>();

            }

            void LateUpdate()
            {
                if (_disableAll)
                    return;
                UpdateInputs();
                UpdateMovementDirection();
                _localVelocity = new Vector3();
                var slope = GameTools.FindSlope(transform.position, _movementDirection * _collider.bounds.extents.x, _collider.bounds.extents.y, 1, _collisionLayer);
                slope = (slope.Abs() <= _collider.bounds.extents.y).ToInt() * slope; // slope isn't considered if it's steeper than the player's half height,
                                                                                     // such as a wall
                groundDistance = GameTools.FindGround(transform.position, _collider.bounds.extents.y, 10, _collisionLayer, out RaycastHit hit);
                groundDistance = (groundDistance - slope.Abs()).Max(0); // removes slope
                _firstTimeGround = !_isOnGround && groundDistance == 0; // if is on ground is false, has a chance to be true if it becomes true.
                if (groundDistance == 0 && _isOnGround && _postJumpDelay <= 0)
                {
                    _postJumpDelay = _postJumpBuffering;
                }
                _isOnGround = groundDistance == 0 || _postJumpDelay > 0;
                if (groundDistance >= 0.05 && _postJumpDelay > 0)
                {
                    _postJumpDelay -= Time.deltaTime;
                }
                if (_firstTimeGround && _preJump)
                {
                    _preJump = false;
                    if (_jumpDelay <= 0 && _doNotMove == false)
                    {
                        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // set rigidbody velocity y to 0
                        _impulse += new Vector3(0, _jumpStrength);
                        _jumpDelay += 0.05f;
                    }
                }
                _isFalling = _rigidbody.velocity.y < 0 && Vector3.Magnitude(transform.position - _lastUnityPosition) > 0.01f;
                _lastUnityPosition = transform.position;
                // bug in rigidbody causes velocity to not reset on strong slopes
                _fallingHeight = !_isFalling ? 0 : _fallingHeight;
                if (!_isOnGround && _direction.y > 0 && groundDistance <= _rigidbody.velocity.y * -1 * _preJumpBuffering && _isFalling)
                {
                    _preJump = true;
                }
                if (!_isOnGround && _isFalling && _direction.y > 0)
                {
                    bool firstTime = _rigidbody.useGravity;
                    _rigidbody.useGravity = false;
                    if (Mathf.Abs(_rigidbody.velocity.y) > _maxGlidingFallSpeed)
                    {
                        _localVelocity.y += (_glidingYDeceleration - _glidingGravity) * Time.smoothDeltaTime;
                    }
                    else
                    {
                        _impulse.y -= _glidingGravity * Time.smoothDeltaTime;
                    }
                    _fallingHeight = 0;
                    _direction.y = 0; // consume direction.y, everything after that can be considered plane math, as Y is negligible
                    _previousDirection.y = 0;
                    bool headingTo0 = _magnitudeVelocity > _direction.magnitude && _magnitudeVelocity > 0;
                    if (_magnitudeVelocity != 0 || _direction.magnitude != 0) // air control => velocity
                    {
                        if (firstTime)
                            _magnitudeVelocity = (_direction.magnitude * _airMaxMagnitude).Min(_airMaxMagnitude); // directly set to target
                        else
                            _magnitudeVelocity =
                               ((_magnitudeVelocity +
                                   ((_direction.magnitude * _airMaxMagnitude).Min(_airMaxMagnitude) - _magnitudeVelocity).Sign()
                                   // scale speed in direction, serves as both scaling and max
                                   * Time.deltaTime
                                   * ((!headingTo0).ToInt() * _airMagnitudeAcceleration + headingTo0.ToInt() * _airMagnitudeDeceleration)
                               // one of either acceleration or deceleration is multiplied by 1, the other 0.
                               // It is the equivalent of a condition, without the additionnal overhead
                               ));
                        if (!headingTo0)
                            _magnitudeVelocity = _magnitudeVelocity.Min(_airMaxMagnitude);
                        else
                            _magnitudeVelocity = _magnitudeVelocity.Max(0);
                        // originally put a clamp to 0 in direction, but doesn't have any use as this value is never supposed to be negative
                    }

                    if (Vector3.Dot(_direction.normalized, _previousDirection.normalized) < 0.99)
                    // for we need that to change the rotation only, we don't care about magnitude
                    // small changes would result in an unstable slerp, as it would constantly overshoot the target
                    {
                        _direction = Vector3.Slerp(_previousDirection, _direction, Time.deltaTime * _airControl);
                        // air control => turning 
                    }
                    // Makes sure the direction magnitude matches the velocity
                    if (_direction.sqrMagnitude != 0)
                    {
                        _savedDirection = _direction.normalized;
                        _direction = _direction.NormalizeTo(_magnitudeVelocity);
                    }
                    else
                    {
                        // potential troulesome area around 0
                        _direction = _savedDirection.NormalizeTo(_magnitudeVelocity);
                    }
                }
                else
                {
                    _savedDirection = _direction.SetY(0);
                    _magnitudeVelocity = 0;
                    if (_ignoreInputs == false)
                        _rigidbody.useGravity = true;
                    if (!_isOnGround && _isFalling)
                    {
                        _fallingHeight -= _rigidbody.velocity.y * Time.smoothDeltaTime; // y velocity is negative as _isfalling and !_isOnGround
                    }
                }
                _previousDirection = _direction;
                if (_isOnGround)
                {
                    Vector3 r = hit.normal.ToQuaternion(transform.rotation).eulerAngles.SetY(0);
                    if (r.x.MinusAngle(0, true).Abs() > _maxGroundAdaptation)
                    {
                        r.x = transform.Find("Base").eulerAngles.x;
                    }
                    if (r.z.MinusAngle(0, true).Abs() > _maxGroundAdaptation)
                    {
                        r.z = transform.Find("Base").eulerAngles.z;
                    }
                    transform.Find("Base").rotation = Quaternion.Lerp(transform.Find("Base").rotation, r.ToQuaternion(), Time.smoothDeltaTime * _groundAdaptationSpeed);
                    if (_direction.y > 0 && _jumpDelay <= 0 && _doNotMove == false)
                    {
                        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // set rigidbody velocity y to 0
                        _impulse += new Vector3(0, _jumpStrength);
                        _jumpDelay += 0.05f;
                    }
                }
                _jumpDelay = Mathf.Max(0, _jumpDelay - Time.smoothDeltaTime);
                _localVelocity.x = _direction.x * _inputMultiplier;
                _localVelocity.z = _direction.z * _inputMultiplier;
                _localVelocity = _localVelocity.ClampXZTotal(_inputMultiplier);
                Vector3 _aim = (_isUsingTPS ? _tpsCamera.transform : _camera.transform).forward;
                _aim.y = 0;
                if (_aim == new Vector3())
                {
                    _aim = transform.forward;
                }
                ApplyImpulse();
                _localVelocity = _doNotMove ? new Vector3() : _localVelocity;
                _immediateImpulse = _doNotMove ? new Vector3() : _immediateImpulse;
                if ((_localVelocity + _immediateImpulse) == Vector3.zero)
                {
                    _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                }
                else
                {
                    _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                }
                Vector3 velocityFinal = _localVelocity.Remap(_aim);
                velocityFinal.y += _rigidbody.velocity.y;
                velocityFinal = velocityFinal.ClampXZKeepRatio(-_maxSpeed, _maxSpeed);
                velocityFinal = velocityFinal.ClampXZTotal(_maxSpeed);
                velocityFinal = velocityFinal.ClampY(-_maxFallSpeed, _maxFallSpeed);
                if (_permanentImpulse == 0)
                {
                    _rigidbody.velocity = (_immediateImpulse + velocityFinal);
                }
                else
                {
                    var r = (_immediateImpulse + velocityFinal);
                    _rigidbody.velocity = r.SetY(Mathf.Lerp(r.y, _permanentImpulse, Time.deltaTime * 4));
                }
                // RELATIVE FORCE - Velocity = constant speed without overwriting velocity
                // so it doesn't fuck with other scripts using the rigidbody
                _immediateImpulse = new Vector3();
            }

            public void SetCanMove(bool canMove)
            {
                _doNotMove = !canMove;
            }
            public void SetIgnoreInput(bool ignoreInput)
            {
                _ignoreInputs = ignoreInput;
            }

            //private bool _tempStoreOfValue = false;
            //private Stopwatch stopwatch = new Stopwatch();
            private void UpdateInputs()
            {
                if (!_ignoreInputs)
                {
                    _direction.x = InputManager.GetAxis("Horizontal");
                    _direction.z = InputManager.GetAxis("Vertical");
                    _direction.y = InputManager.GetButton("Jump") ? 1 : 0;
                }
                else
                {
                    _direction = Vector3.zero;
                }
                // ###### A UTILISER SI IL Y A BESOIN DE MESURER la dur�e d'arr�t complet ######
                //if(Mathf.Abs(_direction.x) == 1)
                //{
                //    _tempStoreOfValue = true;
                //} else
                //{
                //    if (_tempStoreOfValue)
                //    {
                //        if (!stopwatch.IsRunning)
                //        {
                //            stopwatch.Start();
                //        }
                //        if(_rigidbody.velocity.magnitude == 0)
                //        {
                //            stopwatch.Stop();
                //            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
                //            UnityEngine.Debug.Log(stopwatch.ElapsedTicks);
                //            _tempStoreOfValue = false;
                //            stopwatch.Reset();
                //        }
                //    }
                //}
            }

            void OnCollisionEnter(Collision collision)
            {
                _disableAll = false;
                if (_preJump)
                    return;
                foreach(Component component in collision.gameObject.GetComponentsInChildren<Component>())
                {
                    foreach (Object exception in _bounceScriptExceptions)
                    {
                        if (component.name == exception.name)
                        {
                            return;
                        }
                    }
                }
                if (_fallingHeight > _fallBounceThreshold)
                {
                    _immediateImpulse += collision.GetContact(0).normal * (_bounceStrength * (_fallingHeight - _fallBounceThreshold) * _fallBounceRatio).Min(_fallMaxBounce);
                    _fallingHeight = 0;
                }
                else if (collision.GetContact(0).normal.y.Abs() < _wallBouncePrecision && Vector3.Dot(collision.relativeVelocity.SetY(0).normalized, collision.GetContact(0).normal) > _wallBounceThreshold)
                {
                    if (!_isOnGround)
                    {
                        _rigidbody.velocity += collision.GetContact(0).normal * _bounceStrength;
                        _fallingHeight = 0;
                        _disableAll = true;
                        _ = GameTools.DelayedCall(_wallDisableDuration, EnableAll);
                        var r = GetComponentInChildren<FollowVelocity>();
                        if (r != null)
                        {
                            r.enabled = false;
                        }
                    }
                }
            }

            public void EnableAll()
            {
                _disableAll = false;
                var r = GetComponentInChildren<FollowVelocity>();
                if (r != null)
                {
                    r.enabled = true;
                }
            }

            public void DisableAll()
            {
                _disableAll = true;
            }

            private void UpdateMovementDirection() // to follow velocity
            {
                if (_rigidbody.velocity.normalized != Vector3.zero && _rigidbody.velocity.normalized.GetMaxXZ(true) > 0.1f)
                {
                    _movementDirection = _rigidbody.velocity.normalized;
                }
            }

            private void ApplyImpulse()
            {
                _rigidbody.AddForce(_impulse, ForceMode.Impulse);
                _impulse = new Vector3();
            }

            public void Bounce(float value, bool autoHeight = false)
            {
                if (autoHeight)
                {
                    _impulse.y += Mathf.Min(_maxBounceHeight, value + _fallingHeight * _bounceFactor);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                }
                else
                {
                    _impulse.y += Mathf.Min(_maxBounceHeight, value);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                }
            }

            public void Bounce(float value, float height)
            {
                _impulse.y += Mathf.Min(_maxBounceHeight, value + height * _bounceFactor);
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            }


            public void AddImpulse(Vector3 value)
            {
                _immediateImpulse += value;
            }

            public void Push(float value)
            {
                _permanentImpulse = value;
            }
        }
    }
}