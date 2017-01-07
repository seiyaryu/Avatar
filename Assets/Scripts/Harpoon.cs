using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [SerializeField]
    private float burrowingSlowdown = 0.9f;
    [SerializeField]
    private float stopThreshold = 1f;
    [SerializeField]
    private float lifeTimeOnStop = 3f;

    private Rigidbody2D rigidBody;
    private WaterFlask water;

    void Awake()
    {
        water = GameController.GameManager.Player.GetComponentInChildren<WaterFlask>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rigidBody.bodyType == RigidbodyType2D.Kinematic && rigidBody.velocity.sqrMagnitude < stopThreshold)
        {
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
            Destroy(gameObject.GetComponent<Projectile>());
            Destroy(gameObject, lifeTimeOnStop);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Scene") || (other.gameObject.CompareTag("Water") && water.Frozen))
        {
            if (rigidBody.bodyType == RigidbodyType2D.Kinematic)
            {
                rigidBody.velocity *= burrowingSlowdown;
            }
            else if (rigidBody.bodyType == RigidbodyType2D.Dynamic)
            {
                rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (rigidBody.bodyType == RigidbodyType2D.Dynamic)
        {
            rigidBody.constraints = RigidbodyConstraints2D.None;
        }
    }
}
