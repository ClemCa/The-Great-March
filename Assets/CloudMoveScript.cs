using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMoveScript : MonoBehaviour
{
    public float movementSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * movementSpeed * Time.deltaTime;
    }
}
