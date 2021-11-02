using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DevCard : MonoBehaviour
{
    public Dev dev;

    private GameManager gm;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matKnight;
    public Material matMonopoly;
    public Material matPlenty;
    public Material matRoads;
    public Material matVPchapel;
    public Material matVPlibrary;
    public Material matVPmarket;
    public Material matVPpalace;

    private Vector2 startPosition;

    public static event Action<int, Dev> onPlayDevCard;
    public static event Action<int> onPlayDevCardEarly;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    public void SetDev(Dev newDev)
    {
        dev = newDev;
        Material mat = null;
        
        switch (dev)
        {
            case Dev.Knight: mat = matKnight; break;
            case Dev.Monopoly: mat = matMonopoly; break;
            case Dev.Plenty: mat = matPlenty; break;
            case Dev.Roads: mat = matRoads; break;
            case Dev.VP: mat = matVPpalace; break;
        }

        GetComponent<Image>().material = mat;
    }


    // Set in inspector for OnClick
    public void OnClick()
    {
        gm.CmdClearSelectedCards();
        
        if (gm.currentTurn == PlayerController.playerIndex && gm.GameState == GameManager.State.IDLE)
        {
            // Can't play on turn bought
            if (gm.playerDevCards[PlayerController.playerIndex].Where(d => d == dev).Count() > gm.devCardsBoughtThisTurn.Where(d => d == dev).Count())
                onPlayDevCard?.Invoke(PlayerController.playerIndex, dev);
            else if (dev == Dev.VP)
                onPlayDevCard?.Invoke(PlayerController.playerIndex, Dev.VP);
            else
                onPlayDevCardEarly.Invoke(PlayerController.playerIndex);


        }   
    }

    public GameObject tooltipCanvas;
    public GameObject devCard;
    public GameObject shownCard;

    private float cardWidth = 360f;
    private float cardHeight = 520f;

    // Set in inspector for MouseEnter
    public void TooltipOnEnter()
    {
        shownCard = Instantiate(devCard, new Vector3(Screen.width / 2, Screen.height / 2, 0f), Quaternion.identity, gm.tooltipCanvas.transform);
        shownCard.GetComponent<RectTransform>().sizeDelta = new Vector2(cardWidth, cardHeight);
    }
    
    // Set in inspector for MouseEnter
    public void TooltipOnExit()
    {
        if (shownCard != null)
            Destroy(shownCard);
    }
}
