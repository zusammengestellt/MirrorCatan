using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
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
        if (gm.GameState == GameManager.State.WINNER)
            this.GetComponent<Button>().interactable = false;

        if (gm.GameState == GameManager.State.IDLE && PlayerController.playerIndex == gm.currentTurn)
            this.GetComponent<Button>().interactable = true;
        else
            this.GetComponent<Button>().interactable = false;
        
        if (gm.GameState == GameManager.State.IDLE && PlayerController.playerIndex == gm.currentTurn)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pc.CmdRequestNextTurn();
            }
        }
    }

    public void OnClick()
    {
        pc.CmdRequestNextTurn();
    }

}
