using UnityEngine;
using System.Collections.Generic;
using MassiveDesinger.Utils;


public class SeparatingAxisTest
{
	// References
	// Getting the Right Axes to Test with
	//https://gamedev.stackexchange.com/questions/44500/how-many-and-which-axes-to-use-for-3d-obb-collision-with-sat/

	//Unity Code, that nearly worked, but registered collisions incorrectly in some cases
	//http://thegoldenmule.com/blog/2013/12/supercolliders-in-unity/


	Vector3[] aAxes;
	Vector3[] bAxes;
	Vector3[] aVertices;
	Vector3[] bVertices;
	List<Vector3> allAxes;

	float minOverlap = 0;
    private Vector3 minOverlapAxis = Vector3.zero;

	List<Vector3> penetrationAxes;
	List<float> penetrationAxesDistance;

    public void Update(BBox _cubeA, BBox _cubeB)
    {
        if (CheckCollision(_cubeA, _cubeB))
            _cubeA.Hit = _cubeB.Hit = true;
        else
            _cubeA.Hit = _cubeB.Hit = false;
    }

	public bool CheckCollision(BBox a, BBox b)
	{
		minOverlap = 0;
		minOverlapAxis = Vector3.zero;

		aAxes = a.GetAxes();
		bAxes = b.GetAxes();

        allAxes = new List<Vector3>();

        for (int i = 0; i < aAxes.Length; i++)
            allAxes.Add(aAxes[i]);

        for (int i = 0; i < bAxes.Length; i++)
            allAxes.Add(bAxes[i]);

        for (int i = 0; i < aAxes.Length; i++)
        {
            for (int j = 0; j < bAxes.Length; j++)
            {
                allAxes.Add(Vector3.Cross(aAxes[i], bAxes[j]));
            }
        }

        for (int i = 0; i < aAxes.Length; i++)
        {
            //Debug.DrawRay(a.Transform.position, aAxes[i] * 2f, Color.red);
            //Debug.DrawRay(b.Transform.position, bAxes[i] * 2f, Color.green);
        }


		aVertices = a.GetVertices();
		bVertices = b.GetVertices();

		int aVertsLength = aVertices.Length;
		int bVertsLength = bVertices.Length;

        penetrationAxes = new List<Vector3>();
		penetrationAxesDistance = new List<float>();

		bool hasOverlap = false;

		if ( ProjectionHasOverlap(allAxes.Count, allAxes, bVertsLength, bVertices, aVertsLength, aVertices) )
		{
			hasOverlap = true;
		}
		else if (ProjectionHasOverlap(allAxes.Count, allAxes, aVertsLength, aVertices, bVertsLength, bVertices) )
		{
			hasOverlap = true;
		}

		// Penetration can be seen here, but its not reliable 
		// Debug.Log(minOverlap+" : "+minOverlapAxis);

		return hasOverlap;
	}

	/// Detects whether or not there is overlap on all separating axes.
	private bool ProjectionHasOverlap(
		int aAxesLength,
		List<Vector3> aAxes,

		int bVertsLength,
		Vector3[] bVertices,

		int aVertsLength,
		Vector3[] aVertices)
	{
		minOverlap = float.PositiveInfinity;

		for (int i = 0; i < aAxesLength; i++)
		{
			float bProjMin = float.MaxValue, aProjMin = float.MaxValue;
			float bProjMax = float.MinValue, aProjMax = float.MinValue;

			Vector3 axis = aAxes[i];

			// Handles the cross product = {0,0,0} case
			if (aAxes[i] == Vector3.zero ) return true;

			for (int j = 0; j < bVertsLength; j++)
			{
				float val = FindScalarProjection((bVertices[j]), axis);

				if (val < bProjMin)
				{
					bProjMin = val;
				}

				if (val > bProjMax)
				{
					bProjMax = val;
				}
			}

			for (int j = 0; j < aVertsLength; j++)
			{
				float val = FindScalarProjection((aVertices[j]), axis);

				if (val < aProjMin)
				{
					aProjMin = val;
				}

				if (val > aProjMax)
				{
					aProjMax = val;
				}
			}

			float overlap = FindOverlap(aProjMin, aProjMax, bProjMin, bProjMax);

			if ( overlap < minOverlap )
			{
				minOverlap = overlap;
				minOverlapAxis = axis;

				penetrationAxes.Add(axis);
				penetrationAxesDistance.Add(overlap);

			}

			//Debug.Log(overlap);

			if (overlap <= 0)
			{
				// Separating Axis Found Early Out
				return false;
			}
		}

		return true; // A penetration has been found
	}


	/// Calculates the scalar projection of one vector onto another, assumes normalised axes
	private static float FindScalarProjection(Vector3 point, Vector3 axis)
	{
		return Vector3.Dot(point, axis);
	}

	/// Calculates the amount of overlap of two intervals.
	private float FindOverlap(float astart, float aend, float bstart, float bend)
	{
		if (astart < bstart)
		{
			if (aend < bstart)
			{
				return 0f;
			}

			return aend - bstart;
		}

		if (bend < astart)
		{
			return 0f;
		}

		return bend - astart;
	}
}