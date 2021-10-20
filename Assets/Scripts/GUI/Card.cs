using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private GameManager gm;

    public Resource resource;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matBrick;
    public Material matGrain;
    public Material matOre;
    public Material matWood;
    public Material matWool;

    private Vector2 startPosition;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    public void SetResource(Resource res)
    {
        resource = res;
        Material mat = null;
        
        switch (res)
        {
            case Resource.Brick: mat = matBrick; break;
            case Resource.Grain: mat = matGrain; break;
            case Resource.Ore: mat = matOre; break;
            case Resource.Wood: mat = matWood; break;
            case Resource.Wool: mat = matWool; break;
        }

        GetComponent<Image>().material = mat;
    }

    
    // Set in inspector for OnClick
    public void RobberDiscard()
    {
        if (gm.stillToDiscard[PlayerController.playerIndex] > 0)
        {
            Debug.Log("trying to discard this...");
            
        }
           
    }
}
