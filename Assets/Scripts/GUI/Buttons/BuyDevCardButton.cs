using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyDevCardButton : MonoBehaviour
{
    private GameManager gm;
    private PlayerController pc;
    
    public void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {

        if (gm.GameState == GameManager.State.IDLE && pc.CanAffordDevCard() && PlayerController.playerIndex == gm.currentTurn)
            this.GetComponent<Button>().interactable = true;
        else
            this.GetComponent<Button>().interactable = false;
    }

    public void OnClick()
    {
        Debug.Log($"BuildDevCardButton: clicked by {PlayerController.playerIndex}");
        gm.CmdBuyDevCard(PlayerController.playerIndex);
    }
}
