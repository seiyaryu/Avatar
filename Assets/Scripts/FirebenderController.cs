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

public class FirebenderController : MonoBehaviour {

    [Header("Player")]

    public GameObject player;
    private WaterFlaskController water;

    [Header("Shooting")]

    public FireballController fireBall;

    public float firingMaxCooldown = 1.5f;
    private float firingCooldown = 0f;

    public float firingSqrRange = 9f;
    private Transform firingOrigin;

    [Header("Moving")]

    public float moveForce = 7f;
    private Rigidbody2D rigidBody;
    private DamageableController damageable;

    private Animator animator;

    [Header("Audio")]

    public AudioSource walkSound;
    public AudioSource hurtSound;

	void Awake ()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        damageable = GetComponent<DamageableController>();

        water = player.GetComponentInChildren<WaterFlaskController>();

        firingOrigin = transform.GetChild(0);
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

    void ShootFireball (Vector2 toPlayer)
    {
        if (firingCooldown <= 0f && !damageable.IsStunned() && toPlayer.sqrMagnitude < firingSqrRange)
        {
            Face(toPlayer);
            rigidBody.velocity = Vector2.zero;
            firingCooldown = firingMaxCooldown;

            FireballController fireBallInstance = (FireballController) Instantiate(fireBall, firingOrigin.position, fireBall.transform.rotation);

            fireBallInstance.SetDirection(toPlayer.Rotate(Random.Range(-0.15f, 0.15f) * Mathf.PI));

            animator.SetBool("Firing", true);
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

    void Move (Vector2 toPlayer)
    {
        if (firingCooldown <= 0f && !damageable.IsStunned())
        {
            Face(toPlayer);
            if (toPlayer.sqrMagnitude > firingSqrRange)
            {
                rigidBody.velocity += Vector2.right * 0.1f * Mathf.Sign(toPlayer.x);//AddForce(Vector2.right * moveForce * Mathf.Sign(toPlayer.x));
            }           
        }
    }

	void Update ()
    {
        UpdateCooldown();

        Vector2 toPlayer = player.transform.position - transform.position;
        Vector2 toWater = water.GetFrontAttractor() - (Vector2)(transform.position);

        if (toPlayer.sqrMagnitude < toWater.sqrMagnitude)
        {
            ShootFireball(toPlayer);
        }
        else
        {
            ShootFireball(toWater);
        }

        Move(toPlayer);
    }

    void PlayWalkSound ()
    {
        walkSound.Play();
    }

    void PlayHurtSound()
    {
        hurtSound.Play();
    }
}
