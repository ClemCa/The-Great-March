using UnityEngine;
using System;
using System.Linq;
using ClemCAddons;
using UnityEditor;

public class BezierSpline : MonoBehaviour
{

	[Serializable]
	public struct Curves
	{
		public Vector3[] Points;
		public int Frequency;
		public bool Spawn;
		public Curves(Vector3[] points, int frequency = 5, bool spawn = true)
		{
			Points = points;
			Frequency = frequency;
			Spawn = spawn;
		}
	}

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	[SerializeField]
	public Curves[] curves = new Curves[] { new Curves(new Vector3[] {
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(4f, 0f, 0f)
			}) };

	private Vector3[] points
	{
		get
		{
			return curves.SelectMany(t => t.Points).ToArray();
		}
		set
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (i >= 4)
					curves[((i - 4) / 3) + 1].Points[(i - 4) % 3] = value[i];
				else
					curves[0].Points[i] = value[i];
			}
		}
	}
	public bool Loop
	{
		get
		{
			return loop;
		}
		set
		{
			loop = value;
			if (value == true)
			{
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, curves[0].Points[0]);
			}
		}
	}

	public int ControlPointCount
	{
		get
		{
			return points.Length;
		}
	}

	public Vector3 GetControlPoint(int index)
	{
		return points[index];
	}

	public void SetControlPoint(int index, Vector3 point)
	{
		if (index % 3 == 0)
		{
			Vector3 delta = point - points[index];
			if (loop)
			{
				var sum = points.Length;
				if (index == 0)
				{
					points = points.SetAt(points[1] + delta, 1);
					points = points.SetAt(points[sum - 2] + delta, sum - 2);
					points = points.SetAt(point, sum - 1);
				}
				else if (index == sum - 1)
				{
					points = points.SetAt(point, 0);
					points = points.SetAt(points[1] + delta, 1);
					points = points.SetAt(points[index - 1] + delta, index - 1);
				}
				else
				{
					points = points.SetAt(points[index - 1] + delta, index - 1);
					points = points.SetAt(points[index + 1] + delta, index + 1);
				}
			}
			else
			{
				if (index > 0)
				{
					points = points.SetAt(points[index - 1] + delta, index - 1);
				}
				if (index + 1 < points.Length)
				{
					points = points.SetAt(points[index + 1] + delta, index + 1);
				}
			}
		}
		points = points.SetAt(point, index);
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode(int index)
	{
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode(int index, BezierControlPointMode mode)
	{
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop)
		{
			if (modeIndex == 0)
			{
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1)
			{
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	public void RegisterChange()
	{
		if (TryGetComponent(out SplineDecorator spline))
		{
			spline.RegisterChange();
		}
	}

	private void EnforceMode(int index)
	{
		RegisterChange();
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
		{
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex)
		{
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0)
			{
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length)
			{
				enforcedIndex = 1;
			}
		}
		else
		{
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length)
			{
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0)
			{
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned)
		{
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount
	{
		get
		{
			return (points.Length - 1) / 3;
		}
	}

	public Curves GetCurve(int index)
	{
		return curves[index];
	}

	public Vector3 GetPoint(float t, int[] validate = null)
	{
		int i;
		int v;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Length - 4;
			v = CurveCount - 1;
		}
		else
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			v = i;
			t -= i;
			i *= 3;
		}
		if (validate == null || validate.Find(v, -1) != -1)
			return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
		else
			return Vector3.positiveInfinity;
	}

	public Vector3 GetPointInCurve(float t, int curve, int[] validate = null)
	{
		int i;
		if (curve == 0)
			i = 0;
		else
			i = curve * 3;
		if ((validate == null || validate.Find(curve, -1) != -1))
			return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
		else
			return Vector3.positiveInfinity;
	}

	public Vector3 GetVelocity(float t)
	{
		int i;
		if (t >= 1f)
		{
			t = 1f;
			i = points.Length - 4;
		}
		else
		{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}

	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}
	public Vector3 GetDirectionInCurve(float t, int curve)
	{
		if (curves.Length == 1)
			return GetVelocity(t).normalized;
		return GetVelocity(Mathf.Lerp(((curve) / ((float)curves.Length)), (curve + 1) / (float)curves.Length, t)).normalized;
	}


	public void AddCurve()
	{
		Curves curve = curves[curves.Length - 1];
		Vector3 point = points[points.Length - 1];
		Vector3 direction = points[points.Length - 2].Direction(point);
		Array.Resize(ref curves, curves.Length + 1);
		curve.Points = new Vector3[3];
		point += direction;
		curve.Points[curve.Points.Length - 3] = point;
		point += direction;
		curve.Points[curve.Points.Length - 2] = point;
		point += direction;
		curve.Points[curve.Points.Length - 1] = point;
		curves[curves.Length - 1] = curve;
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

	public void Duplicate(int index = -1)
    {
		if (index == -1)
			index = CurveCount - 1;
		Curves copyCurve = curves[index];
		Curves newCurve = new Curves(copyCurve.Points.GetCopy(), copyCurve.Frequency, copyCurve.Spawn);

		Vector3 lastPoint;
		if (index == 0)
			lastPoint = transform.position;
		else
			lastPoint = curves[index-1].Points[curves[index - 1].Points.Length - 1];
		Vector3 myLastPoint = curves[index].Points[curves[index].Points.Length - 1];

		var newDirection = Quaternion.LookRotation(copyCurve.Points[copyCurve.Points.Length - 1].Direction(copyCurve.Points[copyCurve.Points.Length - 2]));

		var direction = Quaternion.LookRotation(copyCurve.Points[0].Direction(lastPoint));

		for (int i = 0; i < copyCurve.Points.Length; i++)
        {
			var targetMovement = copyCurve.Points[i] - lastPoint;
			targetMovement = newDirection * direction.Inverse() * targetMovement;
			newCurve.Points[i] = myLastPoint + targetMovement;

			lastPoint = copyCurve.Points[i];
			myLastPoint = newCurve.Points[i];
        }
		Array.Resize(ref curves, curves.Length + 1);
		curves[curves.Length - 1] = newCurve;
		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

	public void RemoveCurve()
	{
		Array.Resize(ref curves, curves.Length - 1);
		Array.Resize(ref modes, modes.Length - 1);
		EnforceMode(points.Length);
		if (loop)
		{
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
		}
	}

	public void Reset()
	{
		curves = new Curves[]{ new Curves(new Vector3[] {
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(4f, 0f, 0f)
			})
		};
		modes = new BezierControlPointMode[] {
				BezierControlPointMode.Free,
				BezierControlPointMode.Free
		};
	}
}