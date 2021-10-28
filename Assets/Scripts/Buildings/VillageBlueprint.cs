using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageBlueprint : MonoBehaviour
{
    [Range(0.0f, 10.0f)] public float heightAboveBoard;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 180f), 0f));
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 9)))
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            transform.position = new Vector3(hit.rigidbody.position.x, heightAboveBoard, hit.rigidbody.position.z);
        }
        else if (Physics.Raycast(ray, out hit, 50000.0f, (1 << 8)))
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            transform.position = new Vector3(hit.point.x, heightAboveBoard, hit.point.z);
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

    }

}
