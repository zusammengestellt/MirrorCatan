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

public enum Resource { None, Brick, Grain, Ore, Wood, Wool }

public class GameManager : NetworkBehaviour
{
    [SyncVar] public int syncPlayerCount;
    public static int playerCount;

    [SyncVar] public int currentTurn = 0;

    public SyncDictionary<int, NetworkIdentity> playerIds = new SyncDictionary<int, NetworkIdentity>();
    public SyncDictionary<int, string> playerNames = new SyncDictionary<int, string>();
    public readonly SyncDictionary<int, List<Resource>> playerResources = new SyncDictionary<int, List<Resource>>();

    public GameObject gameBoardPrefab;


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

        // Spawn the game board (good luck)
        GameObject gameBoard = Instantiate(gameBoardPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(gameBoard);

    }


    ////
    //TEST ADD RESOURCES
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


    public void OnEnable()
    {

    }

    // Message board system
    public static event Action<string> postMessage;

    // Game state events
    public static event Action<int> onNextTurn;
    public static event Action<int, int> onRollDie;
    

    [Server]
    public void RequestNextTurn(int requestor)
    {
        // check logic

        // if good, then:
        int nextTurn = requestor + 1;
        if (nextTurn > playerCount)
            nextTurn = 1;
        currentTurn = nextTurn;

        RpcAdvanceNextTurn(currentTurn);
        RollDie();
    }

    [ClientRpc]
    private void RpcAdvanceNextTurn(int currentTurn)
    {
        onNextTurn?.Invoke(currentTurn);
        postMessage?.Invoke($"It is now Player {currentTurn}'s turn.");
    }
    
    // Trigger die roll animation on clients.
    [Server]
    private void RollDie() => RpcRollDie(UnityEngine.Random.Range(1,7), UnityEngine.Random.Range(1,7));

    [ClientRpc]
    private void RpcRollDie(int roll1, int roll2)
    {
        onRollDie?.Invoke(roll1, roll2);
        postMessage?.Invoke($"Rolling die...");
    }

    [Server]
    public void RequestFinishRoll(int result)
    {
        if (result != 7)
            ProcessResources(result);
        else
        {
            // robber logic
        }
    }
    
    [Server]
    private void ProcessResources(int result)
    {
        for (int i = 0; i < GameBoard.numHexes; i++)
        {
            if (GameBoard.rolls[i] == result)
            {
                // NOT ACTUALLY TO PLAYER 1 ONLY
                AddResource(1, GameBoard.resources[i]);
            }
        }
    }



}
