using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public bool debugText = false;

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
            //GameObject.Find("Lobby").SetActive(false);
            instance = this;
            playerIndex = syncPlayerIndex;
        }

    }

    private void Update()
    {
        if (!isLocalPlayer) { return; }

        if (!debugText) { return; }

        if (GameBoard.CornerUnderMouse() != null)
        {
            Corner c = GameBoard.CornerUnderMouse().GetComponent<CornerComponent>().corner;
        }
        else if (GameBoard.HexUnderMouse() != null)
        {
            Hex h = GameBoard.HexUnderMouse().GetComponent<HexComponent>().hex;
        }
    }


    [Client]
    public bool CanAffordRoad()
    {
        int woodCount = 0;
        int brickCount = 0;
 

        foreach (Resource res in gm.playerResources[playerIndex])
        {
            switch (res)
            {
                case Resource.Wood:
                    woodCount++;
                    break;
                case Resource.Brick:
                    brickCount++;
                    break;
            }
        }

        if (woodCount >= 1 && brickCount >= 1)
            return true;
        else
            return false;
    }

    [Client]
    public bool CanAffordVillage()
    {
        int woodCount = 0;
        int brickCount = 0;
        int woolCount = 0;
        int grainCount = 0;

        foreach (Resource res in gm.playerResources[playerIndex])
        {
            switch (res)
            {
                case Resource.Wood:
                    woodCount++;
                    break;
                case Resource.Brick:
                    brickCount++;
                    break;
                case Resource.Wool:
                    woolCount++;
                    break;
                case Resource.Grain:
                    grainCount++;
                    break;
            }
        }

        if (woodCount >= 1 && brickCount >= 1 && woolCount >= 1 && grainCount >= 1)
            return true;
        else
            return false;
    }

    [Client]
    public bool CanAffordCity()
    {  
        int grainCount = 0;
        int oreCount = 0;

        foreach (Resource res in gm.playerResources[playerIndex])
        {
            switch (res)
            {
                case Resource.Grain:
                    grainCount++;
                    break;
                case Resource.Ore:
                    oreCount++;
                    break;
            }
        }

        if (grainCount >= 2 && oreCount >= 3)
            return true;
        else
            return false;
    }

    [Client]
    public bool CanAffordDevCard()
    {  
        int woolCount = 0;
        int grainCount = 0;
        int oreCount = 0;

        foreach (Resource res in gm.playerResources[playerIndex])
        {
            switch (res)
            {
                case Resource.Wool:
                    woolCount++;
                    break;
                case Resource.Grain:
                    grainCount++;
                    break;
                case Resource.Ore:
                    oreCount++;
                    break;
            }
        }

        if (woolCount >= 1 && grainCount >= 1 && oreCount >= 1)
            return true;
        else
            return false;
    }
    

    [Command]
    public void CmdRequestNextTurn(int requestor)
    {
        gm.RequestNextTurn(requestor);
    }





}