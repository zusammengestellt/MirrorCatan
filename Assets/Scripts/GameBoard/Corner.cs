using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corner
{
    public Corner(Vector3 pos, int id)
    {
        this.position = pos;
        this.idNum = id;

        neighborHexes = new List<Hex>();
        neighborCorners = new List<Corner>();
        neighborPaths = new Dictionary<Corner,Path>();
    }

    [System.NonSerialized] public GameObject instance;
    
    public Corner corner;

    public Vector3 position;
    public List<Hex> neighborHexes;
    public List<Corner> neighborCorners;
    public Dictionary<Corner,Path> neighborPaths;

    public int idNum;
    public bool isHarbor = false;
    public Resource harborType;

    [System.NonSerialized] public bool owned = false;
    [System.NonSerialized] public int playerOwner = -1;
    [System.NonSerialized] public int devLevel = 0;

}