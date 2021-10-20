using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivateVP : MonoBehaviour
{
    private GameManager gm;
    private Text label;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        label = GetComponentInChildren<Text>();
    }

    void Update()
    {
        gm.CmdRecalculateVP();
        label.text = $"VP: {gm.playerPrivateVP[PlayerController.playerIndex]}";
    }
}
