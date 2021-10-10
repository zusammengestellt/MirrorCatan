using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerGUI : MonoBehaviour
{
    // GUI scale variables
    [SerializeField] int defaultCardSpacing;
    [SerializeField] int defaultCardSmallSpacing;
    [SerializeField] int handZoneMaxWidth;
    [SerializeField] int enemyHandZoneMaxWidth;

    private float cardWidth;
    private float cardSmallWidth;

    public GameObject cardPrefab;
    public GameObject cardSmallPrefab;

    public GameObject nextTurnButton;

    public GameObject handZone;
    public GameObject enemyZoneGrid;
    public GameObject enemyZonePrefab;
    private Dictionary<int, GameObject> enemyZones;
    
    private void OnEnable()
    {
        GameManager.onHandChanged += UpdateHands;
    }

    private void Start()
    { 
        StartCoroutine(WaitForPlayers());
    }

    // Wait until playerIndex is set (players have spawned)
    // before generating rest of GUI.
    private IEnumerator WaitForPlayers()
    {
        yield return new WaitUntil(() => PlayerController.playerIndex != 0 && GameManager.playerCount != 0);

        // Set GUI size variables.
        cardWidth = cardPrefab.GetComponent<RectTransform>().sizeDelta.x;
        cardSmallWidth = cardSmallPrefab.GetComponent<RectTransform>().sizeDelta.x;

        // Using a dictionary so indexing can be non-continuous.
        // I.e. if you are player 2, you only have enemy zones for players 1 and 3.
        enemyZones = new Dictionary<int, GameObject>();

        // Iterate over every player.
        for (int i = 1; i <= GameManager.playerCount; i++)
        {
            // Only make enemy zones for other players.
            if (i != PlayerController.playerIndex)
            {
                GameObject newEnemyZone = Instantiate(enemyZonePrefab, Vector3.zero, Quaternion.identity, enemyZoneGrid.transform);
                enemyZones[i] = newEnemyZone;

                newEnemyZone.transform.Find("PlayerLabel").gameObject.GetComponent<Text>().text = $"Player {i}";
            }
        }

        
    }

    private void Update()
    {

    }

    private void UpdateHands(int targetPlayerIndex, List<Resource> resources)
    {
        if (targetPlayerIndex == PlayerController.playerIndex)
        {
            // The local player's hand was changed. Update the main hand zone.
            // First, clear existing hand zone.
            List<GameObject> allChildren = new List<GameObject>();

            foreach (Transform child in handZone.transform)
                allChildren.Add(child.gameObject);

            foreach (GameObject child in allChildren)
                DestroyImmediate(child);
            
            // Sort matching resources so they appear together.
            List<Resource> resourcesSorted = new List<Resource>();
            for (int i = 0; i < GameManager.resourceSortOrder.Length; i++)
            {
                foreach (Resource res in resources)
                    if (res == GameManager.resourceSortOrder[i])
                        resourcesSorted.Add(res);
            }

            // Dynamically adjust spacing between cards and zone background width.
            float spacing = (handZoneMaxWidth - (resources.Count * cardWidth)) / (resources.Count + 1);
            handZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSpacing, spacing), 0);
            handZone.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Min(handZoneMaxWidth, resources.Count * cardWidth + (resources.Count) * spacing + 10), handZone.GetComponent<RectTransform>().sizeDelta.y);

            // Instantiate the cards.
            foreach (Resource res in resourcesSorted)
            {
                GameObject newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, handZone.transform);
                newCard.GetComponent<Card>().SetResource(res);
            }

            
        }
        else
        {
            // Another player's hand was changed. Update the correct enemy zone.
            // First, identify and clear the correct enemy hand zone.
            GameObject enemyHandZone = enemyZones[targetPlayerIndex].transform.Find("EnemyHandZone").gameObject;

            List<GameObject> allChildren = new List<GameObject>();

            foreach (Transform child in enemyHandZone.transform)
                allChildren.Add(child.gameObject);

            foreach (GameObject child in allChildren)
                DestroyImmediate(child);

            // Instantiate the current number of smallCards (w/ hidden card backs).
            for (int i = 0; i < resources.Count; i++)
            {
                Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
            }

            // Dynamically adjust spacing between cards
            float spacing = (enemyHandZoneMaxWidth - (resources.Count * cardSmallWidth)) / (resources.Count + 1);
            enemyHandZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSmallSpacing, spacing), 0);
        
        }
    }


    
}