using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public Vector3 m_direction;
    public float m_damage;
    public bool m_isPlayerOwned;

    public void cleanup()
    {
        Destroy(transform.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke("cleanup", 5);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += m_direction;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponent<unit>())
        {
            if(collision.transform.GetComponent<unit>().isPlayerUnit != m_isPlayerOwned)
            {
                collision.transform.gameObject.GetComponent<unit>().health -= m_damage;
                Destroy(transform.gameObject);
            }
        }
    }
}
