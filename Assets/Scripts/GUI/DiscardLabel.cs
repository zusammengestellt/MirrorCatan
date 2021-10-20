using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscardLabel : MonoBehaviour
{
    private GameManager gm;
    private Text label;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        label = GetComponentInChildren<Text>();

        // Hide on start, this label only appears when discarding
        GetComponent<RawImage>().enabled = false;
        label.enabled = false;        
    }

    void Update()
    {
        label.text = $"Discard X more cards";
    }
}
