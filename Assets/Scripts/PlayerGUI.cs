using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerGUI : MonoBehaviour
{
    public GameController gc;

    // Set by player controller.
    public int playerIndex;
    public int playerCount;

    public GameObject cardPrefab;
    public GameObject cardSmallPrefab;

    public GameObject handZone;
    public GameObject enemyZoneGrid;
    public GameObject enemyZonePrefab;
    private Dictionary<int, GameObject> enemyZones;

    private void Start()
    {  
        StartCoroutine(WaitForPlayers());
    }

    // Wait until playerIndex is set (players have spawned)
    // before generating rest of GUI.
    private IEnumerator WaitForPlayers()
    {
        yield return new WaitUntil(() => playerIndex != 0 && playerCount != 0);

        // Set GameController.
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        // Using a dictionary so indexing can be non-continuous.
        // I.e. if you are player 2, you only have enemy zones for players 1 and 3.
        enemyZones = new Dictionary<int, GameObject>();

        // Iterate over every player.
        for (int i = 1; i <= playerCount; i++)
        {
            // Only make enemy zones for other players.
            if (i != playerIndex)
            {
                GameObject newEnemyZone = Instantiate(enemyZonePrefab, Vector3.zero, Quaternion.identity, enemyZoneGrid.transform);
                enemyZones[i] = newEnemyZone;

                newEnemyZone.transform.Find("PlayerLabel").gameObject.GetComponent<Text>().text = $"Player {i} ({gc.playerNames[i]})";
            }
        }
    }

    private void OnEnable()
    {
        GameController.handChanged += UpdateHands;
    }

    private void Update()
    {

    }

    private void UpdateHands(int targetPlayerIndex, List<Resource> resources)
    {
        if (targetPlayerIndex == playerIndex)
        {
            // The local player's hand was changed. Update the main hand zone.
            // First, clear existing hand zone.
            List<GameObject> allChildren = new List<GameObject>();

            foreach (Transform child in handZone.transform)
                allChildren.Add(child.gameObject);

            foreach (GameObject child in allChildren)
                DestroyImmediate(child);
            
            // Then create current number of cards.
            foreach (Resource res in resources)
            {
                GameObject newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, handZone.transform);
                newCard.GetComponent<Image>().material = ResourceToMaterial(res);
            }
            handZone.GetComponent<RectTransform>().sizeDelta = new Vector2 (resources.Count * 100 + (resources.Count+1) * 10 + 50, 150);

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

            // Then create current number of small cards (with hidden card backs).
            for (int i = 0; i < resources.Count; i++)
            {
                Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);

            }

        
        }
    }

    [Header("Card Materials")]
    public Material matCardback;
    public Material matBrick;
    public Material matGrain;
    public Material matOre;
    public Material matWood;
    public Material matWool;
    
    public Material ResourceToMaterial(Resource res)
    {
        switch (res)
        {
            case Resource.CardBack: return matCardback;
            case Resource.Brick: return matBrick;
            case Resource.Grain: return matGrain;
            case Resource.Ore: return matOre;
            case Resource.Wood: return matWood;
            case Resource.Wool: return matWool;
            
        }
        return null;
    }
}