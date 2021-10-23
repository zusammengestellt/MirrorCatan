using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevCardZone : MonoBehaviour
{
    public GameObject devCardPrefab;
    
    [SerializeField] private int defaultCardSpacing;
    [SerializeField] private int devCardZoneMax;
    private float cardWidth;

    private void Start()
    {
        cardWidth = devCardPrefab.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void OnEnable()
    {
        GameManager.onDevCardChanged += UpdateDevCardZone;
    }

    private void UpdateDevCardZone(int targetPlayerIndex, List<Dev> devCards)
    {
        if (targetPlayerIndex != PlayerController.playerIndex) { return; }

        // The local player's hand was changed. Update the main hand zone.
        // First, clear existing hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in this.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);
        
        // Sort dev cards so they appear together.
        List<Dev> devCardsSorted = new List<Dev>();
        for (int i = 0; i < GameManager.devCardSortOrder.Length; i++)
        {
            foreach (Dev dev in devCards)
                if (dev == GameManager.devCardSortOrder[i])
                    devCardsSorted.Add(dev);
        }

        // Remove revealed cards from own visible hand
        // Revealed Knights and VPs can go elsewhere for the player
        List<Dev> devCardsFiltered = new List<Dev>();
        foreach (Dev dev in devCardsSorted)
        {
            if (dev != Dev.KnightRevealed && dev != Dev.VPRevealed)
                devCardsFiltered.Add(dev);
        }
        devCardsSorted = devCardsFiltered;

        // Dynamically adjust spacing between cards and zone background width.
        float spacing = (devCardZoneMax - (devCardsSorted.Count * cardWidth)) / (devCardsSorted.Count + 1);
        this.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSpacing, spacing), 0);
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Min(devCardZoneMax, devCardsSorted.Count * cardWidth + (devCardsSorted.Count) * spacing + 10), this.GetComponent<RectTransform>().sizeDelta.y);
        if (this.GetComponent<RectTransform>().sizeDelta.x < devCardZoneMax)
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(devCardZoneMax, this.GetComponent<RectTransform>().sizeDelta.y);

        // Instantiate the cards.
        foreach (Dev dev in devCardsSorted)
        {
            GameObject newCard = Instantiate(devCardPrefab, Vector3.zero, Quaternion.identity, this.transform);
            newCard.GetComponent<DevCard>().SetDev(dev);
        }
    }
}
