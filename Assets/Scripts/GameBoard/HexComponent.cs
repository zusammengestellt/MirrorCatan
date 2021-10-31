using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HexComponent : NetworkBehaviour
{
    public Hex hex;
    
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
    
    // Hex Selector 2.0
    public GameObject HexSelector2;
    

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

    private void Start()
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

    private float flickerRate = 0.5f;
    public void FlickerHexSelector()
    {        
        StartCoroutine(FlickerHexSelectorCoroutine());
    }

    private IEnumerator FlickerHexSelectorCoroutine()
    {
        GameObject hexSelector = Instantiate(HexSelector2, transform.position, Quaternion.identity);
        LineRenderer lr = hexSelector.GetComponent<LineRenderer>();
        Vector3[] vertices = hex.vertices;

        for (int i = 0; i < vertices.Length; i++)
            lr.SetPosition(i, vertices[i] + new Vector3(0f, 3f, 0f));
        lr.SetPosition(vertices.Length, vertices[0] + new Vector3(0f, 3f, 0f));
    
        bool active = true;
        for (int i = 0; i < 5; i++)
        {    
            yield return new WaitForSeconds(flickerRate);
            active = !active;
            hexSelector.SetActive(active);
        }
        Destroy(hexSelector);
    }


}
