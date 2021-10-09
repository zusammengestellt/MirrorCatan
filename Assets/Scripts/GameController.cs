using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// GameObject is tagged as "GameController"
// NetworkIdentity is set to "server-only"
//
// This is spawned by the NetworkManager as server-only. Tagged as 'GameController'.
// Holds the game state variables. Clients access it by calling a command that
// executes GameObject.FindGameObjectWithTag("GameController").

public enum Resource { CardBack, Brick, Grain, Ore, Wood, Wool }

public class GameController : NetworkBehaviour
{
    // playerIds: holds playerIndexed references to identities
    public SyncDictionary<int, NetworkIdentity> playerIds = new SyncDictionary<int, NetworkIdentity>();

    // playerNames: holds playerIndexed references to nicknames
    public SyncDictionary<int, string> playerNames = new SyncDictionary<int, string>();

    // playerResources: holds playerIndexed references to a List<Resource>
    public readonly SyncDictionary<int, List<Resource>> playerResources = new SyncDictionary<int, List<Resource>>();

    // Player count
    [SyncVar] public int playerCount;

    // Only the server copy does anything during Start(), to initialize and sync
    // the game state variables to the clients. Formerly, this object was sever-only,
    // but I wanted easy access to read these core variables from the clients.
    void Start()
    {
        if (!isServer) { return; }
        
        // Set up dummy names
        for (int index = 1; index <= playerCount; index++)
        {
            string name = "NULL";

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
            if (Input.GetKeyDown(number.ToString()))
            {
                if (number > 0 && number <= playerCount)
                {
                    AddResource(number, Resource.Ore);

                    Debug.Log($"Player {number} now has {playerResources[number].Count} cards");
                }
            }
        
    }

    // After the server adds a resource, it calls a ClientRpc to get
    // the event to fire on all handlers on each client.
    public static event Action<int, List<Resource>> handChanged;

    // This can't be shortened into playerResources[playerIndex].Add("") b/c the SyncDictionary won't update without an assignment
    [Server]
    private void AddResource(int playerIndex, Resource res)
    {
        List<Resource> newResources = playerResources[playerIndex];
        newResources.Add(res);
        playerResources[playerIndex] = newResources;

        RpcAddResource(playerIndex, newResources);
    }

    [ClientRpc]
    private void RpcAddResource(int playerIndex, List<Resource> newResources)
    {
        handChanged?.Invoke(playerIndex, newResources);
    }


    public void RemoveResource(int playerIndex, Resource res)
    {
        List<Resource> resources = playerResources[playerIndex];

        if (resources.Count <= 0)
            Debug.LogWarning($"Player {playerIndex} has no resources to remove!");
        if (!resources.Contains(res))
            Debug.LogWarning($"Player {playerIndex} doesn't have any {res} to remove!");
        
        
        resources.Remove(res);
        playerResources[playerIndex] = resources;
    }
    

}
