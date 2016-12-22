using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour {

    [SerializeField]
    private Transform frontWheel;
    private HingeJoint2D frontWheelJoint;
    [SerializeField]
    private Transform backWheel;
    private HingeJoint2D backWheelJoint;
    [SerializeField]
    private float wheelRadius;
    [SerializeField]
    private Transform tankBody;
    [SerializeField]
    private float tankSpeed = 0.5f;

    private Rigidbody2D rigidBody;
    private Collider2D frontCollider;
    private Transform player;

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

    private FireballShooter shooter;
    private float fireballTimer;
    [SerializeField]
    private float fireballCooldownDuration;
    [SerializeField]
    private float minFiringAngle;
    [SerializeField]
    private float maxFiringAngle;

    void Start ()
    {
        chargeDuration += chargeRestDuration;
        chargeWarmUpDuration += chargeDuration;
        chargeCooldownDuration += chargeWarmUpDuration;
    }

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        frontWheelJoint = frontWheel.GetComponent<HingeJoint2D>();
        backWheelJoint = backWheel.GetComponent<HingeJoint2D>();

        shooter = GetComponent<FireballShooter>();

        player = GameController.GetGameManager().Player.transform;
    }
	
	void Update ()
    {
        Orient();
        Charge();
	}

    void Orient ()
    {
        float toPlayer = player.position.x - transform.position.x;
        if (toPlayer * transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    void ShootFireball ()
    {
        if (fireballTimer > 0f)
        {
            fireballTimer -= Time.deltaTime;
        }

        if (fireballTimer < 0f)
        {
            fireballTimer = fireballCooldownDuration;
            Vector2 toTarget = firingOrigin.position - player.position;
            toTarget.Normalize();
            float angle = Mathf.Acos(Vector2.Dot(Vector2.up, toTarget));
            if(minFiringAngle < angle && angle < maxFiringAngle)
            {
            }
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
        }
        else if (chargeTimer < chargeDuration)
        {
            rigidBody.velocity = Vector2.right * chargeSpeed;
            SetWheelRotationSpeed(chargeRotationSpeed);
        }
        else if (chargeTimer < chargeWarmUpDuration)
        {
            rigidBody.velocity = Vector2.right * chargeWarmUpSpeed;
            SetWheelRotationSpeed(chargeRotationSpeed);
        }
        else
        {
            rigidBody.velocity = Vector2.right * tankSpeed;
            SetWheelRotationSpeed(rigidBody.velocity.x * Mathf.Rad2Deg / wheelRadius);
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
