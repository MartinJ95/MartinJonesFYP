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
		Debug.Log(this.transform.rotation);
		Debug.Log(transform.forward);
    }

    // Update is called once per frame
    void Update()
    {
		float zTranslation = Input.GetAxis("Vertical") * cameraSpeed * Time.deltaTime;
		float xTranslation = Input.GetAxis("Horizontal") * cameraSpeed * Time.deltaTime;

		transform.Translate(xTranslation, 0, zTranslation, Space.World);

		if (Input.GetButton("Fire1"))
		{
			RaycastHit hit;

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				if(hit.transform.gameObject.GetComponent<unit>())
				{
					hit.transform.gameObject.GetComponent<unit>().selected = true;
				}
			}
		}
		else if (Input.GetButton("Fire2"))
		{
			RaycastHit hit;

			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out hit))
			{
				GameObject[] obj = FindObjectsOfType<GameObject>();

				foreach (GameObject o in obj)
				{
					if (o.GetComponent<unit>())
					{
						if (o.GetComponent<unit>().selected)
						{
							o.GetComponent<NavMeshAgent>().destination = hit.point;
						}
					}
				}
			}

			
		}
    }
}
