using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HexComponent : NetworkBehaviour
{
    public int id;
    public int roll;
    
    public Resource resource = Resource.None;

    public Material DesertMat;
    public Material ForestMat;
    public Material PastureMat;
    public Material FieldMat;
    public Material HillMat;
    public Material MountainMat;

    public GameObject HexSelector;

    public override void OnStartServer()
    {
        // Remove client-only components.
        GameObject.Destroy(GetComponent<MeshFilter>());
        GameObject.Destroy(GetComponent<MeshRenderer>());
        GameObject.Destroy(GetComponent<MeshCollider>());
    }

    public override void OnStartClient()
    {
        HexSelector.SetActive(false);
    }

    void Update()
    {
 
    }

    public void ChangeResource(Resource res)
    {            
        resource = res;
         
        Material mat = null;
        switch (resource)
        {
            case Resource.None: mat = DesertMat; break;
            case Resource.Wood: mat = ForestMat; break;
            case Resource.Wool: mat = PastureMat; break;
            case Resource.Grain: mat = FieldMat; break;
            case Resource.Brick: mat = HillMat; break;
            case Resource.Ore: mat = MountainMat; break;
        }
        GetComponentInParent<MeshRenderer>().material = mat;
    }


}
