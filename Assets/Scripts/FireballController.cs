using UnityEngine;
using System.Collections;

public class FireballController : MonoBehaviour {

    [Header("Heat")]

    [Tooltip("How much water particles can the fireball boil away ?")]
    public int heat = 30;
    [Tooltip("How far a particle needs to be to be affected by the heat ?")]
    public float heatWave = 3f;

    [Header("Animations")]

    public GameObject explosionAnimation;

    public GameObject steamAnimation;
    [Tooltip("Sound repeating too many times in a short time is not pleasant to the ear")]
    public float steamSoundMaxCooldown = 0.2f;
    private float steamSoundCooldown = 0f;

    private int remainingHeat;
    private float heatSqrRange;

    private CircleCollider2D circleCollider;

    private ParticleSystem waterParticles;
    private WaterFlaskController waterDrop;

    void Awake ()
    {
        GameObject player = GameController.GetGameManager().Player;
        if (player)
        {
            waterDrop = player.GetComponent<WaterFlaskController>();
            waterParticles = player.GetComponent<ParticleSystem>();
        }

        circleCollider = GetComponent<CircleCollider2D>();
        heatSqrRange = heatWave * circleCollider.radius * heatWave * circleCollider.radius;
        remainingHeat = heat;
	}

    void Update()
    {
        // We don't want steam sound to pop each time, so we impose a cooldown on the sound of the animation
        if (steamSoundCooldown > 0f)
        {
            steamSoundCooldown -= Time.deltaTime;
        }

        // If heat is exhausted, destroy the fireball
        if (remainingHeat <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // The fireball explodes if it hits the player or the scene
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Scene"))
        {
            Instantiate(explosionAnimation, transform.position, Quaternion.identity);
        }
        // And it emits steam if it hits an ice shard
        else if (other.gameObject.CompareTag("PlayerProjectile"))
        {
            remainingHeat = 0;
            EmitSteam();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // While the fireball is in contact with water, it drains its heat and emits steam
        if (other.gameObject.CompareTag("Water") && DrainHeat(other))
        {
            EmitSteam();
        }
    }

    bool DrainHeat(Collider2D collider)
    {
        if (waterDrop && waterParticles)
        {
            // We get all water particles and destroy those too close
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[waterParticles.main.maxParticles];
            int particleCount = waterParticles.GetParticles(particles);

            // If the water is frozen, heat decreases twice as fast
            int heatDecrement = 1;
            if (waterDrop.IsFrozen())
            {
                heatDecrement = 2;
            }

            int particleIdx = 0;
            int steamCount = 0;
            // Iterate through particles
            while (particleIdx < particleCount && remainingHeat > 0)
            {
                Vector3 toParticle = particles[particleIdx].position - transform.position;
                // Too close or not ?
                if (toParticle.sqrMagnitude < heatSqrRange)
                {
                    // Setting the lifetime to something negative will trigger particle destruction later
                    particles[particleIdx].remainingLifetime = -1.0f;
                    remainingHeat -= heatDecrement;
                    steamCount++;
                }
                particleIdx++;
            }
            // Put back particles where they belong
            waterParticles.SetParticles(particles, particleCount);

            return steamCount > 0;
        }
        else
        {
            return false;
        }
    }

    void EmitSteam()
    {
        GameObject steamEffect = Instantiate(steamAnimation, transform.position, Quaternion.identity) as GameObject;
        // If the sound of the animation was heard not long ago, we do not want to hear it yet
        if (steamSoundCooldown > 0f)
        {
            Destroy(steamEffect.GetComponent<AudioSource>());
        }
        else
        {
            steamSoundCooldown = steamSoundMaxCooldown;
        }
    }
}
