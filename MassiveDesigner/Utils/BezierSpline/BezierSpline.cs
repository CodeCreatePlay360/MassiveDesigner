using UnityEngine;
using System;
using System.Collections.Generic;


[System.Serializable]
public class BezierSpline
{
    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored,
    }

    public List<Vector3> Points { get { return points; } }
    [SerializeField] List<Vector3> points = new List<Vector3>();

    [SerializeField] private BezierControlPointMode[] modes;
	[SerializeField] private bool loop;

	public bool Loop
    {
		get
        {
			return loop;
		}
		set
        {
			loop = value;
			if (value == true && points.Count > 0)
            {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount
    {
		get
        {
			return points.Count;
		}
	}

	public Vector3 GetControlPoint (int index)
    {
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point)
    {
		if (index % 3 == 0)
        {
			Vector3 delta = point - points[index];
			if (loop)
            {
				if (index == 0)
                {
					points[1] += delta;
					points[points.Count - 2] += delta;
					points[points.Count - 1] = point;
				}
				else if (index == points.Count - 1)
                {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else
                {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else
            {
				if (index > 0)
                {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Count)
                {
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode (int index)
    {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode)
    {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop)
        {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1)
            {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index)
    {
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
				fixedIndex = points.Count - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Count)
            {
				enforcedIndex = 1;
			}
		}
		else
        {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Count)
            {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0)
            {
				enforcedIndex = points.Count - 2;
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
		get { return (points.Count - 1) / 3; }
	}

	public Vector3 GetPoint (float t)
    {
		int i;

		if (t >= 1f)
        {
			t = 1f;
			i = points.Count - 4;
		}
		else
        {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

        // return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
        return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }

    public List<Vector3> GetEventPoints()
    {

        return null;
    }

    public Vector3 GetBezier(int i, float t)
    {
        return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }
	
	public Vector3 GetVelocity (float t)
    {
		int i;
		if (t >= 1f)
        {
			t = 1f;
			i = points.Count - 4;
		}
		else
        {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
        // return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
        return (Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }
	
	public Vector3 GetDirection (float t)
    {
		return GetVelocity(t).normalized;
	}

	public void AddCurve (Vector3 atPos)
    {
        if (points.Count == 0)
        {
            Vector3 p1 = new Vector3(atPos.x, atPos.y, atPos.z);
            Vector3 p2 = new Vector3(atPos.x, atPos.y, atPos.z+5f);

            Vector3 p3 = new Vector3(atPos.x, atPos.y, atPos.z+10);
            Vector3 p4 = new Vector3(atPos.x, atPos.y, atPos.z+15f);

            points = new List<Vector3>(4)
            {
                p1,
                p2,
                p3,
                p4
            };

            modes = new BezierControlPointMode[]
            {
                BezierControlPointMode.Mirrored,
                BezierControlPointMode.Mirrored
            };

            return;
        }

        Vector3 point = points[points.Count - 1];
        var size = points.Count;

        float distance = Vector3.Distance(atPos, point);
        Vector3 direction = atPos - point;
        direction.Normalize();

        point.x += 0.5f;
        point.z += 0.5f;
        points.Add(point);

        // point.x += distance;
        // point.z += distance;
        point += distance * direction;
        points.Add(point);

        point.x += 0.5f;
        point.z += 0.5f;
        points.Add(point);

        Array.Resize(ref modes, modes.Length + 1);

		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Count - 4);
         
		if (loop)
        {
			points[points.Count - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}
	
	public void Clear ()
    {
        points.Clear();
	}
}