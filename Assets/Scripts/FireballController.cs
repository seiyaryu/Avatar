using UnityEngine;
using System.Collections;

public class FireballController : MonoBehaviour {

    private Rigidbody2D rigidBody;

    public float speed = 5.0f;
    public float repelAmplitude = 1000.0f;
    public int heat = 30;

    public ParticleSystem steamAnimation;

    private int remainingHeat;
    private float colliderSqrRadius;

	void Awake ()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        remainingHeat = heat;

        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        colliderSqrRadius = circle.radius * circle.radius;
	}

    public void SetDirection(Vector2 direction)
    {
        rigidBody.velocity = direction.normalized * speed;
        Vector3 angles = transform.eulerAngles;
        angles.z -= Vector2.Angle(direction, Vector2.left) * Mathf.Sign(direction.y);
        transform.eulerAngles = angles;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {          
            other.gameObject.GetComponent<Animator>().SetTrigger("Hurt");

            Vector2 toOther = other.gameObject.transform.position - transform.position;
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(toOther.normalized * repelAmplitude, ForceMode2D.Force);

            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Scene"))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            ParticleSystem water = GameObject.Find("WaterFlask").GetComponent<ParticleSystem>();

            float sqrRange = 9.0f * colliderSqrRadius;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[water.maxParticles];
            int particleCount = water.GetParticles(particles);

            int particleIdx = 0;
            int steamCount = 0;
            while (particleIdx < particleCount && remainingHeat > 0)
            {
                Vector3 toParticle = particles[particleIdx].position - transform.position;
                if (toParticle.sqrMagnitude < sqrRange)
                {
                    particles[particleIdx].lifetime = -1.0f;
                    remainingHeat--;
                    steamCount++;
                }
                particleIdx++;
            }
            water.SetParticles(particles, particleCount);

            if(steamCount > 0)
            {
                Instantiate(steamAnimation, transform.position, Quaternion.identity);
            }

            if (remainingHeat == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
