using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PathComponent : NetworkBehaviour
{
    [System.NonSerialized] public Path path;

    [SyncVar] public bool owned;
    [SyncVar] public int playerOwner = -1;

}