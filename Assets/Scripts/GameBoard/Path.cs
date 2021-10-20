using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public Path(int id, Vector3 pos, Quaternion rot)
    {
        this.idNum = id;
        this.position = pos;
        this.rotation = rot;

        connectedPaths = new List<Path>();
    }

    [System.NonSerialized] public GameObject instance;

    public Path path;
    
    public int idNum;
    public Vector3 position;
    public Quaternion rotation;

    public List<Path> connectedPaths;
    public Corner myCorner;

    [System.NonSerialized] public bool owned = false;
    [System.NonSerialized] public int playerOwner = -1;
}