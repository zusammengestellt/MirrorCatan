using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSmall : MonoBehaviour
{
    public int cardOwner;

    private GameManager gm;
    
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    void Update()
    {
        
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
