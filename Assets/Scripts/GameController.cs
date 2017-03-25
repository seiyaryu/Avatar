using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour {

    [Header("Screens")]

    [SerializeField]
    private GameObject titleScreen;

    [SerializeField]
    private GameObject gameScreen;

    [SerializeField]
    private GameObject pauseScreen;

    [SerializeField]
    private GameObject gameOverScreen;

    [SerializeField]
    private GameObject victoryScreen;

    [Header("Player")]

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Vector2 spawnPosition;

    [SerializeField]
    private Camera viewpoint;

    [Header("Victory")]

    [SerializeField]
    private Boat boat;

    [Header("Temporaries")]

    [SerializeField]
    private GameObject tempScreen;

    [SerializeField]
    private Transform tempRoot;

    private Spawner spawner;

    private static GameController instance = null;

	void Awake ()
    {
	    if (!instance)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }     
        spawner = GetComponent<Spawner>();
    }

    void Update ()
    {
        if (Input.GetButtonDown("Pause") && GameOn)
        {
            Pause();
        }
    }

    public void Pause ()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (gameScreen) gameScreen.SetActive(false);
        if (pauseScreen) pauseScreen.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (gameScreen) gameScreen.SetActive(true);
        if (pauseScreen) pauseScreen.SetActive(false);
    }

    void Clear ()
    {
        foreach (Transform child in tempScreen.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in tempRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Restart()
    {
        spawner.ResetCheckpoint();
        Retry();
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        spawner.BackToCheckpoint();

        Clear();

        viewpoint.GetComponent<CameraController>().Reset();

        player.GetComponent<Waterbender>().Reset();
        player.transform.position = spawnPosition;
    }

    public void ExitGame ()
    {
        Application.Quit();
    }

    public void NotifyDeath ()
    {
        if (spawner)
        {
            spawner.NotifyDeath();
        }
    }

    public void GameOver ()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (gameScreen) gameScreen.SetActive(false);
        if (pauseScreen) gameOverScreen.SetActive(true);
    }

    public void Victory ()
    {
        boat.Leave();
        StartCoroutine(OnVictory());
    }

    IEnumerator OnVictory ()
    {
        yield return new WaitForSeconds(3f);
        if (gameScreen) gameScreen.SetActive(false);
        if (victoryScreen) victoryScreen.SetActive(true);
    }

    static public GameController GameManager
    {
        get
        {
            if (!instance)
            {
                GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
                if (gameManager)
                {
                    instance = gameManager.GetComponent<GameController>();
                }
            }
            return instance;
        }
    }

    public GameObject Player
    {
        get { return player; }
    }

    public bool GameOn
    {
        get { return Time.timeScale > 0 && gameScreen && gameScreen.activeSelf; }
    }

    public Camera Viewpoint
    {
        get { return viewpoint; }
    }

    public Transform Root
    {
        get { return tempRoot; }
    }

    public GameObject MainCanvas
    {
        get { return tempScreen; }
    }

    public int EnemyCount
    {
        get
        {
            if (spawner)
            {
                return spawner.EnemyCount;
            }
            else
            {
                return 0;
            }
        }
    }
}
