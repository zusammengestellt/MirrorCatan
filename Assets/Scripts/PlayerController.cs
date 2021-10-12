using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public PlayerController instance;

    private GameManager gm;
    private GameBoard gb;
    
    [SyncVar] public int syncPlayerIndex;
    public static int playerIndex;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        if (!isLocalPlayer)
        {
            this.gameObject.SetActive(false);
        }

        if (isLocalPlayer)
        {
            instance = this;
            playerIndex = syncPlayerIndex;
        }
        
    }

    private void Update()
    {
        if (!isClient) { return; }

        if (GameBoard.HexUnderMouse() != null)
        {
            Debug.Log($"{GameBoard.hexes[0].Q},{GameBoard.hexes[0].R}");
            //Hex h = gb.hexes[HexUnderMouse().GetComponent<HexComponent>().id];
            //Debug.Log($"{h.Q}, {h.R}");
        }

    }

    [Command]
    public void CmdRequestNextTurn(int requestor)
    {
        Debug.Log($"next turn requested by {requestor}");
        gm.RequestNextTurn(requestor);
    }

    [Command]
    public void CmdFinishRoll(int result) => gm.RequestFinishRoll(result);




}