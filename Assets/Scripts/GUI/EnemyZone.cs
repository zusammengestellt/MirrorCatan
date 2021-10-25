using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyZone : MonoBehaviour
{
    public GameManager gm;

    public GameObject playerLabel;

    public GameObject cardSmallPrefab;
    public GameObject enemyHandZone;

    public GameObject devCardSmallPrefab;
    public GameObject enemyDevCardZone;

    public GameObject specialCardZone;

    public GameObject acceptTradeButton;


    public int forEnemyIndex;

    [SerializeField] private int defaultCardSmallSpacing;
    [SerializeField] private int enemyHandZoneMaxWidth;
    private float cardSmallWidth;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        cardSmallWidth = cardSmallPrefab.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void OnEnable()
    {
        GameManager.onHandChanged += UpdateEnemyHandZone;
        GameManager.onDevCardChanged += UpdateEnemyDevCardZone;
    }


    float buffer = 1.0f;

    private void Update()
    {
        //if (playerLabel.GetComponent<Text>().text == null)
        playerLabel.GetComponent<Text>().text = gm.playerNames[forEnemyIndex];

        if (gm.currentTurn == forEnemyIndex)
        {
            playerLabel.GetComponent<Text>().fontStyle = FontStyle.Bold;

        }
        else
        {
            playerLabel.GetComponent<Text>().fontStyle = FontStyle.Normal;
        }

        if (gm.currentTurn == PlayerController.playerIndex && gm.playerOfferingTrade[forEnemyIndex])
        {
            acceptTradeButton.SetActive(true);
        }
        else
        {
            acceptTradeButton.SetActive(false);
        }

        // Don't redraw every single frame.
        if (Time.deltaTime < buffer)
        {
            buffer -= 0.05f;
        }
        else
        {
            RefreshEnemyZone();
            buffer = 1.0f;
        }

    }

    private void RefreshEnemyZone()
    {
        List<Resource> resources = gm.playerResources[forEnemyIndex];
        List<Resource> selectedCards = gm.playerSelectedCards[forEnemyIndex];

        // First, identify and clear the enemy hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in enemyHandZone.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);

        // Reveal if trade state and current player initiated trade OR if trade state and other player is offering trade
        if (gm.GameState == GameManager.State.TRADE && (gm.currentTurn == forEnemyIndex || gm.playerOfferingTrade[forEnemyIndex]))
        {
            // Instantiate X revealed selectedCards
            foreach (Resource res in selectedCards)
            {

                GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
                card.GetComponent<CardSmall>().SetResource(res);
                card.GetComponent<CardSmall>().cardOwner = forEnemyIndex;
            }

            // Then gen the remainder as normal (hidden)
            for (int i = 0; i < resources.Count - selectedCards.Count; i++)
            {
                GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
                card.GetComponent<CardSmall>().cardOwner = forEnemyIndex;
            }
        }
        else
        {
            // Show cardbacks.
            for (int i = 0; i < resources.Count; i++)
            {
                GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
                card.GetComponent<CardSmall>().cardOwner = forEnemyIndex;
            }
        }

        // Dynamically adjust spacing between cards
        float spacing = (enemyHandZoneMaxWidth - (resources.Count * cardSmallWidth)) / (resources.Count + 1);
        enemyHandZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSmallSpacing, spacing), 0);
    }

    private void UpdateEnemyHandZone(int targetPlayerIndex, List<Resource> resources)
    {
        /*

        if (targetPlayerIndex != forEnemyIndex) { return; }

        // Another player's hand was changed. Update the correct enemy zone.
        // First, identify and clear the correct enemy hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in enemyHandZone.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);

        // Show cardbacks.
        for (int i = 0; i < resources.Count; i++)
        {
            GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
            card.GetComponent<CardSmall>().cardOwner = targetPlayerIndex;
        }

        // Dynamically adjust spacing between cards
        float spacing = (enemyHandZoneMaxWidth - (resources.Count * cardSmallWidth)) / (resources.Count + 1);
        enemyHandZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSmallSpacing, spacing), 0);

        */
    }

    private void UpdateEnemyDevCardZone(int targetPlayerIndex, List<Dev> devCards)
    {
        if (targetPlayerIndex != forEnemyIndex) { return; }

        // Another player's hand was changed. Update the correct enemy zone.
        // First, identify and clear the correct enemy hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in enemyDevCardZone.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);

        // Sort dev cards so they appear together.
        List<Dev> devCardsSorted = new List<Dev>();
        for (int i = 0; i < GameManager.devCardSortOrder.Length; i++)
        {
            foreach (Dev dev in devCards)
                if (dev == GameManager.devCardSortOrder[i])
                    devCardsSorted.Add(dev);
        }
        devCards = devCardsSorted;

        // Instantiate the current number of devSmallCards (w/ hidden card backs).
        for (int i = 0; i < devCards.Count; i++)
        {
            GameObject card = Instantiate(devCardSmallPrefab, Vector3.zero, Quaternion.identity, enemyDevCardZone.transform);
            card.GetComponent<DevCardSmall>().cardOwner = targetPlayerIndex;
            card.GetComponent<DevCardSmall>().UpdateMat(devCards[i]);
        }

        // Dynamically adjust spacing between cards
        float spacing = (enemyHandZoneMaxWidth - (devCards.Count * cardSmallWidth)) / (devCards.Count + 1);
        enemyDevCardZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSmallSpacing, spacing), 0);

    }


    
    private void UpdateSelectedCards(int targetPlayer)
    {
        /*
        List<Resource> resources = gm.playerResources[targetPlayer];
        List<Resource> selectedCards = gm.playerSelectedCards[targetPlayer];

        // Instantiate X revealed selectedCards
        foreach (Resource res in selectedCards)
        {
            Debug.Log("selectedCards running");

            GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
            card.GetComponent<CardSmall>().SetResource(res);
            card.GetComponent<CardSmall>().cardOwner = targetPlayer;
        }

        // Then gen the remainder as normal (hidden)
        for (int i = 0; i < resources.Count - selectedCards.Count; i++)
        {
            GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
            card.GetComponent<CardSmall>().cardOwner = targetPlayer;
        }
        */
    }

    public void OnAcceptTrade()
    {
        gm.CmdPlayAudio(UnityEngine.Random.Range(7,10));
        gm.CmdProcessTrade(forEnemyIndex);
    }

 
}
