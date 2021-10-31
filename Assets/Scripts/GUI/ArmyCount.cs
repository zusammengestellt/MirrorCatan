using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyCount : MonoBehaviour
{
    private Text label;
    private GameManager gm;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        label = GetComponentInChildren<Text>();
    }

    void Update()
    {
        int armyCount = 0;
        
        foreach (Dev dev in gm.playerDevCards[PlayerController.playerIndex])
        {
            if (dev == Dev.KnightRevealed)
                armyCount++;
        }

        label.text = armyCount.ToString();
    }
}
