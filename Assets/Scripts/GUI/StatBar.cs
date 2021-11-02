using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Text numberLabel;
    public Text nameLabel;
    public Text scoreLabel;

    public void SetLabels(string rank, string name, int score)
    {
        numberLabel.text = rank;
        nameLabel.text = name;
        scoreLabel.text = score.ToString();
    }
}
