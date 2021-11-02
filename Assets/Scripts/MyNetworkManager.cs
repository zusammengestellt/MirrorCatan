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
    internal static readonly Dictionary<NetworkConnection, bool> playerReady = new Dictionary<NetworkConnection, bool>();
    
    public GameObject gameManagerPrefab;
    public GameObject gameManager;
    
    public AudioSource audioSourceIntro;
    public AudioSource audioSource;
    public AudioClip introClip;
    public AudioClip buttonClip;

    public bool goFast;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftShift))
            NetworkClient.Send(new StartMessage {});
    }

    // Runs on both Server and Client (Networking is NOT initialized when this fires)
    public override void Awake()
    {
        base.Awake();
        Debug.Log("Server/client awake");
   
        // [Dev-only] Check if this a ParrelSync clone to auto-start server or client.
        #if UNITY_EDITOR
        if (!ClonesManager.IsClone())
        {
            Lobby.SetActive(false);

            Debug.Log("Not a ParrelSync clone, auto-starting server.");
            GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().StartServer();
        }
        #endif

        #if UNITY_SERVER
            StartServer();
        #endif
    }

    private void Start()
    {
        Debug.Log("Server/client start");
        networkAddress = Lobby.GetComponent<Lobby>().ipAddress;

        // [Dev-only] If testing in editor, set networkAddress to localhost.
        #if UNITY_EDITOR
        networkAddress = "localhost";
        #endif

        /*
        #if UNITY_EDITOR
        if (goFast && ClonesManager.IsClone())
        {
            StartClient();
        }
        #endif*/
        
        audioSource.clip = buttonClip;
        audioSourceIntro.clip = introClip;
        audioSourceIntro.Play();
        StartCoroutine(LogoAnimated());
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

    public struct CreatePlayerMessage : NetworkMessage { public string name; }
    public struct ReadyMessage : NetworkMessage { public bool isReady; }
    public struct AllReadyMessage : NetworkMessage { public bool allReady; }
    public struct LobbyListMessage : NetworkMessage { public string lobbyList; }
    public struct ResetReadyMessage : NetworkMessage { }
    public struct StartMessage : NetworkMessage { }
    public struct ClientStartMessage : NetworkMessage { }

    
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
        NetworkServer.RegisterHandler<ReadyMessage>(ReadyChange);
        NetworkServer.RegisterHandler<StartMessage>(StartGame);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<LobbyListMessage>(UpdateClientLobbyList);
        NetworkClient.RegisterHandler<AllReadyMessage>(AllReadyChange);
        NetworkClient.RegisterHandler<ResetReadyMessage>(ResetReady);
        NetworkClient.RegisterHandler<ClientStartMessage>(ClientStartGame);
    }



    [Header("GUI Elements")]
    public GameObject Lobby;
    private bool localReady = false;
    private bool allReady = true;

    [Header("Logo")]
    public GameObject Background;
    public GameObject Logo;
    public GameObject LogoSmall;

    private IEnumerator LogoAnimated()
    {
        float logoStep = 0.0005f;
        float menuStep = 0.0002f;

        Background.SetActive(true);
        Logo.SetActive(true);

        yield return new WaitForSeconds(1f);

        for (int i = 255; i >= 0; i--)
        {
            Logo.GetComponent<Image>().color = new Color32(255, 255, 255, (byte)i);
            yield return new WaitForSeconds(logoStep);
        }

        LoginScreen.SetActive(true);
        
        Image logoSmall = LogoSmall.GetComponent<Image>();
        Image[] loginButtons = LoginScreen.GetComponentsInChildren<Image>();
        Text[] loginTexts = LoginScreen.GetComponentsInChildren<Text>();
        
        // Set button backgrounds and text fuly transparent.
        for (int i = 0; i < loginButtons.Length; i++)
            loginButtons[i].color = new Color32(48, 48, 48, 0);

        for (int i = 0; i < loginTexts.Length; i++)
            loginTexts[i].color = new Color32(255, 255, 255, 0);

        // Fade them in (along with LogoSmall)
        for (int i = 0; i <= 255; i++)
        {
            for (int j = 0; j < loginButtons.Length; j++)
                loginButtons[j].color = new Color32(48, 48, 48, (byte)i);

            for (int j = 0; j < loginTexts.Length; j++)
                loginTexts[j].color = new Color32(255, 255, 255, (byte)i);

            logoSmall.color = new Color32(255, 255, 255, (byte)i); 
            
            yield return new WaitForSeconds(menuStep);
        }
    }

    [Header("LoginScreen")]
    public GameObject LoginScreen;

    public void OnConnectButton()
    {
        audioSource.Play();
        LoginScreen.SetActive(false);   
        ConnectingScreen.SetActive(true);

        localReady = false;

        StartClient();
    }

    [Client]
    public void OnExitButton()
    {
        audioSource.Play();
        Application.Quit();
    }

    [Header("ConnectingScreen")]
    public GameObject ConnectingScreen;

    [Client]
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // tell the server to create a player with this name
        conn.Send(new CreatePlayerMessage { name = PlayerName });

        ConnectingScreen.SetActive(false);
        StartScreen.SetActive(true);

    }

    [Header("StartScreen")]
    public GameObject StartScreen;
    public GameObject ConnectedLabel;
    public GameObject ReadyButton;
    public GameObject NotReadyButton;
    public GameObject StartButton;
    public GameObject LeaveButton;

    [Header("LoadScreen")]
    public GameObject LoadScreen;

    [Client]
    public void OnReadyButton()
    {
        audioSource.Play();        

        localReady = !localReady;
        
        ReadyButton.SetActive(!localReady);
        NotReadyButton.SetActive(localReady);
        
        NetworkClient.connection.Send(new ReadyMessage { isReady = localReady });
    }

    [Client]
    public void OnLeaveButton()
    {
        audioSource.Play();

        StopClient();

        ReadyButton.SetActive(true);
        NotReadyButton.SetActive(false);
        StartButton.GetComponent<Button>().enabled = false;
        StartScreen.SetActive(false);

        LoginScreen.SetActive(true);
    }

    ///////////////
    // First the server creates a player and updates:
    // waitingConnections
    // playerNames
    // playerReady
    // Each Client's lobby list
    ///////////////

    [Server]
    private void OnCreatePlayer(NetworkConnection conn, CreatePlayerMessage createPlayerMessage)
    {
        waitingConnections.Add(conn);

        if (createPlayerMessage.name != "")
            playerNames[conn] = createPlayerMessage.name;
        else
            playerNames[conn] = "Anonymous";

        playerReady[conn] = false;
    
        UpdateLobbyList();

        Debug.Log($"{conn}: {playerNames[conn]} has joined.");

        /*
        #if UNITY_EDITOR
        if (goFast && waitingConnections.Count == 2)
            allReady = true;
            StartCoroutine(GameStart());
        #endif*/
    }

    ///////////////
    // Next the server waits until a player presses the ready button.
    // Updates each player's lobby list accordingly.
    // Then checks if all players are ready.
    // Updates Start buttons interactable with value of allReady
    ///////////////

    public void ReadyChange(NetworkConnection conn, ReadyMessage readyMessage)
    {
        playerReady[conn] = readyMessage.isReady;
        
        UpdateLobbyList();

        allReady = true;
        foreach (KeyValuePair<NetworkConnection, bool> entry in playerReady)
            allReady = allReady && entry.Value;
        
        NetworkServer.SendToAll(new AllReadyMessage{ allReady = allReady });
    }

    public void AllReadyChange(AllReadyMessage allReadyMessage)
    {
        StartButton.GetComponent<Button>().interactable = allReadyMessage.allReady;
    }

    public void OnStartButton()
    {
        NetworkClient.Send(new StartMessage {});
    }

    private void UpdateLobbyList()
    {
        string nameList = $"{playerNames.Count} connected players:";
        foreach (KeyValuePair <NetworkConnection, string> entry in playerNames)
        {
            nameList += $"\n{entry.Value}";
            if (playerReady[entry.Key])
                nameList += " [READY]";
        }
        NetworkServer.SendToAll(new LobbyListMessage { lobbyList = nameList });
    }

    [Client]
    public void UpdateClientLobbyList(LobbyListMessage lobbyListMessage)
    {
        ConnectedLabel.GetComponentInChildren<Text>().text = lobbyListMessage.lobbyList;
    }

    [Client]
    public void ResetReady(ResetReadyMessage resetReadyMessage)
    {
        localReady = false;
        ReadyButton.SetActive(true);
        NotReadyButton.SetActive(false);
    }

    ///////////////
    // When a client disconnects (leaves), some clean-up is needed:
    // remove waitingConnection
    // remove playerName
    // remove playerReady
    // All other players are set to playerReady false (d/c stops game start)
    // Those players also get their ready buttons reset
    // Update each Client's lobby list
    ///////////////

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"{conn}: {playerNames[conn]} has left.");

        waitingConnections.Remove(conn);

        if (playerNames[conn] != null)
            playerNames.Remove(conn);

        // Reset playerReady.
        playerReady.Clear();
        foreach (NetworkConnection readyConn in waitingConnections)
            playerReady[readyConn] = false;
        NetworkServer.SendToAll(new ResetReadyMessage { });

        string nameList = $"{playerNames.Count} connected players:";
        foreach (KeyValuePair <NetworkConnection, string> entry in playerNames)
            nameList += $"\n {entry.Value}";
        NetworkServer.SendToAll(new LobbyListMessage { lobbyList = nameList });

        // Reset game on all disconnect
        if (NetworkManager.singleton.numPlayers == 0)
        {
            GameObject.Destroy(gameManager);
            GameObject.Destroy(GameObject.Find("GameBoard(Clone)"));
            
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Robber"));
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Robber"));
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Robber"));
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Robber"));
            
            playerIds.Clear();
            playerNames.Clear();
            playerReady.Clear();
        }
    }
    
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log($"Disconnected from server.");

        LoginScreen.SetActive(true);
        ConnectingScreen.SetActive(false);
        StartScreen.SetActive(false);
    }


    [Server]
    public void StartGame(StartMessage startMessage)
    {
        if (allReady && GameObject.FindGameObjectWithTag("GameController") == null)
        {
            NetworkServer.SendToAll(new ClientStartMessage{});
            StartCoroutine(GameStart());
        }
    }

    public void ClientStartGame(ClientStartMessage clientStartMessage)
    {
        Lobby.SetActive(false);
    }


    [Server]
    IEnumerator GameStart()
    {
        /*
        #if UNITY_EDITOR
        if (goFast) yield return new WaitForSeconds(3f);
        #endif
        */

        // Assign each player connection a number (1-based, will be the playerIndex).
        int i = 1;
        foreach (NetworkConnection conn in waitingConnections)
        {
            playerConns[i] = conn;
            i++;
        }

        // Spawn the gameManager object.
        gameManager = Instantiate(gameManagerPrefab);
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