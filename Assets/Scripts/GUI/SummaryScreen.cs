using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreen : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject statLedger;
    public GameObject statBarPrefab;

    private GameManager gm;
    private PlayerController pc;


    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private bool calculated = false;

    private void Update()
    {
        if (gm.GameState != GameManager.State.WINNER) { return; }

        if (Input.GetKeyDown(KeyCode.Escape))
            EscapeToggleMenu();
    

        if (!calculated) 
        { 
            Show();
            CalculateStats();
        }
        
    }

    private void Show()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void CalculateStats()
    {
        calculated = true;

        var sortedDict = from entry in gm.playerPrivateVP orderby entry.Value descending select entry;

        int rank = 1;
        foreach (KeyValuePair<int,int> entry in sortedDict)
        {
            GameObject statBar = Instantiate(statBarPrefab, statLedger.transform);
            statBar.GetComponent<StatBar>().SetLabels($"#{rank}", gm.playerNames[entry.Key], entry.Value);
            rank++;
        }
    }

    private void EscapeToggleMenu()
    {
        if (canvasGroup.interactable)
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
