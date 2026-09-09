using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControls;

public class PlayerInputProvider : MonoBehaviour, INetworkRunnerCallbacks, IPlayerActions
{
    private PlayerControls playerControls;

    private Vector2 moveInput;

    private bool jumpPressed;

    private NetworkRunner runner;

    private void Awake()
    {
        // -----------------------------------------
        // Create Input Actions
        // -----------------------------------------

        playerControls = new PlayerControls();

        // -----------------------------------------
        // Get NetworkRunner
        // -----------------------------------------

        runner = GetComponent<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError(
                "PlayerInputProvider: NetworkRunner not found."
            );
        }
    }

    private void OnEnable()
    {
        // -----------------------------------------
        // Register Input Action callbacks
        // -----------------------------------------

        playerControls.Player.SetCallbacks(this);

        playerControls.Enable();

        // -----------------------------------------
        // Register Fusion callbacks
        // -----------------------------------------

        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }

    private void OnDisable()
    {
        // -----------------------------------------
        // Unregister Input Action callbacks
        // -----------------------------------------

        playerControls.Player.SetCallbacks(null);

        playerControls.Disable();

        // -----------------------------------------
        // Unregister Fusion callbacks
        // -----------------------------------------

        if (runner != null)
        {
            runner.RemoveCallbacks(this);
        }
    }

    // ============================================================
    // INPUT SYSTEM
    // ============================================================

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
        {
            moveInput = context.ReadValue<Vector2>();

            Debug.Log(
                $"INPUT MOVE: {moveInput}"
            );
        }

        if (context.canceled)
        {
            moveInput = Vector2.zero;

            Debug.Log("INPUT MOVE RELEASED");
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;

            Debug.Log("INPUT JUMP");
        }
    }

    // ============================================================
    // FUSION INPUT
    // ============================================================

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();

        // -----------------------------------------
        // Movement
        // -----------------------------------------

        data.MoveInput = moveInput;

        // -----------------------------------------
        // Jump
        // -----------------------------------------

        data.Buttons.Set(
            PlayerInputButton.Jump,
            jumpPressed
        );

        // -----------------------------------------
        // Send input to Fusion
        // -----------------------------------------

        input.Set(data);

        Debug.Log(
            $"FUSION INPUT | Move: {data.MoveInput} | Jump: {jumpPressed}"
        );

        // Jump is a one-shot input.
        jumpPressed = false;
    }

    // ============================================================
    // FUSION CALLBACKS
    // ============================================================

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
    }

    public void OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
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
        ReliableKey key,
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

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ReadOnlySpan<byte> data)
    {
    }
}