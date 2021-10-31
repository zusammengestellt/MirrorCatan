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
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    public IEnumerator FlickerHexSelector(int hexId)
    {
        lr.enabled = true;
        
        //lr.material = playerMat;
        Vector3[] vertices = GameBoard.hexes[hexId].vertices;

        for (int i = 0; i < vertices.Length; i++)
            lr.SetPosition(i, vertices[i] + new Vector3(0f, 5f, 0f));

        lr.SetPosition(vertices.Length, vertices[0] + new Vector3(0f, 5f, 0f));

        for (int i = 0; i < 7; i++)
        {
            yield return new WaitForSeconds(flickerRate);
            lr.enabled = !lr.enabled;
        }
        yield return null;

        lr.enabled = false;
    }
}
