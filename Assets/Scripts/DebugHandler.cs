using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DebugHandler : MonoBehaviour
{
    /*

    private Dictionary<int,int> connectionKeys;

    // NetworkServer.connections is a Dictionary with <Int, NetworkConnectionToClient>
    // The keys are random numbers like -81264536
    // This makes them accessible with a simple 1-indexed array.
    void GenerateFriendlyKeys()
    {    
        connectionKeys = new Dictionary<int,int>();

        int i = 1;
        foreach (int key in NetworkServer.connections.Keys)
        {
            connectionKeys[i] = key;
            i++;
        }
    }
    
    void Update()
    {
        //CheckDebugByNumber();
    }

    // Check if should debug by number. The actual test is in DebugByNumber()
    // Used to quickly test player-specific changes.
    void CheckDebugByNumber()
    {
        for (int number = 0; number <= 9; number++)
            if (Input.GetKeyDown(number.ToString()))
            {
                if (number > 0 && number <= NetworkServer.connections.Count)
                {
                    GenerateFriendlyKeys();
                    DebugByNumber(number);
                }
                else if (number == 0)
                {
                    Debug.LogWarning("There can't be a player 0. Player number indexing is already accounted for.");
                }
                else
                {
                    Debug.LogWarning($"Called DebugByNumber for player {number}, but there are only {NetworkServer.connections.Count} players.");
                }
            }
    }

    // The actual test. Whatever you need it to do by player number.
    void DebugByNumber(int number)
    {
        Debug.Log($"Dealing a card to player {number}.");
        NetworkServer.connections[connectionKeys[number]].identity.gameObject.GetComponent<PlayerController>().numCards++;        
        
    }

    */
    
}
