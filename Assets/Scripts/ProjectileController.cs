using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    [SerializeField]
    private float speed = 5.0f;

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = value;
            if (rigidBody)
            {
                rigidBody.velocity = direction * speed;
            }
        }
    }

    [SerializeField]
    private Vector2 direction = Vector2.right;

    public Vector2 Direction
    {
        get
        {
            return direction;
        }
        set
        {
            direction = value.normalized;
            if (rigidBody)
            {
                rigidBody.velocity = direction * speed;
            }
            Vector3 angles = transform.eulerAngles;
            angles.z -= Vector2.Angle(direction, Vector2.left) * Mathf.Sign(direction.y);
            transform.eulerAngles = angles;
        }
    }

    [Tooltip("How much do we push impacted objects away ?")]
    public float repelAmplitude = 10f;
    [Tooltip("Damage inflicted")]
    public int damage = 1;
    [Tooltip("What objects should be affected by this projectile ?")]
    public string[] targetTags;

    [Tooltip("How far away from the screen should the projectile vanish ?")]
    public float vanishingSqrRange = 10f;

    private Rigidbody2D rigidBody;
    private Camera viewpoint;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.velocity = direction * speed;
        viewpoint = GameController.GetGameManager().Viewpoint;
        vanishingSqrRange *= viewpoint.orthographicSize * viewpoint.orthographicSize;
    }

    void Update()
    {
        // Is the projectile too far away from the camera ?
        Vector2 toViewpoint = transform.position - viewpoint.transform.position;
        if (toViewpoint.sqrMagnitude > vanishingSqrRange)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Did it hit something it is supposed to hit ?
        bool hit = false;
        foreach (string tag in targetTags)
        {
            hit |= other.gameObject.CompareTag(tag);
        }
        // We hit a target
        if (hit)
        {
            // Can this stuff be damaged ?
            DamageableController otherHP = other.gameObject.GetComponent<DamageableController>();
            if (otherHP)
            {
                Vector2 toOther = other.gameObject.transform.position - transform.position;
                otherHP.OnHit(damage, toOther.normalized * repelAmplitude);
            }
            Destroy(gameObject);
        }
        // Regardless of targets, if it hits the scene, the projectile is dead
        else if (other.gameObject.CompareTag("Scene"))
        {
            Destroy(gameObject);
        }
    }
}
