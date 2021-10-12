using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CornerComponent : NetworkBehaviour
{
    [System.NonSerialized] public Corner corner;
    
    [SyncVar] public bool owned;
    [SyncVar] public int playerOwner = -1;
    [SyncVar] public int devLevel = 0;

    


}
