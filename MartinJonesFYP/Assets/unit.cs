using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
