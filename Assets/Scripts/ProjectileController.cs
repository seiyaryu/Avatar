using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

    [SerializeField]
    private float speed = 5.0f;

    private Vector2 direction = Vector2.right;

    [SerializeField, Tooltip("Default orientation of the projectile with regard to the Z axis.")]
    private float orientation = 0f;

    [SerializeField, Tooltip("How much do we push impacted objects away ?")]
    private float repelAmplitude = 10f;
    [SerializeField, Tooltip("Damage inflicted")]
    private int damage = 1;
    [SerializeField, Tooltip("What objects should be affected by this projectile ?")]
    private string[] targetTags;
    [SerializeField, Tooltip("Is the projectile destroyed when hitting a target ?")]
    private bool destroyOnHit = true;

    [SerializeField, Tooltip("How far away from the screen should the projectile vanish ?")]
    private float vanishingSqrRange = 10f;

    private Rigidbody2D rigidBody;
    private Camera viewpoint;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.velocity = direction * speed;
        viewpoint = GameController.GameManager.Viewpoint;
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
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }          
        }
        // Regardless of targets, if it hits the scene, the projectile is dead
        else if (destroyOnHit && other.gameObject.CompareTag("Scene"))
        {
            Destroy(gameObject);
        }
    }

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
            angles.z = orientation + Vector2.Angle(direction, Vector2.right) * Mathf.Sign(direction.y);
            transform.eulerAngles = angles;
        }
    }

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
}
