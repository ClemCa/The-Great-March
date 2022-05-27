using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UniverseParallaxScript : MonoBehaviour
{
    [SerializeField] private int population;

    [SerializeField, FormerlySerializedAs("closeStar")] private GameObject[] closeStars;
    [SerializeField] private float closeStarRange;

    [SerializeField, FormerlySerializedAs("mediumStar")] private GameObject[] mediumStars;
    [SerializeField] private float mediumStarRange;

    [SerializeField, FormerlySerializedAs("farAwayStar")] private GameObject[] farAwayStars;
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
                    star = closeStars[Random.Range(0, closeStars.Length)];
                    range = closeStarRange;
                    break;
                case 1:
                    star = mediumStars[Random.Range(0, mediumStars.Length)];
                    range = mediumStarRange;
                    break;
                default:  // case 2
                    star = farAwayStars[Random.Range(0, farAwayStars.Length)];
                    range = farAwayStarRange;
                    break;
            }

            Vector3 randomized = Camera.main.ScreenToWorldPoint(new Vector3(
                Random.Range(0.0f, Camera.main.pixelWidth),
                Random.Range(0.0f, Camera.main.pixelHeight),
                range - Camera.main.transform.position.z
            ));

            stars.Add(Instantiate(star, randomized, Quaternion.identity, transform));
        }

        // By keeping them sorted like this, we can optimize Update() below
        stars.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ViewportPointToRay(Vector3.zero);
        for (int i = 0; i < stars.Count; i++)
        {
            GameObject item = stars[i];
            Vector3 worldCameraCorner = ray.origin + ray.direction * (ray.origin.z - item.transform.position.z).Abs();

            Bounds bounds = item.GetComponent<Collider>().bounds;

            if (worldCameraCorner.x > item.transform.position.x + bounds.extents.x)
            {
                // We want to move it from right outside of the screen to the left, to
                // right outside of the screen to the right
                Vector3 randomized = Camera.main.ScreenToWorldPoint(new Vector3(
                    Camera.main.pixelWidth + Camera.main.WorldToScreenPoint(item.transform.position).x.Abs(),
                    0,
                    item.transform.position.z - Camera.main.transform.position.z
                ));

                item.transform.position = randomized.SetY(item.transform.position.y);

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
