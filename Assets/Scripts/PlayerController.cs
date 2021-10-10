using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public GameManager gm;
    
    [SyncVar] public int syncPlayerIndex;
    public static int playerIndex;

    void Start()
    {
        if (!isLocalPlayer)
        {
            this.gameObject.SetActive(false);
        }

        if (isLocalPlayer)
        {
            playerIndex = syncPlayerIndex;
        }
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CmdRequestNextTurn();
        }
    }

    [Command]
    private void CmdRequestNextTurn()
    {
        gm.RequestNextTurn(playerIndex);
    }

    /*
    
    public GameObject cardPrefab;
    public GameObject cardSmallPrefab;

    public GameObject canvas;
    public GameObject indexLabel;

    public GameObject handZone;
    public GameObject enemyZoneGrid;
    public GameObject enemyZonePrefab;
    public Dictionary<int, GameObject> enemyZones;

    void Start()
    {
        // Disable GUI of other clients and all on server.
        if (!isLocalPlayer) canvas.SetActive(false);

        // Enemy zones
        enemyZones = new Dictionary<int, GameObject>();
        
        
        //int numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        //for (int i = 1; i <= numPlayers; i++)
        //{
          //  if (i == playerIndex) return;

           // GameObject enemyZone = Instantiate(enemyZonePrefab, Vector3.zero, Quaternion.identity, enemyZoneGrid.transform);
            //enemyZones[i] = enemyZone;
        //}

    }

    // Set player label once playerIndex has been assigned
    void SetPlayerIndex(int oldVar, int newVar)
    {
        indexLabel.GetComponent<Text>().text = $"Player {playerIndex}";
    }

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

            CmdUpdateHands();
            //Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyZones[playerIndex].transform.GetChild(0).transform);
        }
    }
    

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

            CmdUpdateHands();
            //Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyZones[playerIndex].transform.GetChild(0).transform);
        }
    }

    

    void CmdUpdateHands()
    {
        public GameManager gc = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        for (int i = 1; i < gc.playerConns.Count; i++)
        {
            if (i != playerIndex)
            {
                Debug.Log($"change hand for player {i}");
            }
        }

        //GameObject zone = gc.playerConns[];

        //Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyZones[playerIndex].transform.GetChild(0).transform);
    }

    */
}