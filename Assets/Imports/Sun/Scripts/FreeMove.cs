using UnityEngine;
using System;


public class FreeMove:MonoBehaviour
{
    public float lookSpeed = 15.0f;
    public float moveSpeed = 15.0f;
    
    public float rotationX = 0.0f;
    public float rotationY = 0.0f;
    
    public void Update()
    {
        rotationX += InputHelper.MouseAxis.x*lookSpeed;
        rotationY += InputHelper.MouseAxis.y*lookSpeed;
        rotationY = (float)Mathf.Clamp ((int)rotationY, -90, 90);
        
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        
        transform.position += transform.forward*moveSpeed*InputHelper.Vertical;
        transform.position += transform.right*moveSpeed*InputHelper.Horizontal;
        }
}