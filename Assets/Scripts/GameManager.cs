using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// GameObject is tagged as "GameManager"
// NetworkIdentity is set to "server-only"
//
// This is spawned by the NetworkManager as server-only. Tagged as 'GameManager'.
// Holds the game state variables. Clients access it by calling a command that
// executes GameObject.FindGameObjectWithTag("GameManager").

public enum Resource { None, Brick, Grain, Ore, Wood, Wool }

public class GameManager : NetworkBehaviour
{
    [Header("Test Settings")]
    public bool skipSetupPhase = false;
    public bool fastRoll = false;
    public int startingWood;
    public int startingWool;
    public int startingBrick;
    public int startingGrain;
    public int startingOre;

    [Header("Audio Files")]
    public AudioSource sfxDieRoll;

    [Header("Remaining Vars")]
    [SyncVar] public int syncPlayerCount;
    public static int playerCount;

    [SyncVar] public int currentTurn = 0;

    public enum State { ROLL, ROBBER, ENDROBBER, STEAL, IDLE, BUILD, TRADE, DEVCARD }
    [SyncVar] public State GameState;

    public SyncDictionary<int, NetworkIdentity> playerIds = new SyncDictionary<int, NetworkIdentity>();
    public SyncDictionary<int, string> playerNames = new SyncDictionary<int, string>();
    public readonly SyncDictionary<int, List<Resource>> playerResources = new SyncDictionary<int, List<Resource>>();
    public SyncDictionary<int, int> playerPublicVP = new SyncDictionary<int, int>();
    public SyncDictionary<int, int> playerPrivateVP = new SyncDictionary<int, int>();
    public SyncDictionary<int, int> stillToDiscard = new SyncDictionary<int, int>();

    public GameObject gameBoardPrefab;
    
    public static Resource[] resourceSortOrder = { Resource.Wood, Resource.Brick, Resource.Wool, Resource.Grain, Resource.Ore };

    private void Update()
    {
        // Checks corner stats
        /*
        if (GameBoard.CornerUnderMouse() != null)
        {
            Corner c = GameBoard.CornerUnderMouse().GetComponent<CornerComponent>().corner;
            Debug.Log($"{c.idNum}: {c.owned}, {c.playerOwner}");
        }
        */

        // Debug commands.

        // Shift+P: force idle
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            CmdDebugForceIdle();
            
    }

    [Command(requiresAuthority=false)]
    public void CmdDebugForceIdle()
    {
        GameState = State.IDLE;
    }

    // Only the server copy does anything during Start(), to initialize and sync
    // the game state variables to the clients. Formerly, this object was sever-only,
    // but I wanted easy access to read these core variables from the clients.
    private void Start()
    {
        playerCount = syncPlayerCount;
        
        if (isServer)
        {

            // Set up dummy names
            for (int index = 1; index <= playerCount; index++)
            {
                string name = "null";

                switch (index)
                {
                    case 1:
                        name = "Alsett"; break;
                    case 2:
                        name = "Anderswo"; break;
                    case 3:
                        name = "fritz"; break;    
                }
                playerNames[index] = name;
            }

            // Set up playerResources
            for (int index = 1; index <= playerCount; index++)
            {
                playerResources[index] = new List<Resource>();
            }

            // Set up playerVP
            for (int index = 1; index <= playerCount; index++)
            {
                playerPublicVP[index] = 0;
                playerPrivateVP[index] = 0;
            }

            // Set up stillToDiscard (keeps track of remaining
            // cards to discard during robber phase)
            for (int index = 1; index <= playerCount; index++)
            {
                stillToDiscard[index] = 0;
            }

            // Spawn the game board (good luck)
            GameObject gameBoard = Instantiate(gameBoardPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(gameBoard);

            StartCoroutine(WaitUntilReady());
        }

    }

    [Server]
    private IEnumerator WaitUntilReady()
    {
        yield return new WaitUntil(() => playerIds != null);
        yield return new WaitForSeconds(1f);

        if (!skipSetupPhase)
        {
            StartCoroutine(SetupPhase());
            currentTurn = 1;
        }
        else
        {
            GameState = State.IDLE;
            currentTurn = 1;
        }

        for (int i = 1; i <= playerCount; i++)
        {
            AddResource(i, Resource.Wood, startingWood);
            AddResource(i, Resource.Brick, startingBrick);
            AddResource(i, Resource.Wool, startingWool);
            AddResource(i, Resource.Grain, startingGrain);
            AddResource(i, Resource.Ore, startingOre);
        }
        
    }


    [Server]
    private IEnumerator SetupPhase()
    {
        for (int i = 1; i <= playerCount; i++)
        {
            currentTurn = i;

            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Village");
            yield return new WaitUntil(() => GameBoard.GetNumVillages() >= i);
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Road");
            yield return new WaitUntil(() => GameBoard.GetNumRoads() >= i);
        }
        
        for (int i = playerCount, j = 1; i > 0; i--)
        {
            currentTurn = i;

            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Village");
            yield return new WaitUntil(() => GameBoard.GetNumVillages() >= playerCount+j);
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Road");
            yield return new WaitUntil(() => GameBoard.GetNumRoads() >= playerCount+j);
            j++;
        }

        DistributeStartingResources();
        yield return null;
    }

    [TargetRpc]
    private void RpcGiveStarterBlueprint(NetworkConnection target, string blueprint)
    {    
        StartCoroutine(Build(blueprint, true));
    }
    
    [TargetRpc]
    private void RpcGiveBlueprint(NetworkConnection target, string blueprint)
    {    
        StartCoroutine(Build(blueprint, false));
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestBuild(int builderIndex, string blueprint)
    {
        switch (blueprint)
        {
            case "Road":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "Road");
                break;
            case "Village":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "Village");
                break;
            case "City":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "City");
                break;
        }
    }
    
    /////////////////////////////////
    // BUILD (road, village, city)
    /////////////////////////////////
    public GameObject roadBlueprint;
    public GameObject villageBlueprint;
    public GameObject cityBlueprint;

    public GameObject roadPrefab;
    public GameObject villagePrefab;
    public GameObject cityPrefab;

    public Material matBlue;
    public Material matRed;
    public Material matGreen;
    public Material matYellow;

    public Material matTransparentBlue;
    public Material matTransparentRed;
    public Material matTransparentGreen;
    public Material matTransparentYellow;

    [Client]
    public IEnumerator Build(string selectedBlueprint, bool starter = false)
    {
        GameState = State.BUILD;

        GameObject blueprintPrefab = null;

        switch (selectedBlueprint)
        {
            case "Road":
                blueprintPrefab = roadBlueprint;
                break;
            case "Village":
                blueprintPrefab = villageBlueprint;
                break;
            case "City":
                blueprintPrefab = cityBlueprint;
                break;
        }

        // The blueprint GameObject handles mouse movement.
        // This code handles mouse clicks, legality, and resources.
        Cursor.visible = false;
        GameObject blueprintObj = (GameObject)Instantiate(blueprintPrefab);
        blueprintObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(PlayerController.playerIndex, true);

        while (GameState == State.BUILD)
        {

            // On left-click, attempt to build.
            if (Input.GetMouseButtonDown(0))
            {
                // Check if location is legal.
                bool legalPlacement = true;

                Path proposedPath = null;
                Corner proposedCorner = null;

                switch (selectedBlueprint)
                {
                    case "Road":

                        if (GameBoard.PathUnderMouse() != null)
                            proposedPath = GameBoard.PathUnderMouse();

                        // Must click on a path
                        if (proposedPath == null)
                        {
                            Debug.Log("Didn't click on any path");
                            legalPlacement = false;
                            break;
                        }

                        // All roads can never overlap an existing road
                        if (proposedPath.owned)
                        {
                            Debug.Log("Didn't click on an open path");
                            legalPlacement = false;
                            //break;
                        }

                        // All roads can never not border an owned village or road
                        if (proposedPath.myCorner.playerOwner != PlayerController.playerIndex)
                        {
                            bool borderingRoad = false;

                            foreach (Path p in proposedPath.connectedPaths)
                                borderingRoad = borderingRoad || (p.playerOwner == PlayerController.playerIndex);
                        
                            if (!borderingRoad)
                                Debug.Log("Not adjacent to an owned village or road");
                            
                            legalPlacement = legalPlacement && borderingRoad;
                        }

                        // Special starter case
                        // Check other paths on the corner to make sure they are empty
                        // and force placement of 2nd starter road on 2nd starter village
                        if (starter)
                        {
                            foreach (Path p in proposedPath.connectedPaths)
                            {
                                if (p.playerOwner == PlayerController.playerIndex)
                                {
                                    Debug.Log("Cannot place 2nd starter road on 1st village");
                                    legalPlacement = false;
                                }
                            }
                        }

                        // Prevent building roads across owned enemy villages
                        // If the far side path (the connected path not on the corner)
                        // is owned by activePlayer, it's not straddling but rather
                        // from other direction and therefore legal. Illegal otherwise.
                        if (proposedPath.myCorner.owned == true && proposedPath.myCorner.playerOwner != PlayerController.playerIndex)
                        {
                            Path farSidePath = null;
                            foreach (Path p in proposedPath.connectedPaths)
                            {
                                if (p.myCorner != proposedPath.myCorner)
                                    farSidePath = p;
                            }

                            if (farSidePath.playerOwner != PlayerController.playerIndex)
                            {
                                Debug.Log("Cannot straddle enemy village");
                                legalPlacement = false;
                            }
                        }
                        
                        // Build road if legal
                        if (legalPlacement || Input.GetKey(KeyCode.LeftShift))
                        {
                            Vector3 blueprintPos = blueprintObj.transform.position;
                            Quaternion blueprintRot = blueprintObj.transform.rotation;

                            Cursor.visible = true;
                            GameObject.Destroy(blueprintObj);
                            GameState = State.IDLE;

                            if (!starter)
                            {
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wood);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Brick);
                            }

                            CmdBuildRoad(proposedPath.idNum, PlayerController.playerIndex, blueprintPos, blueprintRot);
                        }
                        break;

                    
                    case "Village":
                        
                        if (GameBoard.CornerUnderMouse() != null)
                            proposedCorner = GameBoard.CornerUnderMouse().GetComponent<CornerComponent>().corner;

                        // Must click on a corner
                        if (proposedCorner == null)
                        {
                            Debug.Log("Didn't click on any corner");
                            legalPlacement = false;
                            break;
                        }

                        // Can't place on an existing village
                        if (proposedCorner.owned)
                        {
                            Debug.Log("Clicked on an already owned corner");
                            legalPlacement = false;
                            break;
                        }
                        
                        // Villages can't be neighbors
                        if (!proposedCorner.owned)
                        {
                            foreach(Corner c in proposedCorner.neighborCorners)
                            {
                                if (c.owned)
                                {
                                    Debug.Log("Too close to an existing village");
                                    legalPlacement = false;
                                }
                            }
                        }
                        
                        // All non-starter villages must border a road
                        if (!starter)
                        {
                            bool borderingRoad = false;

                            foreach(Path p in proposedCorner.neighborPaths.Values)
                                borderingRoad = borderingRoad || (p.playerOwner == PlayerController.playerIndex);
                                 
                            if (!borderingRoad)
                                Debug.Log("Must border a road");

                            legalPlacement = legalPlacement && borderingRoad;
                        }

                        if (legalPlacement || Input.GetKey(KeyCode.LeftShift))
                        {
                            Vector3 blueprintPos = blueprintObj.transform.position;
                            Quaternion blueprintRot = blueprintObj.transform.rotation;
                            
                            Cursor.visible = true;
                            GameObject.Destroy(blueprintObj);
                            GameState = State.IDLE;

                            if (!starter)
                            {
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wood);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Brick);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wool);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Grain);
                            }

                            CmdBuildVillage(proposedCorner.idNum, PlayerController.playerIndex, blueprintPos, blueprintRot);
                        }
                        break;


                    case "City":
                        
                        if (GameBoard.CornerUnderMouse() != null)
                            proposedCorner = GameBoard.CornerUnderMouse().GetComponent<CornerComponent>().corner;

                        // Must click on a corner
                        if (proposedCorner == null)
                        {
                            Debug.Log("Didn't click on any corner");
                            legalPlacement = false;
                            break;
                        }

                        // Must click on an owned village
                        if (proposedCorner.playerOwner != PlayerController.playerIndex)
                        {
                            Debug.Log("Didn't click on an owned village");
                            legalPlacement = false;
                        }

                        // Can't click on an existing city
                        if (proposedCorner.devLevel == 2)
                        {
                            Debug.Log("Can't build on an existing city");
                            legalPlacement = false;
                        }

                        if (legalPlacement || Input.GetKey(KeyCode.LeftShift))
                        {
                            Vector3 blueprintPos = blueprintObj.transform.position;
                            Quaternion blueprintRot = blueprintObj.transform.rotation;                

                            Cursor.visible = true;
                            GameObject.Destroy(blueprintObj);
                            GameState = State.IDLE;

                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Grain);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Grain); 

                            CmdBuildCity(proposedCorner.idNum, PlayerController.playerIndex, blueprintPos, blueprintRot);
                        }
                        break;
                }
                
            }
            
    

            // On right-click, cancel build. Cancelling is disabled for starter placements.
            if (Input.GetMouseButtonDown(1))
            {
                if (starter)
                    Debug.Log("Can't cancel a starter blueprint!");
                else
                {
                    Debug.Log("Cancelled blueprint");
                    
                    Cursor.visible = true;
                    GameObject.Destroy(blueprintObj);
                    GameState = State.IDLE;
                }
            }

           
            yield return null;
        }

        yield return null;
    }


    [Command(requiresAuthority = false)]
    private void CmdBuildRoad(int pathId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Path p = GameBoard.paths[pathId];

        p.owned = true;
        p.playerOwner = builderIndex;

        RpcBuildRoad(pathId, builderIndex, blueprintPos, blueprintRot);
    }

    [ClientRpc]
    private void RpcBuildRoad(int pathId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Path p = GameBoard.paths[pathId];

        p.owned = true;
        p.playerOwner = builderIndex;

        GameObject buildingObj = (GameObject)Instantiate(roadPrefab, p.position, p.rotation);
        buildingObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex);
    }

    [Command(requiresAuthority = false)]
    private void CmdBuildVillage(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 1;

        RpcBuildVillage(cornerId, builderIndex, blueprintPos, blueprintRot);
    }

    [ClientRpc]
    private void RpcBuildVillage(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 1;

        GameObject buildingObj = (GameObject)Instantiate(villagePrefab, blueprintPos, blueprintRot);
        buildingObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex);
    }

    [Command(requiresAuthority = false)]
    private void CmdBuildCity(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 2;

        RpcBuildCity(cornerId, builderIndex, blueprintPos, blueprintRot);
    }

    [ClientRpc]
    private void RpcBuildCity(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 2;

        GameObject buildingObj = (GameObject)Instantiate(cityPrefab, blueprintPos, blueprintRot);
        buildingObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex);
    }




    // Assigns player numbers to materials
    [Client]
    private Material GetPlayerMaterial(int player, bool transparent = false)
    {
        switch (player)
        {
            case 1:
                if (transparent)
                    return matTransparentBlue;
                return matBlue;

            case 2:
                if (transparent)
                    return matTransparentRed;
                return matRed;

            case 3:
                if (transparent)
                    return matTransparentGreen;
                return matGreen;
            
            case 4:
                if (transparent)
                    return matTransparentYellow;
                return matYellow;
        }

        if (transparent)
            return matTransparentBlue;
        return matBlue;
    }



    // After the server adds a resource, it calls a ClientRpc to get
    // the event to fire on all handlers on each client.
    public static event Action<int, List<Resource>> onHandChanged;

    [Server]
    private void AddResource(int playerIndex, Resource res, int quantity = 1)
    {
        List<Resource> newResources = playerResources[playerIndex];
        
        for (int i = 0; i < quantity; i++)
            newResources.Add(res);
        playerResources[playerIndex] = newResources;

        RpcChangeResource(playerIndex, newResources);
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveResource(int playerIndex, Resource res)
    {
        RemoveResource(playerIndex, res);
    }

    [Server]
    private void RemoveResource(int playerIndex, Resource res)
    {
        List<Resource> newResources = playerResources[playerIndex];
        
        if (newResources.Contains(res))
            newResources.Remove(res);
        else
            Debug.LogWarning("Player doesn't have a resource to remove");

        playerResources[playerIndex] = newResources;

        RpcChangeResource(playerIndex, newResources);
    }

    [Server]
    private void DistributeStartingResources()
    {
        for (int i = 0; i < GameBoard.numCorners; i++)
        {
            if (GameBoard.corners[i].owned)
            {
                foreach (Hex h in GameBoard.corners[i].neighborHexes)
                {
                    AddResource(GameBoard.corners[i].playerOwner, GameBoard.resources[h.id]);
                }
            }
        }
    }

    [Server]
    private void DistributeResources(int rollResult)
    {
        for (int i = 0; i < GameBoard.numHexes; i++)
        {
            if (GameBoard.hexes[i].roll == rollResult && GameBoard.hexes[i].robbed) { Debug.Log($"Hex {i} is robbed!"); }
            if (GameBoard.hexes[i].roll == rollResult && !GameBoard.hexes[i].robbed)
            {
                foreach (Corner c in GameBoard.hexes[i].corners)
                {
                    if (c.owned)
                    {
                        AddResource(c.playerOwner, GameBoard.hexes[i].resource);
                    }
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRecalculateVP()
    {
        for (int i = 1; i <= playerCount; i++)
        {
            playerPrivateVP[i] = 0;
            playerPublicVP[i] = 0;
        }           

        // 1 for villages, 2 for cities
        for (int i = 0; i < GameBoard.numCorners; i++)
        {
            Corner c = GameBoard.corners[i];
            

            if (c.devLevel != 0)
            {
                int tempVP = playerPrivateVP[c.playerOwner];
                tempVP += c.devLevel;
                playerPrivateVP[c.playerOwner] = tempVP;

                tempVP = playerPublicVP[c.playerOwner];
                tempVP += c.devLevel;
                playerPublicVP[c.playerOwner] = tempVP;
            }
        }

    }

    [ClientRpc]
    private void RpcChangeResource(int playerIndex, List<Resource> newResources)
    {
        onHandChanged?.Invoke(playerIndex, newResources);
    }


    public void OnEnable()
    {

    }


    // Message board system
    public static event Action<string> postMessage;

    // Game state events
    public static event Action<int> onNextTurn;
    public static event Action<int, int> onRollDie;

    
    [Server]
    public void RequestNextTurn(int requestor)
    {          

        GameState = State.ROLL;

        int nextTurn = requestor + 1;
        if (nextTurn > playerCount)
            nextTurn = 1;
        currentTurn = nextTurn;

        RpcAdvanceNextTurn(currentTurn);
        RollDie();
    }

    [ClientRpc]
    private void RpcAdvanceNextTurn(int currentTurn)
    {
        onNextTurn?.Invoke(currentTurn);
        postMessage?.Invoke($"It is now Player {currentTurn}'s turn.");
    }
    
    // Trigger die roll animation on clients.
    [Server]
    private void RollDie() => RpcRollDie(UnityEngine.Random.Range(1,7), UnityEngine.Random.Range(1,7));

    [ClientRpc]
    private void RpcRollDie(int roll1, int roll2)
    {
        sfxDieRoll.Play();
        onRollDie?.Invoke(roll1, roll2);
        postMessage?.Invoke($"Rolling die...");
    }

    [Server]
    public void RequestFinishRoll(int result)
    {
        if (result != 7)
        {
            // Distribute new resources.
            DistributeResources(result);

            // Advance game state.
            GameState = State.IDLE;
        }
        else
        {
            // Advance game state.
            GameState = State.ROBBER;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestEndRobber(int hexId)
    {
        GameState = State.ENDROBBER;

        // Unset and reset "robbed" on hexes
        foreach (Hex h in GameBoard.hexes)
        {
            h.robbed = false;

            if (h.id == hexId)
            {
                h.robbed = true;
                Debug.Log($"robber is now on hex {hexId}");
            }
        }

        RpcUpdateRobbed(hexId);

        // Advance game state
        // But bail if no steal targets.
        List<int> stealTargets = new List<int>();

        foreach (Corner c in GameBoard.hexes[hexId].corners)
        {
            if (c.owned && c.playerOwner != currentTurn && playerResources[c.playerOwner].Count > 0)
                stealTargets.Add(c.playerOwner);
        }

        if (stealTargets.Count == 0)
        {
            Debug.Log("no steal targets, on to IDLE");
            GameState = State.IDLE;
        }
        else
        {
            Debug.Log("there are steal targets");
            GameState = State.STEAL;
        }
    }

    [ClientRpc]
    public void RpcUpdateRobbed(int hexId)
    {
        foreach (Hex h in GameBoard.hexes)
        {
            h.robbed = false;

            if (h.id == hexId)
                h.robbed = true;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestStealRandom(int targetPlayer, int thiefPlayer)
    {
        List<Resource> toStealFrom = playerResources[targetPlayer];
        Resource stolenRes = Resource.None;

        int i = 0;
        int resIndex = UnityEngine.Random.Range(0, toStealFrom.Count);
        foreach (Resource toStealRes in toStealFrom)
        {
            if (i == resIndex)
            {
                stolenRes = toStealRes;
            }
            i++;
        }

        if (stolenRes != Resource.None)
        {
            RemoveResource(targetPlayer, stolenRes);
            AddResource(thiefPlayer, stolenRes);
            Debug.Log($"Player {thiefPlayer} stole a {stolenRes} from player {targetPlayer}");
        }
        
        // Advance game state.
        GameState = State.IDLE;
    }



}
