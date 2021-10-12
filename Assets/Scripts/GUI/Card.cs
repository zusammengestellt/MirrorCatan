using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Resource resource;

    [Header("Card Materials")]
    public Material matCardback;
    public Material matBrick;
    public Material matGrain;
    public Material matOre;
    public Material matWood;
    public Material matWool;

    private Vector2 startPosition;
    private bool selectable = false;
    private bool selected = false;

    private void OnEnable()
    {
        GameManager.onNextTurn += OnNextTurn;
    }

    public void OnNextTurn(int newTurn)
    {
        selectable = false;
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

    public void SelectCard()
    {
        Debug.Log(PlayerController.playerIndex);
        
        if (!selectable) { return; }

        startPosition = this.gameObject.GetComponent<RectTransform>().position;

        if (!selected)
        {
            selected = true;
            this.gameObject.GetComponent<RectTransform>().position = new Vector2(startPosition.x, startPosition.y + 10f);
        }
        else
        {
            selected = false;
            this.gameObject.GetComponent<RectTransform>().position = new Vector2(startPosition.x, startPosition.y - 10f);
        }      
    }
}
