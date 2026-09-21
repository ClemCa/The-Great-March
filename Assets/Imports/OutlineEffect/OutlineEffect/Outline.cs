/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//
//  URP port: the outline is rendered by duplicating the mesh with an
//  inverted-hull outline material, which works with the Universal Render
//  Pipeline without any renderer feature.
*/

using UnityEngine;
using UnityEngine.Serialization;

namespace cakeslice
{
	[RequireComponent(typeof(Renderer))]
	public class Outline : MonoBehaviour
	{
		public Renderer Renderer { get; private set; }
		public SpriteRenderer SpriteRenderer { get; private set; }
		public SkinnedMeshRenderer SkinnedMeshRenderer { get; private set; }
		public MeshFilter MeshFilter { get; private set; }

		[SerializeField, FormerlySerializedAs("color")]
		private int _color;

		/// <summary>
		/// Index of the outline colour (0, 1 or 2) defined on the OutlineEffect.
		/// </summary>
		public int color
		{
			get { return _color; }
			set
			{
				if (_color == value)
					return;

				_color = value;
				RefreshMaterial();
			}
		}

		public bool eraseRenderer;

		private GameObject _outlineObject;
		private Material _currentMaterial;

		private void Awake()
		{
			Renderer = GetComponent<Renderer>();
			SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			SpriteRenderer = GetComponent<SpriteRenderer>();
			MeshFilter = GetComponent<MeshFilter>();
		}

		void OnEnable()
		{
			OutlineEffect.Instance?.AddOutline(this);
		}

		void Start()
		{
			// Covers the case where OnEnable ran before OutlineEffect.Awake.
			OutlineEffect.Instance?.AddOutline(this);
		}

		void OnDisable()
		{
			OutlineEffect.Instance?.RemoveOutline(this);
			RemoveOutlineObject();
		}

		internal void RefreshMaterial()
		{
			OutlineEffect effect = OutlineEffect.Instance;
			if (effect == null)
				return;

			Material material = effect.GetMaterialFromID(_color);
			if (material == null)
				return;

			if (_currentMaterial == material && _outlineObject != null)
				return;

			RemoveOutlineObject();
			CreateOutlineObject(material);
			_currentMaterial = material;
		}

		private void CreateOutlineObject(Material material)
		{
			if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null)
			{
				GameObject go = new GameObject("Outline");
				go.transform.SetParent(transform, false);

				SkinnedMeshRenderer renderer = go.AddComponent<SkinnedMeshRenderer>();
				renderer.sharedMesh = SkinnedMeshRenderer.sharedMesh;
				renderer.bones = SkinnedMeshRenderer.bones;
				renderer.rootBone = SkinnedMeshRenderer.rootBone;
				renderer.sharedMaterial = material;
				renderer.updateWhenOffscreen = true;
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				renderer.receiveShadows = false;

				_outlineObject = go;
				return;
			}

			if (MeshFilter != null && MeshFilter.sharedMesh != null)
			{
				GameObject go = new GameObject("Outline");
				go.transform.SetParent(transform, false);
				go.AddComponent<MeshFilter>().sharedMesh = MeshFilter.sharedMesh;

				MeshRenderer renderer = go.AddComponent<MeshRenderer>();
				renderer.sharedMaterial = material;
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				renderer.receiveShadows = false;

				_outlineObject = go;
			}
		}

		private void RemoveOutlineObject()
		{
			if (_outlineObject != null)
			{
				if (Application.isPlaying)
					Destroy(_outlineObject);
				else
					DestroyImmediate(_outlineObject);

				_outlineObject = null;
			}

			_currentMaterial = null;
		}

		private Material[] _SharedMaterials;
		public Material[] SharedMaterials
		{
			get
			{
				if (_SharedMaterials == null && Renderer != null)
					_SharedMaterials = Renderer.sharedMaterials;

				return _SharedMaterials;
			}
		}
	}
}
