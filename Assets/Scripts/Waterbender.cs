using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class Waterbender : MonoBehaviour, IDeathListener {

    private Rigidbody2D rigidBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Damageable damageable;
    private WaterFlask water;

    [Header("Running")]

    [SerializeField]
    private float moveForce = 10.0f;
    [SerializeField]
    private float maxSpeed = 5.0f;
    [SerializeField]
    private float walkingThreshold = 0.2f;

    private float move = 0f;

    [Header("Jumping")]

    [SerializeField]
    private float jumpForce = 10.0f;

    enum JumpState {TakeOff, Jump, None};

    private JumpState jump = JumpState.None;
    private bool grounded = false;

    [SerializeField]
    private float grounderPosition = -1f;
    [SerializeField]
    private float grounderHeight = 0.1f;
    [SerializeField]
    private float grounderWidth = 0.2f;

    private int groundMask;
    private int waterMask;

    [SerializeField]
    private int jumpMaxFrameCount = 10;
    private int jumpFrameCount;

    [Header("Audio")]

    [SerializeField]
    private AudioSource movementSound;
    [SerializeField]
    private AudioSource jumpSound;
    [SerializeField]
    private AudioSource painSound;

    [SerializeField]
    private AudioClip hurtSoundClip;
    [SerializeField]
    private AudioClip deathSoundClip;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
        water = GetComponentInChildren<WaterFlask>();

        groundMask = 1 << LayerMask.NameToLayer("Scene");
        waterMask  = 1 << LayerMask.NameToLayer("Water");
    }

    public void Reset ()
    {
        water.Reset();

        transform.position = Vector3.zero;
        rigidBody.velocity = Vector3.zero;

        damageable.Restore();

        animator.SetBool("Jump", false);
        animator.SetBool("Walking", false);
        animator.SetBool("Bending", false);
        animator.Rebind();

        movementSound.Stop();
        jumpSound.Stop();
        painSound.Stop();

        move = 0f;
        jump = JumpState.None;
    }

    void Update ()
    {
        if (GameController.GameManager.GameOn && damageable.Alive && !water.IsGatheringWater())
        {
            jump = Input.GetButtonDown("Jump") ? JumpState.TakeOff : Input.GetButton("Jump") ? JumpState.Jump : JumpState.None;
            move = Input.GetAxis("Horizontal");
        }
        else
        {
            jump = JumpState.None;
            move = 0f;
        }
    }

    bool IsGrounded ()
    {
        Vector2 position = transform.position;
        Vector2 upperCorner = position + Vector2.up * (grounderPosition + grounderHeight) - Vector2.right * grounderWidth;
        Vector2 lowerCorner = position + Vector2.up * (grounderPosition - grounderHeight) + Vector2.right * grounderWidth;
        bool onGround = Physics2D.OverlapArea(upperCorner, lowerCorner, groundMask);

        if (!onGround && water.Frozen)
        {
            onGround = Physics2D.OverlapArea(upperCorner, lowerCorner, waterMask);
        }
        return onGround;
    }

    void Run ()
    {
        if (move * transform.localScale.x < 0)
        {
            Flip();
        }

        rigidBody.AddForce(Vector2.right * move * moveForce);

        animator.SetFloat("RunSpeed", Mathf.Abs(move));
        animator.SetBool("Walking", Mathf.Abs(move) > walkingThreshold);
    }

    void Jump ()
    {
        bool oldGrounded = grounded;
        grounded = IsGrounded();

        if (jumpFrameCount > 0)
        {
            jumpFrameCount--;
        }

        if (jump == JumpState.TakeOff && grounded)
        {
            rigidBody.AddForce(Vector2.up * jumpForce);
            jumpFrameCount = jumpMaxFrameCount;
        }
        else if (jump == JumpState.Jump && jumpFrameCount > 0)
        {
            rigidBody.AddForce(Vector2.up * jumpForce);
        }

        animator.SetBool("Jump", !grounded);

        if (jump == JumpState.TakeOff && grounded)
        {
            PlayJumpSound();
        }
        if (!oldGrounded && grounded)
        {
            PlayWalkSound();
        }
    }

    void FixedUpdate ()
    {
        if (GameController.GameManager.GameOn && damageable.Alive)
        {
            Run();
            Jump();
        }
        jump = JumpState.None;
    }

    void LateUpdate ()
    {
        if (Mathf.Abs(rigidBody.velocity.x) > maxSpeed)
        {
            Vector3 speed = rigidBody.velocity;
            speed.x *= maxSpeed / Mathf.Abs(speed.x);
            rigidBody.velocity = speed;
        }
    }

    void Flip ()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1.0f;
        transform.localScale = scale;
    }

    void PlayWalkSound ()
    {
        movementSound.Play();
    }

    void PlayJumpSound ()
    {
        jumpSound.Play();
    }

    void PlayHurtSound()
    {
        painSound.clip = hurtSoundClip;
        painSound.Play();
    }

    void PlayDeathSound()
    {
        painSound.clip = deathSoundClip;
        painSound.Play();
    }

    public void OnDeath ()
    {
        gameObject.layer = LayerMask.NameToLayer("Background");
        spriteRenderer.sortingOrder = -1;
        water.LeakWater(water.MaxWater);
        StartCoroutine(LaunchGameOver());
    }

    IEnumerator LaunchGameOver ()
    {
        yield return new WaitForSeconds(3);
        GameController.GameManager.GameOver();
    }
}
