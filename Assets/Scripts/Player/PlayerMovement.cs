using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    //Already Tested
    private float moveSpeed = 3.5f;
    private float jumpForce = 6.5f;
    private Rigidbody2D rb;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    


    [Networked] private NetworkBool IsRunning { get; set; }
    [Networked] private NetworkBool IsJumping { get; set; }
    [Networked] private NetworkBool FacingLeft { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public override void FixedUpdateNetwork()
    {
        //if no input from NetworkInputData
        if (!GetInput(out NetworkInputData input)) return;

        Vector2 moveInput = input.MoveInput;

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed,
            rb.linearVelocity.y);

        IsRunning = Mathf.Abs(moveInput.x) > 0.01f;

        //if moveInput.x is -ve then flip sprite towards left
        if (moveInput.x < 0)
        {
            FacingLeft = true;
        }

        //if moveInput.x is +ve then flip sprite towards left
        else if (moveInput.x > 0)
        {
            FacingLeft = false;
        }

        bool isGrounded = CheckGround();

        if (input.Buttons.IsSet(PlayerInputButton.Jump) && isGrounded)
        {
            Jump();
            IsJumping = true;
        }

        else
        {
            IsJumping = !isGrounded;
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce,ForceMode2D.Impulse);
    }

    private bool CheckGround()
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

    public override void Render()
    {
        UpdateAnimation();
        UpdateSpriteFlip();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        animator.SetBool("isRunning",IsRunning);
        animator.SetBool("isJumping", IsJumping);
    }
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null)return;
        spriteRenderer.flipX = FacingLeft;
    }

}