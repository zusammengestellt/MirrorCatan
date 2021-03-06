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

        if (!gm.playerPrivateVP.ContainsKey(PlayerController.playerIndex)) { return; }

        label.text = $"{gm.playerPrivateVP[PlayerController.playerIndex]}/{gm.VPtoWin}";

        if (gm.GameState == GameManager.State.WINNER)
            label.fontStyle = FontStyle.Bold;
    }
}
