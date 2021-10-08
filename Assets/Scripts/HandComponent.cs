using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HandComponent : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateCards))]
    public int numCards;

    void UpdateCards(int oldVar, int newVar)
    {
        Debug.Log($"updating cards. Am I the local client? {isLocalPlayer}");
    }

    void Update()
    {
        if (isServer && Input.GetKeyDown(KeyCode.Q))
        {
            numCards++;
        }
    }
}
