using ClemCAddons;
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
        var ray = Camera.main.ViewportPointToRay(new Vector3(0,0.5f));

        Bounds bounds = gameObject.GetComponent<Collider>().bounds;

        LineLineIntersection(out Vector3 worldCameraCorner, ray.origin, ray.direction, Camera.main.transform.position.SetZ(0), Vector3.left);

        if (worldCameraCorner.x > transform.position.x + bounds.extents.x)
        {
            float diff = worldCameraCorner.x - (transform.position.x + bounds.extents.x);
            // The bounds center may not be the same as the transform position
            transform.position += Vector3.right * diff;
        }
    }
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
}
