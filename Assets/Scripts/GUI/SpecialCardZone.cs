using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        if (gm.longestRoadOwner == PlayerController.playerIndex && gm.largestArmyOwner == PlayerController.playerIndex && gm.harbormasterOwner == PlayerController.playerIndex)
            GetComponent<GridLayoutGroup>().spacing = new Vector2(-50f, 0f);
        else
            GetComponent<GridLayoutGroup>().spacing = new Vector2(10f, 0f);
    }
}
