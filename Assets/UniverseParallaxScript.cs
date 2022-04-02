using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniverseParallaxScript : MonoBehaviour
{
    [SerializeField] private int population;

    [SerializeField] private GameObject closeStar;
    [SerializeField] private float closeStarRange;

    [SerializeField] private GameObject mediumStar;
    [SerializeField] private float mediumStarRange;

    [SerializeField] private GameObject farAwayStar;
    [SerializeField] private float farAwayStarRange;

    private List<GameObject> stars = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < population; i++)
        {
            GameObject star;
            float range;

            switch (i % 3)
            {
                case 0:
                    star = closeStar;
                    range = closeStarRange;
                    break;
                case 1:
                    star = mediumStar;
                    range = mediumStarRange;
                    break;
                default:  // case 2
                    star = farAwayStar;
                    range = farAwayStarRange;
                    break;
            }

            Vector3 randomized = Camera.main.ScreenToWorldPoint(new Vector3(
                Random.Range(0.0f, Camera.main.pixelWidth),
                Random.Range(0.0f, Camera.main.pixelHeight),
                range - Camera.main.transform.position.z
            ));

            stars.Add(Instantiate(star, randomized, Quaternion.identity));
        }

        // By keeping them sorted like this, we can optimize Update() below
        stars.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldCameraCorner = Camera.main.ViewportToWorldPoint(Vector3.zero);

        for (int i = 0; i < stars.Count; i++)
        {
            GameObject item = stars[i];
            Bounds bounds = item.GetComponent<MeshFilter>().sharedMesh.bounds;

            if (worldCameraCorner.x > bounds.max.x)
            {
                // We want to move it from right outside of the screen to the left, to
                // right outside of the screen to the right
                Vector3 randomized = Camera.main.ScreenToWorldPoint(new Vector3(
                    Camera.main.pixelWidth + bounds.extents.x,
                    Camera.main.pixelHeight + bounds.extents.y,
                    item.transform.position.z - Camera.main.transform.position.z
                ));
                item.transform.position = randomized;

                // Keep the list sorted after moving an item
                stars.RemoveAt(i);
                i--;
                stars.Add(item);
            } else {
                // Because stars is sorted by its X coordinate, once we find a star
                // that is inside the camera we know that all following stars are as
                // well
                break;
            }
        }
    }
}
