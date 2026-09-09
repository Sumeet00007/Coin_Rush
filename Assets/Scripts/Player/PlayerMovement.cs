using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.0f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input))
            return;

        // =========================================
        // MOVEMENT
        // =========================================

        Vector2 moveInput = input.MoveInput;

        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );

        // =========================================
        // JUMP
        // =========================================

        if (input.Buttons.IsSet(PlayerInputButton.Jump)
            && CheckGround())
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );
    }

    private bool CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(
            transform.position,
            transform.position +
            Vector3.down * groundCheckDistance
        );
    }
}