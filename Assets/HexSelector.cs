using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSelector : MonoBehaviour
{
    public GameManager gm;
    public LineRenderer lr;
    private float flickerRate = 0.75f;

    private void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            lr.enabled = !lr.enabled;
            yield return new WaitForSeconds(flickerRate);
        }
        yield return null;
    
    }

    public void Move(int hexId, int playerColor)
    {
        //lr.startColor = lr.endColor = GameManager.PlayerColor(playerColor);
        lr.material = gm.GetPlayerMaterial(playerColor);

        Vector3[] vertices = GameBoard.hexes[hexId].vertices;

        for (int i = 0; i < vertices.Length; i++)
            lr.SetPosition(i, vertices[i] + new Vector3(0f, 0.2f, 0f));
        lr.SetPosition(vertices.Length, vertices[0] + new Vector3(0f, 0.2f, 0f));

    }
}
