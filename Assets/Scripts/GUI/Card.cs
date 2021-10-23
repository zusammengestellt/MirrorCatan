using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private GameManager gm;
    public GameObject selector;

    public Resource resource;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matBrick;
    public Material matGrain;
    public Material matOre;
    public Material matWood;
    public Material matWool;

    private Vector3 startPosition = Vector3.zero;

    private void Awake()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    public void SetResource(Resource res)
    {
        resource = res;
        Material mat = null;
        
        switch (res)
        {
            case Resource.Brick: mat = matBrick; break;
            case Resource.Grain: mat = matGrain; break;
            case Resource.Ore: mat = matOre; break;
            case Resource.Wood: mat = matWood; break;
            case Resource.Wool: mat = matWool; break;
        }

        GetComponent<Image>().material = mat;
    }

    // Set in inspector for OnClick
    public void RobberDiscard()
    {
        if (gm.stillToDiscard[PlayerController.playerIndex] > 0)
        {
            gm.CmdDiscarded(PlayerController.playerIndex, resource);
            GameObject.Destroy(this);

        }
    }

    GameManager.State lastState = GameManager.State.IDLE;

    private void Update()
    {
        // Reset on leave trade state
        if (lastState == GameManager.State.TRADE && gm.GameState == GameManager.State.IDLE)
        {
            gm.CmdClearSelectedCards();
            selector.GetComponent<RawImage>().enabled = false;
            
            if (startPosition != Vector3.zero)
                transform.position = startPosition;
        }

        lastState = gm.GameState;
    }

    // Set in inspector for OnClick
    public void SelectCard()
    {
       // Handle other players' select mechanics SEPARATELY to avoid issues with playerSelectedCards
       if (gm.currentTurn == PlayerController.playerIndex && gm.GameState == GameManager.State.IDLE)
       {

            if (!selector.GetComponent<RawImage>().enabled)
            {
                // Add card to playerSelectedCards.
                gm.CmdAddSelectedCard(PlayerController.playerIndex, resource);

                // Activate selector.
                startPosition = transform.position;

                selector.GetComponent<RawImage>().enabled = true;
                transform.Translate(new Vector3(0f, 10f, 0f));
            }
            else
            {
                // Remove card from playerSelected cards.
                gm.CmdRemoveSelectedCard(PlayerController.playerIndex, resource);

                // Deactivate selector.
                selector.GetComponent<RawImage>().enabled = false;
                transform.position = startPosition;

            }

       }
       else if (gm.currentTurn != PlayerController.playerIndex && gm.GameState == GameManager.State.TRADE && !gm.playerOfferingTrade[PlayerController.playerIndex])
       {
           if (!selector.GetComponent<RawImage>().enabled)
            {
                // Add card to playerSelectedCards.
                gm.CmdAddSelectedCard(PlayerController.playerIndex, resource);

                // Activate selector.
                startPosition = transform.position;

                selector.GetComponent<RawImage>().enabled = true;
                transform.Translate(new Vector3(0f, 10f, 0f));
            }
            else
            {
                // Remove card from playerSelected cards.
                gm.CmdRemoveSelectedCard(PlayerController.playerIndex, resource);

                // Deactivate selector.
                selector.GetComponent<RawImage>().enabled = false;
                transform.position = startPosition;

            }
       }
       else if (gm.currentTurn != PlayerController.playerIndex && gm.GameState == GameManager.State.TRADE && startPosition != Vector3.zero && !gm.playerOfferingTrade[PlayerController.playerIndex])
       {
            selector.GetComponent<RawImage>().enabled = false;
            transform.position = startPosition;
       }

        
    }
}
