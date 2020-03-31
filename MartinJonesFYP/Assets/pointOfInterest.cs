using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum pointOfInterestType
{
	ally,
	enemy,
	healingBuilding,
	cover
}

public class pointOfInterest
{
	public pointOfInterestType m_type;
	public List<GameObject> m_objects = new List<GameObject>();
	public float powerValue;
	public float m_weighting;

	public pointOfInterest()
	{

	}

	public pointOfInterest(pointOfInterestType type, in GameObject obj)
	{
		m_type = type;
		m_objects.Add(obj);
	}

	public float calculatePower()
	{
		powerValue = 0;
		foreach (GameObject o in m_objects)
		{
			powerValue += o.GetComponent<unit>().attackDamage;
		}
		return powerValue;
	}

	public float calculateRadius()
	{
		if (m_objects.Count == 1)
		{
			if (m_objects[0].GetComponent<unit>())
			{
				return m_objects[0].GetComponent<unit>().awarenessRange;
			}
			return m_objects[0].transform.localScale.magnitude;
		}
		float maxRadius = 0;
		foreach (GameObject o in m_objects)
		{
			float length = Vector3.Distance(m_objects[0].transform.position, o.transform.position);
			if (length > maxRadius)
			{
				maxRadius = length;
			}
		}
		return maxRadius;
	}

	public Vector3 calculatePosition()
	{
		if (m_objects.Count == 1)
		{
			return m_objects[0].transform.position;
		}
		else
		{
			Vector3 pointCenter = new Vector3(0, 0, 0);
			foreach (GameObject obj in m_objects)
			{
				pointCenter += obj.transform.position;
			}
			pointCenter /= m_objects.Count;
			return pointCenter;
		}
	}

	public float calculateWeighting(Vector3 position, float awarenessRange, float modifier)
	{
		Vector3 pointPos = calculatePosition();

		Vector3 dirToUnit = Vector3.Normalize(position - pointPos);

		Vector3 closestPoint = pointPos + dirToUnit * calculateRadius();

		m_weighting = modifier * (awarenessRange / Vector3.Distance(position, closestPoint));

		return m_weighting;
	}
}

public enum unitState
{
	idle,
	moving,
	engaging,
	assisting,
	delegating
}