using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterFlaskController : MonoBehaviour, IDamageListener {

    [Tooltip("Maximum number of water particles")]
    public int maximumWater = 200;
    [Tooltip("How many particles can exit the flask at a time ?")]
    public int waterFlow = 10;

    // Current number of water particles
    private int currentWater;
    // How many water particles are still in the flask
    private int storedWater;

    [Header("Particle Attraction")]

    [Tooltip("How many points in space attract particles to them ?")]
    public int attractorCount = 6;
    [Tooltip("How much distance between attractors (relative to the radius of particles) ?")]
    public float attractorSpacing = 1.5f;

    [Tooltip("How much does an attractor attract its particles ?")]
    public float attractAmpl = 30f;
    [Tooltip("How much more does an attractor attract its particles whan attacking or closing the flask ?")]
    public float attractMultiplier = 3f;
    [Tooltip("Minimum attraction")]
    public float attractMin = 0.2f;
    [Tooltip("Maximum attraction")]
    public float attractMax = 30f;

    // List of all attractors positions
    private List<Vector2> attractors;
    // Square radius of a particle
    private float sqrParticleSize;
    // Absolute distance between attractors
    private float attractorRange;
    // Squared absolute distance between attractors
    private float sqrAttractorRange;

    [Header("Particle Repulsion")]

    [Tooltip("How much do particles repel each other?")]
    public float repelAmpl = 1.0f;
    [Tooltip("How close must particles be for repel to be effective (relative to particle radius) ?")]
    public float repelSpacing = 1.2f;

    // Squared absolute repel range
    private float sqrRepelRange;

    [Tooltip("How much are particles naturally slowed down")]
    public float damp = 0.7f;

    [Header("Particle Clustering")]

    // The object holding all bounding elements of the floating water 
    public Transform waterDrop;
    // Average velocity of water particles
    private Vector2 avgVelocity = Vector2.zero;
    // Average position of water particles
    private Vector2 avgPosition = Vector2.zero;

    // All water particles
    private ParticleSystem particles;
    // Temporary particle buffer
    private ParticleSystem.Particle[] buffer;

    [Tooltip("How far away must particles be to be considered separate (relative to particle radius) ?")]
    public float outsiderSpacing = 3f;
    // Squared distance threshold for particles to be considered separate
    private float sqrOutsiderRange;

    [Tooltip("Along how many directions do we construct bounding planes for K-dops ?")]
    public int directionCount = 6;
    // Bounding planes sampling directions
    private Vector2[] directions;
    // Extrema along each sampling direction
    private float[] KDopExtremas;
    // Points making up a single K-dop
    private Vector2[] KDopPoints;

    // Cluster indices for each water particle
    private int[] clusters;
    // Current number of clusters of water particles
    private int clusterCount;

    [Header("Freezing and Melting")]

    [Tooltip("How small must the squared speed of a particle be for this particle to freeze ?")]
    public float freezeThreshold = 2f;
    [Tooltip("How mush time must pass to be able to melt or freeze water again ?")]
    public float stateMaxCooldown = 0.5f;
    private float stateCooldown = 0f;

    // Identifier for the layer of moving objects
    private int movingMask;
    // Can the water be frozen right now ?
    private bool freezable = false;
    // Is water currently frozen ?
    private bool frozen = false;

    [Header("Attacking")]

    [Tooltip("Ice shard Prefab")]
    public ProjectileController iceShard;
    [Tooltip("How much time between each ice shards throw ?")]
    public float iceShardMaxCooldown = 0.5f;
    [Tooltip("How many water particles cost each ice shard ?")]
    public int iceShardCost = 5;

    [Tooltip("How much damage can the whip inflict at most ?")]
    public int whipDamage = 3;
    [Tooltip("How much time to be able to whip again ?")]
    public float whipMaxCooldown = 5f;
    [Tooltip("How many attractors do we add when whipping ?")]
    public int whipAttractorCount = 30;

    private float attackCooldown = 0f;

    [Tooltip("How much time can a whip last ?")]
    public float whipMaxDuration = 1f;
    private float whipDuration = 0f;

    [Tooltip("How big must the squared speed of water be for the whip to be effective ?")]
    public float whipSpeedThreshold = 25f;

    [Tooltip("How much time cato start gathering water again ?")]
    public float gatherMaxCooldown = 1f;
    private float gatherCooldown = 0f;

    [Tooltip("How much does the whip push back stuff ?")]
    public float rejectAmpl = 30f;

    [Tooltip("Prefab for water falling when waterbender is hit")]
    public ParticleSystem fallingWater;
    [Tooltip("Number of particles lost when waterbender takes a hit")]
    public int particleLossOnHit = 20;

    // Is the flask open ?
    private bool flaskOpen = false;
    // Are we attacking something right now
    private bool attacking = false;

    private Animator animator;
    private DamageableController damageable;

    [Header("Audio")]

    public AudioSource waterSound;
    public AudioSource whipSound;
    public AudioSource whipCrackSound;
    public AudioSource throwShardSound;
    public AudioSource freezeSound;

    void Start ()
    {
        movingMask = LayerMask.GetMask(new string[]{"Player", "Enemy"});
    }

    void Awake ()
    {
        currentWater = maximumWater;
        storedWater = maximumWater;

        particles = GetComponent<ParticleSystem>();
        particles.maxParticles = maximumWater;
        buffer = new ParticleSystem.Particle[particles.maxParticles];

        attractors = new List<Vector2>();
        attractors.Add(new Vector2());
        waterDrop.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;

        sqrParticleSize = particles.startSize * particles.startSize;
        attractorRange = particles.startSize * attractorSpacing;
        sqrAttractorRange = attractorRange * attractorRange;
        sqrRepelRange = repelSpacing * repelSpacing * sqrParticleSize;
        sqrOutsiderRange = outsiderSpacing * outsiderSpacing * sqrParticleSize;

        animator = GetComponentInParent<Animator>();
        damageable = GetComponentInParent<DamageableController>();

        // We will uniformly sample directions in 2D on the unit circle
        directions = new Vector2[directionCount];
        for (int directionIdx = 0; directionIdx < directionCount; directionIdx++)
        {
            float theta = directionIdx * Mathf.PI / directionCount;
            directions[directionIdx] = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
        }
        KDopExtremas = new float[2 * directionCount + 1];
        KDopPoints = new Vector2[2 * directionCount];

        clusters = new int[particles.maxParticles];
    }


    // Update the position of points attracting water particles
    void UpdateAttractors()
    {
        if(flaskOpen)
        {
            if (!frozen)
            {
                Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // We have not yet created as many attractors as we want
                if (attractors.Count < attractorCount)
                {
                    Vector2 toAttractor = attractors[0] - mouse;
                    // Are we far enough to add a new attractor at mouse position ?
                    if (toAttractor.sqrMagnitude > sqrAttractorRange)
                    {
                        int newAttractorCount = Mathf.Min(Mathf.FloorToInt(toAttractor.magnitude / attractorRange), attractorCount - attractors.Count);
                        // Add new attractors
                        for (int attractorIdx = 1; attractorIdx <= newAttractorCount; attractorIdx++)
                        {
                            float ratio = (newAttractorCount - attractorIdx) / (float)(newAttractorCount);
                            attractors.Insert(0, mouse + toAttractor * ratio);
                            waterDrop.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
                        }

                    }
                }
                // If there are enough attractors, set the position of the front one to the mouse position
                else
                {
                    attractors[0] = mouse;
                }

                // Move all other attractors such that they are at the right distance from each other
                for (int attractorIdx = 1; attractorIdx < attractors.Count; attractorIdx++)
                {
                    Vector2 toNext = attractors[attractorIdx] - attractors[attractorIdx - 1];
                    attractors[attractorIdx] = attractors[attractorIdx - 1] + attractorRange * toNext.normalized;
                }
            }
        }
        // If the flask is closed, stack all attractors on the waterbender
        else
        {
            for(int attractorIdx = 0; attractorIdx < attractors.Count; attractorIdx++)
            {
                attractors[attractorIdx] = transform.position;
            }
        }
    }

    // Get the position of the attractor associated with the particle of given index
    Vector3 GetAttractor (int particleIdx)
    {
        // Front attractors gather more particles around them
        float ratio = (float)(particleIdx) / maximumWater;
        if(!attacking)
        {
            ratio *= ratio;
        }
        return attractors[Mathf.FloorToInt(attractors.Count * ratio)];
    }

    // Get Position of the main water drop
    public Vector2 GetDropPosition ()
    {
        return waterDrop.position;
    }

    // Slow all particles
    void Damp()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            buffer[particleIdx].velocity *= damp;
        }
    }

    // Attract particles to their respective attractors
    void Attract()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            Vector3 position = buffer[particleIdx].position;
            Vector3 toAttractor = GetAttractor(particleIdx) - position;

            float sqrDistance = toAttractor.sqrMagnitude;
            float distance = Mathf.Sqrt(sqrDistance);

            // If the flask is closed and the particle is close enough from its attractor, then it must disappear and get back in the flask
            if(!flaskOpen && distance < particles.startSize)
            {
                buffer[particleIdx].remainingLifetime = -1f;
                storedWater++;
            }
            // The farther from its attractor, the more attacted the particle is
            float rangeFactor = Mathf.Max(Mathf.Min(sqrDistance / sqrParticleSize, attractMax), attractMin);
            buffer[particleIdx].velocity += toAttractor * attractAmpl * rangeFactor * Time.deltaTime / distance;
        }
    }

    // Fuse clusters of provided identifiers into one
    void FuseClusters(int oldCluster, int newCluster)
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            if (clusters[particleIdx] == oldCluster)
            {
                clusters[particleIdx] = newCluster;
            }
        }
    }

    // Break all clusters
    void ResetClusters()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            clusters[particleIdx] = -1;
        }
        clusterCount = 0;
    }

    // Repel and cluster particles
    void Repel()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            // If this particle is in no cluster, create a new one
            if (clusters[particleIdx] == -1)
            {
                clusters[particleIdx] = clusterCount;
                clusterCount++;
            }

            for (int otherIdx = particleIdx + 1; otherIdx < particles.particleCount; otherIdx++)
            {
                Vector3 toOther = buffer[otherIdx].position - buffer[particleIdx].position;
                float sqrDist = toOther.sqrMagnitude;

                // Are these particles close enough to interact ?
                if (sqrDist < sqrRepelRange)
                {
                    Vector3 repel = toOther.normalized * repelAmpl * (sqrDist - sqrParticleSize) * Time.deltaTime;
                    buffer[particleIdx].velocity += repel;
                    buffer[otherIdx].velocity -= repel;
                }

                // Are these particles close enough to be in the same cluster ?
                if (sqrDist < sqrOutsiderRange && clusters[otherIdx] != clusters[particleIdx])
                {
                    // These particles are in different clusters, but they sould not be, so fuse clusters
                    if (clusters[otherIdx] != -1)
                    {
                        FuseClusters(clusters[otherIdx], clusters[particleIdx]);
                    }
                    // Put the other particle in the same cluster as this one
                    else
                    {
                        clusters[otherIdx] = clusters[particleIdx];
                    }
                }
            }
        }
    }

    // Particles should not die unless desired
    void ResetLifetime()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            if (buffer[particleIdx].remainingLifetime > 0.0f)
            {
                buffer[particleIdx].remainingLifetime = buffer[particleIdx].startLifetime;
            }
        }
    }

    // Freeze particle of given index
    void FreezeParticle(int particleIdx)
    {
        Color32 color = buffer[particleIdx].startColor;
        if (color.a == 105)
        {
            color.a = 255;
            color.r += 60;
            color.g += 60;
            color.b += 30;
            buffer[particleIdx].startColor = color;
        }
    }

    // Melt particle of given index
    void MeltParticle(int particleIdx)
    {
        Color32 color = buffer[particleIdx].startColor;
        if (color.a == 255)
        {
            color.a = 105;
            color.r -= 60;
            color.g -= 60;
            color.b -= 30;
            buffer[particleIdx].startColor = color;
        }
    }

    // Update the state of each particle : frozen or not
    void Freeze()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            if (frozen && buffer[particleIdx].velocity.sqrMagnitude < freezeThreshold)
            {
                buffer[particleIdx].velocity = Vector3.zero;
                FreezeParticle(particleIdx);
            }
            else
            {
                MeltParticle(particleIdx);
            }
        }
    }

    // Physics stuff
    void FixedUpdate()
    {
        // Move particle attractors to the right place
        UpdateAttractors();

        // Get particles into the temporary buffer to work on them
        int particleCount = particles.GetParticles(buffer);
        // Update current water particle count : water in store + particle count
        currentWater = storedWater + particleCount;

        // Reinitialize clusters
        ResetClusters();

        // Do the necessary on particles
        Damp();
        Attract();
        Repel();
        Freeze();
        ResetLifetime();

        // Put particles back in the system
        particles.SetParticles(buffer, particleCount);

        // Compute bounding elements on particles
        ComputeKDop();
    }

    void LateUpdate()
    {
        freezable = CanFreeze();
    }

    // Find the index of the cluster with the most particles
    int FindLargestCluster()
    {
        int[] clusterSizes = new int[clusterCount];

        for (int clusterIdx = 0; clusterIdx < clusterCount; clusterIdx++)
        {
            clusterSizes[clusterIdx] = 0;
        }

        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            clusterSizes[clusters[particleIdx]]++;
        }

        int largestClusterIdx = 0;

        for (int clusterIdx = 1; clusterIdx < clusterCount; clusterIdx++)
        {
            if (clusterSizes[clusterIdx] > clusterSizes[largestClusterIdx])
            {
                largestClusterIdx = clusterIdx;
            }
        }
        return largestClusterIdx;
    }

    // Compute K-dops for each attractor with the particles in the main cluster 
    void ComputeKDop()
    {
        PolygonCollider2D[] colliders = waterDrop.gameObject.GetComponents<PolygonCollider2D>();
        avgVelocity = Vector2.zero;

        // If there are no particles out there
        if (particles.particleCount == 0)
        {
            waterDrop.position = transform.position;
            foreach(PolygonCollider2D collider in colliders)
            {
                collider.pathCount = 0;
            }
            return;
        }
        else
        {
            waterDrop.position = avgPosition;
        }

        avgPosition = Vector2.zero;

        // Get the index of the main cluster
        int clusterIdx = FindLargestCluster();

        // Compute K-dops
        int particleCount = 0;
        for (int attractorIdx = 0; attractorIdx < attractors.Count; attractorIdx++)
        {
            particleCount += ComputeKDop(attractorIdx, clusterIdx, colliders[attractorIdx]);
        }

        if (particleCount == 0)
        {
            waterDrop.position = transform.position;          
        }
        else
        {
            avgVelocity /= particleCount;
            avgPosition /= particleCount;
        }
    }

    // Get the index of the first particle to be associated with this attractor
    int GetAttractorFirstIndex(int attractorIdx)
    {
        return Mathf.CeilToInt(maximumWater * Mathf.Sqrt((float)(attractorIdx) / attractors.Count));
    }

    // Compute K-dop for given attractor with particles of given cluster
    int ComputeKDop(int attractorIdx, int clusterIdx, PolygonCollider2D KDop)
    {
        int particleCount = 0;
        int frstParticleIdx = GetAttractorFirstIndex(attractorIdx);
        int lastParticleIdx = GetAttractorFirstIndex(attractorIdx + 1);
        lastParticleIdx = Mathf.Min(lastParticleIdx, particles.particleCount);

        // Iterate through particles, computing extreme positions along each sampling direction
        for(int particleIdx = frstParticleIdx; particleIdx < lastParticleIdx; particleIdx++)
        {
            // Is it in the right cluster
            if (clusters[particleIdx] == clusterIdx)
            {
                Vector2 position = buffer[particleIdx].position;
                // Is it the first particle we found ?
                if (particleCount == 0)
                {
                    // If yes, initialize extrema
                    for (int directionIdx = 0; directionIdx < directionCount; directionIdx++)
                    {
                        float dot = Vector2.Dot(directions[directionIdx], position);
                        KDopExtremas[directionIdx] = dot + 0.5f * particles.startSize;
                        KDopExtremas[directionCount + directionIdx] = dot - 0.5f * particles.startSize;
                    }
                }
                else
                {
                    // If no, update them : is this particle further away on ay axis than previous particles ?
                    for (int directionIdx = 0; directionIdx < directionCount; directionIdx++)
                    {
                        float dot = Vector2.Dot(directions[directionIdx], position);
                        if (dot + 0.5f * particles.startSize > KDopExtremas[directionIdx])
                        {
                            KDopExtremas[directionIdx] = dot + 0.5f * particles.startSize;
                        }
                        else if (dot - 0.5f * particles.startSize < KDopExtremas[directionCount + directionIdx])
                        {
                            KDopExtremas[directionCount + directionIdx] = dot - 0.5f * particles.startSize;
                        }
                    }
                }
                avgVelocity += (Vector2)(buffer[particleIdx].velocity);
                avgPosition += position;
                particleCount++;
            }
        }
        KDopExtremas[2 * directionCount] = KDopExtremas[0];

        if (particleCount != 0)
        {
            // Now, we will build intersections of bounding planes to get the points making up the K-dop
            for (int directionIdx = 0; directionIdx < 2 * directionCount; directionIdx++)
            {
                int idx1 = directionIdx % directionCount;
                float A1 = directions[idx1].x;
                float B1 = directions[idx1].y;
                float C1 = KDopExtremas[directionIdx];

                int idx2 = (directionIdx + 1) % directionCount;
                float A2 = directions[idx2].x;
                float B2 = directions[idx2].y;
                float C2 = KDopExtremas[directionIdx + 1];

                float det = A1 * B2 - A2 * B1;

                KDopPoints[directionIdx].x = (B2 * C1 - B1 * C2) / det;
                KDopPoints[directionIdx].y = (A1 * C2 - A2 * C1) / det;
                KDopPoints[directionIdx] = waterDrop.InverseTransformPoint(KDopPoints[directionIdx]);
            }
            KDop.SetPath(0, KDopPoints);
        }
        else
        {
            KDop.pathCount = 0;
        }
        return particleCount;
    }

    // Open of close the flask
    void OpenFlask()
    {
        if(Input.GetButtonDown("OpenFlask"))
        {
            if(!flaskOpen)
            {
                flaskOpen = true;
                animator.SetBool("Bending", true);
                attractAmpl /= attractMultiplier;
                StartCoroutine(EmitWater());
            }
            // Can't close the flask if water is frozen
            else if(!frozen)
            {
                flaskOpen = false;
                animator.SetBool("Bending", false);
                attractAmpl *= attractMultiplier;
            }
        }
    }

    // Emit water out of the flask
    IEnumerator EmitWater ()
    {
        while(flaskOpen && storedWater > 0)
        {
            int spawnCount = Mathf.Min(waterFlow, storedWater);
            particles.Emit(spawnCount);
            storedWater -= spawnCount;
            yield return null;
        }
    }

    // Are we gathering water right now ?
    public bool IsGatheringWater ()
    {
        return gatherCooldown > 0f;
    }

    // Gather water back from the environment
    void GatherWater ()
    {
        // Update cooldown
        if(gatherCooldown > 0f)
        {
            gatherCooldown -= Time.deltaTime;
            // Gathering is over, restore original speed
            if(gatherCooldown <= 0f)
            {
                attractAmpl *= attractMultiplier;
            }
        }

        if (flaskOpen && !frozen)
        {
            // Start gathering water
            if (Input.GetButtonDown("Gather") && gatherCooldown <= 0f)
            {
                attractAmpl /= attractMultiplier;
            }
            // Still gathering
            else if (Input.GetButton("Gather"))
            {
                // Reset cooldown
                gatherCooldown = gatherMaxCooldown;
                
                // Iterate through water sources and ask them to spawn water particles
                List<Vector3> spawns = new List<Vector3>();
                GameObject[] sources = GameObject.FindGameObjectsWithTag("WaterSource");
                foreach (GameObject source in sources)
                {
                    source.GetComponent<WaterSourceController>().EmitWater(waterDrop.position, 10f, spawns);
                }

                // We do not want to spawn more particles than allowed to
                int spawnCount = Mathf.Min(spawns.Count, particles.maxParticles - particles.particleCount);
                particles.Emit(spawnCount);

                // Set the position of the new particles as specified by the spawners
                int particleCount = particles.GetParticles(buffer);
                for (int particleIdx = 0; particleIdx < spawnCount; particleIdx++)
                {
                    buffer[particleCount - particleIdx - 1].position = spawns[particleIdx];
                }
                particles.SetParticles(buffer, particleCount);
            }
        }
    }

    // You can't freeze water if it is hovering over something
    bool CanFreeze()
    {
        if(!frozen)
        {
            Collider2D[] colliders = waterDrop.gameObject.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (collider.IsTouchingLayers(movingMask))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Change physical state of water
    void TransformWater()
    {
        // Update state change cooldown
        if (stateCooldown > 0.0f)
        {
            stateCooldown -= Time.deltaTime;
        }

        // Default state when there is no water is melted
        if (currentWater == 0)
        {
            frozen = false;
            MeltWater();
        }

        // Switch physical state of water
        if (Input.GetButtonDown("Freeze") && flaskOpen && currentWater > 0 && stateCooldown <= 0.0f && freezable)
        {
            frozen = !frozen;
            stateCooldown = stateMaxCooldown;
            // Frozen water is unaffected by gravity and a solid obstacle
            if (frozen) FreezeWater();
            else MeltWater();
        }
    }

    void FreezeWater ()
    {
        particles.gravityModifier = 0.0f;
        Collider2D[] colliders = waterDrop.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.isTrigger = false;
        }
        freezeSound.Play();
    }

    void MeltWater ()
    {
        particles.gravityModifier = 1.0f;
        Collider2D[] colliders = waterDrop.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.isTrigger = true;
        }
    }

    // Throw an ice shard
    void ShootIceShard ()
    {
        int particleCount = particles.GetParticles(buffer);

        // Are there enough particles to pay the cost to throw a shard ?
        if(particleCount >= iceShardCost)
        {
            // Remove particles needed to pay the cost
            for (int particleIdx = 0; particleIdx < iceShardCost; particleIdx++)
            {
                buffer[particleCount - particleIdx - 1].remainingLifetime = -1f;
            }
            particles.SetParticles(buffer, particleCount);

            Vector2 center = waterDrop.position;
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 direction = mouse - center;
            Vector2 normal = direction.Rotate(0.5f * Mathf.PI).normalized;

            // Spawn ice shard
            ProjectileController iceShardInstance = (ProjectileController)Instantiate(iceShard, center + Random.Range(-0.5f, 0.5f) * normal, Quaternion.identity);

            iceShardInstance.Direction = direction;

            attackCooldown = iceShardMaxCooldown;

            throwShardSound.Play();
            animator.SetTrigger("Attack");
        }
    }

    // Handles what happens when whip hits something
    public void OnWhipAttack (Collider2D other)
    {
        if (!frozen && attacking && other.gameObject.CompareTag("Enemy") && avgVelocity.sqrMagnitude > whipSpeedThreshold)
        {
            DamageableController damageable = other.gameObject.GetComponent<DamageableController>();
            if (damageable)
            {
                int damage = Mathf.FloorToInt(whipDamage *  Mathf.Clamp(2f * ((float)currentWater / maximumWater - 0.3f), 0f, 1f));
                damageable.OnHit(damage, avgVelocity.normalized * rejectAmpl);
            }
            whipCrackSound.Play();
        }
    }

    void PlayWaterSound()
    {
        float volume = Mathf.Min(0.5f * avgVelocity.magnitude, 1f);
        waterSound.volume = volume;
        whipSound.volume = volume;
    }

    void Attack()
    {
        // Update cooldowns
        if (attackCooldown > 0.0f)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (whipDuration > 0.0f)
        {
            whipDuration -= Time.deltaTime;
        }

        // If water is frozen, we attack by throwing shards
        if(frozen)
        {
            if (Input.GetButton("Attack") && attackCooldown <= 0.0f)
            {
                ShootIceShard();
            }
        }
        // Of not frozen, we whip
        else
        {
            // If not attacking yet
            if (!attacking)
            {
                // Start to attack : speed up water, add attractors, start cooldown for the duration of the attack
                if (Input.GetButtonDown("Attack") && attackCooldown <= 0.0f)
                {
                    attacking = true;
                    attractAmpl *= attractMultiplier;
                    attractorCount += whipAttractorCount;
                    whipDuration = whipMaxDuration;
                    whipSound.Play();
                    animator.SetTrigger("Attack");
                }
            }
            // If whip is over, slow down water, remove attractors, start cooldown to prevent attacking again
            else if (whipDuration <= 0.0f)
            {
                attacking = false;
                attractAmpl /= attractMultiplier;
                attractorCount -= whipAttractorCount;
                if (attractors.Count > attractorCount)
                {
                    PolygonCollider2D[] colliders = waterDrop.GetComponents<PolygonCollider2D>();
                    for(int colliderIdx = attractorCount; colliderIdx < attractors.Count; colliderIdx++)
                    {
                        Destroy(colliders[colliderIdx]);
                    }
                    attractors.RemoveRange(attractorCount, attractors.Count - attractorCount);
                }
                attackCooldown = whipMaxCooldown;
                whipSound.Stop();
            }
        }
    }

    // If waterbender is hit and water is not frozen, he will lose some
    public void OnDamaged(int damage)
    {
        LeakWater(particleLossOnHit);
    }

    public void LeakWater(int particleAmount)
    {
        if (!frozen && particleAmount > 0)
        {
            int particleCount = particles.GetParticles(buffer);

            // Instantiate falling water (affected by gravity) at the same place as the lost particles
            ParticleSystem fallingWaterInstance = (ParticleSystem)Instantiate(fallingWater, Vector3.zero, Quaternion.identity);
            int particleLoss = Mathf.Min(particleCount, particleAmount);

            ParticleSystem.Particle[] lossBuffer = new ParticleSystem.Particle[particleAmount];
            System.Array.Copy(buffer, particleCount - particleLoss, lossBuffer, 0, particleLoss);

            // Kill the original particles and set them back
            for (int particleIdx = 0; particleIdx < particleLoss; ++particleIdx)
            {
                lossBuffer[particleIdx].velocity *= Random.Range(0.5f, 1.5f); 
                buffer[particleCount - 1 - particleIdx].remainingLifetime = -1f;
            }

            fallingWaterInstance.SetParticles(lossBuffer, particleLoss);
            particles.SetParticles(buffer, particleCount);
        }
    }

    void Update ()
    {
        PlayWaterSound();

        if (GameController.GetGameManager().IsGameOn() && damageable.IsAlive())
        {
            OpenFlask();

            TransformWater();

            GatherWater();

            Attack();
        }
    }

    public int GetCurrentWater()
    {
        return currentWater;
    }

    public int GetMaxWater()
    {
        return maximumWater;
    }

    public bool IsFrozen()
    {
        return frozen;
    }
}
