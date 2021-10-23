using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCardZone : MonoBehaviour
{
    public GameManager gm;

    public GameObject longestRoad;
    public GameObject largestArmy;
    public GameObject harbormaster;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    private void Update()
    {
        longestRoad.SetActive(gm.longestRoadOwner == PlayerController.playerIndex);
        largestArmy.SetActive(gm.largestArmyOwner == PlayerController.playerIndex);
        harbormaster.SetActive(gm.harbormasterOwner == PlayerController.playerIndex);
    }
}
