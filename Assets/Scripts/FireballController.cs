using UnityEngine;
using System.Collections;

public class FireballController : MonoBehaviour {

    private Camera viewpoint;
    private Rigidbody2D rigidBody;
    private CircleCollider2D circleCollider;

    public float speed = 5.0f;
    public float repelAmplitude = 1000.0f;
    public int heat = 30;
    public float heatWave = 3f;
    public float vanishingSqrRange = 10f;

    public GameObject steamAnimation;
    public float steamSoundMaxCooldown = 0.2f;
    private float steamSoundCooldown = 0f;

    private int remainingHeat;
    private float colliderSqrRadius;

	void Awake ()
    {
        viewpoint = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        vanishingSqrRange *= viewpoint.orthographicSize * viewpoint.orthographicSize;

        rigidBody = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        remainingHeat = heat;

        colliderSqrRadius = heatWave * circleCollider.radius * heatWave * circleCollider.radius;
	}

    void Update()
    {
        if(steamSoundCooldown > 0f)
        {
            steamSoundCooldown -= Time.deltaTime;
        }

        Vector2 toViewpoint = transform.position - viewpoint.transform.position;
        if (remainingHeat == 0 || toViewpoint.sqrMagnitude > vanishingSqrRange)
        {
            Destroy(gameObject);
        }
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
            bool emitSteam = EmitSteam(other);

            if(emitSteam)
            {
                GameObject steamEffect = Instantiate(steamAnimation, transform.position, Quaternion.identity) as GameObject;
                if(steamSoundCooldown > 0f)
                {
                    Destroy(steamEffect.GetComponent<AudioSource>());
                }
                else
                {
                    steamSoundCooldown = steamSoundMaxCooldown;
                }
            }
        }
    }

    bool EmitSteam(Collider2D collider)
    {
        ParticleSystem water = GameObject.Find("WaterFlask").GetComponent<ParticleSystem>();

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[water.maxParticles];
        int particleCount = water.GetParticles(particles);

        int particleIdx = 0;
        int steamCount = 0;
        while (particleIdx < particleCount && remainingHeat > 0)
        {
            Vector3 toParticle = particles[particleIdx].position - transform.position;
            if (toParticle.sqrMagnitude < colliderSqrRadius)
            {
                particles[particleIdx].lifetime = -1.0f;
                remainingHeat--;
                steamCount++;
            }
            particleIdx++;
        }
        water.SetParticles(particles, particleCount);

        return steamCount > 0;
    }
}
