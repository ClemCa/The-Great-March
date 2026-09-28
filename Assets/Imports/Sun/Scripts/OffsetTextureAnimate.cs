using UnityEngine;

public class OffsetTextureAnimate :MonoBehaviour
{
	public float scrollSpeedX = 0.015f;
	public float scrollSpeedY = 0.015f;
	public float scrollSpeedXMaterial2 = 0.015f;
	public float scrollSpeedYMaterial2 = 0.015f;
	public MeshRenderer meshRenderer;
	public bool applyToNormal = true;

	void Start()
	{
		if (meshRenderer == null) meshRenderer = gameObject.GetComponent<MeshRenderer>();
	}

	void Update()
	{
		float offsetX = Time.time * scrollSpeedX % 1;
		float offsetY = Time.time * scrollSpeedY % 1;
		float offset2X = Time.time * scrollSpeedXMaterial2 % 1;
		float offset2Y = Time.time * scrollSpeedYMaterial2 % 1;
		SetOffsets(meshRenderer.material, new Vector2(offsetX, offsetY));
		if (meshRenderer.materials.Length > 1)
			SetOffsets(meshRenderer.materials[1], new Vector2(offset2X, offset2Y));
	}

	void SetOffsets(Material material, Vector2 offset)
	{
		// URP shaders expose the main texture as _BaseMap; built-in/legacy as _MainTex.
		if (material.HasProperty("_BaseMap")) material.SetTextureOffset("_BaseMap", offset);
		else material.SetTextureOffset("_MainTex", offset);
		if (applyToNormal && material.HasProperty("_BumpMap")) material.SetTextureOffset("_BumpMap", offset);
	}
}