using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class MyNetworkManager : NetworkManager
{

    internal static readonly List<NetworkConnection> waitingConnections = new List<NetworkConnection>();
    

    internal static readonly Dictionary<int, NetworkConnection> playerConns = new Dictionary<int, NetworkConnection>();
    internal static readonly Dictionary<int, NetworkIdentity> playerIds = new Dictionary<int, NetworkIdentity>();
    internal static readonly Dictionary<NetworkConnection, string> playerNames = new Dictionary<NetworkConnection, string>();
    
    public Lobby lobby;
    public GameObject gameManagerPrefab;
    
    // Runs on both Server and Client (Networking is NOT initialized when this fires)
    public override void Awake()
    {
        base.Awake();
        Debug.Log("Server/client awake");

        lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
    
        // [Dev-only] Check if this a ParrelSync clone to auto-start server or client.
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone())
        {
            lobby.gameObject.SetActive(false);

            Debug.Log("Not a ParrelSync clone, auto-starting server.");
            GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().StartServer();
        }
        #endif

        #if UNITY_SERVER
            StartServer();
        #endif

        /*
        #if UNITY_EDITOR
        if (ClonesManager.IsClone())
        else
        {
            Debug.Log("Is a ParrelSync clone, auto-starting client.");
            GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().StartClient();
        }
        #endif
        */
    }

    private void Start()
    {
        Debug.Log("Server/client start");
        networkAddress = lobby.ipAddress;

        // [Dev-only] If testing in editor, set networkAddress to localhost.
        /*
        #if UNITY_EDITOR
        networkAddress = "localhost";
        #endif
        */
    }

    public string PlayerName;
    // Called by UI element inputFieldName.OnValueChanged
    public void SetPlayername(string playername)
    {
        PlayerName = playername;
    }

    // Called by UI element inputIpAddress.OnValueChanged
    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public struct CreatePlayerMessage : NetworkMessage
    {
        public string name;
    }

    public struct StartMessage : NetworkMessage { }

    public struct NamesConnectedMessage : NetworkMessage
    {
        public string namesConnected;
    }

    
    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        NetworkServer.RegisterHandler<StartMessage>(StartGame);
        Debug.Log("OnStartServer");
    }

    [Client]
    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("trying to connect");
        NetworkClient.RegisterHandler<NamesConnectedMessage>(UpdateNumConnected);
    }

    [Client]
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // tell the server to create a player with this name
        conn.Send(new CreatePlayerMessage { name = PlayerName });

        lobby.connectingScreen.SetActive(false);
        lobby.startScreen.SetActive(true);
    }

    

    [Server]
    private void OnCreatePlayer(NetworkConnection conn, CreatePlayerMessage createPlayerMessage)
    {
        waitingConnections.Add(conn);

        if (createPlayerMessage.name != "")
            playerNames[conn] = createPlayerMessage.name;
        else
            playerNames[conn] = "Anonymous";

        string nameList = $"{playerNames.Count} connected players:";
        foreach (KeyValuePair <NetworkConnection, string> entry in playerNames)
        {
            nameList += $"\n {entry.Value}";
        }
        NetworkServer.SendToAll(new NamesConnectedMessage { namesConnected = nameList });

        Debug.Log("OnCreatePlayer");
        
        /*
        // [Dev-only] Auto-start
        #if UNITY_EDITOR
        int autoStartTrigger = 1;
        if (waitingConnections.Count == autoStartTrigger)
        {
            Debug.Log($"There are {autoStartTrigger} players, auto-starting.");
            StartCoroutine(GameStart());
        }
        #endif
        */
    }

    [Client]
    public void OnPressStart()
    {
        Debug.Log("client pressed start");
        NetworkClient.connection.Send(new StartMessage{});
    }

    [Server]
    public void StartGame(StartMessage startMessage)
    {
        StartCoroutine(GameStart());
    }
    
    [Client]
    public void UpdateNumConnected(NamesConnectedMessage namesConnectedMessage)
    {
        lobby.connectedLabel.GetComponentInChildren<Text>().text = namesConnectedMessage.namesConnected;
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"{conn} disconnected.");

        waitingConnections.Remove(conn);
    }
    
    [Client]
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log($"Disconnected from server.");

        lobby.loginScreen.SetActive(true);
        lobby.connectingScreen.SetActive(false);
    }


    [Server]
    IEnumerator GameStart()
    {
        // Assign each player connection a number (1-based, will be the playerIndex).
        int i = 1;
        foreach (NetworkConnection conn in waitingConnections)
        {
            playerConns[i] = conn;
            i++;
        }

        // Spawn the gameManager object.
        GameObject gameManager = Instantiate(gameManagerPrefab);
        gameManager.GetComponent<GameManager>().syncPlayerCount = playerConns.Count;
        NetworkServer.Spawn(gameManager);        

        // Wait a frame for gameManager's Start to finish.
        yield return null;


        // Spawn the player objects, set their playerIndexes, and get their playerIds.
        foreach (KeyValuePair<int, NetworkConnection> entry in playerConns)
        {
            int index = entry.Key;
            NetworkConnection conn = entry.Value;

            GameObject player = Instantiate(NetworkManager.singleton.playerPrefab);
            player.GetComponent<PlayerController>().syncPlayerIndex = index;
            NetworkServer.AddPlayerForConnection(conn, player);

            playerIds[index] = conn.identity;
        }
        
        // Can't have a SyncDictionary in the Network Manager, so here we map the normal Dictionary
        // to gameManager's SyncDictionary, which then syncs itself with the clients.
        foreach (KeyValuePair<int, NetworkIdentity> entry in playerIds)
            gameManager.GetComponent<GameManager>().playerIds[entry.Key] = entry.Value;
        
        // Do the same for playerNames.
        foreach (KeyValuePair<NetworkConnection, string> nameEntry in playerNames)
            foreach (KeyValuePair<int, NetworkIdentity> idEntry in playerIds)
                if (idEntry.Value.connectionToClient == nameEntry.Key)
                    gameManager.GetComponent<GameManager>().playerNames[idEntry.Key] = nameEntry.Value;

    }
}