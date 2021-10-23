using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadComponent : MonoBehaviour
{
    RaycastHit hit;
    Vector3 movePoint;

    public Material defaultMat;
    public Material highlightMat;

    public void OnHighlight()
    {
        this.transform.parent.GetComponentInChildren<MeshRenderer>().material = highlightMat;
    }

    public void OnNoHighlight()
    {
        this.transform.parent.GetComponentInChildren<MeshRenderer>().material = defaultMat;
    }
}