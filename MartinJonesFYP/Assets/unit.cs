using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum pointOfInterestType
{
    ally,
    enemy
}

public class pointOfInterest
{
    pointOfInterestType m_type;
    public List<GameObject> m_objects = new List<GameObject>();
    public float m_weighting;

    public pointOfInterest()
    {

    }

    public pointOfInterest(pointOfInterestType type, in GameObject obj)
    {
        m_type = type;
        m_objects.Add(obj);
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

public class matrixTest
{
    int[] values = new int[9];

    public matrixTest(int v1, int v2, int v3, int v4, int v5, int v6, int v7, int v8, int v9)
    {
        values[0] = v1;
        values[1] = v2;
        values[2] = v3;
        values[3] = v4;
        values[4] = v5;
        values[5] = v6;
        values[6] = v7;
        values[7] = v8;
        values[8] = v9;
    }

    public void rotate()
    {
        int[] newValues = new int[9];
        for(int i = 0; i < values.Length; i++)
        {
            if(i != 4)
            {

                int swapIndex = i + 1;
                swapIndex = swapIndex * 3;
                swapIndex = swapIndex % 10;
                swapIndex -= 1;
                //(((i + 1) * 3) % 10) - 1;

                newValues[swapIndex] = values[i];
            }
            else
            {
                newValues[i] = values[i];
            }
        }
        values = newValues;
    }
}

public enum unitState
{
    idle = 0,
    moving = 1,
    engaging = 2,
    assisting = 3,
    delegating = 4
}


public class unit : MonoBehaviour
{
    public int health = 100;
    public int attackDamage = 5;
    public bool isInPoint = false;
    public float weaponCooldown;
    public bool canShoot = true;
	public bool selected = false;
    public bool isPlayerUnit = true;
    public GameObject target = null;
    public int awarenessRange;
    public int weaponRange;
    public Vector3 setPosition;
    public unitState state;
    public GameObject bullet;
    public List<pointOfInterest> pointsOfInterest = new List<pointOfInterest>();

    // Start is called before the first frame update
    void Start()
    {
        setPosition = transform.position;
        matrixTest test = new matrixTest(1, 2, 3, 4, 5, 6, 7, 8, 9);
        test.rotate();
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
            foreach (pointOfInterest p in pointsOfInterest)
            {
                Gizmos.DrawWireSphere(p.calculatePosition(), 25);
            }
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
                            if(Vector3.Distance(u.transform.position, t.transform.position) > u.awarenessRange && !t.isInPoint && t != this)
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
                            if (Vector3.Distance(u.transform.position, t.transform.position) > u.awarenessRange && !t.isInPoint && t != this)
                            {
                                point.m_objects.Add(t.transform.gameObject);
                                t.isInPoint = true;
                            }
                        }
                        pointsOfInterest.Add(point);
                    }
                }
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
