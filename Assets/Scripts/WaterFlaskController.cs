using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterFlaskController : MonoBehaviour {

    public float damp = 0.7f;

    public int waterMax = 200;
    public int waterCurrent = 100;
    public int waterFlow = 10;

    [Header("Particle Attraction")]
    public int attractorCount = 6;
    public float attractorSpacing = 1.5f;

    public float attractAmpl = 50f;
    public float attractMin = 0.2f;
    public float attractMax = 10f;

    private List<Vector2> attractors;
    private float sqrParticleSize;
    private float sqrAttractorRange;

    [Header("Particle Repulsion")]

    public float repelAmpl = 1.0f;
    public float repelSpacing = 1.2f;
    private float sqrRepelRange;

    [Header("Particle Clustering")]

    public Transform waterDrop;
    private Vector2 avgVelocity;
    private Vector2 avgPosition;

    private ParticleSystem particles;
    private ParticleSystem.Particle[] buffer;

    public float outsiderSpacing = 3f;
    private float sqrOutsiderRange;

    public int directionCount = 6;
    private Vector2[] directions;
    private float[] KDopExtremas;
    private Vector2[] KDopPoints;

    private int[] clusters;
    private int clusterCount;

    [Header("Freezing and Melting")]

    public float freezeThreshold = 2f;
    public float stateMaxCooldown = 0.5f;
    private float stateCooldown = 0f;

    private bool frozen = false;

    [Header("Attacking")]

    public IceShardController iceShard;
    public float iceShardMaxCooldown = 0.5f;
    public int iceShardCost = 5;

    public float whipMaxCooldown = 5f;
    public int whipAttractorCount = 15;
    private float attackCooldown = 0f;

    public float whipMaxDuration = 1f;
    private float whipDuration = 0f;

    public float gatherMaxCooldown = 1f;
    private float gatherCooldown = 0f;

    public float hitThreshold = 25f;

    public float rejectAmpl = 30f;
    public float attackAmpl = 2f;

    private bool flaskOpen = false;
    private bool attacking = false;

    private Animator animator;

    [Header("Audio")]

    public AudioSource waterSound;
    public AudioSource whipSound;
    public AudioSource whipCrackSound;
    public AudioSource throwShardSound;
    public AudioSource freezeSound;

    public Vector2 GetFrontAttractor()
    {
        return waterDrop.position;
    }

    void Awake ()
    {
        particles = GetComponent<ParticleSystem>();
        particles.maxParticles = waterMax;
        buffer = new ParticleSystem.Particle[particles.maxParticles];

        attractors = new List<Vector2>();
        attractors.Add(new Vector2());
        waterDrop.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;

        sqrParticleSize = particles.startSize * particles.startSize;
        sqrAttractorRange = attractorSpacing * sqrParticleSize;
        sqrRepelRange = repelSpacing * sqrParticleSize;
        sqrOutsiderRange = outsiderSpacing * sqrParticleSize;

        animator = GetComponentInParent<Animator>();

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
	
    //Update the position of points attracting water particles
    void UpdateAttractors()
    {
        if(flaskOpen)
        {
            if (!frozen)
            {
                Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if (attractors.Count < attractorCount)
                {
                    Vector2 toAttractor = attractors[0] - mouse;
                    if(toAttractor.sqrMagnitude > sqrAttractorRange)
                    {
                        attractors.Insert(0, mouse);
                        waterDrop.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
                    }
                }
                else
                {
                    attractors[0] = mouse;
                }

                for (int attractorIdx = 1; attractorIdx < attractors.Count; attractorIdx++)
                {
                    Vector2 toNext = attractors[attractorIdx] - attractors[attractorIdx - 1];
                    attractors[attractorIdx] = attractors[attractorIdx - 1] + sqrAttractorRange * toNext.normalized;
                }
            }
        }
        else
        {
            for(int attractorIdx = 0; attractorIdx < attractors.Count; attractorIdx++)
            {
                attractors[attractorIdx] = transform.position;
            }
        }
    }

    Vector3 GetAttractor (int particleIdx)
    {
        float ratio = (float)(particleIdx) / waterMax;
        return attractors[Mathf.FloorToInt(attractors.Count * ratio * ratio)];
    }

    //Slow all particles
    void Damp()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            buffer[particleIdx].velocity *= damp;
        }
    }

    //Attract particles to their respective attractors
    void Attract()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            Vector3 position = buffer[particleIdx].position;
            Vector3 toAttractor = GetAttractor(particleIdx) - position;
            float sqrDistance = toAttractor.sqrMagnitude;
            float distance = Mathf.Sqrt(sqrDistance);
            if(!flaskOpen && distance < particles.shape.radius)
            {
                buffer[particleIdx].lifetime = -1f;
            }
            float rangeFactor = Mathf.Max(Mathf.Min(sqrDistance / sqrParticleSize, attractMax), attractMin);
            buffer[particleIdx].velocity += toAttractor * attractAmpl * rangeFactor * Time.deltaTime / distance;
        }
    }

    //Fuse clusters of provided identifiers into one
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

    //Break all clusters
    void ResetClusters()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            clusters[particleIdx] = -1;
        }
        clusterCount = 0;
    }

    //Repel and cluster particles
    void Repel()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            if (clusters[particleIdx] == -1)
            {
                clusters[particleIdx] = clusterCount;
                clusterCount++;
            }

            for (int otherIdx = particleIdx + 1; otherIdx < particles.particleCount; otherIdx++)
            {
                Vector3 toOther = buffer[otherIdx].position - buffer[particleIdx].position;
                float sqrDist = toOther.sqrMagnitude;

                if (sqrDist < sqrRepelRange)
                {
                    Vector3 repel = toOther.normalized * repelAmpl * (sqrDist - sqrParticleSize) * Time.deltaTime;
                    buffer[particleIdx].velocity += repel;
                    buffer[otherIdx].velocity -= repel;
                }

                if (sqrDist < sqrOutsiderRange && clusters[otherIdx] != clusters[particleIdx])
                {
                    if (clusters[otherIdx] != -1)
                    {
                        FuseClusters(clusters[otherIdx], clusters[particleIdx]);
                    }
                    else
                    {
                        clusters[otherIdx] = clusters[particleIdx];
                    }
                }
            }
        }
    }

    //Particles should not die unless desired
    void ResetLifetime()
    {
        for (int particleIdx = 0; particleIdx < particles.particleCount; particleIdx++)
        {
            if (buffer[particleIdx].lifetime > 0.0f)
            {
                buffer[particleIdx].lifetime = buffer[particleIdx].startLifetime;
            }
        }
    }

    //Freeze particle of given index
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

    //Melt particle of given index
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

    //Update the state of each particle : frozen or not
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

    void LateUpdate()
    {
        UpdateAttractors();

        int particleCount = particles.GetParticles(buffer);

        ResetClusters();

        Damp();
        Attract();
        Repel();
        Freeze();
        ResetLifetime();

        particles.SetParticles(buffer, particleCount);

        ComputeKDop();
    }

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

    void ComputeKDop()
    {
        PolygonCollider2D[] colliders = waterDrop.gameObject.GetComponents<PolygonCollider2D>();
        avgPosition = Vector2.zero;
        avgVelocity = Vector2.zero;

        if (particles.particleCount == 0)
        {
            waterDrop.position = transform.position;
            foreach(PolygonCollider2D collider in colliders)
            {
                collider.pathCount = 0;
            }
            return;
        }

        int clusterIdx = FindLargestCluster();

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
            waterDrop.position = avgPosition;
        }
    }

    int GetAttractorFirstIndex(int attractorIdx)
    {
        return Mathf.CeilToInt(waterMax * Mathf.Sqrt((float)(attractorIdx) / attractors.Count));
    }

    int ComputeKDop(int attractorIdx, int clusterIdx, PolygonCollider2D KDop)
    {
        int particleCount = 0;
        int frstParticleIdx = GetAttractorFirstIndex(attractorIdx);
        int lastParticleIdx = GetAttractorFirstIndex(attractorIdx + 1);
        lastParticleIdx = Mathf.Min(lastParticleIdx, particles.particleCount);
        for(int particleIdx = frstParticleIdx; particleIdx < lastParticleIdx; particleIdx++)
        {
            if (clusters[particleIdx] == clusterIdx)
            {
                Vector2 position = buffer[particleIdx].position;
                if (particleCount == 0)
                {
                    for (int directionIdx = 0; directionIdx < directionCount; directionIdx++)
                    {
                        float dot = Vector2.Dot(directions[directionIdx], position);
                        KDopExtremas[directionIdx] = dot + 0.5f * particles.startSize;
                        KDopExtremas[directionCount + directionIdx] = dot - 0.5f * particles.startSize;
                    }
                }
                else
                {
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

    void OpenFlask()
    {
        if(Input.GetButtonDown("OpenFlask"))
        {
            if(!flaskOpen)
            {
                flaskOpen = true;
                animator.SetBool("Bending", true);
                StartCoroutine(EmitWater());
            }
            else
            {
                flaskOpen = false;
                animator.SetBool("Bending", false);
            }
        }
    }

    IEnumerator EmitWater ()
    {
        int waterSpawned = 0;
        while(waterSpawned < waterCurrent)
        {
            int spawnCount = Mathf.Min(waterFlow, waterCurrent - waterSpawned);
            particles.Emit(spawnCount);
            waterSpawned += spawnCount;
            yield return null;
        }
    }

    void GatherWater ()
    {
        if(gatherCooldown > 0f)
        {
            gatherCooldown -= Time.deltaTime;
            if(gatherCooldown <= 0f)
            {
                attractMax *= 4f;
            }
        }

        if (flaskOpen)
        {
            if (Input.GetButtonDown("Gather") && gatherCooldown <= 0f)
            {
                attractMax /= 4f;
            }
            else if (Input.GetButton("Gather"))
            {
                gatherCooldown = gatherMaxCooldown;

                List<Vector3> spawns = new List<Vector3>();
                GameObject[] sources = GameObject.FindGameObjectsWithTag("WaterSource");
                foreach (GameObject source in sources)
                {
                    source.GetComponent<WaterSourceController>().EmitWater(waterDrop.position, 10f, spawns);
                }

                int spawnCount = Mathf.Min(spawns.Count, particles.maxParticles - particles.particleCount);
                particles.Emit(spawnCount);

                int particleCount = particles.GetParticles(buffer);
                for (int particleIdx = 0; particleIdx < spawnCount; particleIdx++)
                {
                    buffer[particleCount - particleIdx - 1].position = spawns[particleIdx];
                }
                particles.SetParticles(buffer, particleCount);
            }
        }
    }

    void FreezeWater()
    {
        if (stateCooldown > 0.0f)
        {
            stateCooldown -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Freeze") && flaskOpen && stateCooldown <= 0.0f)
        {
            frozen = !frozen;
            stateCooldown = stateMaxCooldown;
            if (frozen)
            {
                particles.gravityModifier = 0.0f;
                Collider2D[] colliders = waterDrop.GetComponents<Collider2D>();
                foreach(Collider2D collider in colliders)
                {
                    collider.isTrigger = false;
                }
                freezeSound.Play();
            }
            else
            {
                particles.gravityModifier = 1.0f;
                Collider2D[] colliders = waterDrop.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    collider.isTrigger = true;
                }
            }
        }
    }

    void ShootIceShard ()
    {
        int particleCount = particles.GetParticles(buffer);

        if(particleCount >= iceShardCost)
        {
            for (int particleIdx = 0; particleIdx < iceShardCost; particleIdx++)
            {
                buffer[particleCount - particleIdx - 1].lifetime = -1f;
            }
            particles.SetParticles(buffer, particleCount);

            Vector2 center = waterDrop.position;
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 direction = mouse - center;
            Vector2 normal = direction.Rotate(0.5f * Mathf.PI).normalized;

            IceShardController fireBallInstance = (IceShardController)Instantiate(iceShard, center + Random.Range(-0.5f, 0.5f) * normal, Quaternion.identity);

            fireBallInstance.SetDirection(direction);

            attackCooldown = iceShardMaxCooldown;

            throwShardSound.Play();
        }
    }

    public void OnWhipAttack (Collider2D other)
    {
        if (!frozen && attacking && other.gameObject.CompareTag("Enemy") && avgVelocity.sqrMagnitude > hitThreshold)
        {
            other.gameObject.GetComponent<Animator>().SetTrigger("Hurt");
            Rigidbody2D otherRB = other.gameObject.GetComponent<Rigidbody2D>();
            if (otherRB)
            {
                otherRB.AddForce(avgVelocity.normalized * rejectAmpl, ForceMode2D.Force);
                whipCrackSound.Play();
            }
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
        if (attackCooldown > 0.0f)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (whipDuration > 0.0f)
        {
            whipDuration -= Time.deltaTime;
        }

        if(frozen)
        {
            if (Input.GetButton("Attack") && attackCooldown <= 0.0f)
            {
                ShootIceShard();
            }
        }
        else
        {
            if (!attacking)
            {
                if (Input.GetButtonDown("Attack") && attackCooldown <= 0.0f)
                {
                    attacking = true;
                    attractMax *= 4f;
                    attractorCount += whipAttractorCount;
                    whipDuration = whipMaxDuration;
                    whipSound.Play();
                }
            }
            else if (Input.GetButtonUp("Attack") || whipDuration <= 0.0f)
            {
                attacking = false;
                attractMax /= 4f;
                attractorCount -= whipAttractorCount;
                if(attractors.Count > attractorCount)
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

    void Update ()
    {
        PlayWaterSound();

        OpenFlask();

        FreezeWater();

        GatherWater();

        Attack();
    }
}
