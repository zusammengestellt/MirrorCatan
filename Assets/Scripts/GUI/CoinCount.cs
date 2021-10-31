using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCount : MonoBehaviour
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
        label.text = gm.playerCoins[PlayerController.playerIndex].ToString();
    }
}
