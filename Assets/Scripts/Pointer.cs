using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Pointer : NetworkBehaviour
{
    [SyncVar] public int owner;
    public GameManager gm;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (isServer) { return; }

        if (PlayerController.playerIndex != owner) { return; }

        GetComponent<MeshRenderer>().material = gm.GetPlayerMaterial(owner, true);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            GetComponent<MeshRenderer>().enabled = true;
            
            CmdUpdateServerPos(GameBoard.HexFreePositionUnderMouse());
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateServerPos(Vector3 pos)
    {
        transform.position = pos;
    }

    [ClientRpc]
    public void RpcUpdateServerPos(Vector3 pos)
    {
        transform.position = pos;
    }
    
}
