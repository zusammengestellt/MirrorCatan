using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevCardSmall : MonoBehaviour
{
    public int cardOwner;

    private GameManager gm;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matKnight;
    public Material matVPpalace;
    
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    public void UpdateMat(Dev dev)
    {
        Material mat = null;

        if (dev == Dev.KnightRevealed)
            mat = matKnight;
        else if (dev == Dev.VPRevealed)
            mat = matVPpalace;
        else
            mat = matCardback;

        GetComponent<Image>().material = mat;
    }

}
