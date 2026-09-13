using System;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Room UI")]
    [SerializeField] private TMP_Text joinCodeText;

    [Header("Player UI")]
    [SerializeField] private GameObject player1Panel;
    [SerializeField] private GameObject player2Panel;
    [SerializeField] private TMP_Text player1Text;
    [SerializeField] private TMP_Text player2Text;

    [Header("Lobby UI")]
    [SerializeField] private TMP_Text lobbyStatusText;
    [SerializeField] private Button enterMatchButton;

    private NetworkRunner runner;

    private bool isPlayer1Joined;
    private bool isPlayer2Joined;

    private static readonly ReliableKey StartMatchKey = ReliableKey.FromInts(100, 200, 300, 400);
    private bool matchStarting;

    private void Start()
    {
        ScreenOrientationManager.SetLandscape();
        runner = NetworkManager.Instance.GetRunner();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found.");
            return;
        }

        runner.AddCallbacks(this);
      
        player1Panel.SetActive(false);
        player2Panel.SetActive(false);
      
        enterMatchButton.interactable = false;

        // Display room code.
        joinCodeText.text = $"{NetworkManager.Instance.roomCode}";

        // Update current players.
        UpdateLobbyUI();
    }

    private void UpdateLobbyUI()
    {
        if (runner == null) return;

        var players = runner.ActivePlayers.ToList();
        int playerCount = players.Count;

        isPlayer1Joined = playerCount >= 1;
        isPlayer2Joined = playerCount >= 2;
        //Debug.Log($"Players in lobby: {playerCount}");

        // Reset UI.
        player1Panel.SetActive(false);
        player2Panel.SetActive(false);

        // Player 1.
        if (isPlayer1Joined)
        {
            player1Panel.SetActive(true);
            player1Text.text = "PLAYER 1\nREADY";
        }

        // Player 2.
        if (isPlayer2Joined)
        {
            player2Panel.SetActive(true);
            player2Text.text = "PLAYER 2\nREADY";
        }

        // Lobby status.
        if (!isPlayer2Joined)
        {
            lobbyStatusText.text = "Waiting for Player 2...";
        }
        else
        {
            lobbyStatusText.text = "Both players are ready!";
        }

        // Both players can click the button.
        enterMatchButton.interactable = isPlayer1Joined && isPlayer2Joined && !matchStarting;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //Debug.Log($"Player joined lobby: {player}");
        UpdateLobbyUI();
    }

    public void OnPlayerLeft(NetworkRunner runner,PlayerRef player)
    {
        //Debug.Log($"Player left lobby: {player}");
        matchStarting = false;
        UpdateLobbyUI();
    }

    public void OnEnterMatchClicked()
    {
        if (runner == null) return;

        if (matchStarting)
        {
            //Debug.Log("Match is already starting.");
            return;
        }

        if (!isPlayer1Joined || !isPlayer2Joined)
        {
            //Debug.Log("Cannot start match. Both players must be joined.");
            return;
        }

        if (runner.ActivePlayers.Count() < 2)
        {
            //Debug.Log("Cannot start match. Need 2 players.");
            return;
        }

        //Debug.Log($"Start Match clicked by {runner.LocalPlayer}");

        matchStarting = true;
        enterMatchButton.interactable = false;
        lobbyStatusText.text = "Starting match...";

        // CASE 1:
        // Local player IS Scene Authority.
        if (runner.IsSceneAuthority)
        {
            //Debug.Log("Local player is Scene Authority. Starting match directly.");
            StartMatch();
            return;
        }

        // CASE 2:
        // Local player is NOT Scene Authority.
        // Send request to Scene Authority.

        //Debug.Log("Local player is not Scene Authority.");
        //Debug.Log("Sending Start Match request to Scene Authority...");

        SendStartMatchRequest();
    }

    private void SendStartMatchRequest()
    {
        if (runner == null) return;
        // Small payload.
        byte[] requestData = new byte[] { 1 };

        runner.SendReliableDataToServer(StartMatchKey, requestData);
        //Debug.Log("Start Match request sent to server.");
    }
   
    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ReadOnlySpan<byte> data)
    {
        // Ignore unrelated reliable data.
        if (key != StartMatchKey)
            return;

        //Debug.Log($"Reliable data received from {player}.");

        // Only Scene Authority is allowed to start the scene.
        if (!runner.IsSceneAuthority)
        {
            //Debug.Log("Received Start Match request, but local player is not Scene Authority.");
            return;
        }

        // Validate request.
        if (data.Length == 0 || data[0] != 1)
        {
            //Debug.LogWarning("Invalid Start Match request.");
            return;
        }

        //Debug.Log($"Start Match request received from {player}. Starting match...");

        StartMatch();
    }

    private void StartMatch()
    {
        if (runner == null)  return;

        // Only Scene Authority may load networked scene.
        if (!runner.IsSceneAuthority)
        {
            //Debug.LogWarning("StartMatch called on a non-authority player.");
            return;
        }

        // Make sure both players still exist.
        if (runner.ActivePlayers.Count() < 2)
        {
            Debug.LogWarning("Cannot start match. Both players are required.");
            matchStarting = false;
            UpdateLobbyUI();
            return;
        }

        if (matchStarting == false)
        {
            matchStarting = true;
        }

        //Debug.Log("Scene Authority is loading Game Scene...");
        lobbyStatusText.text = "Starting match...";
        SceneRef matchScene = SceneRef.FromIndex(2);
        runner.LoadScene(matchScene,UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //Debug.Log("Fusion: Game scene loading started.");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //Debug.Log("Fusion: Game scene loaded for this player.");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner,ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner,NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner,SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        System.Collections.Generic.List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        System.Collections.Generic.Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }

    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }
}