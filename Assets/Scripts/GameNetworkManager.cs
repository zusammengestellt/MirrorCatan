using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

#if UNITY_EDITOR
using ParrelSync;
#endif


/* SUMMARY
    GameNetworkManager handles initial set up and connections.
    Both server and client then make their own calls to the
    canvasController component on each instance's Canvas.

    Interestingly, the NetworkManager GameObject also references
    Canvas's canvasController component -- so there is no need
    to do canvas.GetComponent<canvasController>(). This way ensures
    that the GameObject gets the instance's own canvasController.
*/

public class GameNetworkManager : NetworkManager
{
    // Holds connections before/after players are assigned, game starts, etc.
    internal static readonly List<NetworkConnection> waitingConnections = new List<NetworkConnection>();
    internal static readonly List<NetworkConnection> playingConnections = new List<NetworkConnection>();

    // Player-indexed reference to connections
    internal static readonly Dictionary<int, NetworkConnection> playerConns = new Dictionary<int, NetworkConnection>();
    internal static readonly Dictionary<int, NetworkIdentity> playerIds = new Dictionary<int, NetworkIdentity>();

    public GameObject gameControllerPrefab;


    // Runs on both Server and Client (Networking is NOT initialized when this fires)
    public override void Awake()
    {
        base.Awake();

        // [Dev-only] Check if this a ParrelSync clone to auto-start server or client.
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone())
        {
            Debug.Log("Not a ParrelSync clone, auto-starting server.");
            GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>().StartServer();
        }
        else
        {
            Debug.Log("Is a ParrelSync clone, auto-starting client.");
            GameObject.Find("NetworkManager").GetComponent<GameNetworkManager>().StartClient();
        }
        #endif

        Debug.Log("This server/client has awoken.");

        // [Dev-only] If testing in editor, set networkAddress to localhost.
        #if UNITY_EDITOR
        this.networkAddress = "localhost";
        #endif
    }

    // This is invoked when a server is started
    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"Server has started.");
    }
    
    // This is invoked when the client is started.
    [Client]
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"This client has started.");
    }

    // Called on the server when a client is ready (param: connection from client)
    [Server]
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        if (!NetworkServer.active) return;  // safety valve

        waitingConnections.Add(conn);
        
        Debug.Log($"{conn} with IP {conn.address} has connected. There are {waitingConnections.Count} waiting connections."); 

        /*GameObject player = Instantiate(
            NetworkManager.singleton.playerPrefab,
            new Vector3(0f, 5f, 0f),
            Quaternion.identity
        );
        NetworkServer.AddPlayerForConnection(conn, player);
        
        GameStart();*/
        
        // [Dev-only] Auto-start; currently using auto-spawn in NetworkManager instead
        #if UNITY_EDITOR
        int autoStartTrigger = 3;
        if (waitingConnections.Count == autoStartTrigger)
        {
            Debug.Log($"There are {autoStartTrigger} players, auto-starting. The last player to ready up is assumed to be the match creator and the one who pressed start.");
            StartCoroutine(GameStart());
        }
        #endif
    }

    // Called on the client when connected to a server
    [Client]
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log($"Connected to server as {conn} with IP: {NetworkClient.connection.address}.");
    }

    // Called on clients when disconnected from a server.
    [Client]
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log($"Disconnected from server.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Called on the server when a client disconnects (param: connection from client)
    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"{conn} with IP {conn.address} has disconnected.");
       
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // This is called when a client is stopped.
    [Client]
    public override void OnStopClient()
    {
        Debug.Log($"This client has stopped.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // This is called when a server is stopped - including when a host is stopped.
    [Server]
    public override void OnStopServer()
    {
        Debug.Log($"Server has stopped.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    [Server]
    IEnumerator GameStart()
    {
        // waitingConnections are converted into playingConnections.
        // Any later lobby work would only allow confirmed waiting connections to start.
        foreach (NetworkConnection conn in waitingConnections)
        {
            playingConnections.Add(conn);
        }
        waitingConnections.Clear();

        // Assign each player connection a number (1-based, will be the playerIndex).
        int i = 1;
        foreach (NetworkConnection conn in playingConnections)
        {
            playerConns[i] = conn;
            i++;
        }

        // Spawn the gameController object.
        GameObject gameController = Instantiate(gameControllerPrefab);
        NetworkServer.Spawn(gameController);

        // Set player count.
        gameController.GetComponent<GameController>().playerCount = playerConns.Count;

        // Wait a frame for gameController's Start to finish.
        yield return null;


        // Spawn the player objects, set their playerIndexes, and get their playerIds.
        foreach (KeyValuePair<int, NetworkConnection> entry in playerConns)
        {
            int index = entry.Key;
            NetworkConnection conn = entry.Value;

            GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
            player.GetComponent<PlayerController>().playerIndex = index;
            player.GetComponent<PlayerController>().playerCount = playerConns.Count;
            NetworkServer.AddPlayerForConnection(conn, player);

            playerIds[index] = conn.identity;
        }

        
        // Can't have a SyncDictionary in the Network Manager, so here we map
        // the normal Dictionary to GameController's SyncDictionary
        // (which then syncs itself with the clients).
        foreach (KeyValuePair<int, NetworkIdentity> entry in playerIds)
        {
            gameController.GetComponent<GameController>().playerIds[entry.Key] = entry.Value;
        }
    }
}