using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField]
    private float jumpForce = 8f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundCheckDistance = 0.7f;

    private NetworkButtons previousButtons;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out GameplayInput input)) return;

        Vector2 moveDirection =  input.MoveDirection;

        // Sanitize client input.
        moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);

        Move(moveDirection);

        if (input.Buttons.WasPressed(previousButtons,PlayerInputButton.Jump))
        {
            if (IsGrounded())
            {
                Jump();
            }
        }

        previousButtons = input.Buttons;
    }

    private void Move(Vector2 direction)
    {
        Vector2 velocity = rb.linearVelocity;

        velocity.x = direction.x * moveSpeed;

        rb.linearVelocity = velocity;
    }

    private void Jump()
    {
        Vector2 velocity = rb.linearVelocity;

        velocity.y = jumpForce;

        rb.linearVelocity = velocity;
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast
            (
                transform.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

        return hit.collider != null;
    }
}