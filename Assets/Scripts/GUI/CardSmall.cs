using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSmall : MonoBehaviour
{
    public int cardOwner;

    private GameManager gm;
    
    public Resource resource;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matBrick;
    public Material matGrain;
    public Material matOre;
    public Material matWood;
    public Material matWool;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        
    }

    public void SetResource(Resource res)
    {
        resource = res;
        Material mat = null;
        
        switch (res)
        {
            case Resource.None: mat = matCardback; break;
            case Resource.Wood: mat = matWood; break;
            case Resource.Wool: mat = matWool; break;
            case Resource.Brick : mat = matBrick; break;
            case Resource.Grain: mat = matGrain; break;
            case Resource.Ore: mat = matOre; break;
        }

        GetComponent<Image>().material = mat;
    }

    public void SelectCard()
    {
        // Set in inspector.
        // Only clickable when State.STEAL is true.
        if (gm.currentTurn == PlayerController.playerIndex && gm.GameState == GameManager.State.STEAL)
        {
            if (GameBoard.IsStealTarget(cardOwner))
                gm.CmdRequestStealRandom(cardOwner, PlayerController.playerIndex);
        }
    }
}
