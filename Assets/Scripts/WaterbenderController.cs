using UnityEngine;
using System.Collections;

public class WaterbenderController : MonoBehaviour {

    Rigidbody2D rigidBody;
    Animator animator;

    public float maxSpeed = 5.0f;
    public float moveForce = 10.0f;
    public float jumpForce = 10.0f;
    public float walkingThreshold = 0.2f;

    private Transform groundCheck;
    private int groundMask;
    private bool jump;

    void Start ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundCheck = transform.FindChild("GroundCheck");

        string[] groundLayers = { "Scene", "Water" };
        groundMask = LayerMask.GetMask(groundLayers);
        jump = false;
    }

    void Update ()
    {
        if (Physics2D.Linecast(transform.position, groundCheck.position, groundMask))
        {
            if (Input.GetButtonDown("Jump"))
            {
                jump = true;
                animator.SetBool("Jump", true);
            }
            else
            {
                animator.SetBool("Jump", false);
            }
        }
    }

    void FixedUpdate ()
    {
        float h = Input.GetAxis("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(h));

        if(Mathf.Abs(h) > walkingThreshold)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        if (h * transform.localScale.x < 0)
        {
            Flip();
        }

        rigidBody.AddForce(Vector2.right * h * moveForce);

        if(jump)
        {
            rigidBody.AddForce(Vector2.up * jumpForce);
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
}
