using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex
{
    public Hex(int q, int r) {
        this.Q = q;
        this.R = r;

        
        this.neighbors = new Hex[6];

        // Calculate and store position of center points
        this.position = new Vector3(
            HexMetrics.hexHorizontalSpacing * Q + (R * HexMetrics.hexHorizontalSpacing / 2),
            HexMetrics.hexGroundLevel,
            HexMetrics.hexVerticalSpacing * R
        );

        // Center grid around 0,0 by subtracting the position
        // of (boardSize,boardSize) from all points
        this.position.x -= (HexMetrics.hexHorizontalSpacing * HexMetrics.boardSize) + (HexMetrics.boardSize * HexMetrics.hexHorizontalSpacing / 2);
        this.position.z -= HexMetrics.hexVerticalSpacing * HexMetrics.boardSize;

        // Calculate and store raw vertices
        vertices[0] = new Vector3(
            position.x,
            position.y,
            position.z + HexMetrics.hexHeight / 2
        );
        vertices[1] = new Vector3(
            position.x + HexMetrics.hexWidth / 2,
            position.y,
            position.z + HexMetrics.hexHeight / 4
        );
        vertices[2] = new Vector3(
            position.x + HexMetrics.hexWidth / 2,
            position.y,
            position.z - HexMetrics.hexHeight / 4
        );
        vertices[3] = new Vector3(
            position.x,
            position.y,
            position.z - HexMetrics.hexHeight / 2
        );
        vertices[4] = new Vector3(
            position.x - HexMetrics.hexWidth / 2,
            position.y,
            position.z - HexMetrics.hexHeight / 4
        );
        vertices[5] = new Vector3(
            position.x - HexMetrics.hexWidth / 2,
            position.y,
            position.z + HexMetrics.hexHeight / 4
        );

        // Set corners to null
        for (int i = 0; i < corners.Length; i++)
            corners[i] = null;
    }

    public GameObject instance;

    public int id;
    public int Q;
    public int R;
    public Vector3 position;

    public Hex[] neighbors;
    public Vector3[] vertices = new Vector3[6];
    public Corner[] corners = new Corner[6];
    public Path[] paths = new Path[12];

    public Resource resource;
    public int roll;
    public bool robbed = false;
}

public class HexMetrics
{
    public static float unitScale = 10f;
    public static float hexGroundLevel = 0f;

    public static float hexWidth = unitScale * 1.7320508f; // Sqrt(3)
    public static float hexHeight = unitScale * 2;

    public static float hexHorizontalSpacing = hexWidth;
    public static float hexVerticalSpacing = hexHeight * 3 / 4;

    public static int boardSize = 2;
}