using System.Collections.Generic;
using UnityEngine;

// Drives the procedural Starfield shader. The shader exposes every tuning knob;
// this component feeds it an arbitrary number of star colour types (each with a
// mixture weight and a variance) packed into a small palette texture, so there is
// no hard limit on how many colour types are authored.
[ExecuteAlways]
[AddComponentMenu("The Great March/Starfield Controller")]
public class StarfieldController : MonoBehaviour
{
    public const int MaxColors = 16;

    [System.Serializable]
    public struct StarColorType
    {
        public Color color;

        [Min(0f)] public float weight;

        [Range(0f, 1f)] public float variance;
    }

    // Weights approximate the naked-eye / bright-star mix, i.e. weighted by
    // apparent brightness rather than raw count: luminous B/A stars dominate what
    // a human sees, with K giants filling out the red side. Counting raw stars
    // would instead be ~76% dim M dwarfs, which are mostly invisible to the eye.
    // List is ordered O..M.
    [SerializeField]
    private List<StarColorType> colors = new List<StarColorType>
    {
        new StarColorType { color = new Color(0.60f, 0.69f, 1.00f), weight = 0.30f, variance = 0.10f },
        new StarColorType { color = new Color(0.67f, 0.75f, 1.00f), weight = 25.00f, variance = 0.09f },
        new StarColorType { color = new Color(0.79f, 0.84f, 1.00f), weight = 20.00f, variance = 0.08f },
        new StarColorType { color = new Color(0.96f, 0.97f, 1.00f), weight = 14.00f, variance = 0.06f },
        new StarColorType { color = new Color(1.00f, 0.95f, 0.82f), weight = 13.00f, variance = 0.05f },
        new StarColorType { color = new Color(1.00f, 0.82f, 0.62f), weight = 21.00f, variance = 0.07f },
        new StarColorType { color = new Color(1.00f, 0.63f, 0.45f), weight = 6.70f, variance = 0.09f },
    };

    private const int LutSize = 256;

    private static readonly int LutId = Shader.PropertyToID("_StarColorLUT");

    private Texture2D lut;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void OnDisable()
    {
        ReleaseTexture();
    }

    // Bakes the weighted colour mixture into a 1D lookup so the shader can resolve
    // a star's colour with a single texture sample instead of walking the whole
    // weight list per star.
    [ContextMenu("Apply Colors")]
    public void Apply()
    {
        int count = Mathf.Clamp(colors != null ? colors.Count : 0, 0, MaxColors);
        EnsureTexture();

        if (count == 0)
        {
            for (int t = 0; t < LutSize; t++)
            {
                lut.SetPixel(t, 0, Color.white);
            }

            lut.Apply();
            Shader.SetGlobalTexture(LutId, lut);
            return;
        }

        float total = 0f;
        for (int i = 0; i < count; i++)
        {
            total += Mathf.Max(0f, colors[i].weight);
        }

        if (total <= 0f)
        {
            total = 1f;
        }

        for (int t = 0; t < LutSize; t++)
        {
            float pick = ((t + 0.5f) / LutSize) * total;
            float acc = 0f;
            StarColorType entry = colors[count - 1];

            for (int i = 0; i < count; i++)
            {
                acc += Mathf.Max(0f, colors[i].weight);
                if (pick <= acc)
                {
                    entry = colors[i];
                    break;
                }
            }

            lut.SetPixel(t, 0, new Color(entry.color.r, entry.color.g, entry.color.b, Mathf.Clamp01(entry.variance)));
        }

        lut.Apply();
        Shader.SetGlobalTexture(LutId, lut);
    }

    private void EnsureTexture()
    {
        if (lut != null)
        {
            return;
        }

        lut = new Texture2D(LutSize, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
    }

    private void ReleaseTexture()
    {
        if (lut == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(lut);
        }
        else
        {
            DestroyImmediate(lut);
        }

        lut = null;
    }
}
