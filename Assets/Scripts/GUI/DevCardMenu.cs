using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevCardMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public Text label;
    public GameObject devDropdownObject;
    public Dropdown devDropdown;
    public Button cancelButton;
    public Button playButton;
    public GameObject playButtonObject;
    
    private Dev devCardToPlay;
    private Resource selectedResource = Resource.Wood;

    private GameManager gm;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        
        // Add listener for devDropdown.
        devDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(devDropdown);
        });
        
        DisableDevMenu();
    }

    public void EnableDevMenu(Dev dev)
    {
        canvasGroup.interactable = true;

        devDropdownObject.SetActive(true);
        playButtonObject.SetActive(true);
        canvasGroup.alpha = 1.0f;

        devCardToPlay = dev;

        switch (devCardToPlay)
        {
            case Dev.VP:
                devDropdownObject.SetActive(false);
                playButtonObject.SetActive(false);
                label.text = "Victory Point cards are included in your score and cannot be played.";
                break;

            case Dev.Knight:
                devDropdownObject.SetActive(false);
                label.text = "Play a Knight to move the Robber.";
                break;

            case Dev.Roads:
                devDropdownObject.SetActive(false);
                label.text = "Play Road Building to build two free roads.";
                break;
            
            case Dev.Monopoly:
                devDropdownObject.SetActive(true);
                label.text = "Select a resource to steal every copy owned by other players.";
                break;

            case Dev.Plenty:
                devDropdownObject.SetActive(true);
                label.text = "Select a resource to automatically gain two from the bank.";
                break;
        }
    }

    public void EnableDevMenuEarly()
    {
        canvasGroup.interactable = true;

        devDropdownObject.SetActive(false);
        playButtonObject.SetActive(false);
        canvasGroup.alpha = 1.0f;

        label.text = "You cannot play a development card on the same turn you bought it.";

    }

    public void DisableDevMenu()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0.0f;
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
        Debug.Log($"Selected resource is now: {selectedResource}");
    }

    public void PlayButton()
    {
        DisableDevMenu();
        gm.CmdRemoveDevCard(PlayerController.playerIndex, devCardToPlay);

        switch (devCardToPlay)
        {
            case Dev.Knight:
                gm.CmdAddDevCard(PlayerController.playerIndex, Dev.KnightRevealed);
                gm.CmdPlayKnight(PlayerController.playerIndex);
                break;

            case Dev.Monopoly:
                gm.CmdProcessMonopoly(PlayerController.playerIndex, selectedResource);
                break;
            
            case Dev.Plenty:
                gm.CmdProcessYearOfPlenty(PlayerController.playerIndex, selectedResource);
                break;

            case Dev.Roads:
                gm.CmdProcessRoadBuilding(PlayerController.playerIndex);
                break;

        }
    }



}
