using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIncomeBanner : MonoBehaviour
{
    public GameManager gm;

    [Header("Income Animations")]
    public GameObject incomeBanner;
    public GameObject incomeUnit;
    public Sprite resWood;
    public Sprite resBrick;
    public Sprite resWool;
    public Sprite resGrain;
    public Sprite resOre;
    public Sprite resCoin;


    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        GameManager.onIncomeAnimation += IncomeAnimation;
        GameManager.onLossAnimation += LossAnimation;
    }

    private void IncomeAnimation(int targetPlayer, List<Resource> resIncome, bool coinIncome)
    {
        if (targetPlayer != PlayerController.playerIndex) { return; }

        Dictionary<Resource, int> incomeTally = new Dictionary<Resource, int>();
        incomeTally[Resource.Wood] = 0;
        incomeTally[Resource.Brick] = 0;
        incomeTally[Resource.Wool] = 0;
        incomeTally[Resource.Grain] = 0;
        incomeTally[Resource.Ore] = 0;

        foreach (Resource res in resIncome)
            incomeTally[res] += 1;

        foreach (Resource res in incomeTally.Keys)
        {
            if (incomeTally[res] > 0)
            {
                GameObject unit = Instantiate(incomeUnit, incomeBanner.transform);
                Sprite icon = null;

                switch (res)
                {
                    case Resource.Wood: icon = resWood; break;
                    case Resource.Brick: icon = resBrick; break;
                    case Resource.Wool: icon = resWool; break;
                    case Resource.Grain: icon = resGrain; break;
                    case Resource.Ore: icon = resOre; break;
                }
                unit.GetComponent<IncomeUnit>().label.text = $"+{incomeTally[res].ToString()}";
                unit.GetComponent<IncomeUnit>().icon.sprite = icon;
            }
        }

        if (coinIncome)
        {
            GameObject unit = Instantiate(incomeUnit, incomeBanner.transform);
            unit.GetComponent<IncomeUnit>().icon.sprite = resCoin;
            unit.GetComponent<IncomeUnit>().label.text = "+1";
        }
        
    }

    private void LossAnimation(int targetPlayer, List<Resource> resLoss, bool coinLoss)
    {
        if (targetPlayer != PlayerController.playerIndex) { return; }

        Dictionary<Resource, int> lossTally = new Dictionary<Resource, int>();
        lossTally[Resource.Wood] = 0;
        lossTally[Resource.Brick] = 0;
        lossTally[Resource.Wool] = 0;
        lossTally[Resource.Grain] = 0;
        lossTally[Resource.Ore] = 0;

        foreach (Resource res in resLoss)
            lossTally[res] += 1;

        foreach (Resource res in lossTally.Keys)
        {
            if (lossTally[res] > 0)
            {
                GameObject unit = Instantiate(incomeUnit, incomeBanner.transform);
                
                Sprite icon = null;

                switch (res)
                {
                    case Resource.Wood: icon = resWood; break;
                    case Resource.Brick: icon = resBrick; break;
                    case Resource.Wool: icon = resWool; break;
                    case Resource.Grain: icon = resGrain; break;
                    case Resource.Ore: icon = resOre; break;
                }
                unit.GetComponent<IncomeUnit>().label.text = $"-{lossTally[res].ToString()}";
                unit.GetComponent<IncomeUnit>().icon.sprite = icon;
                unit.GetComponent<IncomeUnit>().MakeLossUnit();
            }
        }

        if (coinLoss)
        {
            GameObject unit = Instantiate(incomeUnit, incomeBanner.transform);
            unit.GetComponent<IncomeUnit>().icon.sprite = resCoin;
            unit.GetComponent<IncomeUnit>().label.text = "-5";
            unit.GetComponent<IncomeUnit>().MakeLossUnit();
        }
    }

}
