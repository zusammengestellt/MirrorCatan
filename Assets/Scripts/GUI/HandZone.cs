using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandZone : MonoBehaviour
{
    public GameObject cardPrefab;
    
    [SerializeField] private int defaultCardSpacing;
    [SerializeField] private int handZoneMaxWidth;
    private float cardWidth;

    private void Start()
    {
        cardWidth = cardPrefab.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void OnEnable()
    {
        GameManager.onHandChanged += UpdateHandZone;
    }

    private void UpdateHandZone(int targetPlayerIndex, List<Resource> resources)
    {
        if (targetPlayerIndex != PlayerController.playerIndex) { return; }

        // The local player's hand was changed. Update the main hand zone.
        // First, clear existing hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in this.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);
        
        // Sort matching resources so they appear together.
        List<Resource> resourcesSorted = new List<Resource>();
        for (int i = 0; i < GameManager.resourceSortOrder.Length; i++)
        {
            foreach (Resource res in resources)
                if (res == GameManager.resourceSortOrder[i])
                    resourcesSorted.Add(res);
        }

        // Dynamically adjust spacing between cards and zone background width.
        float spacing = (handZoneMaxWidth - (resources.Count * cardWidth)) / (resources.Count + 1);
        this.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSpacing, spacing), 0);
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Min(handZoneMaxWidth, resources.Count * cardWidth + (resources.Count) * spacing + 10), this.GetComponent<RectTransform>().sizeDelta.y);

        // Instantiate the cards.
        foreach (Resource res in resourcesSorted)
        {
            GameObject newCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, this.transform);
            newCard.GetComponent<Card>().SetResource(res);
        }
    }
}
