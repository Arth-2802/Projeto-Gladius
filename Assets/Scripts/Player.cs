using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const string DashAnimationState = "Dash";
    private const string AttackTrigger = "Attack";

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

    [Header("Attack Variables")]
    [Tooltip("Tempo em que o jogador fica parado após iniciar o ataque.")]
    public float attackDuration = 0.4f;
    [Tooltip("Intervalo mínimo entre o início de dois ataques.")]
    public float attackCooldown = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 17;
    [SerializeField, Range(0f, 1f)] private float criticalChance = 0.25f;
    [SerializeField] private int criticalDamage = 27;

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

    //Attack
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackCooldownCounter = 0f;

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
        UpdateAttackTimers();
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
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (!isDashing)
        {
            float targetSpeed = moveInput.x * speed;
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (JumpPressed && !isDashing && !isAttacking)
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

    void UpdateAttackTimers()
    {
        if (attackCooldownCounter > 0)
        {
            attackCooldownCounter -= Time.deltaTime;
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
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
        anim.SetBool("isJumping", rb.linearVelocity.y > 1f && !isDashing && !isAttacking);
        anim.SetBool("isGrounded", isGrounded);

        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        anim.SetBool("isIdle", Mathf.Abs(moveInput.x) < .1f && isGrounded && !isDashing && !isAttacking);
        anim.SetBool("isWalking", Mathf.Abs(moveInput.x) > .1f && isGrounded && !isDashing && !isAttacking);
    }

    void Flip()
    {
        if (isAttacking) return;

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
        if (isAttacking) return;

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

        if (value.isPressed && dashCooldownCounter <= 0 && !isDashing && !isAttacking && (isGrounded || canAirDash))
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

    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isDashing && !isAttacking && attackCooldownCounter <= 0)
        {
            isAttacking = true;
            attackTimer = attackDuration;
            attackCooldownCounter = attackCooldown;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            anim.SetTrigger(AttackTrigger);
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (attackPoint == null)
        {
            attackPoint = transform;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        bool isCritical = Random.value < criticalChance;
        int damage = isCritical ? criticalDamage : attackDamage;

        foreach (Collider2D hit in hits)
        {
            minion enemy = hit.GetComponentInParent<minion>();
            if (enemy == null)
            {
                continue;
            }

            enemy.ReceiveAttack(damage, isCritical, transform.position);
            Debug.Log(isCritical
                ? "Jogador acertou um crítico de " + damage + " no minion."
                : "Jogador acertou o minion por " + damage + " de dano.");
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}






