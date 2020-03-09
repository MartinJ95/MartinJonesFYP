using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    public void calculatePower()
    {
        powerValue = 0;
        foreach(GameObject o in m_objects)
        {
            powerValue += o.GetComponent<unit>().attackDamage;
        }
    }

    public float calculateRadius()
    {
        if(m_objects.Count == 1)
        {
            if(m_objects[0].GetComponent<unit>())
            {
                return m_objects[0].GetComponent<unit>().awarenessRange;
            }
            return m_objects[0].transform.localScale.magnitude;
        }
        float maxRadius = 0;
        foreach(GameObject o in m_objects)
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
        if(m_objects.Count == 1)
        {
            return m_objects[0].transform.position;
        }
        else
        {
            Vector3 pointCenter = new Vector3(0, 0, 0);
            foreach(GameObject obj in m_objects)
            {
                pointCenter += obj.transform.position;
            }
            pointCenter /= m_objects.Count;
            return pointCenter;
        }
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
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            if(Vector3.Distance(transform.position, target.transform.position) > weaponRange / 3)
            {
                GetComponent<NavMeshAgent>().destination = target.transform.position;
                GetComponent<NavMeshAgent>().isStopped = false;
            }
            else
            {
                GetComponent<NavMeshAgent>().isStopped = true;
            }
        }
        else if(state == unitState.engaging)
        {
            if(Vector3.Distance(transform.position, target.transform.position) > weaponRange)
            {
                GetComponent<NavMeshAgent>().destination = target.transform.position;
                GetComponent<NavMeshAgent>().isStopped = false;
            }
            else
            {
                GetComponent<NavMeshAgent>().isStopped = true;
            }
        }
        else if(state == unitState.idle || state == unitState.delegating)
        {
            pointsOfInterest.RemoveRange(0, pointsOfInterest.Count);

            unit[] allUnits = FindObjectsOfType<unit>();

            foreach(unit u in allUnits)
            {
                if(Vector3.Distance(transform.position, u.transform.position) < awarenessRange && !u.isInPoint && u != this)
                {
                    if (u.isPlayerUnit == isPlayerUnit)
                    {
                        pointOfInterest point = new pointOfInterest(pointOfInterestType.ally, u.transform.gameObject);
                        u.isInPoint = true;
                        foreach (unit t in allUnits)
                        {
                            if(Vector3.Distance(u.transform.position, t.transform.position) < u.awarenessRange && !t.isInPoint && t != this && t.isPlayerUnit == u.isPlayerUnit)
                            {
                                point.m_objects.Add(t.transform.gameObject);
                                t.isInPoint = true;
                            }
                        }
                        pointsOfInterest.Add(point);
                    }
                    else
                    {
                        pointOfInterest point = new pointOfInterest(pointOfInterestType.enemy, u.transform.gameObject);
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
                }
            }

            cover[] defenses = FindObjectsOfType<cover>();

            foreach(cover c in defenses)
            {
                if(Vector3.Distance(transform.position, c.transform.position) < awarenessRange && !c.isInPoint)
                {
                    pointOfInterest point = new pointOfInterest(pointOfInterestType.cover, c.transform.gameObject);
                    c.isInPoint = true;
                    foreach (cover c1 in defenses)
                    {
                        if(Vector3.Distance(c.transform.position, c1.transform.position) < c.defenseAreaSize && !c1.isInPoint)
                        {
                            point.m_objects.Add(c1.transform.gameObject);
                            c1.isInPoint = true;
                        }
                    }
                    pointsOfInterest.Add(point);
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
        }

        unit[] units = FindObjectsOfType<unit>();

        foreach(unit u in units)
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
