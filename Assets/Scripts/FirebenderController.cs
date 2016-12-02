using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float angle)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}

public class FirebenderController : MonoBehaviour, IDeathListener {

    [Header("Player")]

    private GameObject player;
    private WaterFlaskController water;

    public GameObject Player
    {
        set
        {
            player = value;
            water = value.GetComponentInChildren<WaterFlaskController>();
        }
    }

    [Header("Shooting")]

    public ProjectileController fireBall;

    public float firingMaxCooldown = 1.5f;
    private float firingCooldown = 0f;

    public float firingMinRange = 2f;
    public float firingMaxRange = 5f;
    public Transform firingOrigin;

    public float angularDeviation = 0.3f;
    public float speedDeviation = 0.4f;
    public float rangeVariation = 0.2f;

    [Header("Moving")]

    public float force = 7f;
    public float maxSpeed = 2f;
    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    private DamageableController damageable;
    private SpriteRenderer spriteRenderer;

    private Animator animator;

    [Header("Audio")]

    public AudioSource walkSound;
    public AudioSource hurtSound;

    public AudioClip[] hurtSoundClips;
    public AudioClip[] deathSoundClips;

    void Awake ()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        damageable = GetComponent<DamageableController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        firingMinRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
        firingMaxRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
    }
	
    void UpdateCooldown ()
    {
        if (firingCooldown > 0f)
        {
            firingCooldown -= Time.deltaTime;
            if (firingCooldown <= 0f)
            {
                animator.SetBool("Firing", false);
            }
        }
    }

    void ShootFireball ()
    {
        if (firingCooldown <= 0f && !damageable.IsStunned())
        {
            Vector2 toTarget;
            if (PickTarget(out toTarget))
            {
                Face(toTarget);
                rigidBody.velocity = Vector2.zero;
                firingCooldown = firingMaxCooldown;

                ProjectileController fireBallInstance = (ProjectileController)Instantiate(fireBall, firingOrigin.position, fireBall.transform.rotation);

                fireBallInstance.Direction = toTarget.Rotate(Random.Range(-angularDeviation, angularDeviation));
                fireBallInstance.Speed = fireBallInstance.Speed * Random.Range(1f - speedDeviation, 1f + speedDeviation);
                fireBallInstance.GetComponent<FireballController>().Water = water;

                animator.SetBool("Firing", true);
            }
        }
    }

    void Face (Vector2 direction)
    {
        if (direction.x * transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1.0f;
            transform.localScale = scale;
        }
    }

    bool PickTarget (out Vector2 toTarget)
    {
        toTarget = player.transform.position - transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        Vector2 toWater = water.GetDropPosition() - (Vector2)(transform.position);
        float sqrNorm = toWater.sqrMagnitude;
        if(sqrNorm < sqrDist)
        {
            toTarget = toWater;
            sqrDist = sqrNorm;
        }

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("PlayerProjectile");
        foreach (GameObject projectile in projectiles)
        {
            Rigidbody2D projectileBody = projectile.GetComponent<Rigidbody2D>();
            if (projectileBody)
            {
                Vector2 toThis = transform.position - projectile.transform.position;
                Vector2 aim = projectileBody.velocity.normalized;
                float deviation = Vector2.Dot(toThis, aim);
                sqrNorm = toThis.sqrMagnitude;
                float threshold = Mathf.Sqrt(sqrNorm - 1f);
                if (deviation >= threshold && sqrNorm < sqrDist)
                {
                    toTarget = -toThis;
                    sqrDist = sqrNorm;
                }
            }
        }
        float aggroDist = Random.Range(firingMinRange, firingMaxRange);
        return aggroDist * aggroDist > sqrDist;
    }

    public void OnDeath()
    {
        gameObject.layer = LayerMask.NameToLayer("Background");
        spriteRenderer.sortingOrder = -1;

        boxCollider.size = new Vector2(2f, 1f);
        boxCollider.offset = new Vector2(0f, -0.5f);

        GameController.GetGameManager().NotifyDeath();

        Destroy(gameObject, 5f);
    }

	void Update ()
    {
        if (damageable.IsAlive())
        {
            UpdateCooldown();
            ShootFireball();
        }
    }

    void FixedUpdate ()
    {
        if (damageable.IsAlive())
        {
            Vector2 toPlayer = player.transform.position - transform.position;
            if (firingCooldown <= 0f && !damageable.IsStunned())
            {
                Face(toPlayer);
                rigidBody.AddForce(Vector2.right * force * Mathf.Sign(toPlayer.x));
            }
        }
    }

    void LateUpdate()
    {
        if (damageable.IsAlive())
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
