using UnityEngine;
using System.Collections;

public enum EnemyType { Firebender, FireTank}

[System.Serializable]
public struct Spawn
{
    public float time;
    public Vector2 position;
    public EnemyType type;
}

[System.Serializable]
public struct Wave {
    public Spawn[] spawns;
    public bool checkpoint;
}

public class Spawner : MonoBehaviour {

    [Header("Waves")]

    [SerializeField]
    private Wave[] waves;

    [Header("Prefabs")]

    [SerializeField]
    private GameObject firebender;
    [SerializeField]
    private GameObject tank;
    [SerializeField]
    private float tankLeftBound;
    [SerializeField]
    private float tankRightBound;

    private float time = 0f;
    private int checkpointIndex = 0;
    private int waveIndex = 0;
    private int spawnIndex = 0;
    private int deathCount = 0;

    private int enemyCount = 0;

    public int EnemyCount
    {
        get { return enemyCount; }
    }

    int CountFirebenders()
    {
        int count = 0;
        for (int waveIdx = checkpointIndex; waveIdx < waves.Length; waveIdx++)
        {
            foreach (Spawn spawn in waves[waveIdx].spawns)
            {
                if (spawn.type == EnemyType.Firebender)
                {
                    ++count;
                }
            }
        }
        return count;
    }

    void Awake ()
    {
        enemyCount = CountFirebenders();
    }

    public void ResetCheckpoint ()
    {
        checkpointIndex = 0;
    }

    public void BackToCheckpoint ()
    {
        waveIndex = checkpointIndex;
        enemyCount = CountFirebenders();
        time = 0f;
        deathCount = 0;
        spawnIndex = 0;
    }

	void Update ()
    {
        if (GameController.GameManager.GameOn)
        {
            if (waveIndex < waves.Length)
            {
                if (spawnIndex < waves[waveIndex].spawns.Length)
                {
                    time += Time.deltaTime;
                    while (spawnIndex < waves[waveIndex].spawns.Length && time >= waves[waveIndex].spawns[spawnIndex].time)
                    {
                        switch (waves[waveIndex].spawns[spawnIndex].type)
                        {
                            case EnemyType.Firebender : SpawnFirebender(); break;
                            case EnemyType.FireTank: SpawnTank(); break;
                        }
                        ++spawnIndex;
                    }
                }
                if (deathCount == waves[waveIndex].spawns.Length)
                {
                    ++waveIndex;
                    time = 0f;
                    deathCount = 0;
                    spawnIndex = 0;
                    if (waveIndex < waves.Length && waves[waveIndex].checkpoint)
                    {
                        checkpointIndex = waveIndex;
                    }
                }
            }
            else
            {
                GameController.GameManager.Victory();
            }
        }       
	}

    public void NotifyDeath ()
    {
        ++deathCount;
        --enemyCount;
    }

    void SpawnFirebender ()
    {
        GameObject instance = Instantiate(firebender, waves[waveIndex].spawns[spawnIndex].position, Quaternion.identity, GameController.GameManager.Root);
        Firebender body = instance.GetComponent<Firebender>();
        if (body)
        {
            body.Player = GameController.GameManager.Player.transform;
        }
    }

    void SpawnTank ()
    {
        GameObject instance = Instantiate(tank, waves[waveIndex].spawns[spawnIndex].position, Quaternion.identity, GameController.GameManager.Root);
        TankInitializer initializer = instance.GetComponent<TankInitializer>();
        if (initializer)
        {
            initializer.Initialize(GameController.GameManager.Player.transform, tankLeftBound, tankRightBound);
        }
    }
}
