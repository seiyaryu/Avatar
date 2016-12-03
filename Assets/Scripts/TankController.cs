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
    private Transform player;

    [SerializeField]
    private float chargeMaxCooldown;
    private float chargeCooldown;
    [SerializeField]
    private float chargeLength;
    [SerializeField]
    private float chargeSpeed;
    [SerializeField]
    private float chargeWarmUpDuration;
    [SerializeField]
    private float chargeWarmUpSpeed;

    void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        frontWheelJoint = frontWheel.GetComponent<HingeJoint2D>();
        backWheelJoint = backWheel.GetComponent<HingeJoint2D>();

        player = GameController.GetGameManager().Player.transform;
    }
	
	void Update ()
    {
        rigidBody.velocity = Vector2.right * tankSpeed;
        Orient();
        RotateWheels();
	}

    void RotateWheels ()
    {
        float speed = rigidBody.velocity.x;

        JointMotor2D frontMotor = frontWheelJoint.motor;
        frontMotor.motorSpeed = speed / wheelRadius * Mathf.Rad2Deg;
        frontWheelJoint.motor = frontMotor;

        JointMotor2D backMotor = backWheelJoint.motor;
        backMotor.motorSpeed = speed / wheelRadius * Mathf.Rad2Deg;
        backWheelJoint.motor = backMotor;
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

    void Charge ()
    {
        if (chargeCooldown > 0f)
        {
            chargeCooldown -= Time.deltaTime;
        }
    }
}
