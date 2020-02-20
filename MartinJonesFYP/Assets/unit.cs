using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum unitState
{
    idle = 0,
    moving = 1,
    engaging = 2
}


public class unit : MonoBehaviour
{
	public bool selected = false;
    public bool isPlayerUnit = true;
    public GameObject target = null;
    public int awarenessRange;
    public Vector3 setPosition;
    public unitState state;
    // Start is called before the first frame update
    void Start()
    {
        setPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<NavMeshAgent>().remainingDistance < 0.1f)
        {
            state = unitState.idle;
        }

        GameObject[] obj = FindObjectsOfType<GameObject>();

        foreach(GameObject o in obj)
        {
            if(o.GetComponent<unit>())
            {
                
            }
        }
    }
}
