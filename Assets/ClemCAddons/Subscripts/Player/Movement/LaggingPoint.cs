using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ClemCAddons;
using ClemCAddons.Player;

namespace ClemCAddons
{
    namespace Player
    {
        [ExecuteInEditMode]
        public class LaggingPoint : MonoBehaviour
        {
            [Header("Lag")]
            [SerializeField] private float _distance = 2;
            [SerializeField] private float _speed = 2f;
            [Header("Wave")]
            [SerializeField] private float _delay = 2f;
            [SerializeField, LabelOverride("Speed")] private float _waveSpeed = 0.5f;
            [SerializeField] private float _amplitude = 10f;
            [SerializeField] private float _period = 10f;
            private CharacterMovement _player;
            private Vector3 _location;
            private float _timer = 0;

            public CharacterMovement Player
            {
                get
                {
                    if (_player == null)
                    {
                        _player = GetComponentInParent<CharacterMovement>();
                    }
                    return _player;
                }
                set
                {
                    _player = value;
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                Player = GetComponentInParent<CharacterMovement>();
                _location = Player.transform.position;
            }

            // Update is called once per frame
            void Update()
            {
                if (_timer > _delay)
                {
                    _location = Vector3.MoveTowards(_location, Player.transform.position, Time.smoothDeltaTime * _waveSpeed);
                    float t = Vector3.Distance(_player.transform.position, _location);
                    transform.position = Vector3.Lerp(transform.position,
                        new Vector3(_location.x, _location.y + (Mathf.Sin((t / (_period / 10))) * (_amplitude / 10)), _location.z),
                        Time.smoothDeltaTime * _speed);
                }
                else if (Vector3.Distance(_location, Player.transform.position) > _distance)
                {
                    _location = Vector3.Lerp(_location, Player.transform.position, Time.smoothDeltaTime * _speed);
                    transform.position = _location;
                }
                else if (Player.Rigidbody.velocity.magnitude < 0.001f)
                {
                    _timer += Time.deltaTime;
                    _location = Vector3.Lerp(transform.position, Player.transform.position, Time.smoothDeltaTime / 10);
                    transform.position = _location;
                }
                else
                {
                    _location = Vector3.Lerp(_location, Player.transform.position, Time.smoothDeltaTime);
                    transform.position = _location;
                }
                if (Player.Rigidbody.velocity.magnitude > 0.001f)
                {
                    _timer = 0;
                }
            }
        }
    }
}