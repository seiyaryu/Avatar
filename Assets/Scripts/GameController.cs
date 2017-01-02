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

    private SpawnerController spawner;

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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        spawner = GetComponent<SpawnerController>();
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
        gameScreen.SetActive(false);
        pauseScreen.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        gameScreen.SetActive(true);
        pauseScreen.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
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
        gameScreen.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    static public GameController GameManager
    {
        get { return instance; }
    }

    public bool GameOn
    {
        get { return Time.timeScale > 0 && !titleScreen.activeSelf; }
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
