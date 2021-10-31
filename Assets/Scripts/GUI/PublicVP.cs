using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublicVP : MonoBehaviour
{
    private GameManager gm;
    private EnemyZone ez;
    private Text label;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        ez = GetComponentInParent<EnemyZone>();

        label = GetComponentInChildren<Text>();
    }

    void Update()
    {
        gm.CmdRecalculateVP();

        if (gm.GameState != GameManager.State.WINNER)
            label.text = $"{gm.playerPublicVP[ez.forEnemyIndex]}+";
        else
            label.text = $"{gm.playerPrivateVP[ez.forEnemyIndex]}";
    }
}
