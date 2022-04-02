using UnityEngine;
using ClemCAddons;
using System.Collections.Generic;

public class SplineDecorator : MonoBehaviour
{

	private BezierSpline _spline;

	public BezierSpline spline
	{
		get
		{
			if (_spline == null)
				_spline = GetComponent<BezierSpline>();
			return _spline;
		}
		set
		{
			_spline = value;
		}
	}

	public int StepSize;

	public bool lookForward;

	public Transform[] items;

	public bool spawnOnPlay;

	public bool spawnSelectedOnly;

	public bool autoApplyChanges;

	private int StepSizeI;

	public void SpawnNow()
	{
		StepSizeI = StepSize;
		if (StepSizeI <= 0 || items == null || items.Length == 0)
		{
			return;
		}
		float stepSize = StepSizeI * items.Length;
		if (spline.Loop || stepSize == 1)
		{
			stepSize = 1f / stepSize;
		}
		else
		{
			stepSize = 1f / (stepSize - 1);
		}
		if (spawnSelectedOnly)
		{
			StepSizeI = 0;
			for (int i = 0; i < spline.curves.Length; i++)
			{
				spline.curves[i].Frequency = spline.curves[i].Frequency < 0 ? 0 : spline.curves[i].Frequency;
			}
			List<int> toSpawn = new List<int>();
			for (int t = 0; t < spline.CurveCount; t++)
			{
				toSpawn.Add(t);
				StepSizeI += spline.GetCurve(t).Frequency;
			}
			if (spline.Loop || stepSize == 1)
			{
				stepSize = 1f / StepSizeI;
			}
			else
			{
				stepSize = 1f / (StepSizeI - 1);
			}
			var curve = 0;
			var previousFrequency = 0;
			var curveFrequency = spline.curves[0].Frequency;
			for (int p = 0, f = 0; f - 1 < StepSizeI; f++) // f-1 to put one last item right at the end
			{
				for (int i = 0; i < items.Length; i++, p++)
				{
					while (spline.CurveCount > curve && f > previousFrequency + spline.curves[curve].Frequency || spline.curves[curve].Frequency == 0)
					{
						previousFrequency += curveFrequency;
						curve++;
						curveFrequency = spline.curves[curve].Frequency;
					}
					if (!spline.curves[curve].Spawn)
					{
						continue;
					}
					var currentPoint = (f / (float)StepSizeI - previousFrequency / (float)StepSizeI) / (curveFrequency / (float)StepSizeI);
					Vector3 position = spline.GetPointInCurve(currentPoint, curve);
					if (!position.IsInfinite())
					{
						Transform item = Instantiate(items[i]) as Transform;
						item.transform.localPosition = position;
						if (lookForward)
						{
							item.transform.LookAt(position + spline.GetDirectionInCurve(currentPoint, curve));
						}
						item.transform.parent = transform;
					}
				}
			}
		}
		else
		{
			for (int p = 0, f = 0; f < StepSizeI; f++)
			{
				for (int i = 0; i < items.Length; i++, p++)
				{
					Vector3 position = spline.GetPoint(p * stepSize);
					Transform item = Instantiate(items[i]) as Transform;
					item.transform.localPosition = position;
					if (lookForward)
					{
						item.transform.LookAt(position + spline.GetDirection(p * stepSize));
					}
					item.transform.parent = transform;
				}
			}
		}
	}
	public void Clear()
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}

	public void RegisterChange()
	{
		if (autoApplyChanges)
		{
			Clear();
			SpawnNow();
		}
	}

	private void Awake()
	{
		if (spawnOnPlay)
			SpawnNow();
	}
}