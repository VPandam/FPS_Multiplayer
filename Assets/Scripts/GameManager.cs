using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState
{
    inGame, pause, gameOver, menu, shop
}
public class GameManager : MonoBehaviour
{
    public static GameManager sharedInstance;
    [SerializeField] int maxFrames = 90;
    [SerializeField] GameObject[] spawners;
    int currentRound;

    [SerializeField] TextMeshProUGUI roundText;
    [SerializeField] TextMeshProUGUI roundsSurvivedText;

    string stringRoundText;

    [SerializeField] GameObject zombiePrefab;

    [SerializeField] GameObject gameOverPanel;

    //Depending on whitch platform we are on, we load one or other scene.
    [SerializeField] string menuScene, mainScene;

    bool gamePaused;
    [SerializeField] GameObject pausePanel;

    //Black panel used for fade in when the game starts
    [SerializeField] GameObject fadeInGamePanel;
    GameState currentGameState;
    public GameState CurrentGameState { get => currentGameState; }
    [SerializeField] VendingMachine vendingMachine;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFrames;
        spawners = GameObject.FindGameObjectsWithTag("Spawner");
        StartGame();
    }
    void StartGame()
    {
        currentGameState = GameState.inGame;
        StartCoroutine(FadeInOrOutPanel(fadeInGamePanel, 3, false, false));
        StartCoroutine(StartNextRound());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            switch (currentGameState)
            {
                case GameState.pause:
                    Resume(); break;
                case GameState.inGame:
                    Pause(); break;
                case GameState.shop:
                    vendingMachine.ExitShop(); break;
            }

        }
    }

    void SetRound(int round)
    {
        currentRound = round;
        roundText.text = $"Round: {currentRound}";
        Debug.Log("SetRound");
    }

    IEnumerator StartNextRound()
    {
        //TODO: Animation and sound changing roung
        yield return new WaitForSeconds(3);
        SetRound(currentRound + 1);
        for (int i = 0; i < currentRound; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawners.Length);

            GameObject enemy = Instantiate(zombiePrefab, spawners[randomSpawnIndex].transform.position,
            Quaternion.identity);
            if (enemy != null)
                enemy.GetComponent<ZombieManager>().gameManager = this;
        }
    }

    //TODO: Change round animation
    public IEnumerator LookForEnemies()
    {
        yield return new WaitForSeconds(3.2f);
        GameObject[] enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemiesAlive.Length == 0)
        {
            StartCoroutine(StartNextRound());
        }
    }

    public void GameOver()
    {
        currentGameState = GameState.gameOver;
        roundsSurvivedText.text = $"ROUNDS SURVIVED: {currentRound}";
        StartCoroutine(FadeInOrOutPanel(gameOverPanel, 2, true, true));
    }

    IEnumerator FadeInOrOutPanel(GameObject panel, float time, bool fadeIn, bool stopTime)
    {
        Debug.Log("FadeInOrOut");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(true);

        if (fadeIn)
        {
            while (cg.alpha < 1)
            {
                cg.alpha += Time.deltaTime / time;
                yield return null;
            }
        }
        if (!fadeIn)
        {
            while (cg.alpha > 0)
            {
                cg.alpha -= Time.deltaTime / time;
                yield return null;
            }
        }

        if (stopTime)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Restart()
    {

        Time.timeScale = 1;
        currentGameState = GameState.pause;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        currentGameState = GameState.menu;
        SceneManager.LoadScene(menuScene);
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        currentGameState = GameState.pause;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }
    public void Shop()
    {
        Cursor.lockState = CursorLockMode.None;
        currentGameState = GameState.shop;
        Debug.Log(currentGameState);
    }
    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentGameState = GameState.inGame;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
