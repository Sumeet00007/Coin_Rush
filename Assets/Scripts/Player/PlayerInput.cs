using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControls;

public class PlayerInput : NetworkBehaviour, IPlayerActions
{
    private PlayerControls playerControls;

    private GameplayInput gameplayInput;

    public override void Spawned()
    {
        // Only the player who owns this object reads
        // local keyboard/mobile input.
        if (!HasInputAuthority)
            return;

        playerControls =
            new PlayerControls();

        playerControls.Player.SetCallbacks(this);

        playerControls.Enable();

        Debug.Log(
            $"Input enabled for {Object.InputAuthority}"
        );
    }

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        if (playerControls != null)
        {
            playerControls.Player.SetCallbacks(null);
            playerControls.Disable();
        }
    }

    // ==========================================
    // UNITY INPUT SYSTEM
    // ==========================================

    public void OnMove(
        InputAction.CallbackContext context)
    {
        if (!HasInputAuthority)
            return;

        gameplayInput.MoveDirection =
            context.ReadValue<Vector2>();
    }

    public void OnJump(
        InputAction.CallbackContext context)
    {
        if (!HasInputAuthority)
            return;

        if (context.performed)
        {
            gameplayInput.Buttons.Set(
                PlayerInputButton.Jump,
                true
            );
        }
    }

    // ==========================================
    // FUSION INPUT
    // ==========================================

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        if (!HasInputAuthority)
            return;

        input.Set(gameplayInput);

        // Reset one-shot input.
        gameplayInput.Buttons.Set(
            PlayerInputButton.Jump,
            false
        );
    }
}