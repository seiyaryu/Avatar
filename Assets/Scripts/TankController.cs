using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour, IShooterController {

    [SerializeField]
    private HingeJoint2D frontWheelJoint;
    [SerializeField]
    private HingeJoint2D backWheelJoint;
    [SerializeField]
    private float wheelRadius;

    [SerializeField]
    private Transform tankBody;
    [SerializeField]
    private float tankSpeed = 0.5f;

    private Rigidbody2D rigidBody;

    private Transform player;
    private WaterFlaskController water;

    [SerializeField]
    int collisionDamage = 1;
    [SerializeField]
    float repelAmpl = 10f;

    private float chargeTimer;

    [SerializeField]
    private float chargeCooldownDuration;
    [SerializeField]
    private float chargeDuration;
    [SerializeField]
    private float chargeWarmUpDuration;
    [SerializeField]
    private float chargeRestDuration;
    [SerializeField]
    private float chargeSpeed;
    [SerializeField]
    private float chargeRotationSpeed;
    [SerializeField]
    private float chargeWarmUpSpeed;

    [SerializeField]
    private Collider2D frontCollider;

    [SerializeField]
    private Collider2D pipeCollider;
    [SerializeField]
    private ParticleSystem pipeFumes;

    private ShooterController shooter;

    [SerializeField]
    private float minFiringAngle;
    [SerializeField]
    private float maxFiringAngle;

    void Start ()
    {
        player = GameController.GameManager.Player.transform;
        water = GameController.GameManager.Player.GetComponentInChildren<WaterFlaskController>();
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        shooter = GetComponent<ShooterController>();

        chargeDuration += chargeRestDuration;
        chargeWarmUpDuration += chargeDuration;
        chargeCooldownDuration += chargeWarmUpDuration;

        chargeTimer = chargeCooldownDuration;
    }
	
	void Update ()
    {
        Orient();
        Charge();
	}

    void Orient ()
    {
        float toPlayer = player.position.x - transform.position.x;
        if (toPlayer * transform.localScale.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public void OnShoot ()
    {

    }

    public void OnReload()
    {

    }

    public bool GetTarget (out Vector2 target)
    {
        Vector2 toTarget = shooter.FiringOrigin.position - player.position;
        toTarget.Normalize();
        float angle = Mathf.Acos(Vector2.Dot(Vector2.left * transform.localScale.x, toTarget)) * Mathf.Rad2Deg;
        if(minFiringAngle < angle && angle < maxFiringAngle)
        {
            target = player.position;
            return true;
        }
        else
        {
            target = new Vector2();
            return false;
        }
    }

    void SetWheelRotationSpeed(float rotationSpeed)
    {
        JointMotor2D frontMotor = frontWheelJoint.motor;
        frontMotor.motorSpeed = rotationSpeed;
        frontWheelJoint.motor = frontMotor;

        JointMotor2D backMotor = backWheelJoint.motor;
        backMotor.motorSpeed = rotationSpeed;
        backWheelJoint.motor = backMotor;
    }

    void Charge ()
    {
        if (chargeTimer > 0f)
        {
            chargeTimer -= Time.deltaTime;
        }

        if (chargeTimer < 0f)
        {
            chargeTimer = chargeCooldownDuration;
        }
        else if (chargeTimer < chargeRestDuration)
        {
            rigidBody.velocity = Vector2.zero;
            SetWheelRotationSpeed(0);
            EmitFumes();
        }
        else if (chargeTimer < chargeDuration)
        {
            rigidBody.velocity = Vector2.right * chargeSpeed * transform.localScale.x;
            SetWheelRotationSpeed(chargeRotationSpeed);
        }
        else if (chargeTimer < chargeWarmUpDuration)
        {
            rigidBody.velocity = Vector2.right * chargeWarmUpSpeed * transform.localScale.x;
            SetWheelRotationSpeed(chargeRotationSpeed);
        }
        else
        {
            rigidBody.velocity = Vector2.right * tankSpeed * transform.localScale.x;
            SetWheelRotationSpeed(rigidBody.velocity.x * Mathf.Rad2Deg / wheelRadius);
        }
    }

    void EmitFumes()
    {
        bool blockedByIce = false;
        if (water.Frozen)
        {
            Collider2D[] colliders = water.waterDrop.GetComponents<Collider2D>();
            for (int colliderIdx = 0; !blockedByIce && colliderIdx < colliders.Length; ++colliderIdx)
            {
                blockedByIce |= pipeCollider.IsTouching(colliders[colliderIdx]);
            }
        }
        if (!blockedByIce)
        {
            pipeFumes.Emit(150);
        }
        else
        {

        }
    }

    bool HitFront (ContactPoint2D[] contacts)
    {
        foreach(ContactPoint2D contact in contacts)
        {
            if (contact.collider == frontCollider)
            {
                return true;
            }
        }
        return false;
    }

    int GetDamage (Collision2D collision)
    {
        int damage = collisionDamage;
        if (HitFront(collision.contacts))
        {
            damage += 1;
        }
        if (chargeTimer > chargeRestDuration && chargeTimer < chargeDuration)
        {
            damage += 1;
        }
        return damage;
    }

    Vector2 GetRepelForce (Collision2D collision)
    {
        if(collision.contacts.Length > 0)
        {
            return collision.contacts[0].normal * repelAmpl;
        }
        else
        {
            Vector2 delta = collision.collider.transform.position - transform.position;
            return delta.normalized * repelAmpl;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DamageableController damageable = collision.gameObject.GetComponent<DamageableController>();
            if (damageable)
            {

                damageable.OnHit(GetDamage(collision), GetRepelForce(collision));
            }
        }
    }
}
