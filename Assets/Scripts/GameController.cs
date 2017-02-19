using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]
    private GameObject titleScreen;

    [SerializeField]
    private GameObject gameScreen;

    [SerializeField]
    private GameObject pauseScreen;

    [SerializeField]
    private GameObject gameOverScreen;

    [SerializeField]
    private GameObject player;

    public GameObject Player
    {
        get { return player; }
    }

    [SerializeField]
    private Camera viewpoint;

    private Spawner spawner;

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

    private static GameController instance = null;

	void Awake ()
    {
	    if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            instance.player = player;

            Damageable playerHP = player.GetComponent<Damageable>();
            if (playerHP)
            {
                playerHP.GameCanvas = instance.gameScreen.transform;
            }

            instance.viewpoint = viewpoint;

            UIController UI = instance.gameScreen.GetComponent<UIController>();
            if (UI)
            {
                UI.Player = player;
            }
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    static public GameController GameManager
    {
        get { return instance; }
    }

    public bool GameOn
    {
        get { return Time.timeScale > 0 && titleScreen && !titleScreen.activeSelf; }
    }

    public Camera Viewpoint
    {
        get { return viewpoint; }
    }

    public GameObject MainCanvas
    {
        get { return gameScreen; }
    }
}
