using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMoveScript : MonoBehaviour
{
    [SerializeField] private float timeToCenter = 1.0f;

    private float speed;

    // Start is called before the first frame update
    void Start()
    {
        var r = Camera.main.ViewportToWorldPoint(Vector3.zero);
        speed = (r.x - transform.position.x) / timeToCenter;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;

        // Ensure that the cloud can always be seen by the main camera
        Vector3 worldCameraCorner = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Bounds bounds = gameObject.GetComponent<MeshFilter>().sharedMesh.bounds;

        if (worldCameraCorner.x > bounds.max.x) {
            float diff = worldCameraCorner.x - bounds.max.x;

            // The bounds center may not be the same as the transform position
            transform.position += Vector3.right * diff;
        }
    }
}
