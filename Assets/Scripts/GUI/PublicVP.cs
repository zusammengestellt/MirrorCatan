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

        label = GetComponent<Text>();
    }

    void Update()
    {
        gm.CmdRecalculateVP();
        label.text = $"VP: {gm.playerPublicVP[ez.forEnemyIndex]}+";
    }
}
