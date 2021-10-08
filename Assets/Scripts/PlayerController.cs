using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerController : NetworkBehaviour
{    
    [SyncVar(hook = nameof(SetPlayerIndex))]
    public int playerIndex;

    [SyncVar(hook = nameof(UpdateCards))]
    public int numCards;


    public GameObject cardPrefab;
    public GameObject cardSmallPrefab;

    public GameObject canvas;
    public GameObject indexLabel;

    public GameObject handZone;
    public GameObject enemyZoneGrid;
    public GameObject enemyZonePrefab;
    private Dictionary<int, GameObject> enemyZones;
    

    void Start()
    {
        // Disable GUI of other clients and all on server.
        if (!isLocalPlayer) canvas.SetActive(false);

        // Enemy zones
        enemyZones = new Dictionary<int, GameObject>();

        int numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        for (int i = 1; i <= numPlayers; i++)
        {
            if (i == playerIndex) return;

            GameObject enemyZone = Instantiate(enemyZonePrefab, Vector3.zero, Quaternion.identity, enemyZoneGrid.transform);
            enemyZones[i] = enemyZone;
        }

    }

    // Set player label once playerIndex has been assigned
    void SetPlayerIndex(int oldVar, int newVar)
    {
        indexLabel.GetComponent<Text>().text = $"Player {playerIndex}";
    }



    // This SyncVarHook will update on each client, but on the objects for the same player.
    // e.g. for Player B: Client A's playerObject B, Client B's playerObject B, and Client C's playerObject B.
    // This is why if you try to do "A card was/was not dealt to me, I now have {numCards} cards",
    // you will get the same {numCards} on each Client -- all dealt to the various Player B's.
    [Client]
    void UpdateCards(int oldVar, int newVar)
    {
        if (isLocalPlayer)
        {
            // This was dealt to me, the LocalPlayer.
            // So we update my own hand
            Debug.Log($"Player {playerIndex}: A card was dealt to me, the LocalPlayer. I now have {numCards} cards.");

            Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, handZone.transform);
        }   
        else
        {
            // This was dealt to one of my clones on a remote client.
            // So we update my hand as one of that remote client's enemy hands
            Debug.Log($"Player {playerIndex}: A card was dealt to one of my clones on this remote client. I now have {numCards} cards.");

            Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyZones[playerIndex].transform.GetChild(0).transform);
        }
    }
}
