using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour
{
    private PlayerController pc;

    public void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        GameManager.onNextTurn += UpdateInteractable;
    }

    private void UpdateInteractable(int currentTurn)
    {
        if (PlayerController.playerIndex == currentTurn)
            this.GetComponent<Button>().interactable = true;
        else
            this.GetComponent<Button>().interactable = false;
    }

    public void OnClick()
    {
        Debug.Log($"clicked by {PlayerController.playerIndex}");
        pc.CmdRequestNextTurn(PlayerController.playerIndex);
    }

}
