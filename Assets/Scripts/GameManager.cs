using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

// GameObject is tagged as "GameManager"
// NetworkIdentity is set to "server-only"
//
// This is spawned by the NetworkManager as server-only. Tagged as 'GameManager'.
// Holds the game state variables. Clients access it by calling a command that
// executes GameObject.FindGameObjectWithTag("GameManager").

public enum Resource { None, Brick, Grain, Ore, Wood, Wool }
public enum Dev { None, Knight, KnightRevealed, Roads, Plenty, Monopoly, VP, VPRevealed, PD }

public class GameManager : NetworkBehaviour
{
    public GameObject pointerPrefab;
    public GameObject tooltipCanvas;

    [Header("Game Settings")]
    public int VPtoWin = 11;
    public bool skipSetupPhase = false;
    public bool fastRoll = false;
    public int startingWood;
    public int startingWool;
    public int startingBrick;
    public int startingGrain;
    public int startingOre;

    [Header("Remaining Vars")]
    [SyncVar] public int syncPlayerCount;
    public static int playerCount;

    [SyncVar] public int currentTurn = 0;
    [SyncVar] public int serverTotalTurns = 0;

    public SyncDictionary<int, NetworkIdentity> playerIds = new SyncDictionary<int, NetworkIdentity>();
    public SyncDictionary<int, string> playerNames = new SyncDictionary<int, string>();
    public readonly SyncDictionary<int, List<Resource>> playerResources = new SyncDictionary<int, List<Resource>>();
    public readonly Dictionary<int, int> playerCoins = new Dictionary<int, int>();
    public readonly SyncDictionary<int, List<Dev>> playerDevCards = new SyncDictionary<int, List<Dev>>();
    public SyncDictionary<int, int> playerPublicVP = new SyncDictionary<int, int>();
    public SyncDictionary<int, int> playerPrivateVP = new SyncDictionary<int, int>();
    public SyncDictionary<int, int> stillToDiscard = new SyncDictionary<int, int>();
    public List<Dev> devCardsBoughtThisTurn = new List<Dev>();

    public Dictionary<int, List<Resource>> playerSelectedCards = new Dictionary<int, List<Resource>>();
    public Dictionary<int, bool> playerOfferingTrade = new Dictionary<int, bool>();


    [SyncVar] public int largestArmyOwner = 0;
    [SyncVar] public int longestRoadOwner = 0;
    [SyncVar] public int harbormasterOwner = 0;

    public GameObject gameBoardPrefab;
    
    public static Resource[] resourceSortOrder = { Resource.Wood, Resource.Brick, Resource.Wool, Resource.Grain, Resource.Ore };
    public static Dev[] devCardSortOrder = { Dev.Knight, Dev.Roads, Dev.Plenty, Dev.Monopoly, Dev.VP, Dev.KnightRevealed, Dev.VPRevealed };
  
    //------------------------------------------------------------------

    public enum State { NONE, SETUP, ROLL, DISCARD, ROBBER, ENDROBBER, STEAL, IDLE, BUILD, TRADE, WINNER }
    [SyncVar] public State GameState = State.IDLE;
    [SyncVar] public bool setup = false;

    //------------------------------------------------------------------

    private void Awake()
    {        
        GameState = State.IDLE;
    }

    private void Update()
    {
        if (isClient) { CmdStateDesyncCheck(GameState, false); }

        Cursor.visible = (GameState != State.BUILD);
    }

    private void LateUpdate()
    {
        if (isClient) { CmdStateDesyncCheck(GameState, true); }
    }

    //------------------------------------------------------------------


    [Command(requiresAuthority = false)]
    public void CmdStateDesyncCheck(State clientState, bool isLateUpdate, NetworkConnectionToClient sender = null)
    {
        if (clientState != GameState && GameState != State.NONE)
        {
            if (!isLateUpdate)
                Debug.LogWarning($"[{playerNames[playerIds.FirstOrDefault(x => x.Value == sender.identity).Key]}] State Desync Error: Client was {clientState} on call whereas server is {GameState}");
            
            // hiding late update messages since they clutter the log
            //else
               // Debug.LogWarning($"[{playerNames[playerIds.FirstOrDefault(x => x.Value == sender.identity).Key]}] LateUpdate State Desync Error: Client was {clientState} on call whereas server is {GameState}");

            //RpcCorrectStateDesync(sender, GameState);
            RpcCorrectStateDesync(GameState);
        }
    }

    //[TargetRpc]
    [ClientRpc]
    //private void RpcCorrectStateDesync(NetworkConnection target, State serverState)
    private void RpcCorrectStateDesync(State serverState)
    {
        GameState = serverState;
    }

    private State StateChangeError(State fromState, State toState, string extraMessage = "")
    {
        Debug.LogWarning($"State change error from {fromState} to {toState}! At time of call it was {GameState}");
        
        if (extraMessage != "")
            Debug.LogWarning(extraMessage);
        return State.IDLE;
    }

    //------------------------------------------------------------------

    private void StateNoneToIdle(string extraMessage = "") => GameState = (GameState == State.NONE) ? State.IDLE : StateChangeError(State.NONE, State.IDLE, extraMessage);
    private void StateIdleToRoll(string extraMessage = "") => GameState = (GameState == State.IDLE) ? State.ROLL : StateChangeError(State.IDLE, State.ROLL, extraMessage);
    private void StateRollToIdle(string extraMessage = "") => GameState = (GameState == State.ROLL) ? State.IDLE : StateChangeError(State.ROLL, State.IDLE, extraMessage);
    private void StateRollToDiscard(string extraMessage = "") => GameState = (GameState == State.ROLL) ? State.DISCARD : StateChangeError(State.ROLL, State.DISCARD, extraMessage);
    private void StateRollToRobber(string extraMessage = "") => GameState = (GameState == State.ROLL) ? State.ROBBER : StateChangeError(State.ROLL, State.DISCARD, extraMessage);
    private void StateDiscardToRobber(string extraMessage = "") => GameState = (GameState == State.DISCARD) ? State.ROBBER : StateChangeError(State.DISCARD, State.ROBBER, extraMessage);
    private void StateRobberToEndRobber(string extraMessage = "") => GameState = (GameState == State.ROBBER) ? State.ENDROBBER : StateChangeError(State.ROBBER, State.ENDROBBER, extraMessage);
    private void StateEndRobberToIdle(string extraMessage = "") => GameState = (GameState == State.ENDROBBER) ? State.IDLE : StateChangeError(State.ENDROBBER, State.IDLE, extraMessage);
    private void StateEndRobberToSteal(string extraMessage = "") => GameState = (GameState == State.ENDROBBER) ? State.STEAL : StateChangeError(State.ENDROBBER, State.STEAL, extraMessage);
    private void StateStealToIdle(string extraMessage = "") => GameState = (GameState == State.STEAL) ? State.IDLE : StateChangeError(State.STEAL, State.IDLE, extraMessage);
    private void StateIdleToBuild(string extraMessage = "") => GameState = (GameState == State.IDLE) ? State.BUILD : StateChangeError(State.IDLE, State.BUILD, extraMessage);
    private void StateIdleToRobber(string extraMessage = "") => GameState = (GameState == State.IDLE) ? State.ROBBER : StateChangeError(State.IDLE, State.ROBBER, extraMessage);
    private void StateBuildToIdle(string extraMessage = "") => GameState = (GameState == State.BUILD) ? State.IDLE : StateChangeError(State.BUILD, State.IDLE, extraMessage);

    [Command (requiresAuthority = false)]
    public void CmdPlayAudio(int clipIndex)
    {
        RpcPlayAudio(clipIndex);
    }

    [ClientRpc]
    public void RpcPlayAudio(int clipIndex)
    {    
        GetComponentInChildren<AudioManager>().PlayAudio(clipIndex);
    }

    public void PlayLocalAudio(int clipIndex)
    {
        GetComponentInChildren<AudioManager>().PlayAudio(clipIndex);
    }

    [TargetRpc]
    public void RpcLocalAudio(NetworkConnection target, int clipIndex)
    {
        GetComponentInChildren<AudioManager>().PlayAudio(clipIndex);
    }

    [Command(requiresAuthority = false)]
    public void CmdInterruptAudio()
    {
        RpcInterruptAudio();
    }

    [ClientRpc]
    public void RpcInterruptAudio()
    {
        GetComponentInChildren<AudioManager>().InterruptAudio();
    }



    // Only the server copy does anything during Start(), to initialize and sync
    // the game state variables to the clients. Formerly, this object was sever-only,
    // but I wanted easy access to read these core variables from the clients.
    private void Start()
    {
        playerCount = syncPlayerCount;
        
        // here because syncdict wasn't working
        for (int index = 1; index <= playerCount; index++)
        {
            playerSelectedCards[index] = new List<Resource>();
            playerOfferingTrade[index] = false;

            playerCoins[index] = 3;
        }   

        if (isServer)
        {
            // Set up playerResources
            for (int index = 1; index <= playerCount; index++)
            {
                playerResources[index] = new List<Resource>();
            }            

            // Set up playerDevCards
            for (int index = 1; index <= playerCount; index++)
            {
                playerDevCards[index] = new List<Dev>();
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
    
        /*
        for (int i = 1; i <= playerCount; i++)
        {
            GameObject pointer = Instantiate(pointerPrefab);
            pointer.GetComponent<Pointer>().owner = i;
            NetworkServer.Spawn(pointer);
        }
        */   

        for (int i = 1; i <= playerCount; i++)
        {
            AddResource(i, Resource.Wood, startingWood);
            AddResource(i, Resource.Brick, startingBrick);
            AddResource(i, Resource.Wool, startingWool);
            AddResource(i, Resource.Grain, startingGrain);
            AddResource(i, Resource.Ore, startingOre);
        }

        if (!skipSetupPhase)
            StartCoroutine(SetupPhase());
    }

    [Server]
    private IEnumerator SetupPhase()
    {
        setup = true;

        StateNoneToIdle("Starting setup, NONE -> IDLE");

        // First set of starter buildings.
        for (int i = 1; i <= playerCount; i++)
        {
            currentTurn = i;
            RpcLocalAudio(playerIds[i].connectionToClient, 19);

            RpcNotify(new NotificationMessage {
                targetPlayer = i,
                privateMessage = "Place your starting buildings!",
                publicMessage = $"{playerNames[i]} is placing their starting buildings."
            });

            StateIdleToBuild("Starter blueprint IDLE -> BUILD");
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Village");
            yield return new WaitUntil(() => GameBoard.GetNumVillages() >= i);
            
            StateIdleToBuild("Starter blueprint IDLE -> BUILD");
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Road");
            yield return new WaitUntil(() => GameBoard.GetNumRoads() >= i);
        }
        
        // Second set of starter buildings.
        for (int i = playerCount, j = 1; i > 0; i--)
        {
            currentTurn = i;
            RpcLocalAudio(playerIds[i].connectionToClient, 19);

            RpcNotify(new NotificationMessage {
                targetPlayer = i,
                privateMessage = "Place your starting buildings!",
                publicMessage = $"{playerNames[i]} is placing their starting buildings."
            });

            StateIdleToBuild("Starter blueprint IDLE -> BUILD");
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Village");
            yield return new WaitUntil(() => GameBoard.GetNumVillages() >= playerCount+j);
            
            StateIdleToBuild("Starter blueprint IDLE -> BUILD");
            RpcGiveStarterBlueprint(playerIds[i].connectionToClient, "Road");
            yield return new WaitUntil(() => GameBoard.GetNumRoads() >= playerCount+j);
            j++;
        }

        RpcNotifyClear();

        // Distribute starting resources.
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

        setup = false;
        yield return null;

        currentTurn = 0;
        RequestNextTurn();
    }

    [TargetRpc]
    private void RpcGiveStarterBlueprint(NetworkConnection target, string blueprint)
    {    
        StartCoroutine(Build(blueprint, true, false));
    }

    // Game state events
    public static event Action<int> onNextTurn;
    public static event Action<int, int> onRollDie;

    [Command(requiresAuthority = false)]
    public void CmdRequestNextTurn()
    {
        RequestNextTurn();
    }

    public static event Action<NotificationMessage> onNotification;
    public static event Action onNotificationClear;

    [ClientRpc]
    private void RpcNotify(NotificationMessage notification)
    {
        onNotification?.Invoke(notification);
    }

    [TargetRpc]
    private void RpcSingleNotify(NetworkConnection target, NotificationMessage notification)
    {
        onNotification?.Invoke(notification);
    }

    [ClientRpc]
    private void RpcNotifyClear()
    {
        onNotificationClear?.Invoke();
    }


    [Server]
    public void RequestNextTurn()
    {
        RpcNotifyClear();
        
        for (int i = 1; i <= playerCount; i++)
        {
            playerOfferingTrade[i] = false;
            stillToDiscard[i] = 0;
        }
        devCardsBoughtThisTurn.Clear();
        RpcClearDevCardsBoughtThisTurn();

        StateIdleToRoll();

        Debug.Log($"turn before advance: {currentTurn}");
        currentTurn = currentTurn + 1;
        Debug.Log($"turn after +1: {currentTurn}");
        if (currentTurn > playerCount)
            currentTurn = 1;
        Debug.Log($"turn after playerCount check: {currentTurn}");

        serverTotalTurns += 1;
        Debug.Log($"-----TURN {serverTotalTurns}-----");

        RpcAdvanceNextTurn(currentTurn);
        RollDie();
    }

    [ClientRpc]
    private void RpcAdvanceNextTurn(int currentTurn)
    {
        CmdClearSelectedCards();
        onNextTurn?.Invoke(currentTurn);
    }
    
    // Trigger die roll animation on clients.
    [Server]
    private void RollDie()
    {
        int roll1 = UnityEngine.Random.Range(1,7);
        int roll2 = UnityEngine.Random.Range(1,7);

        StartCoroutine(WaitFinishRoll(roll1 + roll2));

        RpcRollDie(roll1, roll2);
        RpcPlayAudio(UnityEngine.Random.Range(1,6));

    }

    [Server]
    private IEnumerator WaitFinishRoll(int result)
    {
        yield return new WaitForSeconds(2f);
        FinishRoll(result);
    }

    [ClientRpc]
    private void RpcRollDie(int roll1, int roll2)
    {
        onRollDie?.Invoke(roll1, roll2);
    }

    [Server]
    public void FinishRoll(int result)
    {
        if (result != 7)
            DistributeResources(result);
        else
            RobberDiscardCheck();
    }

    public static event Action<int, List<Resource>, bool> onIncomeAnimation;
    public static event Action<int, List<Resource>, bool> onLossAnimation;

    [Server]
    private void DistributeResources(int rollResult)
    {
        Hex h = null;
        Dictionary<int, List<Resource>> resIncome = new Dictionary<int, List<Resource>>();
        Dictionary<int, bool> coinIncome = new Dictionary<int, bool>();

        for (int i = 1; i <= playerCount; i++)
        {
            resIncome[i] = new List<Resource>();
            coinIncome[i] = false;
        }

        for (int i = 0; i < GameBoard.numHexes; i++)
        {
            h = GameBoard.hexes[i];

            // Start hex selector animation.
            if (h.roll == rollResult)
                RpcFlickerHexSelector(h.id);

            if (h.roll == rollResult && h.robbed) { Debug.Log($"Hex {i} is robbed!"); }
    

            if (h.roll == rollResult && !h.robbed)
            {
                // Distribute resources.
                foreach (Corner c in h.corners)
                {
                    if (c.owned)
                    {
                        RpcLocalAudio(playerIds[c.playerOwner].connectionToClient, UnityEngine.Random.Range(16,19));
                        
                        AddResource(c.playerOwner, h.resource);
                        
                        // Add 2nd resource for cities
                        if (c.devLevel == 2)
                            AddResource(c.playerOwner, h.resource);

                        // Also add to a batch dictionary for income animations, etc.
                        resIncome[c.playerOwner].Add(h.resource);

                        // 2nd for cities
                        if (c.devLevel == 2)
                            resIncome[c.playerOwner].Add(h.resource);
                    }
                }
            }
        }

        // Check if anyone earned coins.
        for (int i = 1; i <= playerCount; i++)
        {
            if (resIncome[i].Count == 0)
            {
                bool lowesDev = true;
                int playerDev = GameBoard.VillagesOwnedBy(i) + (GameBoard.CitiesOwnedBy(i) * 2);

                for (int j = 1; j <= playerCount; j++)
                {
                    int opponentDev = GameBoard.VillagesOwnedBy(j) + (GameBoard.CitiesOwnedBy(j) * 2);

                    if (playerDev > opponentDev)
                        lowesDev = false;
                }

                if (lowesDev)
                {
                    playerCoins[i] += 1;

                    Debug.Log($"Server: Added a coin for {i}. {playerNames[i]} now has {playerCoins[i]} coins.");
                    RpcAddCoin(i);
                    coinIncome[i] = true;
                }
            }
        }


        // Start income animations.
        for (int i = 1; i <= playerCount; i++)
        {
            RpcIncomeAnimation(i, resIncome[i], coinIncome[i]);
        }
        

        // Advance game state.
        RpcLocalAudio(playerIds[currentTurn].connectionToClient, 19);
        StateRollToIdle();
    }

    [ClientRpc]
    private void RpcFlickerHexSelector(int hexId)
    {
        GameBoard.hexes[hexId].instance.GetComponent<HexComponent>().FlickerHexSelector();
    }

    [ClientRpc]
    private void RpcAddCoin(int playerIndex)
    {
        playerCoins[playerIndex] += 1;

        if (playerIndex == PlayerController.playerIndex)
            PlayLocalAudio(UnityEngine.Random.Range(33,36));
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveCoins(int requestor, int numCoins)
    {
        playerCoins[requestor] -= 5;
        RpcRemoveCoins(requestor, numCoins);
    }

    [ClientRpc]
    private void RpcRemoveCoins(int playerIndex, int numCoins)
    {
        playerCoins[playerIndex] -= 5;
    }
    
    [Command(requiresAuthority = false)]
    public void CmdIncomeAnimation(int targetPlayer, List<Resource> resIncome, bool coinIncome)
    {
        RpcIncomeAnimation(targetPlayer, resIncome, coinIncome);
    }

    [ClientRpc]
    private void RpcIncomeAnimation(int targetPlayer, List<Resource> resIncome, bool coinIncome)
    {
        onIncomeAnimation?.Invoke(targetPlayer, resIncome, coinIncome);
    }

    [Command(requiresAuthority = false)]
    public void CmdLossAnimation(int targetPlayer, List<Resource> resLoss, bool coinLoss)
    {
        RpcLossAnimation(targetPlayer, resLoss, coinLoss);
    }

    
    [ClientRpc]
    private void RpcLossAnimation(int targetPlayer, List<Resource> resLoss, bool coinLoss)
    {
        onLossAnimation?.Invoke(targetPlayer, resLoss, coinLoss);
    }


    public void RobberDiscardCheck()
    {
        // Check whether to go into DISCARD or ROBBER
        bool discardPhase = false;

        // Play robber sound as something bad happens either way
        RpcPlayAudio(11);

        // Calculate stillToDiscard
        for (int i = 1; i <= playerCount; i++)
        {
            Debug.Log($"playerResources: {playerResources[i].Count}");
            if (playerResources[i].Count > 7)
            {
                stillToDiscard[i] = playerResources[i].Count / 2;
                discardPhase = true;
            }
        }
        
        // Advance game state.
        if (discardPhase)
        { 
            StateRollToDiscard();
            StartCoroutine(DiscardPhase());
        }
        else
        {
            StateRollToRobber();
            RobberPhase();
        }
    }

    [Server]
    public IEnumerator DiscardPhase()
    {
        yield return new WaitUntil(() => DiscardAllReady());
        
        for (int i = 1; i <= playerCount; i++)
            stillToDiscard[i] = 0;

        StateDiscardToRobber();
        RobberPhase();
    }

    // Check if all players have fully discarded for coroutine above.
    [Server]
    public bool DiscardAllReady()
    {
        // Discard notifications.
        for (int i = 1; i <= playerCount; i++)
        {
            if (stillToDiscard[i] > 0)
            {
                RpcSingleNotify(playerIds[i].connectionToClient, new NotificationMessage{
                    targetPlayer = i,
                    privateMessage = $"Robber was rolled!\nDiscard {stillToDiscard[i]} cards.",
                    publicMessage = $"" });
            }
            else
            {
                RpcSingleNotify(playerIds[i].connectionToClient, new NotificationMessage{
                    targetPlayer = i,
                    privateMessage = $"Robber was rolled!\nWaiting for other players to discard.",
                    publicMessage = $"" });
            }
        }

        for (int i = 1; i <= playerCount; i++)
            if (stillToDiscard[i] > 0)
                return false;

        return true;
    }

    [Command(requiresAuthority = false)]
    public void CmdDiscarded(int playerIndex, Resource res)
    {
        // First decrement for the discarding player.        
        int tempDiscard = stillToDiscard[playerIndex];
        tempDiscard -= 1;
        stillToDiscard[playerIndex] = tempDiscard;

        RemoveResource(playerIndex, res);
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayKnight(int knightPlayer)
    {
        RpcPlayAudio(10);

        RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"Move the robber!",
                publicMessage = $"{playerNames[knightPlayer]} has recruited a Knight to their standing army!",
                temporary = true});

        if (GameState != State.IDLE) { Debug.LogWarning("CmdPlayKnight was not idle when requested, returning"); return; }

        // Before moving robber, check for LargestArmy.
        int myKnightCount = 0;
        int previousLargestArmyOwner = largestArmyOwner;

        foreach (Dev dev in playerDevCards[knightPlayer])
        {
            if (dev == Dev.KnightRevealed)
                myKnightCount += 1;
        }

        // If no one is largest army owner and player has 3 knights
        if (largestArmyOwner == 0 && myKnightCount == 3)
            largestArmyOwner = knightPlayer;
        

        // If player isn't already largest army owner and has 3 or more knights
        if (largestArmyOwner != knightPlayer && myKnightCount >= 3)
        {
            // Find largest current army.
            int largestArmy = 0;
            for (int i = 1; i <= playerCount; i++)
            {
                int otherKnightCount = 0;
                if (i != knightPlayer)
                {
                    foreach (Dev dev in playerDevCards[i])
                    {
                        if (dev == Dev.KnightRevealed)
                            otherKnightCount += 1;
                    }
                }

                if (otherKnightCount > largestArmy)
                    largestArmy = otherKnightCount;
            }

            // Check for new largest army.
            if (myKnightCount > largestArmy)
                largestArmyOwner = knightPlayer;
        }

        if (largestArmyOwner != previousLargestArmyOwner)
        {
            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"You now have the Largest Army!\n(Move the robber.)",
                publicMessage = $"{playerNames[knightPlayer]}'s army has no equal!\n(+2 Victory Points)",
                temporary = true});

            RpcInterruptAudio();
            RpcPlayAudio(31);
        }
        
        // Advance game state.
        StateIdleToRobber();
        RobberPhase();
    }

    public void RobberPhase()
    {
        RpcNotify(new NotificationMessage{
            targetPlayer = currentTurn,
            privateMessage = "Move the robber.",
            publicMessage = $"{playerNames[currentTurn]} is moving the robber."});
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestEndRobber(int requestor, int hexId, NetworkConnectionToClient sender = null)
    {
        if (GameState != State.ROBBER) { return; }
        if (playerIds[requestor] != sender.identity) { return; }
        

        StateRobberToEndRobber();

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
            StateEndRobberToIdle($"{GameState} / stealTargets.Count: {stealTargets.Count}");
            RpcNotifyClear();
        }
        else
        {
            Debug.Log($"State is {GameState}, there are steal targets");
            StateEndRobberToSteal();

            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = "Steal a card from a player!",
                publicMessage = $"{playerNames[currentTurn]} is stealing a card."});
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
    public void CmdRequestStealCard(int targetPlayer, int thiefPlayer)
    {
        if (GameState != State.STEAL) { StateStealToIdle(); }

        // Steal a random card from the oponent.
        List<Resource> toStealFrom = playerResources[targetPlayer];
        Resource stolenRes = Resource.None;

        int i = 0;
        int resIndex = UnityEngine.Random.Range(0, toStealFrom.Count);
        foreach (Resource toStealRes in toStealFrom)
        {
            if (i == resIndex)
                stolenRes = toStealRes;
            i++;
        }

        if (stolenRes != Resource.None)
        {
            RemoveResource(targetPlayer, stolenRes);
            AddResource(thiefPlayer, stolenRes);

            RpcIncomeAnimation(
                thiefPlayer,
                new List<Resource>() { stolenRes },
                false);

            RpcLossAnimation(
                targetPlayer,
                new List<Resource>() { stolenRes },
                false);

            Debug.Log($"Player {thiefPlayer} stole a {stolenRes} from player {targetPlayer}");
        }
        
        // Advance game state.
        StateStealToIdle($"State was {GameState}, toStealFrom: {toStealFrom.Count}, targetPlayer {targetPlayer} thiefPlayer {thiefPlayer}");
        
        RpcNotifyClear();
        RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = "It is your turn.",
                publicMessage = $""});
    }
    
    [Command(requiresAuthority = false)]
    public void CmdProcessMonopoly(int cardPlayer, Resource res)
    {
        RpcPlayAudio(36);

        Dictionary<int, int> toStealCount = new Dictionary<int, int>();
        List<Resource> resIncome = new List<Resource>();
        Dictionary<int, List<Resource>> resLosses = new Dictionary<int, List<Resource>>();

        for (int i = 1; i <= playerCount; i++)
        {
            toStealCount[i] = 0;
            resLosses[i] = new List<Resource>();

            if (i != cardPlayer)
                foreach (Resource match in playerResources[i].Where(r => r == res))
                    toStealCount[i] += 1;
        }

        for (int i = 1; i <= playerCount; i++)
        {
            for (int j = 0; j < toStealCount[i]; j++)
            {
                RemoveResource(i, res);
                AddResource(cardPlayer, res);

                resLosses[i].Add(res);
                resIncome.Add(res);
            }
        }

        if (resIncome.Count > 0)
        {
            RpcIncomeAnimation(cardPlayer, resIncome, false);

            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = "",
                publicMessage = $"{playerNames[cardPlayer]} has declared a Monopoly on {res} and seized every copy from all players!",
                temporary = true});
        }
        else
        {
            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"Your tax collectors searched far and wide, but there is simply no {res} to be found.",
                publicMessage = $"{playerNames[cardPlayer]} tried to declare a Monopoly on {res}, but shortages ravage the continent!",
                temporary = true});
        }

        for (int i = 1; i <= playerCount; i++)
            RpcLossAnimation(i, resLosses[i], false);
    }

    [Command(requiresAuthority = false)]
    public void CmdProcessYearOfPlenty(int playerIndex, Resource res)
    {
        RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = "",
                publicMessage = $"{playerNames[currentTurn]} benefits from a Year of Plenty and takes two free {res}!",
                temporary = true});

        List<Resource> resIncome = new List<Resource>();
        resIncome.Add(res);
        resIncome.Add(res);

        AddResource(playerIndex, res, 2);

        RpcIncomeAnimation(playerIndex, resIncome, false);
    }

    [Command(requiresAuthority = false)]
    public void CmdProcessRoadBuilding(int playerIndex)
    {   
        RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"Place two free roads.",
                publicMessage = $"{playerNames[currentTurn]} has enacted a program of Road Building and is placing two free roads!",
                temporary = true});

        StartCoroutine(RoadBuilding(playerIndex));
    }

    private IEnumerator RoadBuilding(int playerIndex)
    {
        int startRoadCount = GameBoard.GetNumRoads();

        StateIdleToBuild("Starter blueprint IDLE -> BUILD");
        RpcGiveBlueprint(playerIds[playerIndex].connectionToClient, "Road", false, true);
        yield return new WaitUntil(() => GameBoard.GetNumRoads() >= startRoadCount + 1);

        StateIdleToBuild("Starter blueprint IDLE -> BUILD");
        RpcGiveBlueprint(playerIds[playerIndex].connectionToClient, "Road", false, true);
        yield return new WaitUntil(() => GameBoard.GetNumRoads() >= startRoadCount + 2);

        RpcNotifyClear();
    }

    [TargetRpc]
    private void RpcGiveBlueprint(NetworkConnection target, string blueprint, bool starter, bool free)
    {    
        StartCoroutine(Build(blueprint, starter, free));
    }

    [Command(requiresAuthority = false)]
    public void CmdRequestBuild(int builderIndex, string blueprint)
    {
        StateIdleToBuild($"On CmdRequestBuild, state was {GameState} when {playerNames[builderIndex]} requested to build a {blueprint}");

        switch (blueprint)
        {
            case "Road":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "Road", false, false);
                break;
            case "Village":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "Village", false, false);
                break;
            case "City":
                RpcGiveBlueprint(playerIds[builderIndex].connectionToClient, "City", false, false);
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

    public GameObject selectorRingPrefab;

    public Material matBlue;
    public Material matRed;
    public Material matGreen;
    public Material matYellow;

    public Material matTransparentBlue;
    public Material matTransparentRed;
    public Material matTransparentGreen;
    public Material matTransparentYellow;

    [Client]
    public IEnumerator Build(string selectedBlueprint, bool starter = false, bool free = false)
    {
        //GameState = State.BUILD;

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
        GameObject blueprintObj = (GameObject)Instantiate(blueprintPrefab);
        blueprintObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(PlayerController.playerIndex, true);

        yield return new WaitUntil(() => GameState == State.BUILD);
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

                            //GameObject.Destroy(blueprintObj);
                            //GameState = State.IDLE;
                            
                            if (!starter && !free)
                            {
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wood);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Brick);

                                CmdLossAnimation(
                                    PlayerController.playerIndex,
                                    new List<Resource>() { Resource.Wood, Resource.Brick },
                                    false);
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
                            
                            //GameObject.Destroy(blueprintObj);
                            //GameState = State.IDLE;

                            if (!starter)
                            {
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wood);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Brick);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Wool);
                                CmdRemoveResource(PlayerController.playerIndex, Resource.Grain);

                                CmdLossAnimation(
                                    PlayerController.playerIndex,
                                    new List<Resource>() { Resource.Wood, Resource.Brick, Resource.Wool, Resource.Grain },
                                    false);
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

                            //GameObject.Destroy(blueprintObj);
                            //GameState = State.IDLE;

                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Ore);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Grain);
                            CmdRemoveResource(PlayerController.playerIndex, Resource.Grain); 

                            CmdLossAnimation(
                                PlayerController.playerIndex,
                                new List<Resource>() { Resource.Ore, Resource.Ore, Resource.Ore, Resource.Grain, Resource.Grain },
                                false);

                            CmdBuildCity(proposedCorner.idNum, PlayerController.playerIndex, blueprintPos, blueprintRot);
                        }
                        break;
                }
            }

            // On right-click, cancel build. Cancelling is disabled for starter placements.
            if (Input.GetMouseButtonDown(1))
            {
                if (starter || free)
                    Debug.Log("Can't cancel a starter blueprint!");
                else
                {
                    Debug.Log("Cancelled blueprint");
                    
                    GameObject.Destroy(blueprintObj);
                    CmdCanceledBlueprint();
                }
            }
            yield return null;
        }
        yield return null;

        if (blueprintObj != null)
            Destroy(blueprintObj);
        
    }

    [Command(requiresAuthority = false)]
    private void CmdCanceledBlueprint()
    {
        StateBuildToIdle();
    }

    [Command(requiresAuthority = false)]
    private void CmdBuildRoad(int pathId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Path p = GameBoard.paths[pathId];

        p.owned = true;
        p.playerOwner = builderIndex;

        // Look for any change in longest road.
        int previousLongestRoadOwner = longestRoadOwner;
        longestRoadOwner = GameBoard.LongestRoadFinder();

        if (!setup)
            RpcPlayAudio(UnityEngine.Random.Range(13, 16));

        if (longestRoadOwner != previousLongestRoadOwner)
        {
            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"You now have the Longest Road!",
                publicMessage = $"All roads lead to {playerNames[longestRoadOwner]}'s demense!\n(+2 Victory Points)"});

            RpcInterruptAudio();
            RpcPlayAudio(30);
        }

        RpcBuildRoad(pathId, builderIndex, blueprintPos, blueprintRot);

        StateBuildToIdle("CmdBuildRoad");
    }
    

    [ClientRpc]
    private void RpcBuildRoad(int pathId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Path p = GameBoard.paths[pathId];

        p.owned = true;
        p.playerOwner = builderIndex;

        GameObject buildingObj = (GameObject)Instantiate(roadPrefab, p.position, p.rotation);
        buildingObj.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex);
        buildingObj.GetComponent<RoadComponent>().defaultMat = GetPlayerMaterial(builderIndex);

        GameObject selectorRing = Instantiate(selectorRingPrefab, p.position, Quaternion.Euler(90f, 90f, 0f));
        selectorRing.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex, true);
    }

    [Command(requiresAuthority = false)]
    private void CmdBuildVillage(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 1;

        if (!setup)
            RpcPlayAudio(UnityEngine.Random.Range(13, 16));
        CheckHarbormaster();

        RpcBuildVillage(cornerId, builderIndex, blueprintPos, blueprintRot);

        StateBuildToIdle("CmdBuildVillage");
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

        GameObject selectorRing = Instantiate(selectorRingPrefab, c.position, buildingObj.transform.rotation * Quaternion.Euler(90f, 0f, 0f));
        selectorRing.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex, true);
    }

    [Command(requiresAuthority = false)]
    private void CmdBuildCity(int cornerId, int builderIndex, Vector3 blueprintPos, Quaternion blueprintRot)
    {
        Corner c = GameBoard.corners[cornerId];

        c.owned = true;
        c.playerOwner = builderIndex;
        c.devLevel = 2;

        if (!setup)
            RpcPlayAudio(UnityEngine.Random.Range(13, 16));
        CheckHarbormaster();

        RpcBuildCity(cornerId, builderIndex, blueprintPos, blueprintRot);

        StateBuildToIdle("CmdBuildCity");
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

        GameObject selectorRing = Instantiate(selectorRingPrefab, c.position, buildingObj.transform.rotation * Quaternion.Euler(90f, 0f, 0f));
        selectorRing.GetComponent<MeshRenderer>().material = GetPlayerMaterial(builderIndex, true);
    }


    // After the server adds a resource, it calls a ClientRpc to get
    // the event to fire on all handlers on each client.
    public static event Action<int, List<Resource>> onHandChanged;

    [Command(requiresAuthority = false)]
    public void CmdAddResource(int playerIndex, Resource res)
    {
        AddResource(playerIndex, res);
    }

    [Server]
    private void AddResource(int playerIndex, Resource res, int quantity = 1)
    {
        List<Resource> newResources = playerResources[playerIndex];
        
        for (int i = 0; i < quantity; i++)
            newResources.Add(res);
        
        if (newResources.Contains(Resource.None)) newResources.Remove(Resource.None);
        
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

        if (newResources.Contains(Resource.None)) newResources.Remove(Resource.None);

        playerResources[playerIndex] = newResources;

        RpcChangeResource(playerIndex, newResources);
    }

    [ClientRpc]
    private void RpcChangeResource(int playerIndex, List<Resource> newResources)
    {
        onHandChanged?.Invoke(playerIndex, newResources);
    }


    // After the server adds a DevCard, it calls a ClientRpc to get
    // the event to fire on all handlers on each client.
    public static event Action<int, List<Dev>> onDevCardChanged;

    [Command(requiresAuthority = false)]
    public void CmdAddDevCard(int playerIndex, Dev dev)
    {
        AddDevCard(playerIndex, dev);
    }

    [Server]
    private void AddDevCard(int playerIndex, Dev dev)
    {
        List<Dev> newDevCards = playerDevCards[playerIndex];
        newDevCards.Add(dev);
        playerDevCards[playerIndex] = newDevCards;

        devCardsBoughtThisTurn.Add(dev);
        RpcChangeDevCard(playerIndex, newDevCards);
        RpcUpdateDevCardsBoughtThisTurn(devCardsBoughtThisTurn);
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveDevCard(int playerIndex, Dev dev)
    {
        RemoveDevCard(playerIndex, dev);
    }

    [Server]
    private void RemoveDevCard(int playerIndex, Dev dev)
    {
        List<Dev> newDevCards = playerDevCards[playerIndex];
        
        if (newDevCards.Contains(dev))
            newDevCards.Remove(dev);
        else
            Debug.LogWarning("Player doesn't have the dev card to remove");

        playerDevCards[playerIndex] = newDevCards;

        RpcChangeDevCard(playerIndex, newDevCards);
    }

    [ClientRpc]
    private void RpcChangeDevCard(int playerIndex, List<Dev> newDevCards)
    {
        onDevCardChanged?.Invoke(playerIndex, newDevCards);
    }

    [ClientRpc]
    private void RpcUpdateDevCardsBoughtThisTurn(List<Dev> devCardsBought)
    {
        devCardsBoughtThisTurn = devCardsBought;
    }

    [ClientRpc]
    private void RpcClearDevCardsBoughtThisTurn()
    {
        devCardsBoughtThisTurn.Clear();
    }

    [Command(requiresAuthority = false)]
    public void CmdBuyDevCard(int buyerIndex)
    {
        
        RpcNotify(new NotificationMessage {
                targetPlayer = buyerIndex,
                privateMessage = "",
                publicMessage = $"{playerNames[buyerIndex]} bought a development card."
            });

        RemoveResource(buyerIndex, Resource.Wool);
        RemoveResource(buyerIndex, Resource.Grain);
        RemoveResource(buyerIndex, Resource.Ore);

        // CHOOSE A RANDOM DEV CARD
        int randomDevCard = UnityEngine.Random.Range(0,25);
        Dev randomDev = Dev.None;

        // 2/3 chance for knight, 1/3 for progress, 1/3 for VP
        if (randomDevCard >= 0 && randomDevCard < 13)
        {
            randomDev = Dev.Knight;
        }
        else if (randomDevCard >= 13 && randomDevCard < 18)
        {
            randomDev = Dev.VP;
        }
        else if (randomDevCard >= 18 && randomDevCard < 20)
        {
            randomDev = Dev.Monopoly;
        }
        else if (randomDevCard >= 20 && randomDevCard < 22)
        {
            randomDev = Dev.Roads;
        }
        else if (randomDevCard >= 22 && randomDevCard <= 24)
        {
            randomDev = Dev.Plenty;
        }

        AddDevCard(buyerIndex, randomDev);
    }

    // Selected cards
    [Command(requiresAuthority = false)]
    public void CmdAddSelectedCard(int playerIndex, Resource res)
    {
        playerSelectedCards[playerIndex].Add(res);
        RpcChangeSelectedCard(playerIndex, playerSelectedCards[playerIndex]);
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveSelectedCard(int playerIndex, Resource res)
    {
        playerSelectedCards[playerIndex].Remove(res);
        RpcChangeSelectedCard(playerIndex, playerSelectedCards[playerIndex]);
    }

    [ClientRpc]
    private void RpcChangeSelectedCard(int playerIndex, List<Resource> selectedCards)
    {
        playerSelectedCards[playerIndex] = selectedCards;
    }

    [Command(requiresAuthority = false)]
    public void CmdClearSelectedCards()
    {
        for (int i = 1; i <= playerCount; i++)
        {
            playerSelectedCards[i].Clear();
            RpcChangeSelectedCard(i, playerSelectedCards[i]);
        }
    }



    // Trade
    [Command(requiresAuthority = false)]
    public void CmdEnterTradeState(int requestor)
    {
        if (GameState != State.IDLE) { Debug.LogWarning($"CmdEnterTradeState: Game state was not IDLE when requested by {playerNames[requestor]}, returning"); return; }
        
        GameState = State.TRADE;
        RpcChangeSelectedCard(requestor, playerSelectedCards[requestor]);
    }

    [Command(requiresAuthority = false)]
    public void CmdExitTradeState()
    {
        GameState = State.IDLE;
    }

    [Command(requiresAuthority = false)]
    public void CmdOfferTrade(int playerIndex, bool offeringTrade)
    {
        playerOfferingTrade[playerIndex] = offeringTrade;
        RpcOfferTrade(playerIndex, offeringTrade);
    }

    [ClientRpc]
    public void RpcOfferTrade(int playerIndex, bool offeringTrade)
    {
        playerOfferingTrade[playerIndex] = offeringTrade;
    }

    [Command(requiresAuthority = false)]
    public void CmdProcessTrade(int otherTrader)
    {
        int trader = currentTurn;

        List<Resource> traderIncome = new List<Resource>();
        List<Resource> otherTraderIncome = new List<Resource>();
        
        foreach (Resource res in playerSelectedCards[otherTrader])
        {
            AddResource(trader, res);
            RemoveResource(otherTrader, res);
            traderIncome.Add(res);
        }

        foreach (Resource res in playerSelectedCards[trader])
        {
            AddResource(otherTrader, res);
            RemoveResource(trader, res);
            otherTraderIncome.Add(res);            
        }

        RpcIncomeAnimation(trader, traderIncome, false);
        RpcLossAnimation(trader, otherTraderIncome, false);
        RpcIncomeAnimation(otherTrader, otherTraderIncome, false);
        RpcLossAnimation(otherTrader, traderIncome, false);
    
        for (int i = 1; i <= playerCount; i++)
        {
            playerSelectedCards[i].Clear();
            playerOfferingTrade[i] = false;
            RpcOfferTrade(i, playerOfferingTrade[i]);
            RpcChangeSelectedCard(i, playerSelectedCards[i]);
        }

        GameState = State.IDLE;
    }

    [Server]
    public void CheckHarbormaster()
    {
        int harborPoints = 0;
        int previousHarbormasterOwner = harbormasterOwner;
        
        for (int i = 1; i <= playerCount; i++)
        {
            harborPoints = GetHarborPoints(i);

            if (harbormasterOwner == 0 && harborPoints >= 3)
            {
                harbormasterOwner = i;
                // new harbormaster
            }
            
            if (harbormasterOwner != 0 && harbormasterOwner != i)
                if (harborPoints > GetHarborPoints(harbormasterOwner))
                {
                    harbormasterOwner = i;
                    // new harbormaster
                }
        }

        if (harbormasterOwner != previousHarbormasterOwner)
        {
            RpcNotify(new NotificationMessage{
                targetPlayer = currentTurn,
                privateMessage = $"You are now the Harbormaster!\n(+2 Victory Points)",
                publicMessage = $"{playerNames[currentTurn]}'s merchant fleets rule the seas!\n(+2 Victory Points)"});
            
            RpcInterruptAudio();
            RpcPlayAudio(32);
        }
    }

    [Server]
    public int GetHarborPoints(int playerIndex)
    {
        int harborPoints = 0;

        foreach (Corner c in GameBoard.corners.ToList().Where(c => c.playerOwner == playerIndex && c.isHarbor))
            harborPoints += c.devLevel;
        
        return harborPoints;
    }

    [Command(requiresAuthority = false)]
    public void CmdRecalculateVP()
    {
        if (GameState == State.WINNER) { return;}

        for (int i = 1; i <= playerCount; i++)
        {
            playerPrivateVP[i] = 0;
            playerPublicVP[i] = 0;
        }           

        // 1 public VP for villages, 2 for cities
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

        // 2 public VP for special cards
        if (longestRoadOwner != 0)
        {
            playerPrivateVP[longestRoadOwner] += 2;
            playerPublicVP[longestRoadOwner] += 2;
        }
        if (largestArmyOwner != 0)
        {
            playerPrivateVP[largestArmyOwner] += 2;
            playerPublicVP[largestArmyOwner] += 2;
        }
        if (harbormasterOwner != 0)
        {
            playerPrivateVP[harbormasterOwner] += 2;
            playerPublicVP[harbormasterOwner] += 2;
        }

        // 1 private VP for each  VP dev card, revealed or not
        for (int i = 1; i <= playerCount; i++)
            foreach (Dev dev in playerDevCards[i])
                if (dev == Dev.VP || dev == Dev.VPRevealed)
                    playerPrivateVP[i] += 1;

        // 1 public VP for each revealed VP dev card
        for (int i = 1; i <= playerCount; i++)
            foreach (Dev dev in playerDevCards[i])
                if (dev == Dev.VPRevealed)
                    playerPublicVP[i] += 1;


        // Look for a winner
        for (int i = 1; i <= playerCount; i++)
        {
            // Need 11 VP since harbormaster is in game
            if (playerPrivateVP[i] >= VPtoWin && i == currentTurn)
            {
                DeclareWinner(i);
            }
        }
    }

    [Server]
    private void DeclareWinner(int winnerIndex)
    {
        GameState = State.WINNER;

        // Play audio.
        RpcPlayAudio(12);

        // Reveal VP dev cards.
        List<Dev> tempDevCards = new List<Dev>();

        for (int i = 1; i <= playerCount; i++)
        {
            tempDevCards.Clear();

            foreach (Dev dev in playerDevCards[i])
            {
                tempDevCards.Add(dev);
            }

            foreach (Dev dev in tempDevCards)
            {
                if (dev == Dev.VP)
                {
                    //RemoveDevCard(i, Dev.VP);
                    AddDevCard(i, Dev.VPRevealed);
                }
            }
        }

        RpcNotifyClear();
        RpcNotify(new NotificationMessage{
            targetPlayer = currentTurn,
            privateMessage = $"You have won the game!",
            publicMessage = $"{playerNames[currentTurn]} has won the game!"});
    
    }



    public static Color PlayerColor(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                return Color.blue;
            case 2:
                return Color.red;
            case 3:
                return Color.yellow;
            case 4:
                return Color.green;
        }

        return Color.white;
    }

        // Assigns player numbers to materials
    [Client]
    public Material GetPlayerMaterial(int player, bool transparent = false)
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
                    return matTransparentYellow;
                return matYellow;

            case 4:
                if (transparent)
                    return matTransparentGreen;
                return matGreen;
        }

        if (transparent)
            return matTransparentBlue;
        return matBlue;
    }


    [Command(requiresAuthority=false)]
    public void CmdDebugForceIdle()
    {
        for (int i = 1; i < playerCount; i++)
        {
            stillToDiscard[i] = 0;
            playerSelectedCards[i].Clear();
        }
        
        GameState = State.IDLE;
    }
}