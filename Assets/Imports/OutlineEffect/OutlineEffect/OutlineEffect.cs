/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//
//  URP port: the effect is now implemented with an inverted-hull outline
//  material, so no auxiliary camera or command buffers are required.
*/

using System.Collections.Generic;
using UnityEngine;

namespace cakeslice
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class OutlineEffect : MonoBehaviour
	{
		public static OutlineEffect Instance { get; private set; }

		private readonly HashSet<Outline> outlines = new HashSet<Outline>();

		[Range(1.0f, 6.0f)]
		public float lineThickness = 1.25f;
		[Range(0, 10)]
		public float lineIntensity = .5f;
		[Range(0, 1)]
		public float fillAmount = 0.2f;

		public Color lineColor0 = Color.red;
		public Color lineColor1 = Color.green;
		public Color lineColor2 = Color.blue;

		public bool additiveRendering = false;

		public bool backfaceCulling = true;

		public Color fillColor = Color.blue;
		public bool useFillColor = false;

		[Header("These settings can affect performance!")]
		public bool cornerOutlines = false;
		public bool addLinesBetweenColors = false;

		[Header("Advanced settings")]
		public bool scaleWithScreenSize = true;
		[Range(0.0f, 1.0f)]
		public float alphaCutoff = .5f;
		public bool flipY = false;
		public Camera sourceCamera;
		public bool autoEnableOutlines = false;

		private Material outline1Material;
		private Material outline2Material;
		private Material outline3Material;

		private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
		private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		void Start()
		{
			CreateMaterialsIfNeeded();
			UpdateMaterialsPublicProperties();

			if (sourceCamera == null)
			{
				sourceCamera = GetComponent<Camera>();
			}
		}

		internal Material GetMaterialFromID(int ID)
		{
			CreateMaterialsIfNeeded();

			if (ID == 1)
				return outline2Material;
			else if (ID == 2)
				return outline3Material;
			else
				return outline1Material;
		}

		private void CreateMaterialsIfNeeded()
		{
			if (outline1Material == null)
				outline1Material = CreateMaterial(lineColor0);
			if (outline2Material == null)
				outline2Material = CreateMaterial(lineColor1);
			if (outline3Material == null)
				outline3Material = CreateMaterial(lineColor2);
		}

		private Material CreateMaterial(Color color)
		{
			Shader shader = Resources.Load<Shader>("OutlineURP");
			if (shader == null)
				shader = Shader.Find("Outline/URP Outline");

			if (shader == null)
			{
				Debug.LogError("OutlineEffect: could not find the 'Outline/URP Outline' shader.");
				return null;
			}

			Material material = new Material(shader)
			{
				hideFlags = HideFlags.HideAndDontSave,
			};
			material.SetColor(OutlineColorId, color);
			material.SetFloat(OutlineThicknessId, lineThickness * 0.01f);
			return material;
		}

		public void UpdateMaterialsPublicProperties()
		{
			ApplyMaterialProperties(outline1Material, lineColor0);
			ApplyMaterialProperties(outline2Material, lineColor1);
			ApplyMaterialProperties(outline3Material, lineColor2);

			foreach (Outline outline in outlines)
			{
				if (outline != null)
				{
					outline.RefreshMaterial();
				}
			}
		}

		private void ApplyMaterialProperties(Material material, Color color)
		{
			if (material == null)
				return;

			material.SetColor(OutlineColorId, color);
			material.SetFloat(OutlineThicknessId, lineThickness * 0.01f);
		}

		public void AddOutline(Outline outline)
		{
			if (outline == null)
				return;

			if (outlines.Add(outline))
			{
				outline.RefreshMaterial();
			}
		}

		public void RemoveOutline(Outline outline)
		{
			if (outline != null)
			{
				outlines.Remove(outline);
			}
		}
	}
}
