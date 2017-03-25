using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {


    public Transform Player
    {
        set { player = value; }
    }
    private Transform player;

    [Header("Movement")]

    [SerializeField]
    private HingeJoint2D frontWheelJoint;
    [SerializeField]
    private HingeJoint2D backWheelJoint;
    [SerializeField]
    private float wheelRadius;

    [SerializeField]
    private float tankSpeed = 0.5f;

    private Rigidbody2D rigidBody;

    [Header("Collision")]

    [SerializeField]
    int collisionDamage = 1;
    [SerializeField]
    float repelAmpl = 10f;

    [Header("Scene Bounds")]

    [SerializeField]
    private float sceneLeftBound;
    [SerializeField]
    private float sceneRightBound;

    [Header("Charge")]

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

    private float chargeTimer;

    public enum ChargeState { None, WarmUp, Charge, Recovery }
    private ChargeState chargeState;

    [Header("Engine")]

    [SerializeField]
    private Engine engine;

    [Header("Audio")]

    [SerializeField]
    private AudioSource wheelSound;
    [SerializeField]
    private AudioClip wheelRegularClip;
    [SerializeField]
    private AudioClip wheelFastClip;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        chargeDuration += chargeRestDuration;
        chargeWarmUpDuration += chargeDuration;
        chargeCooldownDuration += chargeWarmUpDuration;

        chargeTimer = chargeCooldownDuration;
    }
	
    void PlayWheelSound ()
    {
        if (rigidBody.velocity.x == 0f)
        {
            if (wheelSound.isPlaying)
            {
                wheelSound.Pause();
            }
        }
        else if (!wheelSound.isPlaying)
        {
            wheelSound.Play();
        }
    }

	void Update ()
    {
        Charge();
        PlayWheelSound();
	}

    void LateUpdate ()
    {
        if (transform.position.x > sceneRightBound)
        {
            Vector3 position = transform.position;
            position.x = sceneRightBound;
            transform.position = position;
        }
        else if (transform.position.x < sceneLeftBound)
        {
            Vector3 position = transform.position;
            position.x = sceneLeftBound;
            transform.position = position;
        }
    }

    void Orient ()
    {
        float toPlayer = player.position.x - transform.position.x;
        if (toPlayer * transform.localScale.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            scale.z *= -1f;
            transform.localScale = scale;
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

    bool UpdateChargeState ()
    {
        ChargeState oldChargeState = chargeState;

        if (chargeTimer < chargeRestDuration)
        {
            chargeState = ChargeState.Recovery;
        }
        else if (chargeTimer < chargeDuration)
        {
            chargeState = ChargeState.Charge;
        }
        else if (chargeTimer < chargeWarmUpDuration)
        {
            chargeState = ChargeState.WarmUp;
        }
        else
        {
            chargeState = ChargeState.None;
        }

        return chargeState != oldChargeState;
    }

    void Charge ()
    {
        bool stateChange = false;

        if (!engine.Overheating)
        {
            if (chargeTimer > 0f)
            {
                chargeTimer -= Time.deltaTime;
            }
            else
            {
                chargeTimer = chargeCooldownDuration;
            }
            stateChange = UpdateChargeState();
        }

        switch (chargeState)
        {
            case ChargeState.None :
                {
                    if (stateChange)
                    {
                        engine.Restart();
                        wheelSound.Stop();
                        wheelSound.clip = wheelRegularClip;
                        wheelSound.Play();
                    }
                    Orient();
                    rigidBody.velocity = Vector2.right * tankSpeed * Orientation;
                    SetWheelRotationSpeed(rigidBody.velocity.x * Mathf.Rad2Deg / wheelRadius);
                }
                break;
            case ChargeState.Recovery :
                {
                    if (stateChange)
                    {
                        engine.Decelerate();
                    }
                    rigidBody.velocity = Vector2.zero;
                    SetWheelRotationSpeed(0);
                    engine.EmitFumes();
                }
                break;
            case ChargeState.Charge :
                {
                    if (stateChange)
                    {
                        wheelSound.Stop();
                        wheelSound.clip = wheelFastClip;
                        wheelSound.Play();
                    }
                    rigidBody.velocity = Vector2.right * chargeSpeed * Orientation;
                    SetWheelRotationSpeed(chargeRotationSpeed);
                }
                break;
            case ChargeState.WarmUp :
                {
                    if (stateChange)
                    {
                        engine.Accelerate();
                    }
                    Orient();
                    rigidBody.velocity = Vector2.right * chargeWarmUpSpeed * Orientation;
                    SetWheelRotationSpeed(chargeRotationSpeed);
                    engine.SetVolume(Mathf.Sqrt((chargeWarmUpDuration - chargeTimer) / (chargeWarmUpDuration - chargeDuration)));
                }
                break;
        }
    }

    int GetDamage (Collision2D collision)
    {
        int damage = collisionDamage;
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
        OnCollisionStay2D(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Damageable damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.OnHit(GetDamage(collision), GetRepelForce(collision));
            }
        }
    }

    public ChargeState CurrentState
    {
        get { return chargeState; }
    }

    public float Orientation
    {
        get { return Mathf.Sign(transform.localScale.x); }
    }

    public float SceneLeftBound
    {
        set { sceneLeftBound = value; }
    }

    public float SceneRightBound
    {
        set { sceneRightBound = value; }
    }
}
