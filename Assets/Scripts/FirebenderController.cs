using UnityEngine;
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

    public GameObject player;
    private WaterFlaskController water;

    public FireballController fireBall;
    public float firingSqrRange = 9.0f;
    public float firingCooldown = 1.5f;
    public Vector3 firingPosition = new Vector3(-0.6f, 0.5f, 0.0f);
    private float firingDelay = 0.0f;

    private bool pause = false;

    public float moveForce = 7.0f;
    private Rigidbody2D rigidBody;

    private Animator animator;

	void Awake ()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        water = player.GetComponentInChildren<WaterFlaskController>();
	}
	
    void UpdateCooldown ()
    {
        if (pause)
        {
            firingDelay += Time.deltaTime;
            if (firingDelay > firingCooldown)
            {
                pause = false;
                animator.SetBool("Firing", false);
            }
        }
    }

    void ShootFireball (Vector2 toPlayer)
    {
        if (toPlayer.sqrMagnitude < firingSqrRange && !pause)
        {
            pause = true;
            Face(toPlayer);
            rigidBody.velocity = Vector2.zero;
            firingDelay = 0.0f;

            FireballController fireBallInstance = (FireballController) Instantiate(fireBall, transform.position + firingPosition, fireBall.transform.rotation);

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
            firingPosition.x *= -1.0f;
        }
    }

    void Move (Vector2 toPlayer)
    {
        if (!pause)
        {
            Face(toPlayer);
            if (toPlayer.sqrMagnitude > firingSqrRange)
            {
                rigidBody.AddForce(Vector2.right * moveForce * Mathf.Sign(toPlayer.x));
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
}
