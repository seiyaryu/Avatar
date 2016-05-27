using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaterbenderController : MonoBehaviour {

    Rigidbody2D rigidBody;
    Animator animator;

    public float maxSpeed = 5.0f;
    public float moveForce = 10.0f;
    public float jumpForce = 10.0f;
    public float walkingThreshold = 0.2f;

    public float grounderPosition = -1f;
    public float grounderHeight = 0.1f;
    public float grounderWidth = 0.2f;
    private int groundMask;
    private bool jump = false;
    private bool grounded = false;

    public AudioSource snowWalkSound;
    public AudioSource jumpSound;

    void Start ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        string[] groundLayers = { "Scene", "Water" };
        groundMask = LayerMask.GetMask(groundLayers);
    }

    void Update ()
    {
        jump = grounded && Input.GetButtonDown("Jump");
    }

    void FixedUpdate ()
    {
        Vector2 position = transform.position;
        Vector2 upperCorner = position + Vector2.up * (grounderPosition + grounderHeight) - Vector2.right * grounderWidth;
        Vector2 lowerCorner = position + Vector2.up * (grounderPosition - grounderHeight) + Vector2.right * grounderWidth;
        grounded = Physics2D.OverlapArea(upperCorner, lowerCorner, groundMask);

        float h = Input.GetAxis("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(h));
        animator.SetBool("Jump", !grounded);
        animator.SetBool("Walking", Mathf.Abs(h) > walkingThreshold);

        if (h * transform.localScale.x < 0)
        {
            Flip();
        }

        rigidBody.AddForce(Vector2.right * h * moveForce);

        if(jump)
        {
            rigidBody.AddForce(Vector2.up * jumpForce);
            PlayJumpSound();
            jump = false;
        }
    }

    void LateUpdate()
    {
        if (Mathf.Abs(rigidBody.velocity.x) > maxSpeed)
        {
            Vector3 speed = rigidBody.velocity;
            speed.x *= maxSpeed / Mathf.Abs(speed.x);
            rigidBody.velocity = speed;
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1.0f;
        transform.localScale = scale;
    }

    public void PlayWalkSound()
    {
        snowWalkSound.Play();
    }

    void PlayJumpSound()
    {
        jumpSound.Play();
    }
}
