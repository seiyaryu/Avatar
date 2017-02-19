using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Firebender : FirebenderBase, IDeathListener {

    [Header("Moving")]

    public float force = 7f;
    public float maxSpeed = 2f;
    public AudioSource walkingSound;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Shooter shooter;

    [Header("Dying")]

    [SerializeField]
    private Vector2 dyingColliderOffset = new Vector2(0f, -0.5f);
    [SerializeField]
    private Vector2 dyingColliderSize = new Vector2(2f, 1f);

    protected override void Awake ()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        damageable = GetComponent<Damageable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        shooter = GetComponent<Shooter>();
    }

    void Face (Vector2 direction)
    {
        if (direction.x * transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public void OnDeath()
    {
        gameObject.layer = LayerMask.NameToLayer("Background");
        spriteRenderer.sortingOrder = -1;

        boxCollider.size = dyingColliderSize;
        boxCollider.offset = dyingColliderOffset;

        GameController.GameManager.NotifyDeath();

        Destroy(gameObject, 5f);
    }

    void FixedUpdate ()
    {
        Vector2 toPlayer = player.position - transform.position;
        if (!shooter.CoolingDown && !damageable.Stunned)
        {
            Face(toPlayer);
            rigidBody.AddForce(Vector2.right * force * Mathf.Sign(toPlayer.x));
        }
    }

    void LateUpdate()
    {
        if (damageable.Alive)
        {
            float speed = rigidBody.velocity.magnitude;
            if (speed > maxSpeed)
            {
                rigidBody.velocity = rigidBody.velocity * maxSpeed / speed;
            }
        }
    }

    public override void OnShoot(Projectile projectile)
    {
        animator.SetBool("Firing", true);
        rigidBody.velocity = Vector2.zero;
    }

    public override void OnReadyToShoot()
    {
        animator.SetBool("Firing", false);
    }

    void PlayWalkSound ()
    {
        walkingSound.Play();
    }
}
