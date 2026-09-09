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

    private void Start()
    {
        //Set Orientation to Potrait
        ScreenOrientationManager.SetLandscape();
        runner = NetworkManager.Instance.GetRunner();

        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found.");
            return;
        }

        // Register this object for Fusion callbacks.
        runner.AddCallbacks(this);

        // Initially hide player slots.
        player1Panel.SetActive(false);
        player2Panel.SetActive(false);

        // Match button disabled until 2 players are present.
        enterMatchButton.interactable = false;

        // Display room code.
        joinCodeText.text =$"{NetworkManager.Instance.RoomCode}";

        // Update current players.
        UpdateLobbyUI();
    }

    private void UpdateLobbyUI()
    {
        if (runner == null)
            return;

        var players = runner.ActivePlayers.ToList();

        int playerCount = players.Count;

        Debug.Log($"Players in lobby: {playerCount}");

        // Reset UI.
        player1Panel.SetActive(false);
        player2Panel.SetActive(false);

        // Player 1
        if (playerCount >= 1)
        {
            player1Panel.SetActive(true);

            player1Text.text = "PLAYER 1\nREADY";
        }

        // Player 2
        if (playerCount >= 2)
        {
            player2Panel.SetActive(true);

            player2Text.text =
                "PLAYER 2\nREADY";
        }

        // Update status.
        if (playerCount < 2)
        {
            lobbyStatusText.text =
                "Waiting for Player 2...";
        }
        else
        {
            lobbyStatusText.text =
                "Both players are ready!";
        }

        // Only allow entering match when both players exist.
        enterMatchButton.interactable =
            playerCount == 2;
    }

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log(
            $"Player joined lobby: {player}"
        );

        UpdateLobbyUI();
    }

    public void OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log(
            $"Player left lobby: {player}"
        );

        UpdateLobbyUI();
    }

    public void OnEnterMatchClicked()
    {
        if (runner == null)
            return;

        if (!runner.IsSceneAuthority)
        {
            Debug.Log(
                "Only Scene Authority can start the match."
            );

            return;
        }

        if (runner.ActivePlayers.Count() < 2)
        {
            Debug.Log(
                "Cannot start match. Need 2 players."
            );

            return;
        }

        Debug.Log("Starting Match...");

        SceneRef matchScene =
            SceneRef.FromIndex(2);

        runner.LoadScene(
            matchScene,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    // --------------------------------------------------
    // Required Fusion callbacks
    // --------------------------------------------------

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
    }

    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    public void OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(
        NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
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

    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
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

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        System.ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }

    public void OnSceneLoadDone(
        NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(
        NetworkRunner runner)
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

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
    {
        throw new NotImplementedException();
    }
}