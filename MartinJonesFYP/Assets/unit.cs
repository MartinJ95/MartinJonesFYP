using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unit : MonoBehaviour
{
    public float health = 100;
    public float attackDamage = 5;
    public bool isInPoint = false;
    public float weaponCooldown;
    public bool canShoot = true;
	public bool selected = false;
    public bool isPlayerUnit = true;
    public bool debugPoints = false;
    public GameObject target = null;
    public float awarenessRange;
    public float weaponRange;
    public Vector3 setPosition;
    public unitState state;
    public GameObject bullet;
    public List<pointOfInterest> pointsOfInterest = new List<pointOfInterest>();

    // Start is called before the first frame update
    void Start()
    {
        setPosition = transform.position;
    }

    public void cooldown()
    {
        canShoot = true;
    }

    public void shoot(in GameObject obj)
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, obj.transform.position - transform.position, out hit, weaponRange))
        {
            if(hit.transform.gameObject == obj)
            {
                GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);

                newBullet.GetComponent<bullet>().m_damage = attackDamage;
                newBullet.GetComponent<bullet>().m_direction = Vector3.Normalize(obj.transform.position - transform.position);
                newBullet.GetComponent<bullet>().m_isPlayerOwned = isPlayerUnit;

                canShoot = false;
                Invoke("cooldown", weaponCooldown);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(pointsOfInterest.Count > 0)
        {
            if (debugPoints)
            {
                foreach (pointOfInterest p in pointsOfInterest)
                {
                    if(p.m_type == pointOfInterestType.ally || p.m_type == pointOfInterestType.healingBuilding)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if(p.m_type == pointOfInterestType.enemy)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if(p.m_type == pointOfInterestType.cover)
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawWireSphere(p.calculatePosition(), p.calculateRadius());
                }
            }
        }
        if(debugPoints)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, awarenessRange);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(setPosition, 1);
        }
    }

	private void moveTo(Vector3 destination, float distance, float range = 0.1f)
	{
		if(distance > range)
		{
			GetComponent<NavMeshAgent>().destination = destination;
			GetComponent<NavMeshAgent>().isStopped = false;
		}
		else
		{
			GetComponent<NavMeshAgent>().isStopped = true;
		}
	}

	private void generatePoint(unit u, pointOfInterestType type, unit[] allUnits)
	{
		pointOfInterest point = new pointOfInterest(type, u.transform.gameObject);
		u.isInPoint = true;
		foreach (unit t in allUnits)
		{
			if (Vector3.Distance(u.transform.position, t.transform.position) < u.awarenessRange && !t.isInPoint && t != this && t.isPlayerUnit == u.isPlayerUnit)
			{
				point.m_objects.Add(t.transform.gameObject);
				t.isInPoint = true;
			}
		}
		pointsOfInterest.Add(point);
	}

	private void generatePoint(cover c, cover[] defenses)
	{
		pointOfInterest point = new pointOfInterest(pointOfInterestType.cover, c.transform.gameObject);
		c.isInPoint = true;
		foreach (cover c1 in defenses)
		{
			if (Vector3.Distance(c.transform.position, c1.transform.position) < c.defenseAreaSize && !c1.isInPoint)
			{
				point.m_objects.Add(c1.transform.gameObject);
				c1.isInPoint = true;
			}
		}
		pointsOfInterest.Add(point);
	}

	private GameObject findClosestUnit(pointOfInterest point)
	{
		GameObject closestUnit = point.m_objects[0];
		foreach (GameObject o in point.m_objects)
		{
			if (Vector3.Distance(transform.position, o.transform.position) < Vector3.Distance(transform.position, closestUnit.transform.position))
			{
				closestUnit = o;
			}
		}
		return closestUnit;
	}

	private pointOfInterest findHighestWeighted(float allyScore, float enemyScore, ref pointOfInterest lowestWeighted)
	{
		pointOfInterest highestWeighted = null;
		foreach (pointOfInterest p in pointsOfInterest)
		{
			if (p.m_type == pointOfInterestType.ally)
			{
				p.calculateWeighting(setPosition, awarenessRange, enemyScore - allyScore);
			}
			else if (p.m_type == pointOfInterestType.enemy)
			{
				p.calculateWeighting(setPosition, awarenessRange, allyScore - enemyScore);
			}
			else if (p.m_type == pointOfInterestType.cover)
			{
				p.calculateWeighting(setPosition, awarenessRange, (enemyScore - allyScore) * 2);
			}
			if (lowestWeighted == null)
			{
				lowestWeighted = p;
			}
			else if(p.m_weighting < lowestWeighted.m_weighting)
			{
				lowestWeighted = p;
			}
			if (highestWeighted == null)
			{
				highestWeighted = p;
			}
			else if (p.m_weighting > highestWeighted.m_weighting)
			{
				if (p.m_type == pointOfInterestType.cover)
				{
					bool isOccupied = false;
					foreach (pointOfInterest p1 in pointsOfInterest)
					{
						if (p1 != p)
						{
							if (Vector3.Distance(p1.calculatePosition(), p.calculatePosition()) < p.calculateRadius() && p1.m_type == pointOfInterestType.enemy)
							{
								isOccupied = true;
								break;
							}
						}
					}
					if (isOccupied)
					{
						p.m_weighting = 0;
					}
					else
					{
						highestWeighted = p;
					}
				}
				else
				{
					highestWeighted = p;
				}
			}
		}
		return highestWeighted;
	}

	// Update is called once per frame
	void Update()
    {
		unit[] allUnits = FindObjectsOfType<unit>();

		if (state == unitState.moving)
        {
            if (GetComponent<NavMeshAgent>().remainingDistance < 0.1f)
            {
                GetComponent<NavMeshAgent>().isStopped = true;
                state = unitState.idle;
            }
        }
        else if(state == unitState.assisting)
        {
			moveTo(target.transform.position, Vector3.Distance(transform.position, target.transform.position), weaponRange / 3);
        }
        else if(state == unitState.engaging)
        {
			moveTo(target.transform.position, Vector3.Distance(transform.position, target.transform.position), weaponRange);
        }
        else if(state == unitState.idle || state == unitState.delegating)
        {
            pointsOfInterest.RemoveRange(0, pointsOfInterest.Count);

            foreach(unit u in allUnits)
            {
                if(Vector3.Distance(setPosition, u.transform.position) < awarenessRange && !u.isInPoint && u != this)
                {
                    if (u.isPlayerUnit == isPlayerUnit)
                    {
						generatePoint(u, pointOfInterestType.ally, allUnits);
                    }
                    else
                    {
						generatePoint(u, pointOfInterestType.enemy, allUnits);
                    }
                }
            }

            cover[] defenses = FindObjectsOfType<cover>();

            foreach(cover c in defenses)
            {
                if(Vector3.Distance(transform.position, c.transform.position) < awarenessRange && !c.isInPoint)
                {
					generatePoint(c, defenses);
                }
            }

            foreach(cover c in defenses)
            {
                c.isInPoint = false;
            }

            foreach(unit u in allUnits)
            {
                u.isInPoint = false;
            }

			float allyScore = this.attackDamage;
			float enemyScore = 0;

			foreach(pointOfInterest p in pointsOfInterest)
			{
				if(p.m_type == pointOfInterestType.ally)
				{
					allyScore += p.calculatePower();
				}
				else if(p.m_type == pointOfInterestType.enemy)
				{
					enemyScore += p.calculatePower();
				}
			}

			pointOfInterest lowestWeighted = null;
			pointOfInterest highestWeighted = findHighestWeighted(allyScore, enemyScore, ref lowestWeighted);
			if(highestWeighted == null)
			{
				moveTo(setPosition, Vector3.Distance(transform.position, setPosition), 0.5f);
			}
			else
			{
				if(debugPoints)
				{
					Debug.Log(highestWeighted.m_weighting);
				}
				if(enemyScore > allyScore * 2 && lowestWeighted.m_type == pointOfInterestType.enemy)
				{
					GameObject closestUnit = findClosestUnit(lowestWeighted);
					Vector3 dest = transform.position + (((transform.position - closestUnit.transform.position).normalized) * 2);
					moveTo(dest, Vector3.Distance(transform.position, dest));
;				}
				else if(highestWeighted.m_weighting > attackDamage)
				{
					if(highestWeighted.m_type == pointOfInterestType.ally)
					{
						GameObject closestUnit = findClosestUnit(highestWeighted);
						moveTo(closestUnit.transform.position, Vector3.Distance(transform.position, closestUnit.transform.position), weaponRange / 3);
					}
					else if(highestWeighted.m_type == pointOfInterestType.enemy)
					{
						GameObject closestUnit = findClosestUnit(highestWeighted);
						moveTo(closestUnit.transform.position, Vector3.Distance(transform.position, closestUnit.transform.position), weaponRange);
					}
					else if(highestWeighted.m_type == pointOfInterestType.cover)
					{
						moveTo(highestWeighted.calculatePosition(), Vector3.Distance(transform.position, highestWeighted.calculatePosition()), 0.5f);
					}
				}
				else
				{
					moveTo(setPosition, Vector3.Distance(transform.position, setPosition), 0.5f);
				}
			}
        }

        foreach(unit u in allUnits)
        {
            if(u.isPlayerUnit != isPlayerUnit && Vector3.Distance(transform.position, u.transform.position) < weaponRange)
            {
                if (canShoot)
                {
                    shoot(u.transform.gameObject);
                }
            }
        }
    }
}
