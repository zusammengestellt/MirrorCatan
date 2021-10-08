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
    // Holds connections before players are assigned, game starts, etc.
    internal static readonly List<NetworkConnection> waitingConnections = new List<NetworkConnection>();

    public readonly Dictionary<int, NetworkIdentity> playerIds = new Dictionary<int, NetworkIdentity>();

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
        foreach (NetworkConnection conn in waitingConnections)
        {
            GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        yield return null;

        int i = 1;
        foreach (int key in NetworkServer.connections.Keys)
        {
            playerIds[i] = NetworkServer.connections[key].identity;
            playerIds[i].gameObject.GetComponent<PlayerController>().playerIndex = i;
            i++;
        }
    }
}