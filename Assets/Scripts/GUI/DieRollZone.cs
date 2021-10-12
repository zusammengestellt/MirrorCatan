using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DieRollZone : MonoBehaviour
{
    private PlayerController pc;

    public GameObject die1;
    public GameObject die2;

    public Texture dieFace1;
    public Texture dieFace2;
    public Texture dieFace3;
    public Texture dieFace4;
    public Texture dieFace5;
    public Texture dieFace6;

    public Texture[] dieFaces;

    void Awake()
    {
        this.gameObject.SetActive(true);

        die1.SetActive(false);
        die2.SetActive(false);

        dieFaces = new Texture[6];

        dieFaces[0] = dieFace1;
        dieFaces[1] = dieFace2;
        dieFaces[2] = dieFace3;
        dieFaces[3] = dieFace4;
        dieFaces[4] = dieFace5;
        dieFaces[5] = dieFace6;
    }

    private void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        GameManager.onRollDie += StartRollDie;
    }

    private void StartRollDie(int roll1, int roll2)
    {
        die1.SetActive(true);
        die2.SetActive(true);
        StartCoroutine(RollDie(roll1, roll2));
    }

    public IEnumerator RollDie(int roll1, int roll2)
    {
        int timesRolled = 14;
        int rollStart1 = roll1 - timesRolled;
        int rollStart2 = roll2 - timesRolled;

        die1.GetComponent<RawImage>().texture = dieFaces[System.Math.Abs(rollStart1 % 6)];
        die2.GetComponent<RawImage>().texture = dieFaces[System.Math.Abs(rollStart2 % 6)];

        for (int i = 0; i < timesRolled; i++)
        {
            die1.GetComponent<RawImage>().texture = dieFaces[System.Math.Abs((rollStart1 + i) % 6)];
            die2.GetComponent<RawImage>().texture = dieFaces[System.Math.Abs((rollStart2 + i) % 6)];
            yield return new WaitForSeconds(0.05f + (i * 0.03f));
        }

        // Selection animation.
        int result = roll1 + roll2;

        for (int i = 0; i < GameBoard.numHexes; i++)
        {

            if (GameBoard.hexes[i].instance.GetComponent<HexComponent>().roll == result)
                Debug.Log($"{GameBoard.hexes[i].Q},{GameBoard.hexes[i].R}");
        }


        pc.CmdFinishRoll(result);
        yield return null;
    }

}
