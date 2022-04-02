using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowVelocity : MonoBehaviour
{
    [SerializeField] private float _speed = 10;
    [SerializeField] private bool _x = true;
    [SerializeField] private bool _y = true;
    [SerializeField] private bool _z = true;
    private Rigidbody _rigidbody;
    Quaternion r;
    void Start()
    {
        _rigidbody = transform.GetComponentInParent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if (_rigidbody.velocity.normalized != Vector3.zero && _rigidbody.velocity.normalized.GetMaxXZ(true) > 0)
        {
            r = Quaternion.LookRotation(_rigidbody.velocity.normalized, transform.up);
            r = Quaternion.Euler(_x ? r.eulerAngles.x : transform.localEulerAngles.x, _y ? r.eulerAngles.y : transform.localEulerAngles.y, _z ? r.eulerAngles.z : transform.localEulerAngles.z);
            if (((!_x) || (transform.localEulerAngles.x - r.eulerAngles.x).Abs() > 1)
                && ((!_y) || (transform.localEulerAngles.y - r.eulerAngles.y).Abs() > 1)
                && ((!_z) || (transform.localEulerAngles.z - r.eulerAngles.z).Abs() > 1))
                transform.localRotation = Quaternion.Lerp(transform.localRotation, r, Time.smoothDeltaTime * _speed);
        }
    }
}
