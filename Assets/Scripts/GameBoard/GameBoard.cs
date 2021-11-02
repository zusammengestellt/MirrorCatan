using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class GameBoard : NetworkBehaviour
{    
    public GameObject HexPrefab;
    public GameObject CornerPrefab;
    public GameObject PathPrefab;
    public GameObject NumberTokenPrefab;
    public GameObject RobberPrefab;
    public GameObject HarborPrefab;

    public Material DesertMat;
    public Material ForestMat;
    public Material PastureMat;
    public Material FieldMat;
    public Material HillMat;
    public Material MountainMat;

    public int boardSize;

    [System.NonSerialized] public static Hex[] hexes;
    [System.NonSerialized] public static Corner[] corners;
    [System.NonSerialized] public static Path[] paths;

    [System.NonSerialized] public static GameObject[] hexObjects;
    [System.NonSerialized] public static GameObject[] cornerObjects;
    [System.NonSerialized] public static GameObject[] pathObjects;
    [System.NonSerialized] public static GameObject[] tokenObjects;
    [System.NonSerialized] public static GameObject[] harborObjects;

    [System.NonSerialized] public static int numHexes;
    [System.NonSerialized] public static int numCorners;
    [System.NonSerialized] public static int numPaths;

    public static Resource[] resources;
    public static int[] rolls;

    private System.Random _random = new System.Random();

    private static int boardsReady = 0;

    private void Start()
    {
        // Both server and client calculate the underlying math.
        CalculateBoard();

        // Then server randomizes the terrain and number tokens while client generates the board objects.
        if (isServer) { StartCoroutine(RandomizeBoard()); }
        else { GenerateBoard(); }

        // When the clients are all ready, they inform the server and the server runs an Rpc call
        // to apply the randomized settings to each client board.
    }

    public void CalculateBoard()
    {

        // Calculate number of hexes the board has
        numHexes = boardSize * 2 + 1;
        for (int i = 1; i <= boardSize; i++)
            numHexes += 2 * ((boardSize * 2 + 1) - i);
        hexes = new Hex[numHexes];

        // Calculate number of corners the board has
        numCorners = (boardSize + 1) * 2 + (boardSize + 1) * 4;
        for (int i = 1; i <= boardSize; i++)
            numCorners += (boardSize + 1 + i) * 4;
        corners = new Corner[numCorners];

        // Calculate number of paths the board has
        numPaths = (6 * numHexes) + (12 * boardSize) + 6;
        paths = new Path[numPaths];
        
        // Get q,r coordinates for each hex
        for (int i = 0, r = 0; r < (boardSize * 2 + 1); r++)
        {    
            for (int q = 0; q < (boardSize * 2 + 1); q++)
            {
                // If q,r is on the game board, make it
                if (r + q >= boardSize && r + q <= boardSize * 3)
                {
                    hexes[i] = new Hex(q, r);
                    hexes[i].id = i;
                    i++;
                }
            }
        }

        // Find and store neighboring hexes
        for (int i = 0; i < hexes.Length; i++)
        {
            Hex h = hexes[i];
            int q = hexes[i].Q;
            int r = hexes[i].R;

            // Each index of array is set to either a hex or null
            // 0: NW
            h.neighbors[0] = GetHexAt(q - 1, r + 1);
            // 1: NE
            h.neighbors[1] = GetHexAt(q, r + 1);
            // 2: E
            h.neighbors[2] = GetHexAt(q + 1, r);
            // 3: SE
            h.neighbors[3] = GetHexAt(q + 1, r - 1);
            // 4: SW
            h.neighbors[4] = GetHexAt(q, r - 1);
            // 5: W
            h.neighbors[5] = GetHexAt(q - 1, r);
        }

        // Calculate corners and assign them to hexes (no duplicates)
        // Also assign hexes to corners (neighborHexes)
        for (int cornerIndex = 0, i = 0; i < hexes.Length; i++)
        {
            Hex h = hexes[i];

            for (int j = 0; j < h.corners.Length; j++)
            {
                int k = (j + 1) % 6;
                //Debug.Log($"iterators: j: {j}, k: {k}, j_opp: {(j+2)%6}, k_opp: {(k+3)%6}");

                // Grab an existing corner before trying to making a new one                
                if (GetHexNeighbor(h, j) != null && GetHexNeighbor(h, j).corners[((j + 2) % 6)] != null)
                {
                    //Debug.Log($"hex {h.Q},{h.R} building corner {j} taking from {GetHexNeighbor(h, j).Q},{GetHexNeighbor(h, j).R}'s corner {(j + 2) % 6}");
                    h.corners[j] = GetHexNeighbor(h, j).corners[(j + 2) % 6];
                    h.corners[j].neighborHexes.Add(h);
                }
                else if (GetHexNeighbor(h, k) != null && GetHexNeighbor(h, k).corners[(k + 3) % 6] != null)
                {
                    //Debug.Log($"hex {h.Q},{h.R} building corner {j} taking from {GetHexNeighbor(h, k).Q},{GetHexNeighbor(h, k).R}'s corner {(k + 3) % 6}");
                    h.corners[j] = GetHexNeighbor(h, k).corners[(k + 3) % 6];
                    h.corners[j].neighborHexes.Add(h);     
                }
                else
                {
                    //Debug.Log($"hex {h.Q},{h.R} building new corner {j}");
                    corners[cornerIndex] =  h.corners[j] = new Corner(h.vertices[j], cornerIndex);
                    h.corners[j].neighborHexes.Add(h);
                    cornerIndex++;
                }
            }
        }

        // Connect the corners (PART ONE)
        // Check each pair of corners. If they share 2 hexes, they are linked
        for (int i = 0; i < corners.Length; i++)
        {
            Corner cA = corners[i];

            for (int j = 0; j < corners.Length; j++)
            {
                Corner cB = corners[j];

                // Don't compare a corner to itself
                if (cA != cB)
                {
                    int matchingHexes = 0;

                    foreach (Hex hA in cA.neighborHexes)
                    {
                        foreach (Hex hB in cB.neighborHexes)
                        {
                            if (hA == hB)
                                matchingHexes++;
                        }
                    }

                    // If a pair of corners shares 2 hexes, they are connected
                    if (matchingHexes == 2)
                    {
                        if (!cA.neighborCorners.Contains(cB))
                            cA.neighborCorners.Add(cB);
                        if (!cB.neighborCorners.Contains(cA))
                            cB.neighborCorners.Add(cA);

                        //Debug.Log($"With matchingHexes {matchingHexes}, corner {cA.idNum} borders {cB.idNum}");
                    }

                }
            }
        }

        // Connect the corners (PART TWO)
        // For edge paths, find the edge corners and rotate around them
        List<Corner> edgeCorners = new List<Corner>();
        List<(Corner,Corner)> edgeCornerPairs = new List<(Corner,Corner)>();
        
        for (int i = 0; i < hexes.Length; i++)
        {
            Hex h = hexes[i];
            bool isEdgeHex = false;

            for (int j = 0; j < h.corners.Length; j++)
            {
                // If a single corner has less than 3 neighborHexes, it's on an edge hex
                if (h.corners[j].neighborHexes.Count < 3)
                    isEdgeHex = true;
            }

            if (isEdgeHex)
            {
                // Iterate around the corners
                for (int k = 0; k < h.corners.Length; k++)
                {
                    Corner cA = h.corners[k];
                    Corner cB = h.corners[(k + 1) % 6];

                    // Both corners must be edge corners
                    if (cA.neighborHexes.Count < 3 && cB.neighborHexes.Count < 3)
                    {
                        if (!cA.neighborCorners.Contains(cB))
                            cA.neighborCorners.Add(cB);
                        if (!cB.neighborCorners.Contains(cA))
                            cB.neighborCorners.Add(cA);

                        //Debug.Log($"With special edge logic, corner {cA.idNum} borders {cB.idNum}");

                        // Grab list of edge corners for harbors
                        if (!edgeCorners.Contains(cA))
                            edgeCorners.Add(cA);
                        if (!edgeCorners.Contains(cB))
                            edgeCorners.Add(cB);
                        
                        if (!edgeCornerPairs.Contains((cA,cB)) && !edgeCornerPairs.Contains((cB,cA)))
                            edgeCornerPairs.Add((cA,cB));                        
                    }
                }
            }
        }

        bool validCoast = true;
        List<(Corner,Corner)> harborPairs = new List<(Corner, Corner)>();

        // Calculate harbor corner locations
        foreach ((Corner,Corner) coastline in edgeCornerPairs)
        {
            validCoast = true;
            Corner cA = coastline.Item1;
            Corner cB = coastline.Item2;
            
            foreach (Corner ca in cA.neighborCorners.Where(c => c != cB))
                if (ca.isHarbor)
                    validCoast = false;

            foreach (Corner cb in cB.neighborCorners.Where(c => c != cA))
                if (cb.isHarbor)
                    validCoast = false;
                
            if (validCoast)
            {
                cA.isHarbor = cB.isHarbor = true;
                harborPairs.Add((cA,cB));
            }
        }

        // Calculate harbor types
        List<Resource> harborTypes = new List<Resource>{ Resource.Wood, Resource.Brick, Resource.Wool, Resource.Grain, Resource.Ore };

        // Add any's and randomize
        for (int i = 0; i < harborPairs.Count - 5; i++)
            harborTypes.Add(Resource.None);

        // Shuffle resource distribution.
        System.Random _random = new System.Random();
        for (int i = 0; i < harborTypes.Count - 1; i++)
        {
            int j = _random.Next(i, harborTypes.Count);
            Resource temp = harborTypes[i];
            harborTypes[i] = harborTypes[j];
            harborTypes[j] = temp;
        }
        
        int index = 0;
        foreach ((Corner,Corner) coastline in harborPairs)
        {
            Corner cA = coastline.Item1;
            Corner cB = coastline.Item2;

            Resource res = harborTypes[index];
            cA.harborType = cB.harborType = res;
            
            index++;
        }
       

        // Calculate path positions
        for (int pathIndex = 0, i = 0; i < corners.Length; i++)
        {
            Corner cA = corners[i];

            foreach (Corner cB in cA.neighborCorners)
            {
                // Using Vector3.Lerp for vector interpolation (0.5f is halfway)
                Vector3 midpoint = Vector3.Lerp(cA.position, cB.position, 0.5f);

                // Adjusted slightly for spacing
                Vector3 pathPosition = Vector3.Lerp(cA.position, midpoint, 0.6f);
                float rotate;
                
                if (Mathf.Round(cA.position.x * 10f) == (Mathf.Round(cB.position.x * 10f)))
                    rotate = 90f;
                else if (cA.position.x < cB.position.x && cA.position.z < cB.position.z)
                    rotate = -30f;
                else if (cA.position.x > cB.position.x && cA.position.z > cB.position.z)
                    rotate = -30f;
                else if (cA.position.x < cB.position.x && cA.position.z > cB.position.z)
                    rotate = 30f;
                else if (cA.position.x > cB.position.x && cA.position.z < cB.position.z)
                    rotate = 30f;
                else
                    rotate = 0f;
        
                Quaternion pathAngle = Quaternion.Euler(new Vector3(0f,rotate,0f));

                Path p = new Path(pathIndex, pathPosition, pathAngle);
                p.idNum = pathIndex;
                p.myCorner = cA;
                paths[pathIndex] = p;
                pathIndex++;

                // assign to corner A the path in the direction or corner B
                cA.neighborPaths[cB] = p;
            }
        }

        // Connect paths
        for (int i = 0; i < corners.Length; i++)
        {
            Corner cA = corners[i];

            // Connect to paths that share same corner
            foreach(Path pA in cA.neighborPaths.Values)
            {
                foreach(Path pB in cA.neighborPaths.Values)
                {
                    if (pA != pB && !pA.connectedPaths.Contains(pB))
                        pA.connectedPaths.Add(pB);
                }
            }
            
            // Link to path in each opposite corner
            foreach(Corner cB in cA.neighborPaths.Keys)
            {
                Path pA = cA.neighborPaths[cB];
                Path pB = cB.neighborPaths[cA];

                if (!pA.connectedPaths.Contains(pB))
                    pA.connectedPaths.Add(pB);
                if (!pB.connectedPaths.Contains(pA))
                    pB.connectedPaths.Add(pA);
            }
            
        }
    }

    [Client]
    public void GenerateBoard()
    {        
        // Generate and spawn hex game objects.
        hexObjects = new GameObject[numHexes];

        for (int i = 0; i < numHexes; i++)
        {
            hexObjects[i] = Instantiate(HexPrefab, hexes[i].position, Quaternion.identity, this.transform);
            hexObjects[i].GetComponent<HexComponent>().hex = hexes[i];
            hexObjects[i].GetComponent<HexComponent>().id = i;
            hexObjects[i].transform.name = $"Hex{i}";
            hexes[i].instance = hexObjects[i];
        }
  
        // Generate and spawn cornerGameObjects.
        cornerObjects = new GameObject[corners.Length];
        harborObjects = new GameObject[corners.Length];
        
        for (int i = 0; i < corners.Length; i++)
        {
            cornerObjects[i] = (GameObject)Instantiate(CornerPrefab, corners[i].position, Quaternion.identity, this.transform);
            cornerObjects[i].GetComponent<CornerComponent>().corner = corners[i];
            cornerObjects[i].transform.name = $"Corner{i}";
            corners[i].instance = cornerObjects[i];

            if (corners[i].isHarbor)
            {
                GameObject harbor = Instantiate(HarborPrefab, corners[i].position, Quaternion.identity, this.transform);
                harborObjects[i] = harbor;

                /*
                if (corners[i].harborType != Resource.None)
                    harbor.GetComponentInChildren<Text>().text = $"{corners[i].harborType.ToString()}\n2:1";
                else
                    harbor.GetComponentInChildren<Text>().text = "Any\n3:1";
                */
            }
        }

        // Generate and spawn pathGameObjects.
        pathObjects = new GameObject[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            pathObjects[i] = (GameObject)Instantiate(PathPrefab, paths[i].position, paths[i].rotation, this.transform);
            pathObjects[i].GetComponent<PathComponent>().path = paths[i];
            pathObjects[i].transform.name = $"Path{i}";
            paths[i].instance = pathObjects[i];
        }

        // Generate number tokens
        tokenObjects = new GameObject[hexes.Length];
        
        for (int i = 0; i < hexes.Length; i++)
        {
            tokenObjects[i] = (GameObject)Instantiate(NumberTokenPrefab, hexObjects[i].transform.position, Quaternion.Euler(0f,180f,0f));
        }


        CmdBoardReady();
    }

    [Command(requiresAuthority=false)]
    private void CmdBoardReady()
    {
        boardsReady++;
    }

    [Server]
    public IEnumerator RandomizeBoard()
    {
        boardsReady = 0;

        // Randomize resources.
        Resource[] resDistribution = {
            Resource.None,
            Resource.Wood,      Resource.Wood,      Resource.Wood,      Resource.Wood,
            Resource.Wool,      Resource.Wool,      Resource.Wool,      Resource.Wool,
            Resource.Grain,     Resource.Grain,     Resource.Grain,     Resource.Grain,
            Resource.Brick,     Resource.Brick,     Resource.Brick,
            Resource.Ore,       Resource.Ore,       Resource.Ore
        };

        bool regenerateBoard = true;

        while (regenerateBoard)
        {
            regenerateBoard = false;

            // Shuffle resource distribution.
            System.Random _random = new System.Random();
            for (int i = 0; i < resDistribution.Length - 1; i++)
            {
                int j = _random.Next(i, resDistribution.Length);
                Resource temp = resDistribution[i];
                resDistribution[i] = resDistribution[j];
                resDistribution[j] = temp;
            }

            // Check for unacceptable bunching up.
            /*
            Dictionary<Resource,int> borderLimits = new Dictionary<Resource,int>();
            borderLimits.Add(Resource.None, 0);
            borderLimits.Add(Resource.Wood, 1);
            borderLimits.Add(Resource.Wool, 1);
            borderLimits.Add(Resource.Grain, 1);
            borderLimits.Add(Resource.Brick, 0);
            borderLimits.Add(Resource.Ore, 2);

            for (int i = 0; i < numHexes; i++)
            {
                Resource myRes = hexObjects[i].GetComponent<HexComponent>().resource;
                int sameResCount = 0;

                for (int j = 0; j < 6; j++)
                {
                    if (hexes[i].neighbors[j] != null)
                        if (myRes != Resource.Ore && myRes == hexes[i].neighbors[j].instance.GetComponent<HexComponent>().resource)
                            sameResCount++;
                }

                if (sameResCount > borderLimits[myRes])
                    regenerateBoard = true;
            }*/
            yield return null;
        }

        // Number tokens
        // Assign die rolls to tiles
        int[] rollDistribution = {2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12};

        // shuffle rollDistribution array
        int rollp = rollDistribution.Length;
        for (int n = rollp-1; n > 0 ; n--)
        {
            int r = _random.Next(1, n);
            int t = rollDistribution[r];
            rollDistribution[r] = rollDistribution[n];
            rollDistribution[n] = t;
        }

        // Prepare harbors for sync
        bool[] areHarbors = new bool[numCorners];
        Resource[] harborTypes = new Resource[numCorners];
        
        for (int i = 0; i < corners.Length; i++)
        {
            areHarbors[i] = corners[i].isHarbor;

            if (corners[i].isHarbor)
                harborTypes[i] = corners[i].harborType;
        }
        //
        //

        // Wait for all clients to finish loading.
        yield return new WaitUntil(() => boardsReady >= GameManager.playerCount);
        //yield return new WaitForSeconds(2f);

        // when ready
        resources = resDistribution;
        rolls = rollDistribution;

        RpcSyncBoards(resDistribution, rollDistribution, areHarbors, harborTypes);
    }


    [ClientRpc]
    public void RpcSyncBoards(Resource[] newResDistribution, int[] newRollDistribution, bool[] areHarbors, Resource[] harborTypes)
    {
                
        // Resources.
        resources = newResDistribution;

        for (int i = 0; i < numHexes; i++)
        {
            hexObjects[i].GetComponent<HexComponent>().ChangeResource(newResDistribution[i]);
            hexes[i].resource = newResDistribution[i];
        }

        // Hex rolls.
        rolls = newRollDistribution;

        for (int rollIt = 0, i = 0; i < hexes.Length; i++)
        {
            // Desert is fixed at 7
            if (hexObjects[i].GetComponent<HexComponent>().resource == Resource.None)
            {
                hexObjects[i].GetComponent<HexComponent>().roll = 7;
                hexes[i].roll = 7;
            }
            else
            {
                hexObjects[i].GetComponent<HexComponent>().roll = newRollDistribution[rollIt];
                hexes[i].roll = newRollDistribution[rollIt];
                rollIt++;
            }
        }

        // Number token labels.
        for (int i = 0; i < numHexes; i++)
        {
            int hexRoll = hexObjects[i].GetComponent<HexComponent>().roll;

            if (hexRoll == 7)
                tokenObjects[i].SetActive(false);

            if (tokenObjects[i] != null)
                tokenObjects[i].GetComponent<NumberTokenComponent>().SetLabel(hexRoll);
        }

        Resource[] hexResources = new Resource[numHexes];
        int[] hexRolls = new int[numHexes];

        for (int i = 0; i < numHexes; i++)
        {
            hexResources[i] = hexObjects[i].GetComponent<HexComponent>().resource;
            hexRolls[i] = hexObjects[i].GetComponent<HexComponent>().roll;
        }

        // Harbors
        for (int i = 0; i < areHarbors.Length; i++)
        {
            corners[i].isHarbor = areHarbors[i];

            if (corners[i].isHarbor)
            {
                corners[i].harborType = harborTypes[i];
                harborObjects[i].GetComponent<HarborComponent>().SetIcon(corners[i].harborType);
            }
        }

        CmdUpdateServer(hexResources, hexRolls);
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateServer(Resource[] hexResources, int[] hexRolls)
    {
        for (int i = 0; i < numHexes; i++)
        {
            hexes[i].resource = hexResources[i];
            hexes[i].roll = hexRolls[i];
        }

        // Spawn robber. Honestly all hexes should have been spawned
        // and this definitely would be earlier but here we are
        // with our decisions in October 2021
        
        for (int i = 0; i < numHexes; i++)
        {
            if (hexes[i].roll == 7)
            {
                GameObject robberObject = Instantiate(RobberPrefab, hexes[i].position, Quaternion.identity);
                NetworkServer.Spawn(robberObject);
            }
        }

    }

    [Client]
    public static bool IsStealTarget(int targetPlayer)
    {
        Hex robberLocation = null;

        for (int i = 0; i < hexes.Length; i++)
        {
            if (hexes[i].robbed)
            {
                robberLocation = hexes[i];
            }
        }

        if (robberLocation == null)
        {
            Debug.Log("robberLocation is null!");
        }

        foreach (Corner c in robberLocation.corners)
        {
            if (c.playerOwner != GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().currentTurn && c.playerOwner == targetPlayer)
            {
                Debug.Log($"Player {targetPlayer} is a steal target");
                return true;
            }
        }

        Debug.Log($"Player {targetPlayer} is not a steal target");
        return false;
    }


    ///////////////
    // MOUSE CONTROLS

    [Client]
    public static GameObject HexUnderMouse()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 8)))
            return hitInfo.rigidbody.gameObject;
        return null;
    }

    [Client]
    public static Vector3 HexFreePositionUnderMouse()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 8)))
        {
            Vector3 returnPosition = new Vector3(hitInfo.point.x, 0f, hitInfo.point.z);
            return returnPosition;
        }
        return Vector3.zero;
    }

    [Client]
    public static GameObject CornerUnderMouse()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 9)))
            return hitInfo.rigidbody.gameObject;
        return null; 
    }

    [Client]
    public static Path PathUnderMouse()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            return hitInfo.rigidbody.gameObject.GetComponentInChildren<PathComponent>().path;
        return null; 
    }



    // This function must return null to properly set hex neighbors (lack thereof)
    public Hex GetHexAt(int q, int r)
    {
        for (int i = 0; i < hexes.Length; i++)
        {
            if (hexes[i].Q == q && hexes[i].R == r) 
                return hexes[i];
        }
        return null;
    }


    // Uses int direction indexing
    //  0: NW   3: SE
    //  1: NE   4: SW
    //  2: E    5: W
    public Hex GetHexNeighbor(Hex h, int direction)
    {
        direction = direction % 6;
        
        if (h == null)
        {
            Debug.Log("GetHexNeighbor: null hex supplied");
            return null;
        }
        
        if (direction < 0 || direction > 5)
        {
            Debug.Log("GetHexNeighbor: invalid direction");
            return null;
        }
        
        if (h.neighbors[direction] != null)
            return h.neighbors[direction];
        else
            return null;
    }

    public static int GetNumVillages()
    {
        int villageCount = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            if (corners[i].owned)
                villageCount++;
        }
        return villageCount;
    }

    public static int GetNumRoads()
    {
        int roadCount = 0;
        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i].owned)
                roadCount++;
        }
        return roadCount;
    }

    public static int VillagesOwnedBy(int playerIndex)
    {
        int count = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            if (corners[i].owned && corners[i].playerOwner == playerIndex && corners[i].devLevel == 1)
                count++;
        }
        return count;
    }
    public static int CitiesOwnedBy(int playerIndex)
    {
        int count = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            if (corners[i].owned && corners[i].playerOwner == playerIndex && corners[i].devLevel == 2)
                count++;
        }
        return count;
    }


    public static int longestRoad = 0;
    public static int longestRoadOwner = 0;
    public static string longestRoadSummary = "";
    public static List<Path> longestRoadSummaryRoute = new List<Path>();

    public static int LongestRoadFinder()
    {
        List<Path> traversedPaths = new List<Path>();

        int roadLength = 0;
        int roadHead = 0;
        string pathSummary = "";
        List<Path> pathSummaryRoute = new List<Path>();

        for (int i = 0; i < paths.Length; i++)
        {
            traversedPaths.Clear();

            // Head of road.
            if (paths[i].owned)
            {
                roadHead = paths[i].idNum;
                pathSummary = "";
                pathSummaryRoute = new List<Path>();
                Traverse(roadHead, null, paths[i], paths[i].playerOwner, traversedPaths, roadLength, pathSummary, pathSummaryRoute);
            }

        }

        // test announce longest pathSummaryRoute to then try with LineRenderer
        foreach (Path p in pathSummaryRoute)
            Debug.Log(p.idNum);

        return longestRoadOwner;
    }

    public static void Traverse(int roadHead, Path twoRoadsAgo, Path prevRoad, int roadOwner, List<Path> traversedPaths, int roadLength, string pathSummary, List<Path> pathSummaryRoute)
    {
        traversedPaths.Add(prevRoad);
        roadLength += 1;
        pathSummary += $"{prevRoad.idNum.ToString()} ";
        pathSummaryRoute.Add(prevRoad);
        Debug.Log($"(Player {roadOwner}) Road length of {roadLength}: {pathSummary}");

        if (roadLength >= 5 && roadLength > longestRoad)
        {
            // New longest road owner!
            longestRoad = roadLength;
            longestRoadSummary = pathSummary;
            longestRoadSummaryRoute = pathSummaryRoute;
            longestRoadOwner = prevRoad.playerOwner;
        }

        foreach (Path p in prevRoad.connectedPaths)
        {
            if (p.owned && p.playerOwner == roadOwner)
            {

                if (!traversedPaths.Contains(p))
                {
                    if (twoRoadsAgo == null)
                    {
                        Traverse(roadHead, prevRoad, p, roadOwner, traversedPaths, roadLength, pathSummary, pathSummaryRoute);
                    }
                    else if (!twoRoadsAgo.connectedPaths.Contains(p))
                    {
                        Traverse(roadHead, prevRoad, p, roadOwner, traversedPaths, roadLength, pathSummary, pathSummaryRoute);
                    }
                }
            }
            
        }

    }

}





