using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinMenu : MonoBehaviour
{
    public Dropdown coinDropdown;
    public Button coinExchangeButton;
    public CanvasGroup canvasGroup;

    private GameManager gm;
    private Resource selectedResource = Resource.Wood;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        // Add listener for coinDropdown.
        coinDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(coinDropdown);
        });
    }

    private void Update()
    {
        if (gm.GameState == GameManager.State.WINNER)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
        else if (gm.GameState == GameManager.State.IDLE && gm.currentTurn == PlayerController.playerIndex && gm.playerCoins[PlayerController.playerIndex] >= 5)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }

    private void DropdownValueChanged(Dropdown change)
    {
        // Find selected resource.
        switch(change.value)
        {
            case 0: selectedResource = Resource.Wood; break;
            case 1: selectedResource = Resource.Brick; break;
            case 2: selectedResource = Resource.Wool; break;
            case 3: selectedResource = Resource.Grain; break;
            case 4: selectedResource = Resource.Ore; break;
        }
    }

    public void OnExchangeButton()
    {
        if (gm.playerCoins[PlayerController.playerIndex] < 5) { return; }

        gm.CmdPlayAudio(UnityEngine.Random.Range(7,10));

        gm.CmdAddResource(PlayerController.playerIndex, selectedResource);
        gm.CmdRemoveCoins(PlayerController.playerIndex, 5);

        gm.CmdIncomeAnimation(PlayerController.playerIndex, new List<Resource>(){selectedResource}, false);
        gm.CmdLossAnimation(PlayerController.playerIndex, new List<Resource>(), true);
    }

}
