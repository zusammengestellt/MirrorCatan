using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpecialCardZone : MonoBehaviour
{
    public GameManager gm;
    public int forEnemyIndex;

    public GameObject longestRoad;
    public GameObject largestArmy;
    public GameObject harbormaster;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        forEnemyIndex = GetComponentInParent<EnemyZone>().forEnemyIndex;
    }

    private void Update()
    {
        longestRoad.SetActive(gm.longestRoadOwner == forEnemyIndex);
        largestArmy.SetActive(gm.largestArmyOwner == forEnemyIndex);
        harbormaster.SetActive(gm.harbormasterOwner == forEnemyIndex);
    }
}
