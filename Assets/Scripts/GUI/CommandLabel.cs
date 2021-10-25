using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandLabel : MonoBehaviour
{
    private GameManager gm;
    private RawImage background;
    private Text label;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        background = GetComponent<RawImage>();
        label = GetComponentInChildren<Text>();

        // Hide on start, this label only appears when needed
        background.enabled = false;
        label.enabled = false;        
    }

    private void Show()
    {
        background.enabled = true;
        label.enabled = true; 
    }

    private void Hide()
    {
        background.enabled = false;
        label.enabled = false; 
    }

    private void Update()
    {
        if (gm.setup)
            Setup();
        else if (gm.GameState == GameManager.State.DISCARD)
            Discard();
        else if (gm.GameState == GameManager.State.ROBBER)
            Robber();
        else if (gm.GameState == GameManager.State.STEAL)
            Steal();
        else if (gm.GameState == GameManager.State.TRADE)
            Trade();
        else if (gm.GameState == GameManager.State.WINNER)
            Winner();
        else
            Hide();            
        
        
    }

    private void Setup()
    {
        Show();

        if (PlayerController.playerIndex == gm.currentTurn)
        {
            label.color = Color.red;
            label.text = "Place your starting buildings.";
        }
        else
        {
            label.color = Color.black;
            label.text = $"{gm.playerNames[gm.currentTurn]} is placing...";
        }
    }

    private void Discard()
    {
        Show();

        int stillToDiscard = gm.stillToDiscard[PlayerController.playerIndex];
        if (stillToDiscard > 0)
        {
            label.color = Color.red;
            background.enabled = true;
            label.enabled = true;

            if (stillToDiscard == 1)
                label.text = $"Discard 1 more card!";
            else
                label.text = $"Discard {stillToDiscard} more cards!";
        }
        else
        {
            label.color = Color.black;
            label.text = "Waiting on other players to discard...";
        }
    }

    private void Robber()
    {
        Show();

        if (PlayerController.playerIndex == gm.currentTurn)
        {
            label.color = Color.red;
            label.text = "Move the robber!";
        }
        else
        {
            label.color = Color.black;
            label.text = $"{gm.playerNames[gm.currentTurn]} is moving the robber...";
        }
    }

    private void Steal()
    {
        Show();

        if (PlayerController.playerIndex == gm.currentTurn)
        {
            label.color = Color.red;
            label.text = "Steal a card from an opponent!";
        }
        else
        {
            label.color = Color.black;
            label.text = $"{gm.playerNames[gm.currentTurn]} is stealing...";
        }
    }

    private void Trade()
    {
        Show();

        if (PlayerController.playerIndex != gm.currentTurn)
        {
            label.color = Color.red;
            label.text = $"{gm.playerNames[gm.currentTurn]} has offered a trade.";
        }
        else
        {
            label.color = Color.black;
            label.text = $"Waiting for offers...";
        }
    }

    private void Winner()
    {
        gm.CmdInterruptAudio();
        gm.CmdPlayAudio(12);
        
        Show();

        label.color = Color.red;
        label.fontSize = 40;
        label.text = $"{gm.playerNames[gm.currentTurn]} has won the game!";
        
 
    }
}
