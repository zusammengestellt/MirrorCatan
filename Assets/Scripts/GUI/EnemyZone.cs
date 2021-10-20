using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyZone : MonoBehaviour
{
    public GameObject cardSmallPrefab;
    public GameObject enemyHandZone;

    public int forEnemyIndex;

    [SerializeField] private int defaultCardSmallSpacing;
    [SerializeField] private int enemyHandZoneMaxWidth;
    private float cardSmallWidth;

    private void Start()
    {
        cardSmallWidth = cardSmallPrefab.GetComponent<RectTransform>().sizeDelta.x;
    }

    private void OnEnable()
    {
        GameManager.onHandChanged += UpdateEnemyHandZone;
    }

    private void UpdateEnemyHandZone(int targetPlayerIndex, List<Resource> resources)
    {
        if (targetPlayerIndex != forEnemyIndex) { return; }

        // Another player's hand was changed. Update the correct enemy zone.
        // First, identify and clear the correct enemy hand zone.
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in enemyHandZone.transform)
            allChildren.Add(child.gameObject);

        foreach (GameObject child in allChildren)
            DestroyImmediate(child);

        // Instantiate the current number of smallCards (w/ hidden card backs).
        for (int i = 0; i < resources.Count; i++)
        {
            GameObject card = Instantiate(cardSmallPrefab, Vector3.zero, Quaternion.identity, enemyHandZone.transform);
            card.GetComponent<CardSmall>().cardOwner = targetPlayerIndex;
        }

        // Dynamically adjust spacing between cards
        float spacing = (enemyHandZoneMaxWidth - (resources.Count * cardSmallWidth)) / (resources.Count + 1);
        enemyHandZone.GetComponent<GridLayoutGroup>().spacing = new Vector2(Mathf.Min(defaultCardSmallSpacing, spacing), 0);
    }
 
 
}
