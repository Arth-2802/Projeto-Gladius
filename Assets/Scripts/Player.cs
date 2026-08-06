using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const string DashAnimationState = "Dash";

    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator anim;

    [Header("Movement Variables")]
    public float speed;
    public float JumpForce;
    public float JumpCutMultiplier = .5f;
    public int extraJumps = 1;
    public float normalGravity;
    public float fallGravity;
    public float JumpGravity;

    [Header("Dash Variables")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.5f;
    public int airDashes = 1;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    private bool isGrounded;

    public int facingDirection = 1;
    //Inputs
    public Vector2 moveInput;
    private bool JumpPressed;
    private bool JumpReleased;
    private int extraJumpsRemaining;

    //Dash
    private float dashCooldownCounter = 0f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private int airDashesRemaining;

    private void Start()
    {
        rb.gravityScale = normalGravity;
        extraJumpsRemaining = extraJumps;
        airDashesRemaining = airDashes;
    }

    void Update()
    {
        Flip();
        HandleAnimations();
        UpdateDashCooldown();
    }

    void FixedUpdate()
    {
        ApplyVariableGravity();
        CheckGrounded();
        HandleMovement();
        HandleJump();
        HandleDash();
    }

    private void HandleMovement()
    {
        if (!isDashing)
        {
            float targetSpeed = moveInput.x * speed;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (JumpPressed && !isDashing)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                JumpReleased = false;
            }
            else if (extraJumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                extraJumpsRemaining--;
                JumpReleased = false;
            }

            JumpPressed = false;
        }
        if (JumpReleased)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * JumpCutMultiplier);
            }
            JumpReleased = false;
        }
    }

    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0);
            
            if (dashTimer <= 0)
            {
                isDashing = false;
                anim.SetBool("isDashing", false);
            }
        }
    }

    void UpdateDashCooldown()
    {
        if (dashCooldownCounter > 0)
        {
            dashCooldownCounter -= Time.deltaTime;
        }
    }

    void ApplyVariableGravity()
    {
        if (isDashing) return;

        if (rb.linearVelocity.y < -0.1f)
        {
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0.1f)
        {
            rb.gravityScale = JumpGravity;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    } 

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            extraJumpsRemaining = extraJumps;
            airDashesRemaining = airDashes;
        }
    }

    void HandleAnimations()
    {
        anim.SetBool("isJumping", rb.linearVelocity.y > 1f && !isDashing);
        anim.SetBool("isGrounded", isGrounded);

        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        anim.SetBool("isIdle", Mathf.Abs(moveInput.x) < .1f && isGrounded && !isDashing);
        anim.SetBool("isWalking", Mathf.Abs(moveInput.x) > .1f && isGrounded && !isDashing);
    }

    void Flip()
    {
        if(moveInput.x >0.1f)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if(moveInput.x < -0.1f)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpPressed = true;
        }
        else
        {
            JumpReleased = true;
        }
    }

    public void OnDash(InputValue value)
    {
        bool canAirDash = !isGrounded && airDashesRemaining > 0;

        if (value.isPressed && dashCooldownCounter <= 0 && !isDashing && (isGrounded || canAirDash))
        {
            if (!isGrounded)
            {
                airDashesRemaining--;
            }

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownCounter = dashCooldown;
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isJumping", false);
            anim.SetBool("isDashing", true);
            anim.Play(DashAnimationState, 0, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}






