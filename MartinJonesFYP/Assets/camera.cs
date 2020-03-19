using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class camera : MonoBehaviour
{
	public float cameraSpeed;
	public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {
		float zTranslation = Input.GetAxis("Vertical") * cameraSpeed * Time.deltaTime;
		float xTranslation = Input.GetAxis("Horizontal") * cameraSpeed * Time.deltaTime;

		transform.Translate(xTranslation, 0, zTranslation, Space.World);

		if (Input.GetButtonDown("Fire1"))
		{
			RaycastHit hit;

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.green, 5);

            if (!Input.GetButton("function"))
            {
                GameObject[] obj = FindObjectsOfType<GameObject>();

                foreach (GameObject o in obj)
                {
                    if (o.GetComponent<unit>())
                    {
                        if (o.GetComponent<unit>().selected)
                        {
                            o.GetComponent<unit>().selected = false;
                        }
                    }
                }
            }

			if (Physics.Raycast(ray, out hit))
			{
				Debug.Log(hit.transform.gameObject.name);
				if(hit.transform.gameObject.GetComponent<unit>())
				{
                    if (hit.transform.gameObject.GetComponent<unit>().isPlayerUnit)
                    {
                        switch(hit.transform.gameObject.GetComponent<unit>().selected)
                        {
                            case true:
                                hit.transform.gameObject.GetComponent<unit>().selected = false;
                                break;
                            case false:
                                hit.transform.gameObject.GetComponent<unit>().selected = true;
                                break;
                            default:
                                Debug.Log("WARNING: unit has no selection value");
                                break;
                        }
                    }
				}
			}
			else
			{
				Debug.Log("did not select anything");
			}
		}
		else if (Input.GetButton("Fire2"))
		{
			RaycastHit hit;

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 15, Color.green, 5);

            if (Physics.Raycast(ray, out hit))
			{
				GameObject[] obj = FindObjectsOfType<GameObject>();

				foreach (GameObject o in obj)
				{
					if (o.GetComponent<unit>())
					{
						if (o.GetComponent<unit>().selected)
						{
                            if (hit.transform.gameObject.GetComponent<unit>())
                            {
                                if (hit.transform.gameObject.GetComponent<unit>().isPlayerUnit)
                                {
                                    o.GetComponent<unit>().state = unitState.assisting;
                                    o.GetComponent<unit>().target = hit.transform.gameObject;
                                }
                                else
                                {
                                    o.GetComponent<unit>().state = unitState.engaging;
                                    o.GetComponent<unit>().target = hit.transform.gameObject;
                                }
                            }
                            else
                            {
                                NavMeshPath path = new NavMeshPath();
                                o.GetComponent<NavMeshAgent>().CalculatePath(hit.point, path);
                                if (path.status == NavMeshPathStatus.PathComplete)
                                {
                                    o.GetComponent<NavMeshAgent>().destination = hit.point;
                                    o.GetComponent<NavMeshAgent>().isStopped = false;
                                    o.GetComponent<unit>().setPosition = hit.point;
                                    o.GetComponent<unit>().state = unitState.moving;
                                }
                            }
						}
					}
				}
			}            			
		}
    }
}
