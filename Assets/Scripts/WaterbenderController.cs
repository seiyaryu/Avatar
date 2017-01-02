using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaterbenderController : MonoBehaviour, IDeathListener {

    private Rigidbody2D rigidBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private DamageableController damageable;
    private WaterFlaskController water;

    public float maxSpeed = 5.0f;
    public float moveForce = 10.0f;
    public float jumpForce = 10.0f;
    public float walkingThreshold = 0.2f;

    public float grounderPosition = -1f;
    public float grounderHeight = 0.1f;
    public float grounderWidth = 0.2f;

    private int groundMask;
    private int waterMask;
    private bool jump = false;
    private bool grounded = false;
    private float move = 0f;

    public AudioClip snowWalkSoundClip;
    public AudioClip jumpSoundClip;
    public AudioClip hurtSoundClip;
    public AudioClip deathSoundClip;

    public AudioSource movementSound;
    public AudioSource painSound;

    void Start ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageable = GetComponent<DamageableController>();
        water = GetComponentInChildren<WaterFlaskController>();

        groundMask = 1 << LayerMask.NameToLayer("Scene");
        waterMask  = 1 << LayerMask.NameToLayer("Water");

    }

    void Update ()
    {
        if (GameController.GameManager.GameOn && damageable.Alive && !water.IsGatheringWater())
        {
            jump = grounded && Input.GetButtonDown("Jump");
            move = Input.GetAxis("Horizontal");
        }
        else
        {
            jump = false;
            move = 0f;
        }
    }

    void UpdatePhysics()
    {
        Vector2 position = transform.position;
        Vector2 upperCorner = position + Vector2.up * (grounderPosition + grounderHeight) - Vector2.right * grounderWidth;
        Vector2 lowerCorner = position + Vector2.up * (grounderPosition - grounderHeight) + Vector2.right * grounderWidth;
        grounded = Physics2D.OverlapArea(upperCorner, lowerCorner, groundMask);

        if (!grounded && water.Frozen)
        {
            grounded = Physics2D.OverlapArea(upperCorner, lowerCorner, waterMask);
        }

        if (move * transform.localScale.x < 0)
        {
            Flip();
        }

        rigidBody.AddForce(Vector2.right * move * moveForce);

        if (jump)
        {
            rigidBody.AddForce(Vector2.up * jumpForce);
        }
    }

    void UpdateGraphicsAndSound()
    {
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("Jump", !grounded);
        animator.SetBool("Walking", Mathf.Abs(move) > walkingThreshold);

        if (jump)
        {
            PlayJumpSound();
        }
    }

    void FixedUpdate ()
    {
        if (GameController.GameManager.GameOn && damageable.Alive)
        {
            UpdatePhysics();
            UpdateGraphicsAndSound();
        }
        jump = false;
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
        movementSound.clip = snowWalkSoundClip;
        movementSound.Play();
    }

    void PlayJumpSound ()
    {
        movementSound.clip = jumpSoundClip;
        movementSound.Play();
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
        water.LeakWater(water.maximumWater);
        StartCoroutine(LaunchGameOver());
    }

    IEnumerator LaunchGameOver ()
    {
        yield return new WaitForSeconds(3);
        GameController.GameManager.GameOver();
    }
}
