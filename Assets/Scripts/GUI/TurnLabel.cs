using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnLabel : MonoBehaviour
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

        label.text = gm.currentTurn.ToString();

        if (gm.currentTurn == PlayerController.playerIndex)
        {
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
        }
        else
        {
            label.fontStyle = FontStyle.Normal;
            label.color = Color.black;
        }
    }
}
