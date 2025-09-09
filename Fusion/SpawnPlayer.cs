using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using TMPro;

public enum GameState
{
    Lobby,
    InGame
}

public class SpawnPlayer : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner networkRunner;
    private NetworkRunner lobbyRunner;

    [SerializeField] public GameObject PlayerPrefab;
    [SerializeField] private SessionListUIhandler sessionListUI;
    [SerializeField] private bool isHost = false; // Host olup olmadýðýný kontrol etmek için
    [SerializeField] private Transform[] spawnPoints;
    public GameObject[] Karakterler;
    public int Number;
    private bool attackBuffer = false;
    private bool jumpBuffer = false;

    // UI References
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject roomUI;


    // Scene Settings
    [SerializeField] private int gameSceneIndex = 1;

    // Room UI Elements
    [Header("Room UI")]
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    private string currentRoomName = "";

    // State
    private GameState currentGameState = GameState.Lobby;
    public Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public static SpawnPlayer Instance;

    private void Awake()
    {
        // Tekrarlý objeleri önlemek için singleton kontrolü
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        SetGameState(GameState.Lobby);
        await StartSessionDiscovery();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            attackBuffer = true;
        }
        if(Input.GetButtonDown("Jump"))
        {
            jumpBuffer = true;
        }
    }

    public void ChangeCharacter(int karakterIndex)
    {
        for (int i = 0; i < Karakterler.Length; i++)
        {
            Karakterler[i].SetActive(false);
        }

        Number += karakterIndex;

        if (Number >= Karakterler.Length )
        {
            Number = 0;
        }
        else if (Number < 0)
        {
            Number = Karakterler.Length-1;
        }

        Karakterler[Number].SetActive(true);
        string karakterAdi = Karakterler[Number].name;
        PlayerPrefs.SetString("SelectedCharacter", karakterAdi);
        Debug.Log($"Selected character: {karakterAdi}");
    }



    private void SetGameState(GameState newState)
    {
        currentGameState = newState;
        UpdateUIBasedOnGameState();
        Debug.Log($"Game state: {currentGameState}");
    }

    private void UpdateUIBasedOnGameState()
    {
        // Tüm UI'larý kapat
        if (lobbyUI != null) lobbyUI.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (roomUI != null) roomUI.SetActive(false);

        // State'e göre UI aç
        switch (currentGameState)
        {
            case GameState.Lobby:
                if (lobbyUI != null) lobbyUI.SetActive(true);
                break;
            case GameState.InGame:
                if (gameUI != null) gameUI.SetActive(true);
                break;
        }
    }

    private async Task StartSessionDiscovery()
    {
        if (lobbyRunner != null && lobbyRunner.IsRunning)
            await lobbyRunner.Shutdown();

        GameObject lobbyObject = new GameObject("LobbyRunner");
        lobbyObject.transform.SetParent(this.transform);
        lobbyRunner = lobbyObject.AddComponent<NetworkRunner>();
        lobbyRunner.AddCallbacks(this);

        try
        {
            await lobbyRunner.JoinSessionLobby(SessionLobby.Shared);
            Debug.Log("Session discovery started");
        }
        catch (Exception e)
        {
            Debug.LogError($"Session discovery failed: {e.Message}");
        }
    }

    public async void CreateRoom(string roomName, int maxPlayers = 4)
    {
        if (lobbyRunner != null && lobbyRunner.IsRunning)
            await lobbyRunner.Shutdown();
        isHost = true; // Host olarak ayarla
        await StartNetworkRunner(roomName, maxPlayers);
    }

    public async void JoinRoom(SessionInfo sessionInfo)
    {
        if (lobbyRunner != null && lobbyRunner.IsRunning)
            await lobbyRunner.Shutdown();

        await StartNetworkRunner(sessionInfo.Name, 4);
    }

    private async Task StartNetworkRunner(string sessionName, int maxPlayers)
    {
        networkRunner = gameObject.GetComponent<NetworkRunner>();
        if (networkRunner == null)
            networkRunner = gameObject.AddComponent<NetworkRunner>();

        networkRunner.ProvideInput = true; 
        networkRunner.AddCallbacks(this);
        var scene = SceneRef.FromIndex(gameSceneIndex);
        currentRoomName = sessionName;

        try
        {
            await networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                IsVisible = true,
                IsOpen = true
            });

            SetGameState(GameState.InGame);
            Debug.Log($"Network runner started: {sessionName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Network runner failed: {e.Message}");
            SetGameState(GameState.Lobby);
            await StartSessionDiscovery();
        }
    }

    // Host oyunu baþlatýr
    public async void StartGame()
    {
        if (currentGameState != GameState.Lobby) return;

        Debug.Log("Host starting game...");

        SetGameState(GameState.InGame);
        networkRunner.ProvideInput = true; // Input'u aktif et

        // Oyun scene'ine geç
        var gameScene = SceneRef.FromIndex(gameSceneIndex);

        try
        {
            await networkRunner.LoadScene(gameScene);
            Debug.Log("Transitioned to game scene");
        }
        catch (Exception e)
        {
            Debug.LogError($"Scene transition failed: {e.Message}");
            SetGameState(GameState.Lobby);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner != networkRunner) return;

        Debug.Log($"Player joined: {player}");

        // Oyuncuyu spawn et
        if (player == runner.LocalPlayer)
        {
            SpawnPlayerCharacter(player, runner);
        }
        UpdateRoomUI();
    }

    private void SpawnPlayerCharacter(PlayerRef player, NetworkRunner runner)
    {
        if (PlayerPrefab == null) return;

        Vector3 spawnPos = GetSpawnPosition(player);

        // Player prefabýný spawn et
        NetworkObject networkObject = runner.Spawn(
            PlayerPrefab,
            spawnPos,
            Quaternion.identity,
            player
        );

        if (networkObject != null)
        {
            spawnedCharacters[player] = networkObject;

            // Sadece kendi karakterimiz için model seçimini uygula
            if (player == runner.LocalPlayer)
            {
                var playerController = networkObject.GetComponent<Hareket>();
                if (playerController != null && playerController.HasStateAuthority)
                {
                    string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter");
                    if (!string.IsNullOrEmpty(selectedCharacter))
                    {
                        playerController.SelectedCharacterName = selectedCharacter;
                    }
                }
            }

            Debug.Log($"Player spawned: {player}");
        }
    }

    private void SetActiveCharacterModel(GameObject playerObject)
    {
        // Seçilen karakter index'ini al
        string selectedCharacterIndex = PlayerPrefs.GetString("SelectedCharacter");

       for(int i = 0; i < playerObject.transform.childCount; i++)
        {
            Transform child = playerObject.transform.GetChild(i);
            if(child.name == "Root")
            {
                continue;
            }
            child.gameObject.SetActive(child.name == selectedCharacterIndex );
            
        }

        
    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        if (currentGameState == GameState.InGame)
        {
           var transform  = spawnPoints[player.RawEncoded % spawnPoints.Length];
            return transform.position;
        }
        else
        {
            // Lobby spawn pozisyonlarý
            return new Vector3(player.RawEncoded * 2f, 0, 0);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner != networkRunner) return;

        Debug.Log($"Player left: {player}");

        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            if (networkObject != null)
                runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);
        }

        UpdateRoomUI();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log($"Scene loaded - State: {currentGameState}");

        if (currentGameState == GameState.InGame)
        {
            // GameManager'a ownership ver (sadece host)
            if (isHost)
            {
                AssignGameManagerAuthority();
            }
        }
    }

    private void AssignGameManagerAuthority()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && gameManager.Object != null)
        {
            if (gameManager.Object.HasStateAuthority == false)
            {
                gameManager.Object.RequestStateAuthority();
                Debug.Log("StateAuthority requested for GameManager.");
            }
            else
            {
                Debug.Log("Already has StateAuthority for GameManager.");
            }
        }
        else
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (runner != networkRunner || currentGameState != GameState.InGame) return;

        NetworkInputData data = new NetworkInputData
        {
            move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            jump = jumpBuffer,
            crouch = Input.GetKey(KeyCode.LeftControl),
            run = Input.GetKey(KeyCode.LeftShift),
            attack = attackBuffer
        };
        input.Set(data);
        attackBuffer = false;
        jumpBuffer = false;
    }

    private void UpdateRoomUI()
    {
        if (roomNameText != null)
            roomNameText.text = currentRoomName;

        if (playerCountText != null)
            playerCountText.text = "2/4";
    }

    public void BackToLobby()
    {
        if (lobbyUI != null) lobbyUI.SetActive(true);
        if (roomUI != null) roomUI.SetActive(false);
    }

    public async void LeaveRoom()
    {
        if (networkRunner != null && networkRunner.IsRunning)
            await networkRunner.Shutdown();

        SetGameState(GameState.Lobby);

        spawnedCharacters.Clear();
        await StartSessionDiscovery();
    }

    // UI Methods
    public void onCreateRoom()
    {
        if (roomUI != null) roomUI.SetActive(true);
        if (lobbyUI != null) lobbyUI.SetActive(false);
    }

    public async void RefreshSessionList()
    {
        if (lobbyRunner != null && lobbyRunner.IsRunning)
        {
            await lobbyRunner.Shutdown();
            await Task.Delay(500);
        }
        await StartSessionDiscovery();
    }

    // Session List Handling
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (runner != lobbyRunner || sessionListUI == null) return;

        sessionListUI.ClearList();

        if (sessionList.Count == 0)
        {
            sessionListUI.OnNoSessionsFound();
        }
        else
        {
            foreach (var session in sessionList)
            {
                if (session.IsOpen && session.PlayerCount < session.MaxPlayers && session.IsVisible)
                {
                    sessionListUI.AddToList(session);
                }
            }
        }
    }

    public void OnJoinSessionRequested(SessionInfo sessionInfo)
    {
        JoinRoom(sessionInfo);
    }

    // Cleanup
    private async void OnDestroy()
    {
        if (lobbyRunner != null && lobbyRunner.IsRunning)
            await lobbyRunner.Shutdown();
        if (networkRunner != null && networkRunner.IsRunning)
            await networkRunner.Shutdown();
    }

    #region Network Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log(runner == lobbyRunner ? "Connected to lobby" : "Connected to session");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Connection failed: {reason}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected: {reason}");
        if (runner == networkRunner)
        {
            SetGameState(GameState.Lobby);
            _ = StartSessionDiscovery();
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Runner shutdown: {shutdownReason}");
    }

    // Empty implementations
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    #endregion
}