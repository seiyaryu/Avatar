using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Firebender : MonoBehaviour, IDeathListener {

    [Header("Player")]

    private GameObject player;

    public GameObject Player
    {
        set { player = value; }
    }

    [Header("Moving")]

    public float force = 7f;
    public float maxSpeed = 2f;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    private Damageable damageable;
    private SpriteRenderer spriteRenderer;
    private Shooter shooter;

    [Header("Audio")]

    public AudioSource walkSound;
    public AudioSource hurtSound;

    public AudioClip[] hurtSoundClips;
    public AudioClip[] deathSoundClips;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        damageable = GetComponent<Damageable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        boxCollider.size = new Vector2(2f, 1f);
        boxCollider.offset = new Vector2(0f, -0.5f);

        GameController.GameManager.NotifyDeath();

        Destroy(gameObject, 5f);
    }

    void FixedUpdate ()
    {
        Vector2 toPlayer = player.transform.position - transform.position;
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

    void PlayWalkSound ()
    {
        walkSound.Play();
    }

    void PlayHurtSound()
    {
        hurtSound.clip = hurtSoundClips[Random.Range(0, hurtSoundClips.Length)];
        hurtSound.Play();
    }

    void PlayDeathSound()
    {
        hurtSound.clip = deathSoundClips[Random.Range(0, deathSoundClips.Length)];
        hurtSound.Play();
    }
}
