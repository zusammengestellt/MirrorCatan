using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// GameObject is tagged as "GameManager"
// NetworkIdentity is set to "server-only"
//
// This is spawned by the NetworkManager as server-only. Tagged as 'GameManager'.
// Holds the game state variables. Clients access it by calling a command that
// executes GameObject.FindGameObjectWithTag("GameManager").

public enum Resource { CardBack, Brick, Grain, Ore, Wood, Wool }

public class GameManager : NetworkBehaviour
{
    // Player count
    [SyncVar] public int syncPlayerCount;
    public static int playerCount;
    
    // playerIds: holds playerIndexed references to identities
    public SyncDictionary<int, NetworkIdentity> playerIds = new SyncDictionary<int, NetworkIdentity>();

    // playerNames: holds playerIndexed references to nicknames
    public SyncDictionary<int, string> playerNames = new SyncDictionary<int, string>();

    // playerResources: holds playerIndexed references to a List<Resource>
    public readonly SyncDictionary<int, List<Resource>> playerResources = new SyncDictionary<int, List<Resource>>();

    

    public static Resource[] resourceSortOrder = { Resource.Wood, Resource.Brick, Resource.Wool, Resource.Grain, Resource.Ore };

    // Only the server copy does anything during Start(), to initialize and sync
    // the game state variables to the clients. Formerly, this object was sever-only,
    // but I wanted easy access to read these core variables from the clients.
    void Start()
    {
        playerCount = syncPlayerCount;
        
        if (!isServer) { return; }    

        // Set up dummy names
        for (int index = 1; index <= playerCount; index++)
        {
            string name = "null";

            switch (index)
            {
                case 1:
                    name = "Alsett"; break;
                case 2:
                    name = "Anderswo"; break;
                case 3:
                    name = "fritz"; break;    
            }
            playerNames[index] = name;
        }

        // Set up playerResources
        for (int index = 1; index <= playerCount; index++)
        {
            playerResources[index] = new List<Resource>();
        }
    }

////////

    void Update()
    {
        if (!isServer) { return; }

        for (int number = 0; number <= 9; number++)
        {
   
            if (Input.GetKeyDown(number.ToString()))
            {
                if (number > 0 && number <= playerCount)
                {
                    Resource randomRes = resourceSortOrder[UnityEngine.Random.Range(0,5)];
                    AddResource(number, randomRes);

                    Debug.Log($"Player {number} now has {playerResources[number].Count} cards");
                }
            }

        }
        
    }

    // Game flow events
    public static event Action<int> onNextTurn;

    [Server]
    public void RequestNextTurn(int playerIndex)
    {
        // check logic
        RpcAdvanceNextTurn(playerIndex);
    }

    [ClientRpc]
    private void RpcAdvanceNextTurn(int playerIndex)
    {
        onNextTurn?.Invoke(playerIndex);
    }


    // After the server adds a resource, it calls a ClientRpc to get
    // the event to fire on all handlers on each client.
    public static event Action<int, List<Resource>> onHandChanged;

    [ClientRpc]
    private void RpcChangeResource(int playerIndex, List<Resource> newResources)
    {
        onHandChanged?.Invoke(playerIndex, newResources);
    }

    [Server]
    private void AddResource(int playerIndex, Resource res)
    {
        List<Resource> newResources = playerResources[playerIndex];
        newResources.Add(res);
        playerResources[playerIndex] = newResources;

        RpcChangeResource(playerIndex, newResources);
    }

    [Server]
    private void RemoveResource(int playerIndex, Resource res)
    {
        List<Resource> newResources = playerResources[playerIndex];
        
        if (newResources.Contains(res))
            newResources.Remove(res);
        else
            Debug.LogWarning("Player doesn't have a resource to remove");

        playerResources[playerIndex] = newResources;

        RpcChangeResource(playerIndex, newResources);
    }


}
