using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CloudMoveScript : MonoBehaviour
{
    [SerializeField] private float timeToCenter = 1.0f;

    private float speed;

    private static CloudMoveScript instance;

    public static CloudMoveScript Instance { get => instance; }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        var r = Camera.main.ViewportToWorldPoint(Vector3.zero);
        speed = (r.x - transform.position.x) / timeToCenter;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * speed * Time.smoothDeltaTime;

        // Ensure that the cloud can always be seen by the main camera
        var ray = Camera.main.ViewportPointToRay(new Vector3(0,0.5f));

        Bounds bounds = gameObject.GetComponent<Collider>().bounds;

        var cameraCorner = ray.direction * 20 / ray.direction.z; // resize to 20 of length in Z direction;
        cameraCorner = ray.origin + cameraCorner;

        if (cameraCorner.x > transform.position.x + bounds.extents.x)
        {
            float diff = cameraCorner.x - (transform.position.x + bounds.extents.x);
            // The bounds center may not be the same as the transform position
            transform.position += Vector3.right * diff;
        }

        if (Planet.LeaderPlanet != null && GetComponent<BoxCollider>().bounds.Contains(Planet.LeaderPlanet.transform.position))
        {
            Planet.Unselect();  
            Time.timeScale = 0;
            _ = SceneManager.LoadSceneAsync("LoseMenu");
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
