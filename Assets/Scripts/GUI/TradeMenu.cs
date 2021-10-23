using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour
{
    public Dropdown tradeDropdown;
    public Button tradeExchangeButton;
    public Text tradeExchangeButtonText;
    public Dictionary<Resource, int> exchangeRates = new Dictionary<Resource, int>();
    private Resource selectedResource = Resource.Wood;  // default is wood

    public Button tradeOfferButton;
    public Text tradeOfferButtonText;
    private bool offeredTrade = false;

    public Button tradeAcceptButton;

    private GameManager gm;
    private PlayerController pc;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        // Add listener for tradeDropdown.
        tradeDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(tradeDropdown);
        });
    }


    private void Update()
    {
        exchangeRates = UpdateExchangeRates();

        if (gm.GameState == GameManager.State.WINNER)
            this.gameObject.SetActive(false);


        // Trade Exchange button is available during IDLE if player has selected
        // X resources of the same type (where X is the exchange rate)
        if (gm.GameState == GameManager.State.IDLE && gm.currentTurn == PlayerController.playerIndex)
        {
            bool allMatching = true;
            Resource lastRes = Resource.None;


            foreach (Resource res in gm.playerSelectedCards[PlayerController.playerIndex])
            {
                if (lastRes != Resource.None && res != lastRes)
                    allMatching = false;
                lastRes = res;
            }

            if (allMatching)
                if (gm.playerSelectedCards[PlayerController.playerIndex].Count(res => res == lastRes) >= exchangeRates[lastRes])
                {
                    tradeExchangeButton.interactable = true;
                    
                    // Set exchange rate text.
                    tradeExchangeButtonText.text = $"{exchangeRates[lastRes]}:1 for";
                }
                else
                    tradeExchangeButton.interactable = false;
            else
                tradeExchangeButton.interactable = false;

    
        }
        else
        {
            tradeExchangeButton.interactable = false;
        }

        // Trade Dropdown is available when the dropdown is.
        tradeDropdown.interactable = tradeExchangeButton.interactable;
    
        
        // Trade Offer buttons (current player)
        if (gm.currentTurn == PlayerController.playerIndex)
        {
            if (gm.GameState == GameManager.State.TRADE)
            {
                tradeOfferButtonText.text = "Cancel Trade";
                tradeOfferButton.interactable = true;
            }
            else if (gm.GameState == GameManager.State.IDLE && gm.playerSelectedCards[PlayerController.playerIndex].Count > 0)
            {
                tradeOfferButtonText.text = "Offer Trade";
                tradeOfferButton.interactable = true;
            }
            else
            {
                tradeOfferButtonText.text = "Offer Trade";
                tradeOfferButton.interactable = false;

            }
        }

        // Trade offer buttons (other players)
        if (gm.currentTurn != PlayerController.playerIndex)
        {
            if (gm.GameState == GameManager.State.TRADE && offeredTrade)
            {
                tradeOfferButtonText.text = "Rescind Offer";
                tradeOfferButton.interactable = true;
            }
            else if (gm.GameState == GameManager.State.TRADE && gm.playerSelectedCards[PlayerController.playerIndex].Count > 0)
            {
                tradeOfferButtonText.text = "Offer Trade";
                tradeOfferButton.interactable = true;
            }
            else
            {
                tradeOfferButtonText.text = "Offer Trade";
                tradeOfferButton.interactable = false;
                offeredTrade = false;
            }

        }
        

    }

    private Dictionary<Resource, int> UpdateExchangeRates()
    {
        Dictionary<Resource, int> exchangeRates = new Dictionary<Resource, int>();
        Resource[] resourceValues = (Resource[])Enum.GetValues(typeof(Resource));

        int defaultExchange = 4;

        // Find any 3:1 ("any") harbors.
        if (GameBoard.corners.Where(c => c.isHarbor && c.playerOwner == PlayerController.playerIndex && c.harborType == Resource.None).ToList().Count > 0)
            defaultExchange = 3;

        // Assign default exchange rates.
        for (int i = 0; i < resourceValues.Length; i++)
        {
            Resource res = resourceValues[i];
            exchangeRates[res] = defaultExchange;
        }

        // Find and assign any 2:1 harbors.
        foreach (Corner c in GameBoard.corners.Where(c => c.isHarbor && c.playerOwner == PlayerController.playerIndex && c.harborType != Resource.None))
            exchangeRates[c.harborType] = 2;

        return exchangeRates;
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

    public void OnClickExchangeButton()
    {
        int quantity = exchangeRates[selectedResource];
        int i = 0;

        Debug.Log($"quantity: {quantity}, exchangerate: {exchangeRates[selectedResource]}, selectedCards: {gm.playerSelectedCards[PlayerController.playerIndex].Count}");

        foreach (Resource res in gm.playerSelectedCards[PlayerController.playerIndex])
        {
            // Player can have 4 wool, click 3:1, and still have 1 wool leftover.
            if (i < quantity)
            {
                Debug.Log($"removing resource {res} from {PlayerController.playerIndex}");
                gm.CmdRemoveResource(PlayerController.playerIndex, res);
            }
            i++;
        }
            
        //gm.playerSelectedCards[PlayerController.playerIndex].Clear();
        gm.CmdClearSelectedCards();

        gm.CmdAddResource(PlayerController.playerIndex, selectedResource);
    }

    public void OnClickOfferTrade()
    {
        // currentTurn player controls trade state
        if (gm.currentTurn == PlayerController.playerIndex)
        {
            if (gm.GameState == GameManager.State.IDLE)
            {
                gm.CmdEnterTradeState(PlayerController.playerIndex);
            }
            else
            {
                for (int i = 1; i <= GameManager.playerCount; i++)
                    gm.CmdOfferTrade(i, false);
                gm.CmdExitTradeState();
            }
        }

        // other players
        if (gm.currentTurn != PlayerController.playerIndex)
        {
            if (!offeredTrade)
            {
                offeredTrade = true;
                gm.CmdOfferTrade(PlayerController.playerIndex, true);
            }
            else
            {
                offeredTrade = false;
                gm.CmdOfferTrade(PlayerController.playerIndex, false);
            }
            
        }   
    }

}
