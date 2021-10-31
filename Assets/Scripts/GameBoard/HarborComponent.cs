using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarborComponent : MonoBehaviour
{
    public Resource res;
    
    public GameObject iconAny;
    public GameObject iconWood;
    public GameObject iconBrick;
    public GameObject iconWool;
    public GameObject iconGrain;
    public GameObject iconOre;

    void Start()
    {


        
    }

    public void SetIcon(Resource resIn)
    {
        res = resIn;
    
        switch (res)
        {
            case Resource.None: iconAny.SetActive(true); break;
            case Resource.Wood: iconWood.SetActive(true); break;
            case Resource.Brick: iconBrick.SetActive(true); break;
            case Resource.Wool: iconWool.SetActive(true); break;
            case Resource.Grain: iconGrain.SetActive(true); break;
            case Resource.Ore: iconOre.SetActive(true); break;
        }
        
    }
}
