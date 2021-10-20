using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBlueprint : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 9)))
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            transform.position = new Vector3(hit.rigidbody.position.x, 0.2f, hit.rigidbody.position.z);
        }
        else if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            transform.position = new Vector3(hit.point.x, 0.2f, hit.point.z);
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

    }
}
