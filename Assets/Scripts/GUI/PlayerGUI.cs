using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerGUI : MonoBehaviour
{  

    public GameObject nextTurnButton;

    public GameObject handZone;
    public GameObject enemyZoneGrid;
    public GameObject enemyZonePrefab;
    public GameObject dieRollZonePrefab;
    private Dictionary<int, GameObject> enemyZones;
    
    public TextMeshProUGUI messageBoard;

    private void Start()
    { 
        StartCoroutine(WaitForPlayers());

        GameObject dieRollZone = Instantiate(dieRollZonePrefab, this.transform);
    }

    void Update()
    {
        
    }

    // Wait until playerIndex is set (players have spawned)
    // before generating rest of GUI.
    private IEnumerator WaitForPlayers()
    {
        yield return new WaitUntil(() => PlayerController.playerIndex != 0 && GameManager.playerCount != 0);
        yield return null;

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
                newEnemyZone.GetComponent<EnemyZone>().forEnemyIndex = i;
                enemyZones[i] = newEnemyZone;

                newEnemyZone.transform.Find("PlayerLabel").gameObject.GetComponent<Text>().text = $"Player {i}";
            }
        }
       
    }

    
}