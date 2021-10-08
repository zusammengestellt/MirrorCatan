using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public Text label;
    public GameObject[] playersArray;

    void Start()
    {
        // no need, set in inspector.
        //label = this.GetComponent<Text>();
    }

    void Update()
    {
        playersArray = GameObject.FindGameObjectsWithTag("Player");
        
        if (playersArray.Length == 1)
            label.text = "1 player connected";
        else
            label.text = $"{playersArray.Length} players connected";
    }
}
