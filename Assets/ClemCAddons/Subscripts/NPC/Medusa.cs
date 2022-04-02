using ClemCAddons.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Utilities;
namespace ClemCAddons
{
    namespace NPCMovement
    {
        public class Medusa : MonoBehaviour
        {
            [Header("Jumps")]
            [SerializeField] private float _jumpsDistance = 0.5f;
            [SerializeField] private float _jumpVariation = 0.1f;
            [SerializeField] private float _precision = 0.1f;
            [SerializeField, LabelOverride("Max Angle Difference (Roaming only)")] private float _maxAnglePercDifference = 0.2f;
            [Header("Scare")]
            [SerializeField] private float _scareDistance = 2;
            [SerializeField] private float _scareDelay = 1;
            [SerializeField] private float _scareDuration = 1;
            [Header("Speed")]
            [SerializeField] private float _speed = 1;
            [SerializeField] private float _fleeingSpeed = 1;
            [SerializeField] private float _factorLowerSpeedCurve = 2;
            [Header("Zone")]
            [SerializeField] private Collider _assignedCollider;
            [Header("Debug")]
            [SerializeField] private bool _showDebug = false;
            [SerializeField] private Color _debugColor = Color.red;
            [SerializeField] private float _debugRadius = 0.1f;
            private MedusaStates _state = MedusaStates.Roaming;

            private bool _roamingDisabled = false;

            private CharacterMovement _player;

           
            private float _timing = 0;
            
            [SerializeField] private bool _isInside = true;

            private Vector3 _currentGoal;

            public bool IsInside { get => _isInside; set => _isInside = value; }

            public enum MedusaStates
            {
                Roaming,
                Fleeing,
                ComingBack
            }

            public void EnableRoaming()
            {
                _roamingDisabled = false;
            }
            public void DisableRoaming()
            {
                _roamingDisabled = true;
            }

            void Start()
            {
                _player = FindObjectOfType<CharacterMovement>();
                _currentGoal = transform.position;
            }

            void Update()
            {
                switch (_state)
                {
                    case MedusaStates.Roaming:
                        ActIfOutside();
                        CheckShouldFlee();
                        Roam();
                        break;
                    case MedusaStates.ComingBack:
                        CheckBackInside();
                        CheckShouldFlee();
                        BackToBase();
                        break;
                    case MedusaStates.Fleeing:
                        Flee();
                        break;
                }
            }

            
            void OnDrawGizmos()
            {
                if (_showDebug)
                {
                    EditorTools.DrawSphereInEditor(_currentGoal, _debugRadius, _debugColor);
                    EditorTools.DrawSphereInEditor(transform.position, _debugRadius, _debugColor);
                    EditorTools.DrawLineInEditor(transform.position, transform.position + transform.position.Direction(_assignedCollider.transform.position) * transform.position.Distance(_assignedCollider.transform.position), _debugColor);
                    EditorTools.DrawCubeInEditor(_assignedCollider.transform.position, _assignedCollider.bounds.extents, _debugColor.SetA(0.2f));
                }
            }

            void OnTriggerStay(Collider other)
            {
                if(other == _assignedCollider)
                {
                    _isInside = true;
                }
            }

            void OnTriggerExit(Collider other)
            {
                if (other == _assignedCollider)
                {
                    _isInside = false;
                }
            }

            private void MoveTo(Vector3 newPosition)
            {
                if(GetComponent<Rigidbody>().SweepTest(transform.position.Direction(newPosition), out var hit, transform.position.Distance(newPosition),QueryTriggerInteraction.Ignore))
                {
                    transform.position = hit.point;
                    _currentGoal = transform.position + hit.normal * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation));
                }
                else
                {
                    transform.position = newPosition;
                }
                
            }

            private void ActIfOutside()
            {
                if (!_isInside)
                {
                    _state = MedusaStates.ComingBack;
                }
            }

            private void CheckBackInside()
            {
                if (_isInside)
                {
                    _state = MedusaStates.Roaming;
                }
            }

            private void CheckShouldFlee()
            {
                if(_player.transform.Distance(transform) <= _scareDistance)
                {
                    _timing += Time.deltaTime;
                    if(_timing >= _scareDelay)
                    {
                        _state = MedusaStates.Fleeing;
                        _timing = _scareDuration;
                        NewPathFlee();
                    }
                } else
                {
                    _timing = 0;
                }
            }

            private void Roam()
            {
                if (!_roamingDisabled)
                {
                    MoveTo(Vector3.Lerp(transform.position, _currentGoal, Time.deltaTime * _speed + (transform.Distance(_currentGoal) * 0.01f * _factorLowerSpeedCurve)));
                    transform.rotation = Quaternion.Lerp(transform.rotation, (transform.position - _currentGoal).normalized.ToQuaternion(Quaternion.identity),Time.deltaTime * 5);
                    if (transform.Distance(_currentGoal) < _precision)
                    {
                        NewPath();
                    }
                }
            }

            private void NewPath()
            {
                Vector3 dir = transform.position.Direction(_currentGoal).RandomizeInBounds(_maxAnglePercDifference,transform.position, _assignedCollider.bounds);
                _currentGoal = transform.position + (dir * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation))) + (dir.Right() * (Random.Range(-1f, 1) * _jumpVariation));
            }

            private void NewPathFlee()
            {
                Vector3 dir = transform.position.Direction(_player.transform.position, true).SetY(0).normalized; // only flee horizontally
                _currentGoal = transform.position + (dir * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation))) + (dir.Right() * (Random.Range(-1f, 1) * _jumpVariation));
            }

            private void NewPathBack()
            {
                Vector3 dir = transform.position.Direction(_assignedCollider.transform.position);
                _currentGoal = transform.position + (dir * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation))) + (dir.Right() * (Random.Range(-1f, 1) * _jumpVariation));
            }

            private void BackToBase()
            {
                MoveTo(Vector3.Lerp(transform.position, _currentGoal, Time.deltaTime * _speed + (transform.Distance(_currentGoal) * 0.01f * _factorLowerSpeedCurve)));
                transform.rotation = Quaternion.Lerp(transform.rotation, (transform.position - _currentGoal).normalized.ToQuaternion(Quaternion.identity), Time.deltaTime * 5);
                if (transform.Distance(_currentGoal) < _precision)
                {
                    NewPathBack();
                }
            }

            private void Flee()
            {
                _roamingDisabled = false;
                if (_player.transform.Distance(transform) > _scareDistance)
                {
                    _timing -= Time.deltaTime;
                    if(_timing <= 0)
                        _state = MedusaStates.ComingBack;
                }
                MoveTo(Vector3.Lerp(transform.position, _currentGoal, Time.deltaTime * _fleeingSpeed + (transform.Distance(_currentGoal) * 0.01f * _factorLowerSpeedCurve)));
                transform.rotation = Quaternion.Lerp(transform.rotation, (transform.position - _currentGoal).normalized.ToQuaternion(Quaternion.identity), Time.deltaTime * 5);
                if (transform.Distance(_currentGoal) < _precision)
                {
                    NewPathFlee();
                }
            }
        }
    }
}

